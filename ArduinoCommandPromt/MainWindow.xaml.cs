using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ArduinoCommandPromt.Helpers;
using System;
using Helpers.ObjectsExtentions;
using System.IO.Ports;
using System.Text;
using System.Configuration;
using System.IO;
using ArduinoCommandPromt.Properties;

namespace ArduinoCommandPromt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public ArduinoController Arduino { get; set; }
        public StringBuilderWrapper ConsoleContent { get; private set; }
        //private readonly StringBuilderWrapper _consoleContent = new StringBuilderWrapper();

        //public string ConsoleContent
        //{
        //    get { return _consoleContent.ToString(); }
        //}


        public MainWindow()
        {
            ConsoleContent = new StringBuilderWrapper();


            InitializeComponent();
            this.DataContext = this;
            PortsComboBox.ItemsSource = ArduinoController.ComPorts;
            if (PortsComboBox.HasItems) PortsComboBox.SelectedIndex = 0;
            BaundrateComboBox.ItemsSource = ArduinoController.BaundRates;
            BaundrateComboBox.SelectedValue = "115200";

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //var zz = new ArduinoController();
            //zz.FindComPorts();

            ArduinoController.FindComPorts();
            //       ArduinoController.SetComPort();

            //ArduinoController.test();
        }

        private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.Arduino.SerialPort.IsOpen)
            {
                this.Arduino.Send(TextBoxCommand.Text + "\n");
            }

        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {

            try
            {
                Arduino = new ArduinoController(PortsComboBox.SelectedValue.ToString(), BaundrateComboBox.SelectedValue.ToString().Parse<int>());
                Arduino.SerialPort.DataReceived += SerialPort_DataReceived;
                this.ButtonConnect.Content = "Disconnect";



            }
            catch (Exception zzz)
            {
                MessageBox.Show(zzz.Message, "Error");
            }

        }

        StringBuilder SerialPortData = new StringBuilder();

        void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            var time = DateTime.Now.ToString();
            //var ttt = new StringBuilder();
            // ttt.Remove(0, 1);
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
                 ConsoleContent.AppendLine(time + " " + command);
                if (Playing == false) continue;
                if (command.Trim().ToUpper() == "OK")
                {
                    doSend = true;
                }
            }
            if (!doSend) return;
            this.Dispatcher.Invoke((Action)(() => { SendNextLine(); }));

        }

        private void MenuOpen_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            var initialDirectory = (string)Settings.Default["LastDialogOpenLocation"];
            if (initialDirectory.IsNullOrEmpty())
            {
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            var result = dlg.ShowDialog();
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                ListBoxCode.Items.Clear();
                ListBoxCode.Load(filename);

                Settings.Default["LastDialogOpenLocation"] = Path.GetDirectoryName(filename);
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
        private bool Playing = false;


        private void ButtonPlay_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.ListBoxCode.Items.Count > 0)
            {
                CurentCodePossitionIndex = -1;
                Playing = true;
                SendNextLine();
            }

        }

        private void SendNextLine()
        {
            CurentCodePossitionIndex = CurentCodePossitionIndex + 1;
            string command = "";
            var foundToSendCommand = false;
            while (this.ListBoxCode.Items.Count > CurentCodePossitionIndex)
            {
                this.ListBoxCode.SelectedIndex = CurentCodePossitionIndex;
                command = this.ListBoxCode.SelectedItem.ToString().Trim();

                if (command.IsNullOrEmpty() || command.Substring(0, 1) == "#" || command.Substring(0, 1) == ";")
                {
                    CurentCodePossitionIndex++;
                    continue;
                }

                command = command.Trim() + " \n";
                this.Arduino.Send(command);
                //ConsoleContent.AppendLine("Sending:" + command);
                foundToSendCommand = true;
                break;
            }


            if (foundToSendCommand == false) Playing = false;
        }

        private void ButtonStop_OnClick(object sender, RoutedEventArgs e)
        {
            Playing = false;
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            ConsoleContent.Clear();
        }



    }
}
