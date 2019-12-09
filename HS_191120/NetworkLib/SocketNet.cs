using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShareLib;
using DefineLib;

namespace NetworkLib
{
    public delegate void DelClientConnectCheck(int Port, int ClientIP, bool Connect);
    
    public class SocketNet
    {
        public Server server = null;

        public SocketNet(int ServerPort, bool ConnectCheck) // port 번호, bool 변수 추가... connectcheck
        {
            server = new Server(ServerPort, ConnectCheck);
        }

        public void SocetNetEnd()
        {
            server.Stop();
        }
    }
         
    public class Server
    {
        public Share share = Share.Initialize;

        int _serverPort = 0;
        private bool shutdown = false;

        string SocketName = string.Empty;

        TcpListener server = null;
        //TcpClient client = null;

        Thread AcceptThread = null;
        HandleClient h_client = null;
        CancellationTokenSource cts = new CancellationTokenSource();

        Thread ConnectCheckThread = null;

        public ConcurrentDictionary<HandleClient, HandleClient> ClientList = new ConcurrentDictionary<HandleClient, HandleClient>();
        public event DelClientConnectCheck DelClientConnectCheckHandler = null;

        public Server(int serverPort, bool ConnectCheck)
        {
            _serverPort = serverPort;
            server = new TcpListener(new IPEndPoint(IPAddress.Any, serverPort));
            //client = default(TcpClient);
            server.Start();

            AcceptThread = new Thread(InitSocket);
            AcceptThread.IsBackground = true;
            AcceptThread.Start();

            if (ConnectCheck)
            {
                ConnectCheckThread = new Thread(ConnectCheckLoop);
                ConnectCheckThread.IsBackground = true;
                ConnectCheckThread.Start();
            }
        }

