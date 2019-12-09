using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ShareLib;
using DefineLib;

namespace NetworkLib
{
    public class SerialNet
    {
        public Share share = Share.Initialize;

        public SerialPort SerialHandle = new SerialPort();

        public SerialNet(string SerialPortName)
        {
            try
            {
                SerialHandle.PortName = SerialPortName;
                SerialHandle.BaudRate = 115200;
                SerialHandle.DataBits = 8;
                SerialHandle.StopBits = StopBits.One;// StopBits.One;
                SerialHandle.Parity = Parity.None;
                //SerialHandle.WriteTimeout = 30;
                //SerialHandle.ReadTimeout = 30;

                SerialHandle.Open();
            }
            catch (Exception ex)
            {
                //.EventLog(Convert.ToInt32(SerialPortName), string.Empty, string.Format("\t{0} > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);
            }
        }

        public bool SerialSendRecv(byte[] SendByteData, out byte[] ReadByteData, bool ReadCheck)
        {
            ReadByteData = new byte[64];

            object LockObject = new object();
            
            if (SerialHandle.IsOpen)
            {
                //lock (LockObject)
                {
                    try
                    {
                        //Before
                        //Base.Sleep(500);
                        //After
                        //System.Threading.Thread.Sleep(500);

                        //Before
                        //byte[] values = new byte[1];
                        //
                        //for (int i = 0; i < SendByteData.Length; i++)
                        //{
                        //    values[0] = SendByteData[i];
                        //    SerialHandle.Write(values, 0, 1);
                        //    System.Threading.Thread.Sleep(5);
                        //}

                        //After
                        //foreach (byte values in SendByteData)
                        //{
                        //    byte[] _values = new byte[] { values };
                        //    SerialHandle.Write(_values, 0, 1);
                        //    System.Threading.Thread.Sleep(5);
                        //}
                        
                        SerialHandle.Write(SendByteData, 0, SendByteData.Count());
                        System.Threading.Thread.Sleep(100);

                        //Before
                        //Base.Sleep(100);
                        //After
                        //System.Threading.Thread.Sleep(10);
                        if (ReadCheck)
                            SerialHandle.Read(ReadByteData, 0, ReadByteData.Count());
                    }
                    catch (Exception ex)
                    {
                        share.EventLog(null, string.Empty, string.Format("\t{0} (0) > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);
                        
                        return false;
                    }
                }
            }
            else
            {
                share.EventLog(null, "[ERROR]", string.Format("Serial Port is Not Open : {0}", SerialHandle.PortName));

                return false;
            }

            return true;
        }


        public void SerialClose()
        {
            SerialHandle.Close();
        }
    }
}
