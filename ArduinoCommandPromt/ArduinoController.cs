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
using Nito.AsyncEx;

namespace ArduinoCommandPromt
{

    public class PortInfo
    {
        public string Name;
        public string Description;
    }

    public class ArduinoController
    {
        private AsyncAutoResetEvent OkInputReceived = new AsyncAutoResetEvent(false);


        public static string[] ComPorts {
            get
            {
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

        public SerialPort SerialPort { get; set; }


        public ArduinoController(string port, int baundRate)
        {
            SerialPort = new SerialPort(port, baundRate);
            SerialPort.Open();
            if (!SerialPort.IsOpen) throw new Exception("Port not reachable");
            SerialPort.DataReceived+=SerialPort_DataReceived;

        }


        // Method to prepare the WMI query connection options.
        public static ConnectionOptions PrepareOptions()
        {
            ConnectionOptions options = new ConnectionOptions();
            return options;
        }

        // Method to prepare WMI query management scope.
        public static ManagementScope PrepareScope(string machineName, ConnectionOptions options, string path)
        {
            ManagementScope scope = new ManagementScope();
            scope.Path = new ManagementPath(@"\\" + machineName + path);
            scope.Options = options;
            scope.Connect();
            return scope;
        }


        // Method to retrieve the list of all COM ports.
        public static List<PortInfo> FindComPorts()
        {

            //List<PortInfo> portInfoList;

            List<PortInfo> portInfoList = new List<PortInfo>();
            ConnectionOptions options = PrepareOptions();
            ManagementScope scope =  PrepareScope(Environment.MachineName, options, @"\root\CIMV2");

            // Prepare the query and searcher objects.
            ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
            ManagementObjectSearcher portSearcher = new ManagementObjectSearcher(scope, objectQuery);

            using (portSearcher)
            {
                string caption = null;
                // Invoke the searcher and search through each management object for a COM port.
                foreach (ManagementObject currentObject in portSearcher.Get())
                {
                    if (currentObject != null)
                    {
                        object currentObjectCaption = currentObject["Caption"];
                        if (currentObjectCaption != null)
                        {
                            caption = currentObjectCaption.ToString();
                            if (caption.Contains("(COM"))
                            {
                                PortInfo portInfo = new PortInfo();
                                portInfo.Name = caption.Substring(caption.LastIndexOf("(COM")).Replace("(", string.Empty).Replace(")", string.Empty);
                                portInfo.Description = caption;
                                portInfoList.Add(portInfo);
                            }
                        }
                    }
                }
            }
            return portInfoList;
        }



        internal void Send(string command)
        {
            this.SerialPort.Write(command);
        }

        internal async Task<int> PlayFile(string GCodeFilePath)
        {
            if (!File.Exists(GCodeFilePath))
            {
                 throw new Exception("File not Existents");
            }
            GCode = File.ReadAllLines(GCodeFilePath);
            this.Playing = true;
            this.CurentCodePossitionIndex = -1;
            while (this.Playing)
            {
                SendNextLine();
                var inputReceivedTimeOut = new CancellationTokenSource();
                inputReceivedTimeOut.CancelAfter(TimeSpan.FromSeconds(5));
                try
                {
                    await OkInputReceived.WaitAsync(inputReceivedTimeOut.Token);
                }
                catch (TaskCanceledException ex)
                {
                    this.Playing = false;
                    break;
                }
            }

            return 0;
        }




        public string[] GCode { get; set; }

        public int CurentCodePossitionIndex {  get; private set; }


        private void SendNextLine()
        {

            CurentCodePossitionIndex = CurentCodePossitionIndex + 1;
            string command = "";
            var foundToSendCommand = false;
            while (this.GCode.Count() > CurentCodePossitionIndex)
            {
                //this.ListBoxCode.SelectedIndex = CurentCodePossitionIndex;
               // command = this.ListBoxCode.SelectedItem.ToString().Trim();
                command = GCode[CurentCodePossitionIndex];
                if (command.IsNullOrEmpty() || command.Substring(0, 1) == "#" || command.Substring(0, 1) == ";")
                {
                    CurentCodePossitionIndex++;
                    continue;
                }

                command = command.Trim() + " \n";
                this.Send(command);
                foundToSendCommand = true;
                break;
            }

            Playing = foundToSendCommand && Playing;

        }


        StringBuilder SerialPortData = new StringBuilder();

        void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            var time = DateTime.Now.ToString();
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

                //this.Dispatcher.Invoke((Action)(() =>
                //{
                //TextBlockConsole.AppendText(string.Format("{0} {1}\n", time, command));
                //}));
                //ConsoleContent.AppendLine(time + " " + command);
                if (Playing == false) continue;
                if (command.Trim().ToUpper() == "OK")
                {
                    doSend = true;
                }
            }
            if (!doSend) return;
            OkInputReceived.Set();
            // SendNextLine();
            // this.Dispatcher.Invoke((Action)(() => { SendNextLine(); }));

        }



        public bool Playing { get; set; }
    }
}
