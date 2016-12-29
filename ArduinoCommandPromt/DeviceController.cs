using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows.Annotations;
using Helpers.ObjectsExtentions;
using System.Text;
using System.Threading.Tasks;
using ArduinoCommandPromt.Models;


namespace ArduinoCommandPromt
{

    public class PortInfo
    {
        public string Name;
        public string Description;
    }


    public  enum DeviceGcodePlayStates {Start, Middle, End};

    public class DeviceController : IDisposable
    {
        //        private AsyncAutoResetEvent OkInputReceived = new AsyncAutoResetEvent(false);
        //private AutoResetEvent OkInputReceived = new AutoResetEvent(false);
        private ManualResetEvent OkInputReceived = new ManualResetEvent(true);

        private IDeviceSerialPort _serial;
        public IDeviceSerialPort Serial
        {
            get
            {
                return _serial;
            }
            set
            {
                if (_serial != null) _serial.DataReceived -= SerialPort_DataReceived;

                _serial = value;
                _serial.Open();
                if (!_serial.IsOpen) throw new Exception("Port not reachable");
                _serial.DataReceived += SerialPort_DataReceived;
            }
        }





        //public CancellationTokenSource InputReceivedTimeOut { get; private set; }
        public int DeviceTimoutSeconds { get; set; }


        private Func<string, DeviceGcodePlayStates, string[]> PlayAction;




        public event EventHandler<ControllerEvent<string>> MessageReceived;
        private void OnMessageReceived(string message)
        {
            if (MessageReceived != null) MessageReceived(this, new ControllerEvent<string>(message));
        }

        public event EventHandler<ControllerEvent<Exception>> ErrorOccurred;
        private void OnErrorOccurred(Exception message)
        {
            if (ErrorOccurred != null) ErrorOccurred(this, new ControllerEvent<Exception>(message));
        }




        public event EventHandler<ControllerEvent<string>> MessageSend;
        private void OnMessageSend(string message)
        {
            if (MessageSend != null) MessageSend(this, new ControllerEvent<string>("N"+CurentCodePossitionIndex+" "+message));
        }

        public event EventHandler<ControllerEvent<string>> TimoutOccurred;
        private void OnTimoutOccurred(string message)
        {
            if (TimoutOccurred != null) TimoutOccurred(this, new ControllerEvent<string>(message));
        }


        public event EventHandler<ControllerEvent<bool>> JobFinished;
        private void OnJobFinished(bool message)
        {
            if (JobFinished != null) JobFinished(this, new ControllerEvent<bool>(message));
        }



        public static string[] ComPorts
        {
            get
            {
                //return ISerialPort.GetPortNames();

                //Serial.GetPortNames();
                return SerialPort.GetPortNames();
            }
        }

        public static string[] BaundRates
        {
            get
            {
                return new string[]
                {
                    "9600",
                    "14400",
                    "19200",
                    "28800",
                    "38400",
                    "57600",
                    "115200",
                    "230400"
                };
            }
        }



        public DeviceController()
        {
            DeviceTimoutSeconds = 60;
        }


        public DeviceController(IDeviceSerialPort serial)
        {
            // string port, int baundRate
            Serial = serial;

        }






        //async
        internal void PlayFile(string GCodeFilePath, Func<string, DeviceGcodePlayStates, string[]> task = null)
        {
            if (!File.Exists(GCodeFilePath))
            {
                throw new Exception("File not Existents");
            }
            GCode = new List<string>(File.ReadAllLines(GCodeFilePath));
            this.Playing = true;
            this.CurentCodePossitionIndex = -1;
            SerialPortData.Clear();
            PlayAction = task;


            if (this.PlayAction != null)
            {
                var extraGCode = this.PlayAction(null, DeviceGcodePlayStates.Start);
                this.priorityCommands.AddRange(extraGCode);
            }

            try
            {
                while (this.Playing)
                {

                    //InputReceivedTimeOut.CancelAfter(TimeSpan.FromSeconds(5));

                    if (!SendNextLine())
                    {
                        if (this.PlayAction == null) break;

                        var extraGCode = this.PlayAction(null, DeviceGcodePlayStates.End);
                        this.priorityCommands.AddRange(extraGCode);
                        if (!SendNextLine()) break;

                    }

                    if (!OkInputReceived.WaitOne(TimeSpan.FromSeconds(this.DeviceTimoutSeconds)))
                    {
                        this.OnTimoutOccurred("Device not responding");
                        this.OnJobFinished(false);
                        break;
                    }

                }
            }
            catch (Exception ex)
            {

                this.OnErrorOccurred(ex);
            }
            finally
            {
                this.Playing = false;
            }

            return;
        }

