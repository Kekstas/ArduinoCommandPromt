using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Ports;

using System.Management;
using System.Threading;
using System.Windows.Annotations;
using Helpers.ObjectsExtentions;
using ArduinoCommandPromt.Models;



namespace ArduinoCommandPromt.Serial
{
    public class DeviceSerialPort : IDeviceSerialPort
    {

        private SerialPort Serial { get;   set; }
        private string _port;
        private int _baundRate;

        public event EventHandler<Models.ControllerEvent<string>> DataReceived;

        public DeviceSerialPort(string port, int baundRate)
        {
            _port = port;
            _baundRate = baundRate;
            Serial = new SerialPort(port, baundRate);
            Serial.DataReceived += OnMessageReceived;
        }



        private void OnMessageReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (DataReceived != null) DataReceived(this, new ControllerEvent<string>("Message"));
        }


        public void Open()
        {
           Serial.Open();
        }

        public void Write(string command)
        {
            Serial.Write(command);
        }

        public bool IsOpen
        {
            get { return Serial.IsOpen; }

        }

        public void Close()
        {
           Serial.Close();
        }





        public string ReadExisting()
        {
            return Serial.ReadExisting();
        }
    }
}
