using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.IO.Ports;
using System.IO;
using System.Management;

namespace WpfApplication1
{

    public class PortInfo
    {
        public string Name;
        public string Description;
    }

    public class ArduinoController
    {
        //List<PortInfo> portInfoList;

        //ArduinoController()
        //{
        //    portInfoList=new List<PortInfo>();
        //}


        // Method to prepare the WMI query connection options.
        public static ConnectionOptions PrepareOptions()
        {
            ConnectionOptions options = new ConnectionOptions();

            //options.ImpersonationLevel = ImpersonationLevel.Impersonate;
            //options.AuthenticationLevel = AuthenticationLevel.Default;
            //options.EnablePriveleges = true;
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









        SerialPort currentPort;
        bool portFound;
























        public string AutodetectArduinoPort()
        {
            ManagementScope connectionScope = new ManagementScope();
            //SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_PnPEntity");


            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);

            try
            {
                foreach (ManagementObject item in searcher.Get())
                {
                    string desc = item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();

                    if (desc.Contains("Arduino"))
                    {
                        return deviceId;
                    }
                }
            }
            catch (ManagementException e)
            {
                /* Do Nothing */
            }

            return null;
        }



        public static void SetComPort()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    var currentPort = new SerialPort(port, 9600);
                    //if (DetectArduino())
                    //{
                    //    portFound = true;
                    //    break;
                    //}
                    //else
                    //{
                    //    portFound = false;
                    //}
                }
            }
            catch (Exception e)
            {
            }
        }

        private bool DetectArduino()
        {
            try
            {
                //The below setting are for the Hello handshake
                byte[] buffer = new byte[5];
                buffer[0] = Convert.ToByte(16);
                buffer[1] = Convert.ToByte(128);
                buffer[2] = Convert.ToByte(0);
                buffer[3] = Convert.ToByte(0);
                buffer[4] = Convert.ToByte(4);

                int intReturnASCII = 0;
                char charReturnValue = (Char)intReturnASCII;

                currentPort.Open();
                currentPort.Write(buffer, 0, 5);
                Thread.Sleep(1000);

                int count = currentPort.BytesToRead;
                string returnMessage = "";
                while (count > 0)
                {
                    intReturnASCII = currentPort.ReadByte();
                    returnMessage = returnMessage + Convert.ToChar(intReturnASCII);
                    count--;
                }
                //ComPort.name = returnMessage;

                currentPort.Close();

                if (returnMessage.Contains("HELLO FROM ARDUINO"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }


        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }


        public static bool test()
        {
            try
            {
                //The below setting are for the Hello handshake
                //byte[] buffer = new byte[5];
                //buffer[0] = Convert.ToByte("P");
                //buffer[1] = Convert.ToByte(128);
                //buffer[2] = Convert.ToByte(0);
                //buffer[3] = Convert.ToByte(0);
                //buffer[4] = Convert.ToByte(4);


                var command= "M10 XY 395 365 0.00 0.00 A0 B1 H0 S94 U65 D72\n";
                //var buffer= GetBytes(command);

                //int intReturnASCII = 0;
                //char charReturnValue = (Char)intReturnASCII;

                var currentPort = new SerialPort("COM4", 115200);

                currentPort.Open();
                //currentPort.Write(buffer, 0, buffer.Length);
                currentPort.WriteLine(command);


                while (true)
                {
                    Thread.Sleep(1000);
                    if (currentPort.BytesToRead != 0)
                    {
                     //var test_get=currentPort.ReadLine();
                        var test_get = currentPort.ReadExisting();

                    }

                    //string returnMessage = "";
                    //while (count > 0)
                    //{
                    //    intReturnASCII = currentPort.ReadByte();
                    //    returnMessage = returnMessage + Convert.ToChar(intReturnASCII);
                    //    count--;
                    //}

                    //string read = serialPort1.ReadLine();
                }
                //int count = currentPort.BytesToRead;
                //string returnMessage = "";
                //while (count > 0)
                //{
                    //intReturnASCII = currentPort.ReadByte();
                    //returnMessage = returnMessage + Convert.ToChar(intReturnASCII);
                    //count--;
                //}
                //ComPort.name = returnMessage;


                currentPort.Close();

                //if (returnMessage.Contains("HELLO FROM ARDUINO"))
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }
            catch (Exception e)
            {
                return false;
            }
        }






    }
}