        private void InitSocket()
        {
            //server = new TcpListener(new IPEndPoint(IPAddress.Any, _serverPort));
            ////client = default(TcpClient);
            //server.Start();

            while (!shutdown)
            {
                try
                {
                    var Clients = server.AcceptTcpClientAsync();
                    Clients.Wait(cts.Token);
                    var AcceptClient = Clients.Result;

                    h_client = new HandleClient(AcceptClient);
                    //client = server.AcceptTcpClient();
                    
                    if (_serverPort == 1010)
                        SocketName = "Common Socket";
                    else if (_serverPort == 1011)
                        SocketName = "Voltage Read Socket";

                    if (!ClientList.TryAdd(h_client, h_client))
                    {
                        share.EventLog(h_client.ClientNumber, "[Connect Fail]", SocketName);

                        throw new InvalidOperationException("Tried to add connection twice");
                    }
                    else
                    {
                        share.EventLog(h_client.ClientNumber, "[Connected]", SocketName);

                        if (DelClientConnectCheckHandler != null)
                            DelClientConnectCheckHandler.Invoke(_serverPort, h_client.ClientNumber, true);
                    }

                    //h_client = new HandleClient(client);
                    //h_client.OnReceived += new HandleClient.MessageDisplayHandler(DataReceived);    //  1 Client / 1 Thread
                    //h_client.OnDisconnected += new HandleClient.DisconnectedHandler(RemoveClient);
                    //h_client.StartHandle();
                }
                catch (SocketException se)
                {
                    //share.EventLog(string.Format("{0} (0) > {1}", MethodBase.GetCurrentMethod().Name, se.Message), false);
                    share.EventLog(null, string.Empty, string.Format("\t{0} (0) > {1}", MethodBase.GetCurrentMethod().Name, se.Message), false);

                    RemoveClient(h_client);
                }
                catch (Exception ex)
                {
                    //share.EventLog(string.Format("{0} (1) > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);
                    share.EventLog(null, string.Empty, string.Format("\t{0} (1) > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);

                    RemoveClient(h_client);
                }
            }
        }

        private void ConnectCheckLoop()
        {
            while (!shutdown)
            {
                //IPEndPoint[] endPoints = IPGlobalProperties.GetActiveTcpListeners();
                try
                {
                    foreach (KeyValuePair<HandleClient, HandleClient> el in ClientList)
                    {
                        try
                        {
                            TcpState ConnectState = GetState(el.Key.ClientSocket);

                            if (!ConnectState.Equals(TcpState.Established))
                                RemoveClient(el.Key);
                            else
                                continue;
                        }
                        catch (SocketException se)
                        {
                            //share.EventLog(string.Format("{0} (0) > {1}", MethodBase.GetCurrentMethod().Name, se.Message), false);
                            share.EventLog(null, string.Empty, string.Format("\t{0} (0) > {1}", MethodBase.GetCurrentMethod().Name, se.Message), false);

                            RemoveClient(el.Key);
                        }
                        catch (Exception ex)
                        {
                            //share.EventLog(string.Format("{0} (1) > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);
                            share.EventLog(null, string.Empty, string.Format("\t{0} (1) > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);

                            RemoveClient(el.Key);
                        }

                        System.Threading.Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    //share.EventLog(string.Format("{0} (2) > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);
                    share.EventLog(null, string.Empty, string.Format("\t{0} (2) > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);
                }
            }
        }

        public TcpState GetState(TcpClient tcpClient)
        {
            //var foo = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
            //return foo != null ? foo.State : TcpState.Unknown;

            try
            {
                TcpConnectionInformation tcpConnectionInformation = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint) && x.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint));

                return tcpConnectionInformation != null ? tcpConnectionInformation.State : TcpState.Unknown;

                //TcpConnectionInformation[] tcpConnectionInformation = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint) && x.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint)).ToArray();
                //
                //if (tcpConnectionInformation != null && tcpConnectionInformation.Length > 0)
                //{
                //    TcpState tcpState = tcpConnectionInformation.First().State;
                //
                //    return tcpState;
                //}
                //else
                //    return TcpState.Unknown;
            }
            catch (InvalidOperationException Ie)
            {
                //share.EventLog(string.Format("{0} (0) > {1}", MethodBase.GetCurrentMethod().Name, Ie.Message), false);
                share.EventLog(null, string.Empty, string.Format("\t{0} (0) > {1}", MethodBase.GetCurrentMethod().Name, Ie.Message), false);

                return TcpState.Unknown;
            }
            catch (Exception ex)
            {
                //share.EventLog(string.Format("{0} (1) > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);
                share.EventLog(null, string.Empty, string.Format("\t{0} (1) > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);

                return TcpState.Unknown;
            }
        }

        public void RemoveClient(HandleClient clientSocket)
        {
            HandleClient rmclient;

            if(clientSocket != null)
            {
                if (ClientList.ContainsKey(clientSocket))
                {
                    clientSocket.DisConnectTime = DateTime.Now;
                    ClientList.TryRemove(clientSocket, out rmclient);
                }

                if (DelClientConnectCheckHandler != null)
                    DelClientConnectCheckHandler.Invoke(_serverPort, clientSocket.ClientNumber, false);

                share.EventLog(clientSocket.ClientNumber, "[Disconnected]", SocketName);
            }
        }

        public void Stop()
        {
            shutdown = true;
            cts.Cancel();

            if (AcceptThread != null)
                AcceptThread.Abort();
            //AcceptThread.Join();

            if (ConnectCheckThread != null)
                ConnectCheckThread.Abort();
                //ConnectCheckThread.Join();

            if (server != null)
            {
                server.Stop();
                server = null;
            }

            share.EventLog(null, "[Closing]\t", "Socket Thread Stop");
        }
    }

    public class HandleClient
    {
        //Thread t_handler;

        //public delegate void MessageDisplayHandler(string text);
        //public event MessageDisplayHandler OnReceived;

        //public delegate void DisconnectedHandler(TcpClient client);
        //public event DisconnectedHandler OnDisconnected;

        public TcpClient ClientSocket { get; private set; }
        public int ClientNumber { get; set; }
        public DateTime ConnectTime { get; set; }
        public DateTime? DisConnectTime { get; set; }

        public HandleClient(TcpClient clientSocket)
        {
            char[] SplitStd = new char[2];
            SplitStd[0] = '.';
            SplitStd[1] = ':';

            string[] Temp = clientSocket.Client.LocalEndPoint.ToString().Split(SplitStd);

            int clientNumber = 0;
            Int32.TryParse(Temp[3], out clientNumber);

            /////////////////////////////////////////////////////////////////////////////////////////////////

            this.ClientSocket = clientSocket;
            this.ClientNumber = clientNumber;
            this.ConnectTime = DateTime.Now;
        }
    }
}
