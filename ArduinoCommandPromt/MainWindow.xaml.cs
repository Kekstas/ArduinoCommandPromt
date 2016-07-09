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

namespace ArduinoCommandPromt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public CNCSimulator Simulator { get; set; }



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
                    _device.MessageReceived += Arduino_MessageReceived;
                    _device.JobFinished += Arduino_JobFinished;
                    _device.MessageSend += Arduino_MessageSend;
                    _device.TimoutOccurred += Arduino_TimoutReceived;
                    _device.ErrorOccurred += _Arduino_ErrorOccurred;
                    _device.Serial = new DeviceSerialPortVirtual("port-1", 0);


                }
                return _device;

            }
        }


        public string DeviceStopSequenceFileGCodeFilePath { get; set; }
        private Thread DeviceThread { get; set; }
        private string GCodeFilePath{get; set; }

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
            //            ConsoleContent = new StringBuilderWrapper();
            InitializeComponent();
            this.DataContext = this;
            PortsComboBox.ItemsSource = DeviceController.ComPorts;
            if (PortsComboBox.HasItems) PortsComboBox.SelectedIndex = 0;
            BaundrateComboBox.ItemsSource = DeviceController.BaundRates;
            BaundrateComboBox.SelectedValue = "115200";


            var fileName = (string)Settings.Default["LastDialogOpenLocation"];
            OpenFile(fileName);
            Simulator = new CNCSimulator();
        }

        private bool OpenFile(string fileName)
        {
            if (!File.Exists(fileName)) return false;

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
            try
            {
                this.Device.SerialSend(TextBoxCommand.Text + "\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Device.Serial = new DeviceSerialPort(PortsComboBox.SelectedValue.ToString(), BaundrateComboBox.SelectedValue.ToString().Parse<int>());
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

            var time = DateTime.Now.ToString();
            //this.Dispatcher.Invoke((Action)(() => TextBlockConsole.AppendText()));
            var messageFormated = string.Format("{0} {1}", time, message.Trim());
            Logging.Log.Info(messageFormated);
            try
            {
                this.Dispatcher.Invoke((Action)(() => ConsoleList.Add(messageFormated)));
               // Logging.Log.Info(messageFormated);
            }
            catch (Exception)
            {

            }

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
            //async
            //Action zz = ( () => {  this.Arduino.PlayFile(GCodeFilePath); });

            //try
            //{
                if (DeviceThread != null && DeviceThread.IsAlive) throw new Exception("Thread not stopped!");
                DeviceThread = new Thread(() =>
                    {
                        this.Device.PlayFile(GCodeFilePath, refresh);
                        this.ExecuteStopSequence();
                    })
            ;
                DeviceThread.Start();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "Error");
            //}
        }


        private  string[] refresh(string command, TimeSpan timeSpan)
        {


            if (timeSpan.TotalSeconds < 20) return null;

            var filename = "Sequence.g";
            if (!File.Exists(filename))
            {
                //string[] tmp = {String.Empty};
                return null;
            };
            var GCode = File.ReadAllLines(filename);

            for(int i=0;i<GCode.Count();i++)
            {
                if (GCode[i].Contains("{NextPoint}"))
                {
                    GCode[i] = command;
                }
            }


            return GCode;



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
                this.Device.SerialSend(command + "\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void ZSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(this.Device==null) return;
            if (this.Device.Serial == null) return;
            if (!this.Device.Serial.IsOpen) return;
            //if (this.DeviceThread != null && this.DeviceThread.IsAlive) return;
            this.Device.SerialSend("M1 A" + ((int)Math.Floor(e.NewValue)) +"\n",true);
        }

        private void ZTextBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                if (this.Device == null) return;
                if (this.Device.Serial == null) return;
                if (!this.Device.Serial.IsOpen) return;
                //if (this.DeviceThread != null && this.DeviceThread.IsAlive) return;
                this.Device.SerialSend("M1 A" + ((int)Math.Floor(ZTextBox.Text.TryParse<double>(0) )) + "\n", true);
            }
        }
    }
}
