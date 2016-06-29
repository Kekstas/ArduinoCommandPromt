using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using System.Threading.Tasks;

namespace ArduinoCommandPromt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public ArduinoController Arduino { get; set; }
        public string GCodeFilePath { get; set; }
        public StringBuilderWrapper ConsoleContent { get; private set; }



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
                this.ButtonConnect.Content = "Disconnect";
            }
            catch (Exception zzz)
            {
                MessageBox.Show(zzz.Message, "Error");
            }

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
                GCodeFilePath = dlg.FileName;
                ListBoxCode.Items.Clear();
                ListBoxCode.Load(GCodeFilePath);
                Settings.Default["LastDialogOpenLocation"] = Path.GetDirectoryName(GCodeFilePath);
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
            Action zz = (async () => { await this.Arduino.PlayFile(GCodeFilePath); });
            zz();


        }



        private void ButtonStop_OnClick(object sender, RoutedEventArgs e)
        {
            this.Arduino.Playing = false;
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            ConsoleContent.Clear();
        }



    }
}
