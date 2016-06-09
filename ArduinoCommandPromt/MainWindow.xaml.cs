using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var zz = new ArduinoController();
            //zz.FindComPorts();

            ArduinoController.FindComPorts();
     //       ArduinoController.SetComPort();

            //ArduinoController.test();
        }

        private void ButtonSend_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
