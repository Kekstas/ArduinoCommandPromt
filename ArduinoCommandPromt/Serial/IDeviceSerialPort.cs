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


namespace ArduinoCommandPromt
{
    public interface IDeviceSerialPort
    {
        void Open();
        //void Open(string port, int baundRate);
        void Write(string command);
        bool IsOpen { get; }
        void Close();


       // void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e);


        event EventHandler<ControllerEvent<string>> DataReceived;

        //public event EventHandler<ControllerEvent<string>> MessageReceived;



        string ReadExisting();
    }


    public static class ISerialPortExtentions
    {
        public static string[] GetPortNames(this IDeviceSerialPort serialPort)
        {
            return SerialPort.GetPortNames();
        }


    }




}