        public List<string> GCode { get; set; }

        public int CurentCodePossitionIndex { get; private set; }


        private string GetNextGcodeline()
        {
            string command = "";

            while(true)
            {
                if (this.priorityCommands.Count > 0)
                {
                    command = this.priorityCommands[0].Trim();
                    this.priorityCommands.RemoveAt(0);
                }
                else
                {
                    CurentCodePossitionIndex = CurentCodePossitionIndex + 1;
                    if (this.GCode.Count() <= CurentCodePossitionIndex) break;

                    command = (GCode[CurentCodePossitionIndex]).Trim();
                    if (this.PlayAction != null)
                    {
                        var extraGCode = this.PlayAction(command, DeviceGcodePlayStates.Middle);
                        if (extraGCode != null && extraGCode.Length > 0)
                        {
                            this.priorityCommands.AddRange(extraGCode);
                            continue;
                        }
                    }

                }
                if (command.IsNullOrEmpty() || command.Substring(0, 1) == "#" || command.Substring(0, 1) == ";")
                {
                    continue;
                }
                break;
            }
            return command+"\n";


        }

        private bool SendNextLine()
        {
            string command = GetNextGcodeline();

            if (command.Trim().IsNullOrEmpty())
            {
                Playing = false;
                return false;
            }
            this.SerialSend(command, true);
            return true;
        }

        private Object SerialSendLockObject = new Object();
        private bool SerialSend(string command, bool waitToSend = false)
        {
            if (Serial == null || !Serial.IsOpen) throw new Exception("Port is Closed");

            lock (SerialSendLockObject)
            {
                var timespan = TimeSpan.FromSeconds(0);
                if (waitToSend)
                {
                    timespan = TimeSpan.FromSeconds(this.DeviceTimoutSeconds);
                }

                if (!OkInputReceived.WaitOne(timespan) && !waitToSend)
                {
                    return false;
                }


                OkInputReceived.Reset();
                this.Serial.Write(command);
                this.OnMessageSend(command);
                return true;
            }
        }

        public void Send(string command, bool waitToSend = false)
        {
            if (this.Playing)
            {
                this.priorityCommands.Add(command);

            }
            else
            {
                SerialSend(command, waitToSend);
            }


        }





        readonly StringBuilder SerialPortData = new StringBuilder();

        object SerialPort_DataReceivedLockObject = new object();
        private List<String> priorityCommands=new List<string>();
        void SerialPort_DataReceived(object sender, ControllerEvent<string> e)
        {
            lock (SerialPort_DataReceivedLockObject)
            {
                ProcessReceivedSerialData((IDeviceSerialPort)sender);
            }

        }

        void ProcessReceivedSerialData(IDeviceSerialPort port)
        {

            string indata = port.ReadExisting();
            SerialPortData.Append(indata);
            SerialPortData.Replace("\r", "");
            SerialPortData.Replace("\t", "");
            var doSend = false;

            int commandEndPossition = -1;
            while ((commandEndPossition = SerialPortData.IndexOf("\n")) >= 0)
            {
                var command = SerialPortData.ToString(0, commandEndPossition);
                SerialPortData.Remove(0, commandEndPossition + 1);
                if (command.IsNullOrEmpty()) continue;

                this.OnMessageReceived(command);
                if (command.Trim().ToUpper() == "OK")
                {
                    doSend = true;
                }
            }
            if (!doSend) return;
            OkInputReceived.Set();
        }


        public bool Playing { get; set; }

        internal void Stop()
        {

            Playing = false;
            //InputReceivedTimeOut.Cancel();

        }

        public void SerialClose()
        {
            if (Serial != null && Serial.IsOpen)
            {
                Serial.Close();
            }
        }

        public void Dispose()
        {
            SerialClose();
            Stop();
        }

        //internal void SerialConnect(string p1, int p2)
        //{
        //    throw new NotImplementedException();
        //}

        public bool IsConnected
        {
            get
            {
                if(this.Serial==null)return false;
                return this.Serial.IsOpen;

            }
        }
    }
}
