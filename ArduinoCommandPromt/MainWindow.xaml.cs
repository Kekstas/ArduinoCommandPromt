using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArduinoCommandPromt.Helpers;
using System;
using ArduinoCommandPromt.Serial;
using Helpers.ObjectsExtentions;
using System.IO.Ports;
using System.Text;
using System.Configuration;
using System.IO;
using ArduinoCommandPromt.Properties;
using System.Threading.Tasks;
using ArduinoCommandPromt.Models;
using log4net.Config;

namespace ArduinoCommandPromt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private CNCSimulator _simulartor ;

        public CNCSimulator Simulator
        {
            get
            {
                if (_simulartor == null) _simulartor = new CNCSimulator();
                return _simulartor;

            }

        }



        private DeviceController _device;
        public DeviceController Device
        {
            get
            {
                if (_device == null)
                {

                    //Serial.Open(port, baundRate);
                    //if (!Serial.IsOpen) throw new Exception("Port not reachable");
                    //Serial.DataReceived += SerialPort_DataReceived;


                    _device = new DeviceController();
                    //var timoutSeconds = ConfigurationSettings.AppSettings.Get("DeviceTimoutSeconds");
                    var timoutSeconds = (string)Settings.Default["DeviceTimoutSeconds"];

                    if (timoutSeconds != null && !timoutSeconds.IsNullOrEmpty())
                    {
                        _device.DeviceTimoutSeconds = timoutSeconds.Parse<int>();
                    }
                  // _device.Serial = new DeviceSerialPortVirtual("port-1", 0);
                   //_device.Serial.Open();

                    _device.MessageReceived += Arduino_MessageReceived;
                    _device.JobFinished += Arduino_JobFinished;
                    _device.MessageSend += Arduino_MessageSend;
                    _device.TimoutOccurred += Arduino_TimoutReceived;
                    _device.ErrorOccurred += _Arduino_ErrorOccurred;

                }
                return _device;

            }
        }


        public string DeviceStopSequenceFileGCodeFilePath { get; set; }
        private Thread DeviceThread { get; set; }
        private string GCodeFilePath { get; set; }

        public string TimeRefMap { get; set; }

        //public StringBuilderWrapper ConsoleContent { get; private set; }
        //public Queue<string> ConsoleList { get; private set; }


        private ObservableCollection<string> _consoleList;
        public ObservableCollection<string> ConsoleList
        {
            get
            {
                if (_consoleList == null)
                    _consoleList = new ObservableCollection<string>();
                return _consoleList;
            }
        }


        public MainWindow()
        { 
            TimeRefMap = "20";
            //            ConsoleContent = new StringBuilderWrapper();
            InitializeComponent();
            this.DataContext = this;
            PortsComboBox.ItemsSource = DeviceController.ComPorts;
            if (PortsComboBox.HasItems) PortsComboBox.SelectedIndex = 0;
            BaundrateComboBox.ItemsSource = DeviceController.BaundRates;
            BaundrateComboBox.SelectedValue = "115200";


            var fileName = (string)Settings.Default["LastDialogOpenLocation"];
            OpenFile(fileName);

        }

        private bool OpenFile(string fileName)
        {
            if (!File.Exists(fileName)) return false;
            this.Title = fileName;
            GCodeFilePath = fileName;
            ListBoxCode.Items.Clear();
            ListBoxCode.Load(GCodeFilePath);

            return true;
        }

        //private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        //{
        //    //var zz = new ArduinoController();
        //    //zz.FindComPorts();

        //    SerialController.FindComPorts();
        //    //       ArduinoController.SetComPort();

        //    //ArduinoController.test();
        //}

        private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
        {
            SendCommand(TextBoxCommand.Text + "\n");
        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Device.Serial = new DeviceSerialPort(PortsComboBox.SelectedValue.ToString(), BaundrateComboBox.SelectedValue.ToString().Parse<int>());
                //Device.Serial.Open();
                //Device.SerialConnect(PortsComboBox.SelectedValue.ToString(), BaundrateComboBox.SelectedValue.ToString().Parse<int>());
                this.ButtonConnect.Content = "Disconnect";
            }
            catch (Exception zzz)
            {
                MessageBox.Show(zzz.Message, "Error");
            }

        }



        private void Log(string message)
        { 
            Logging.Log.Info(message.Trim());
            //try
            //{
            //    //   this.Dispatcher.Invoke((Action)(() => ConsoleList.Add(messageFormated)));
            //    // Logging.Log.Info(messageFormated);
            //}
            //catch (Exception)
            //{

            //}

            //if (ConsoleList.Count > 50) ConsoleList.RemoveAt(ConsoleList.Count);

        }


        void Arduino_MessageSend(object sender, ControllerEvent<string> e)
        {

            Simulator.Send(e.GetData);
            Log("MessageSend " + e.GetData);
        }

        void Arduino_TimoutReceived(object sender, ControllerEvent<string> e)
        {
            Log(e.GetData);
        }
        void Arduino_JobFinished(object sender, ControllerEvent<bool> e)
        {
            Log("JobFinished");
        }


        void Arduino_MessageReceived(object sender, ControllerEvent<string> e)
        {
            Log("MessageReceived " + e.GetData);
        }

        void _Arduino_ErrorOccurred(object sender, ControllerEvent<Exception> e)
        {
            Log("Error " + e.GetData.ToString());
            MessageBox.Show(e.GetData.Message, "Error");

        }

        private void MenuOpen_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            var initialDirectory = Path.GetDirectoryName((string)Settings.Default["LastDialogOpenLocation"]);
            if (initialDirectory.IsNullOrEmpty())
            {
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            var result = dlg.ShowDialog();
            if (result == true)
            {
                OpenFile(dlg.FileName);
                Settings.Default["LastDialogOpenLocation"] = GCodeFilePath;//Path.GetDirectoryName(GCodeFilePath);
                Settings.Default.Save();
            }
        }


        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            bool AutoScrollToEnd = true;
            if (sv.Tag != null)
            {
                AutoScrollToEnd = (bool)sv.Tag;
            }
            if (e.ExtentHeightChange == 0)// user scroll
            {
                AutoScrollToEnd = sv.ScrollableHeight == sv.VerticalOffset;
            }
            else// content change
            {
                if (AutoScrollToEnd)
                {
                    sv.ScrollToEnd();
                }
            }
            sv.Tag = AutoScrollToEnd;
            return;
        }

        private int CurentCodePossitionIndex = 0;



        private void ButtonPlay_OnClick(object sender, RoutedEventArgs e)
        {

            try
            {
                if (DeviceThread != null && DeviceThread.IsAlive) throw new Exception("Thread not stopped!");
                DeviceThread = new Thread(() =>
                    {
                        this.Device.PlayFile(GCodeFilePath, refresh);
                        this.ExecuteStopSequence();
                    })
            ;
                DeviceThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }


        private string[] refresh(string command, DeviceGcodePlayStates playState)
        {
            string[] GCode = new string[] { };
            if(command!=null && command.Trim().Length>0) Simulator.Send(command);

            if (playState == DeviceGcodePlayStates.Middle)
            {
                if (RenewColor(command, out GCode)) return GCode;
                if (OnBrush(command, out GCode)) return GCode;
                if (OfBrush(command, out GCode)) return GCode;
            }
            if (playState == DeviceGcodePlayStates.Start)
            {
                StartSequence(command, out GCode); return GCode;
            }
            if (playState == DeviceGcodePlayStates.End)
            {
                EndSequence(command, out GCode); return GCode;
            }

            return GCode;



        }



        private bool OnBrush(string command, out string[] gCode, bool forceCheck = false)
        {
            gCode = null;
            command = command.Trim();
            if (command.IsNullOrEmpty()) return false;
           // if (command[0] != 'Z') return false;
            if (!forceCheck && !Simulator.ZposChange ) return false;
            if (Simulator.Zpos >= 1) return false;
            var filename = "SequenceOn.g"; 

            if (!File.Exists(filename))
            {
                throw new Exception("File " + filename + " not found");
            }
            ;
            GetSequence(command, ref gCode, filename);
            return true;
        }
       

        private bool OfBrush(string command, out string[] gCode, bool forceCheck = false)
        {
            gCode = null;
            command = command.Trim();
            if (command.IsNullOrEmpty()) return false;
            //if (command[0] != 'Z') return false;
            if (!forceCheck && !Simulator.ZposChange) return false;
            if (Simulator.Zpos < 1) return false;
            var filename = "SequenceOff.g";

            if (!File.Exists(filename))
            {
                throw new Exception("File " + filename + " not found");
            }
    ;
            GetSequence(command, ref gCode, filename);
            return true;
        }



        private bool EndSequence(string command, out string[] gCode)
        {
            gCode = null;
            var filename = "SequenceEnd.g";
            GetSequence(command, ref gCode, filename);
            return true;
        }

        private bool StartSequence(string command, out string[] gCode)
        {
            gCode = null;
            var filename = "SequenceStart.g";
            GetSequence(command, ref gCode, filename);
            return true;
        }
 
        private void GetSequence(string command, ref string[] gCode, string filename)
        {
            if (!File.Exists(filename))
            {
                throw new Exception("File " + filename + " not found");
            }
            var gText = File.ReadAllText(filename);

            if (gText.IndexOf(@"{CurentPoint}") >= 0)
            {
                string[] gCodeOn = null;

                string curentPoint=$"X{Simulator.XposPrevious} Y{Simulator.YposPrevious}";

                if (OnBrush($"Z{Simulator.Zpos}", out gCodeOn, true))
                {
                    curentPoint = curentPoint+"\n" + string.Join("\n", gCodeOn);
                }

                gText = gText.Replace(@"{CurentPoint}", curentPoint);
               
            }  


            gText = gText.Replace(@"{NextPoint}", command);
            gText = gText.Replace(@"{ARef}", ((int)this.ARefvalue).ToString());
            gCode = gText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        }

        private DateTime? _renewColorStartTime = null;
        private bool RenewColor(string command, out string[] gCode)
        {
            if (this._renewColorStartTime == null) _renewColorStartTime = DateTime.UtcNow;
            TimeSpan timeSpan = DateTime.UtcNow - (DateTime)_renewColorStartTime;

            gCode = null;
            if (timeSpan.TotalSeconds < TimeRefMap.Replace('.', ',').Parse<double>()) return false;
 

            _renewColorStartTime = DateTime.UtcNow;
            var filename = "SequenceRefresh.g";

            ;
            GetSequence(command, ref gCode, filename);
            return true;
        }


        private void ExecuteStopSequence()
        {
            //var DeviceStopSequenceFile=ConfigurationSettings.AppSettings.Get("DeviceStopSequenceFile");
            var DeviceStopSequenceFile = (string)Settings.Default["DeviceStopSequenceFile"];


            //  ArduinoCommandPromt.Properties.Settings

            if (this.Device == null) return;
            if (this.Device.Serial == null) return;
            if (!this.Device.Serial.IsOpen) return;


            //if (DeviceThread != null && DeviceThread.IsAlive)
            //{
            this.Device.Stop();
            //    return;
            //}

            if (DeviceStopSequenceFile.IsNullOrEmpty()) return;

            if (!File.Exists(DeviceStopSequenceFile)) return;

            this.Device.PlayFile(DeviceStopSequenceFile);

        }




        private void ButtonStop_OnClick(object sender, RoutedEventArgs e)
        {
            ExecuteStopSequence();
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            this.ConsoleList.Clear();
        }


        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            //if (DeviceThread != null)
            //{
            //}

            this.Device.Stop();
            this.Device.Dispose();
        }

        private void ListBoxCodeDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var command = this.ListBoxCode.SelectedItem.ToString();
            try
            {
                this.Device.Send(command + "\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }



        void SendCommand(string command)
        {
            if (this.Device == null) return;
            if (!this.Device.IsConnected) return;

            try
            {

                this.Device.Send(command + "\n", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void ARefTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                SendCommand("M1 A" + ((int)Math.Floor(ARefTextBox.Text.Replace('.', ',').TryParse<double>(0))));
            }
        }
        private void ARef_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SendCommand("M1 A" + ((int)Math.Floor(ARef.Value)));
        }


        private void BRefTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendCommand("M1 B" + ((int)Math.Floor(BRefTextBox.Text.Replace('.', ',').TryParse<double>(0))));
            }
        }

        private void CRefTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendCommand("M1 C" + ((int)Math.Floor(CRefTextBox.Text.Replace('.', ',').TryParse<double>(0))));
            }
        }

        private void DRefTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendCommand("M1 D" + ((int)Math.Floor(DRefTextBox.Text.Replace('.', ',').TryParse<double>(0))));
            }
        }

        private void ERefTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendCommand("M1 E" + ((int)Math.Floor(ERefTextBox.Text.Replace('.', ',').TryParse<double>(0))));
            }
        }

        private void FRefTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendCommand("M1 F" + ((int)Math.Floor(FRefTextBox.Text.Replace('.', ',').TryParse<double>(0))));
            }
        }

        private void FRef_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SendCommand("M1 F" + ((int)Math.Floor(FRef.Value)));
        }


        private void ARefTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            this.ARefvalue = getDouble(((TextBox)sender).Text);
        }

        public double ARefvalue { get; set; }




        private double getDouble(string stringDouble)
        {
            return stringDouble.Replace('.', ',').Parse<double>();
        }


        private void TextBoxCommand_OnKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                SendCommand(TextBoxCommand.Text + "\n");
            }

        }

        private void ExecuteCustomSequenceButton_OnClick(object sender, RoutedEventArgs e)
        {
            string[] GCode;
            RenewColor("G1", out GCode);
            if (GCode == null) return;

            foreach (var Gline in GCode)
            {
                this.SendCommand(Gline+"\n");
            }


        }

        private void TimeRefTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            TimeRefMap = this.TimeRefTextBox.Text;
        }

        private void TimeRef_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeRefMap = this.TimeRefTextBox.Text;
        }


    }
}
