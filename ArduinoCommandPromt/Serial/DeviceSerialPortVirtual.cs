using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArduinoCommandPromt.Models;

namespace ArduinoCommandPromt.Serial
{
    public class DeviceSerialPortVirtual : IDeviceSerialPort
    {
        public event EventHandler<Models.ControllerEvent<string>> DataReceived;

        private SerialPort Serial { get;   set; }
        private string _port;
        private int _baundRate;
        private bool _isOpen;


        public DeviceSerialPortVirtual(string port, int baundRate)
        {
            _port = port;
            _baundRate = baundRate;
        }


        //public void Open(string port, int baundRate)
        //{
        //    _isOpen = true;
        //    return;
        //}

        public void Write(string command)
        {
            Thread.Sleep(10);
            OnMessageReceived(@"Message should come");
        }



        private void OnMessageReceived(string message)
        {
            if (DataReceived != null) DataReceived(this, new ControllerEvent<string>(message));
        }


        public bool IsOpen
        {
            get { return _isOpen; }


        }

        public void Close()
        {
            _isOpen = false;
        }




        public void Open()
        {
            _isOpen = true;
        }



        public string ReadExisting()
        {
            return "OK\n";
        }


    }
}
