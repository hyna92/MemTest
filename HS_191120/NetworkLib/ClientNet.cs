using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShareLib;
using DefineLib;
using System.IO;
using System.Reflection;
using System.Net;
using System.ComponentModel;

namespace NetworkLib
{
    public delegate void DelReceiveData(int ClietNumber, DataPacket RecvPacket);
    public delegate void DelRequestInitData(int ClietNumber);//, int Step, int? option);

    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    public class ClientNet
    {
        public Share share = null;
        
        public int ClientNum;

        string ConnectIP;
        int ConnectPort = 12344;

        public TcpClient ClientSocket = null;

        private object LockObject = null;
        public Thread SocketThread = null;

        public event DelReceiveData DelReceiveDataHandler = null;
        public event DelRequestInitData DelRequestInitDataHandler = null;

        //private static ManualResetEvent sendDone = null;// new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = null;// new ManualResetEvent(false);

        //private AsyncCallback ConnectCallback = new AsyncCallback();

        public ClientNet(int clientnum)
        {
            share = Share.Initialize;

            ClientNum = clientnum;

            ConnectIP = "192.168.0." + ClientNum.ToString();

            //sendDone = new ManualResetEvent(false);
            receiveDone = new ManualResetEvent(false);

            SocketThread = new Thread(SocketProceeding);
        }

        private void SocketProceeding()
        {
            LockObject = new object();

            while (!share.shutdown)
            {
                //lock (LockObject)
                {
                    try
                    {
                        // Check Network
                        bool NetConnected = TargetPingCheck();

                        // If Network is not connected, remove socket
                        if (!NetConnected)
                        {
                            RemoveSocket(ClientNum);
                            continue;
                        }

                        // If Socket is not exist, make socket 
                        if (ClientSocket == null)
                            CreateSocket();


                        if(ClientSocket != null)
                        {
                            SocketSendRecv(new DataPacket(StaticDefine.MSG_CATEGORY_TEST, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_STATUS, StaticDefine.MSG_OPTION_NULL));

                            //if (share.BoardInfoDic[ClientNum].IsTesting)
                            if ((share.BoardInfoDic[ClientNum].TestState > StaticDefine.TEST_STATE_TESTSTATE_START) && (share.BoardInfoDic[ClientNum].TestState < StaticDefine.TEST_STATE_TESTSTATE_END))
                            {
                                lock (LockObject)
                                {
                                    SocketSendRecv(new DataPacket(StaticDefine.MSG_CATEGORY_TEST, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_ERROR_COUNT, StaticDefine.MSG_OPTION_NULL));

                                    System.Threading.Thread.Sleep(300);

                                    //if (share.BoardInfoDic[ClientNum].TestOccurErrorCount != 0)
                                    for (int cnt = 0; cnt < share.BoardInfoDic[ClientNum].TestOccurErrorCount; cnt++)
                                    {
                                        SocketSendRecv(new DataPacket(StaticDefine.MSG_CATEGORY_TEST, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_ERROR_DATA, share.BoardInfoDic[ClientNum].ErrorList.Count));
                                        System.Threading.Thread.Sleep(20);
                                    }

                                    SocketSendRecv(new DataPacket(StaticDefine.MSG_CATEGORY_TEST, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_PROGRESS, StaticDefine.MSG_OPTION_NULL));

                                    //System.Threading.Thread.Sleep(20);
                                    //SocketSendRecv(new DataPacket(StaticDefine.MSG_CATEGORY_TEST, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_STATUS, StaticDefine.MSG_OPTION_NULL));
                                }
                            }
                        
                            System.Threading.Thread.Sleep(500);
                        }
                    }
                    catch(SocketException se)
                    {
                        share.EventLog(ClientNum, "[ERROR]\t", string.Format("{0} SocketException : {1}", MethodBase.GetCurrentMethod().Name, se.ToString()), true);
                        //RemoveSocket(ClientNum);
                    }
                    catch (InvalidOperationException opex) 
                    {
                        share.EventLog(ClientNum, "[ERROR]\t", string.Format("{0} InvalidOperationException : {1}", MethodBase.GetCurrentMethod().Name, opex.ToString()), true);
                        //{"연결되지 않은 소켓에서 작업할 수 없습니다."}
                    }
                    catch (IOException ioex)
                    {
                        share.EventLog(ClientNum, "[ERROR]\t", string.Format("{0} IOException : {1}", MethodBase.GetCurrentMethod().Name, ioex.ToString()), true);
                    }
                    catch (Exception ex)
                    {
                        share.EventLog(ClientNum, "[ERROR]\t", string.Format("{0} Exception : {1}", MethodBase.GetCurrentMethod().Name, ex.ToString()), true);
                        
                        //if (!ExcepMsg.Contains("응답이 없어 연결하지 못했거나"))
                        //if(!ClientSocket.Connected)
                        //    RemoveSocket(ClientNum);

                        // if Receive null ??
                    }

                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        private bool TargetPingCheck()
        {
            bool ConnectCheck = false;

            try
            {
                Ping pingSender = new Ping();
                PingReply pingReply = pingSender.Send(ConnectIP, 1000);

                if (pingReply.Status == IPStatus.Success)
                    ConnectCheck = true;
            }
            catch (Exception ex)
            {
                ConnectCheck = false;
                share.EventLog(ClientNum, "[ERROR]\t", "Ping Reply Error", false);
            }

            return ConnectCheck;
        }

        private void CreateSocket()
        {
            ClientSocket = new TcpClient();

            IPAddress ip = IPAddress.Parse(ConnectIP);

            IAsyncResult async = ClientSocket.Client.BeginConnect(ip, ConnectPort, null, null);

            //System.Threading.WaitHandle waitHandle = async.AsyncWaitHandle;

            try
            {
                if (!async.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2), false))
                {
                    //ClientSocket.EndConnect(async);
                    RemoveSocket(ClientNum);
                }
                else
                {
                    share.BoardInfoDic[ClientNum].ConnectState = true;

                    share.EventLog(ClientNum, "[CONNECT]\t", "Socket", false);

                    if (DelRequestInitDataHandler != null)
                        DelRequestInitDataHandler.Invoke(ClientNum);
                }

            }
            catch (Exception ex)
            {
                share.EventLog(ClientNum, "[ERROR]\t", string.Format("{0} Create Socket : {1}", MethodBase.GetCurrentMethod().Name, ex.ToString()), true);
                RemoveSocket(ClientNum);
            }
            finally
            {
                //waitHandle.Close();
            }
        }
        
        public void RemoveSocket(int BoardNum)
        {
            //LockObject = new object();

            //lock(LockObject)
            {

                try
                {
                    share.BoardInfoDic[BoardNum].ConnectState = false;
                    share.BoardInfoDic[BoardNum].TestState = StaticDefine.TEST_STATE_NULL;

                    //sendDone.Dispose();
                    receiveDone.Dispose();


                    if (ClientSocket != null)
                    {
                        try
                        {
                            //ClientSocket.Client.Disconnect(true);
                            //System.Threading.Thread.Sleep(20);
                            ClientSocket.Close();
                            //System.Threading.Thread.Sleep(20);
                            //ClientSocket.Dispose();

                            ClientSocket = null;
                        }
                        catch (Exception ex)
                        {
                            share.EventLog(ClientNum, "[ERROR]\t", string.Format("{0} Remove Socket 1 : {1}", MethodBase.GetCurrentMethod().Name, ex.ToString()), true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    share.EventLog(ClientNum, "[ERROR]\t", string.Format("{0} Remove Socket 2 : {1}", MethodBase.GetCurrentMethod().Name, ex.ToString()), true);
                }
            }
        }

        public int SocketSendRecv(DataPacket packet)
        {
            LockObject = new object();

            int SocketResult = StaticDefine.NET_FAIL;
            //lock (LockObject)
            {
                try
                {
                    //if (ClientSocket != null)
                    {
                        Send(ClientSocket.Client, packet);
                        //sendDone.WaitOne();

                        Receive(ClientSocket.Client);
                        //receiveDone.WaitOne();

                        //System.Threading.Thread.Sleep(100);

                        SocketResult = StaticDefine.NET_SUCCESS;
                    }
                }
                catch(SocketException se)
                {
                    share.EventLog(ClientNum, "[ERROR]\t", string.Format("{0} SocketException : {1}", MethodBase.GetCurrentMethod().Name, se.ToString()), true);
                    RemoveSocket(ClientNum);

                    SocketResult = StaticDefine.NET_FAIL;
                }
                catch(Exception ex)
                {
                    share.EventLog(ClientNum, "[ERROR]\t", string.Format("{0} Exception : {1}", MethodBase.GetCurrentMethod().Name, ex.ToString()), true);
                    SocketResult = StaticDefine.NET_FAIL;
                }
            }

            return SocketResult;
        }

        private void Send(Socket client, DataPacket data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Convert_PacketToByte(data);


            // Begin sending the data to the remote device.  
            //client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
            client.BeginSend(byteData, 0, byteData.Length, 0, null, null);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;
        
                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);
        
                // Signal that all bytes have been sent.  
                //sendDone.Set();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                RemoveSocket(ClientNum);
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                 client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);

                //IAsyncResult ar = client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, null, null);
                //
                //// Read data from the remote device.  
                //int bytesRead = client.EndReceive(ar);
                //
                //if (bytesRead > 0)
                //{
                //    // There might be more data, so store the data received so far.  
                //    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                //
                //    // Get the rest of the data.  
                //
                //    DataPacket data = Convert_ByteToPacket(state.buffer);
                //
                //    if (DelReceiveDataHandler != null)
                //        DelReceiveDataHandler.Invoke(ClientNum, data);
                //}
            }
            catch (Exception e)
            {
                share.EventLog(ClientNum, "[ERROR]\t", string.Format("Receive Error : {0}", e.ToString()), true);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
        
                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);
        
                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
        
                    // Get the rest of the data.  
                    //client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,  new AsyncCallback(ReceiveCallback), state);
        
                    DataPacket data = Convert_ByteToPacket(state.buffer);
        
                    if (DelReceiveDataHandler != null)
                        DelReceiveDataHandler.Invoke(ClientNum, data);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        //response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                share.EventLog(ClientNum, "[ERROR]\t", string.Format("Receive Callback Error : {0}", e.ToString()), true);
            }
        }

        private byte[] Convert_PacketToByte(DataPacket packet)
        {
            byte[] TempPacket = new byte[5];

            System.Buffer.BlockCopy(BitConverter.GetBytes(packet.MSGCategory), 0, TempPacket, 0, 1);
            System.Buffer.BlockCopy(BitConverter.GetBytes(packet.MSGAction), 0, TempPacket, 1, 1);
            System.Buffer.BlockCopy(BitConverter.GetBytes(packet.MSGContent), 0, TempPacket, 2, 1);
            System.Buffer.BlockCopy(BitConverter.GetBytes(packet.MSGOption), 0, TempPacket, 3, 2);

            return TempPacket;
        }
        
        private DataPacket Convert_ByteToPacket(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer, false);
            BinaryReader br = new BinaryReader(ms);

            int MsgCategory = ConvertByteToInt(br.ReadBytes(1));
            int MsgAction = ConvertByteToInt(br.ReadBytes(1));
            int MsgContent = ConvertByteToInt(br.ReadBytes(1));
            int MsgOption = ConvertByteToInt(br.ReadBytes(2));
            string MsgData = Encoding.UTF8.GetString(br.ReadBytes(64)).Trim('\0');

            DataPacket RecvData = new DataPacket(MsgCategory, MsgAction, MsgContent, MsgOption, MsgData);

            br.Close();
            ms.Close();

            return RecvData;
        }

        int ConvertByteToInt(byte[] array)
        {
            int pos = 0;
            int result = 0;
            foreach (byte by in array)
            {
                result |= ((int)by) << pos;
                pos += 8;
            }
            return result;
        }

        //public void StopThread()
        //{
        //    try
        //    {
        //        //if (sendDone != null)
        //        //    sendDone = null;
        //
        //        //if (receiveDone != null)
        //        //    receiveDone = null;
        //
        //
        //        if (SocketThread != null)
        //        {
        //            SocketThread.Abort();
        //            SocketThread = null;
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //
        //    }
        //}

    }
}
