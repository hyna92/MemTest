using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using DefineLib;
using ShareLib;
using NetworkLib;
using FunctionUC;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections;

namespace HS
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public Share share = null;

        public int MaxBoardCount;

        public PowerUC PowerFun = null;
        public VoltageUC VoltFun = null;
        public TestUC TestFun = null;
        public KvmUC KvmFun = null;
        public SerialConfigUC SerialConfigFun = null;

        public List<BoardInfo> BdInfoList = new List<BoardInfo>();
        public Dictionary<int, ClientNet> SocketDic = new Dictionary<int, ClientNet>();

        public Thread PowerDetectThread = null;
        bool IsDetect = true;

        SerialNet serial = null;


        public MainWindow()
        {
            InitializeComponent();

            if (System.Diagnostics.Process.GetProcessesByName("HS").Length > 1)
            {
                MessageBox.Show("HS is already Running");
                Application.Current.Shutdown();
                return;
            }

            share = Share.Initialize;

            share.SettingBoardColumnCount = 1;
            share.SettingBoardRowCount = 10;

            MaxBoardCount = share.SettingBoardColumnCount * share.SettingBoardRowCount;

            // Create List of Board Information
            InitSetting(out share.BoardInfoDic);

            // Create Funtion UserControl
            FunctionGridSetting();

            // Create Network object
            NetworkSetting();

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            //foreach(DataGridRow c in MainTable.RowStyle.)
            //{
            //    if(double.IsNaN(c.Width))
            //    {
            //        c.Width = c.ActualWidth;
            //    }
            //    c.Width = double.NaN;
            //}
            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            this.KeyDown += Window_KeyDown;
            this.Closing += MainWindow_Closing;


            share.EventLog(null, "[INIT]\t", "Loading Completed.");
        }


        #region 0. MainTable Event Setting


        private void MainTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid TempDataGrid = sender as DataGrid;
            List<BoardInfo> SelectBdList = TempDataGrid.SelectedItems.Cast<BoardInfo>().ToList();

            // Initial Select (Set UnSelect All) 
            foreach (BoardInfo board in BdInfoList)
            {
                board.IsSelected = false;
            }

            // Set Select Value
            foreach (BoardInfo selboard in SelectBdList)
            {
                selboard.IsSelected = true;
            }
        }
        
        private void MainTable_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid TempDataGrid = sender as DataGrid;

            //List<BoardInfo> SelectBdList = TempDataGrid.SelectedItems.Cast<BoardInfo>().ToList();
            //
            //foreach (BoardInfo board in SelectBdList)
            //{
            //    board.IsSelected = false;
            //}

            // Set UnSelect All 
            TempDataGrid.UnselectAll();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Set Expanded & Collapsed the Test Detail View
            var row = (DataGridRow)sender;
            row.DetailsVisibility = row.DetailsVisibility == Visibility.Collapsed ?
                Visibility.Visible : Visibility.Collapsed;
        }


        #endregion


        #region 0. Initial Setting

        private void InitSetting(out Dictionary<int, BoardInfo> ParaBoardDic)
        {
            ParaBoardDic = new Dictionary<int, BoardInfo>();

            // Initial Board Information
            for (int BoardCount = 0; BoardCount < MaxBoardCount; BoardCount++)
            {
                BoardInfo TempBoard = new BoardInfo();
                TempBoard.BoardNum = BoardCount + 1;
                TempBoard.IsSelected = false;
                ParaBoardDic.Add(TempBoard.BoardNum, TempBoard);

                BdInfoList.Add(TempBoard);
            }

            MainTable.ItemsSource = BdInfoList;

            //////////////////////////////////////////////////////////////////////////////////////////////
            //MainTable.Loaded += MainTable_Loaded;
            //////////////////////////////////////////////////////////////////////////////////////////////
        }

        private void FunctionGridSetting()
        {
            // Add to "Power" Function UserControl & Add to Send Event Handler (Serial)
            PowerFun = new PowerUC();
            PowerFun.DelSerialCommandSendEventHandler += Serial_CommandSendEventHandler;
            PowerFunGrid.Children.Add(PowerFun);

            // Add to "Voltage" Function UserControl & Add to Send Event Handler (TCP)
            VoltFun = new VoltageUC();
            VoltFun.DelCommandSendEventHandler += Socket_CommandSendEventHandler;
            VoltFunGrid.Children.Add(VoltFun);

            // Add to "Test" Function UserControl & Add to Send Event Handler (TCP)
            TestFun = new TestUC();
            TestFun.DelCommandSendEventHandler += Socket_CommandSendEventHandler;
            TestFunGrid.Children.Add(TestFun);

            // Add to "KVM" Function UserControl & Add to Send Event Handler (Serial)
            KvmFun = new KvmUC();
            KvmFun.DelSerialCommandSendEventHandler += Serial_CommandSendEventHandler;
            KvmFunGrid.Children.Add(KvmFun);

            // Add to "Seral Port Setting" Function UserControl & Add to Send Event Handler
            SerialConfigFun = new SerialConfigUC();
            SerialConfigFun.DelSerialSettingCommandSendHandler += SerialConfigSetting_CommandSendHandler;
            SerialConfigFunGrid.Children.Add(SerialConfigFun);
        }

        private void NetworkSetting()
        {
            // Initial Serial Network (RS232)
            if (share.SerialPortList.Count != 0)
            {
                serial = new SerialNet(share.SerialPortList[0]);

                if (serial.SerialHandle.IsOpen)
                    SerialConfigFun.ResultStringView(string.Format("Open, {0}", share.SerialPortList[0]));
                else
                    SerialConfigFun.ResultStringView(string.Format("Not yet Open, {0}", share.SerialPortList[0]));
            }
            else
                share.EventLog(null, "[INIT]\t", "SERIAL PORT does not Exist");

            
            PowerDetectThread = new Thread(BoardPowerDetecting);
            PowerDetectThread.Start();


            // Initial TCP Network (Client) 
            foreach (KeyValuePair<int, BoardInfo> el in share.BoardInfoDic)
            {
                ClientNet client = new ClientNet(el.Value.BoardNum);
                client.DelRequestInitDataHandler += RequestInitData;
                client.DelReceiveDataHandler += ReceiveSocketData;
                client.SocketThread.Start();

                SocketDic.Add(el.Value.BoardNum, client);
            }
        }
        
        private void BoardPowerDetecting()
        {
            object LockObject = new object();

            // Detect Board Power Status in Real-Time
            while (!share.shutdown)
            {
                 if (IsDetect)
                 {
                     //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                     //{
                         try
                         {
                             if ((serial != null) && (serial.SerialHandle.IsOpen))
                             {
                                 // Assign value to a SendByte (Size 3)
                                 byte[] SendByteData = new byte[3];
                                 SendByteData[0] = StaticDefine.SERIAL_CMD_PW_STATE; // Command;
                                 SendByteData[1] = StaticDefine.SERIAL_CMD_PW_STATE; // CheckSum;
                                 SendByteData[2] = StaticDefine.SERIAL_RET_CMD_END;  // EndCommand;

                                 // Send & Receive
                                 serial.SerialSendRecv(SendByteData, out byte[] RecvByteData, true);

                                 //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                                 // Assign value to a RecvByte (Size 3)
                                 byte RecvCommand = RecvByteData[0];
                                 byte RecvMSB = RecvByteData[1];
                                 byte RecvLSB = RecvByteData[2];
                                 byte RecvCheckSum = (byte)(StaticDefine.SERIAL_CMD_PW_STATE + RecvMSB + RecvLSB);

                                 // Power Status Check 
                                 if (RecvByteData[3] == RecvCheckSum)
                                 {
                                     foreach (KeyValuePair<int, BoardInfo> el in share.BoardInfoDic)
                                     {
                                         if (el.Value.BoardNum < 9)
                                         {
                                             if ((RecvLSB & (1 << el.Value.BoardNum - 1)) != 0)
                                                 el.Value.PowerState = true;
                                             else
                                                 el.Value.PowerState = false;
                                         }
                                         else
                                         {
                                             if ((RecvMSB & (1 << el.Value.BoardNum - 9)) != 0)
                                                 el.Value.PowerState = true;
                                             else
                                                 el.Value.PowerState = false;
                                         }
                                     }
                                 }
                             }
                             else
                             {

                             }
                         }
                         catch (Exception ex)
                         {
                             share.EventLog(null, "[ERROR]\t", string.Format("PowerDetect {0}", ex.ToString()), true);
                         }
                     //}));
                 }

                 System.Threading.Thread.Sleep(1000);
            }
        }

        private void RequestInitData(int ClientNumber)
        {
            // When socket connection is completed, Each Board Information initialization and initial data are requested.
            try
            {
                int Result = StaticDefine.NET_FAIL;
                
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    share.BoardInfoDic[ClientNumber].EventMessage = "READY";
                    share.BoardInfoDic[ClientNumber].Cpu = string.Empty;
                    share.BoardInfoDic[ClientNumber].Memory = string.Empty;
                    share.BoardInfoDic[ClientNumber].VddQ = 0;
                    //share.BoardInfoDic[ClientNumber].IsTesting = false;
                    share.BoardInfoDic[ClientNumber].TestStep = -1;
                    share.BoardInfoDic[ClientNumber].TestItemCount = 0;
                    share.BoardInfoDic[ClientNumber].TestState = StaticDefine.TEST_STATE_NULL;
                    share.BoardInfoDic[ClientNumber].TestString = string.Empty;
                    share.BoardInfoDic[ClientNumber].TestTotalProgress = "0";
                    share.BoardInfoDic[ClientNumber].TestTotalErrorCount = 0;
                    share.BoardInfoDic[ClientNumber].TestOccurErrorCount = 0;
                    share.BoardInfoDic[ClientNumber].TestItemList.Clear();
                    share.BoardInfoDic[ClientNumber].ErrorList.Clear();
                }));


                //System.Threading.Thread.Sleep(10);
                //Result = SendSocketData(ClientNumber, new DataPacket(StaticDefine.MSG_CATEGORY_SYSTEM, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_CPU, StaticDefine.MSG_OPTION_NULL));
                Result = SocketDic[ClientNumber].SocketSendRecv(new DataPacket(StaticDefine.MSG_CATEGORY_SYSTEM, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_CPU, StaticDefine.MSG_OPTION_NULL));
                System.Threading.Thread.Sleep(10);
                
                //Result = SendSocketData(ClientNumber, new DataPacket(StaticDefine.MSG_CATEGORY_SYSTEM, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_MEMORY, StaticDefine.MSG_OPTION_NULL));
                Result = SocketDic[ClientNumber].SocketSendRecv(new DataPacket(StaticDefine.MSG_CATEGORY_SYSTEM, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_MEMORY, StaticDefine.MSG_OPTION_NULL));
                System.Threading.Thread.Sleep(10);

                //Result = SendSocketData(ClientNumber, new DataPacket(StaticDefine.MSG_CATEGORY_VOLTAGE, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_CPU0_VDDQ, StaticDefine.MSG_OPTION_NULL));
                Result = SocketDic[ClientNumber].SocketSendRecv(new DataPacket(StaticDefine.MSG_CATEGORY_VOLTAGE, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_CPU0_VDDQ, StaticDefine.MSG_OPTION_NULL));
                System.Threading.Thread.Sleep(300);

                //Result = SendSocketData(ClientNumber, new DataPacket(StaticDefine.MSG_CATEGORY_TEST, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_ITEM_COUNT, StaticDefine.MSG_OPTION_NULL));
                Result = SocketDic[ClientNumber].SocketSendRecv(new DataPacket(StaticDefine.MSG_CATEGORY_TEST, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_ITEM_COUNT, StaticDefine.MSG_OPTION_NULL));
                System.Threading.Thread.Sleep(1200);

                for (int num = 0; num < share.BoardInfoDic[ClientNumber].TestItemCount; num++)
                {
                    //Result = SendSocketData(ClientNumber, new DataPacket(StaticDefine.MSG_CATEGORY_TEST, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_ITEM_NAME, num));
                    Result = SocketDic[ClientNumber].SocketSendRecv(new DataPacket(StaticDefine.MSG_CATEGORY_TEST, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_ITEM_NAME, num));
                    System.Threading.Thread.Sleep(20);
                }

                //Result = SendSocketData(ClientNumber, new DataPacket(StaticDefine.MSG_CATEGORY_TEST, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_STATUS, StaticDefine.MSG_OPTION_NULL));

                if(Result == StaticDefine.NET_SUCCESS)
                    share.BoardInfoDic[ClientNumber].EventMessage = "READY COMPLETE";
                else
                    share.BoardInfoDic[ClientNumber].EventMessage = "READY FAIL";
            
            }
            catch(Exception ex)
            {

            }
        }

        private void SerialConfigSetting_CommandSendHandler(int Command, string Port)
        {
            if(!Port.Contains("COM"))
            {
                SerialConfigFun.ResultStringView("Plz, Select COM Port");
                return;
            }

            switch (Command)
            {
                case StaticDefine.COMMAND_SERIAL_OPEN:
                    serial = new SerialNet(Port);

                    if ((serial != null) && (serial.SerialHandle.IsOpen))
                        SerialConfigFun.ResultStringView(string.Format("Open, {0}", Port));
                    else
                        SerialConfigFun.ResultStringView(string.Format("Open Fail, {0}", Port));
                    break;
                case StaticDefine.COMMAND_SERIAL_CLOSE:
                    if ((serial != null) && (serial.SerialHandle.IsOpen))
                    {
                        serial.SerialClose();
                        SerialConfigFun.ResultStringView(string.Format("Closed, {0}", Port));
                    }
                    else
                        SerialConfigFun.ResultStringView(string.Format("Not yet Open, {0}", Port));
                    break;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Create KVM ShortCut-Key

            Button TempBtn = new Button();

            if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.D1)
            {
                TempBtn.Name = "KvmBtn1";
                KvmFun.KvmBtn_Click(TempBtn, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.D2)
            {
                TempBtn.Name = "KvmBtn2";
                KvmFun.KvmBtn_Click(TempBtn, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.D3)
            {
                TempBtn.Name = "KvmBtn3";
                KvmFun.KvmBtn_Click(TempBtn, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.D4)
            {
                TempBtn.Name = "KvmBtn4";
                KvmFun.KvmBtn_Click(TempBtn, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.D5)
            {
                TempBtn.Name = "KvmBtn5";
                KvmFun.KvmBtn_Click(TempBtn, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.D6)
            {
                TempBtn.Name = "KvmBtn6";
                KvmFun.KvmBtn_Click(TempBtn, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.D7)
            {
                TempBtn.Name = "KvmBtn7";
                KvmFun.KvmBtn_Click(TempBtn, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.D8)
            {
                TempBtn.Name = "KvmBtn8";
                KvmFun.KvmBtn_Click(TempBtn, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.D9)
            {
                TempBtn.Name = "KvmBtn9";
                KvmFun.KvmBtn_Click(TempBtn, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.D0)
            {
                TempBtn.Name = "KvmBtn0";
                KvmFun.KvmBtn_Click(TempBtn, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.H)
            {
                TempBtn.Name = "KvmBtnH";
                KvmFun.KvmBtn_Click(TempBtn, null);
            }

            TempBtn = null;
        }


        //private void MainTable_Loaded(object sender, RoutedEventArgs e)
        //{
        //    MainTable.RowHeight = MainTable.ActualHeight / MainTable.Items.Count;
        //}

        #endregion


        #region 0. Send Command

        public void Serial_CommandSendEventHandler(int Cmd, byte Option)
        {
            IsDetect = false;

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                switch (Cmd)
                {
                    case StaticDefine.COMMAND_POWER_ON:
                    case StaticDefine.COMMAND_POWER_OFF:
                        PowerCommandSend(Option);
                        break;
                    case StaticDefine.COMMAND_KVM_SELECT:
                        if(Option != 0)
                            KVMCommandSend(Option);
                        break;
                }
            }));

            IsDetect = true;
        }

        private void PowerCommandSend(byte Cmd)
        {
            bool CheckSelectBoard = false;

            BitArray bitArray = new BitArray(16);

            foreach (KeyValuePair<int, BoardInfo> el in share.BoardInfoDic)
            {
                if (el.Value.IsSelected)
                {
                    CheckSelectBoard = true;

                    if (el.Value.PowerState == true && Cmd == StaticDefine.SERIAL_CMD_PW_ON)
                    {
                        el.Value.EventMessage = "Already PW ON";
                        continue;
                    }
                    else if (el.Value.PowerState == false && Cmd == StaticDefine.SERIAL_CMD_PW_OFF)
                    {
                        el.Value.EventMessage = "Already PW OFF";
                        continue;
                    }
                    
                    bitArray.Set(el.Value.BoardNum - 1, true);
                }
            }

            if (!CheckSelectBoard)
            {
                MessageBox.Show("Please Select the Board", null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                share.EventLog(null, "[SHOW]\t", "MessageBox > Please Select the Board");
                return;
            }
            
            try
            {
                if (serial != null)
                {
                    // Convert From BitArray to Byte
                    byte[] LSBnMSB = new byte[2];
                    bitArray.CopyTo(LSBnMSB, 0);

                    // Assign a value to a Byte 
                    byte Command = Cmd;
                    byte LSB = LSBnMSB[0];
                    byte MSB = LSBnMSB[1];
                    byte CheckSum = (byte)(Command + LSB + MSB);
                    byte EndCommand = StaticDefine.SERIAL_RET_CMD_END;

                    // Assign value to SendByte (Size 5)
                    byte[] SendByteData = new byte[5];
                    SendByteData[0] = Command;
                    SendByteData[1] = MSB;
                    SendByteData[2] = LSB;
                    SendByteData[3] = CheckSum;
                    SendByteData[4] = EndCommand;
                
                    // Send
                    serial.SerialSendRecv(SendByteData, out byte[] ReadByteData, false);

                    foreach (KeyValuePair<int, BoardInfo> el in share.BoardInfoDic)
                    {
                        if (bitArray.Get(el.Value.BoardNum - 1))
                        {
                            if(Cmd == StaticDefine.SERIAL_CMD_PW_ON)
                                el.Value.EventMessage = "PW ON SUCCESS";
                            else if(Cmd == StaticDefine.SERIAL_CMD_PW_OFF)
                                el.Value.EventMessage = "PW OFF SUCCESS";
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<int, BoardInfo> el in share.BoardInfoDic)
                    {
                        if (bitArray.Get(el.Value.BoardNum - 1))
                        {
                            if (Cmd == StaticDefine.SERIAL_CMD_PW_ON)
                                el.Value.EventMessage = "PW ON FAIL";
                            else if (Cmd == StaticDefine.SERIAL_CMD_PW_OFF)
                                el.Value.EventMessage = "PW OFF FAIL";
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                foreach (KeyValuePair<int, BoardInfo> el in share.BoardInfoDic)
                {
                    if (bitArray.Get(el.Value.BoardNum - 1))
                    {
                        if (Cmd == StaticDefine.SERIAL_CMD_PW_ON)
                            el.Value.EventMessage = "PW ON FAIL";
                        else if (Cmd == StaticDefine.SERIAL_CMD_PW_OFF)
                            el.Value.EventMessage = "PW OFF FAIL";
                    }
                }
            }
        }

        private void KVMCommandSend(byte SelectBoard)
        {
            if (serial != null)
            { 
                // Assign a value to a Byte
                byte Command = StaticDefine.SERIAL_CMD_BD_SEL;
                byte BoardNum = SelectBoard;
                byte CheckSum = (byte)(Command + BoardNum);
                byte EndCommand = StaticDefine.SERIAL_RET_CMD_END;

                // Assign value to SendByte (Size 4)
                byte[] SendByteData = new byte[4];
                SendByteData[0] = Command;
                SendByteData[1] = BoardNum;
                SendByteData[2] = CheckSum;
                SendByteData[3] = EndCommand;

                // Send
                serial.SerialSendRecv(SendByteData, out byte[] ReadByteData, false);
            }
        }

        private void Socket_CommandSendEventHandler(int Cmd, int Option)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                switch (Cmd)
                {
                    case StaticDefine.COMMAND_VOLTAGE_SET:  // TCP Socket
                        VoltageCommandSend(StaticDefine.MSG_ACTION_SET, Option);
                        break;
                    case StaticDefine.COMMAND_VOLTAGE_GET:  // TCP Socket
                        VoltageCommandSend(StaticDefine.MSG_ACTION_GET, Option);
                        break;
                    case StaticDefine.COMMAND_TEST_RUN: // TCP Socket
                    case StaticDefine.COMMAND_TEST_PAUSE:
                    case StaticDefine.COMMAND_TEST_STOP:
                        TestCommandSend(Option);
                        break;
                }
            }));
        }


        //private void SystemCommandSend()
        //{
        //    int Result;
        //
        //    foreach (ClientNet el in SocketList)
        //    {
        //        if ((el.ClientSocket != null) && (el.ClientSocket.Connected))
        //        {
        //            // GET CPU Information
        //            DataPacket CpuDataRequest = new DataPacket(StaticDefine.MSG_CATEGORY_SYSTEM, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_CPU, StaticDefine.MSG_OPTION_NULL);
        //            Result = el.SendSocketMsg(CpuDataRequest);
        //            //Result = SendSocketData(el.ClientNum, CpuDataRequest);
        //
        //            if (Result != StaticDefine.NET_SUCCESS)
        //            {
        //                // Enter Log
        //            }
        //
        //            System.Threading.Thread.Sleep(StaticDefine.PacketSendDelay);
        //
        //            // GET Memory Information
        //            DataPacket MemoryDataRequest = new DataPacket(StaticDefine.MSG_CATEGORY_SYSTEM, StaticDefine.MSG_ACTION_GET, StaticDefine.MSG_CONTENT_MEMORY, StaticDefine.MSG_OPTION_NULL);
        //            Result = el.SendSocketMsg(MemoryDataRequest);
        //            //Result = SendSocketData(el.ClientNum, MemoryDataRequest);
        //
        //            if (Result != StaticDefine.NET_SUCCESS)
        //            {
        //                // Enter Log
        //            }
        //
        //        }
        //    }
        //}


        private void VoltageCommandSend(int action, int volt)  //int channel,
        {
            bool CheckSelectBoard = false;

            foreach (KeyValuePair<int, BoardInfo> el in share.BoardInfoDic)
            {
                if (el.Value.IsSelected)
                {
                    CheckSelectBoard = true;

                    DataPacket SendData = new DataPacket(StaticDefine.MSG_CATEGORY_VOLTAGE, action, StaticDefine.MSG_CONTENT_CPU0_VDDQ, volt);
                    //DataPacket SendData = new DataPacket(StaticDefine.MSG_CATEGORY_VOLTAGE, action, channel, volt);

                    //int Result = SendSocketData(el.Value.BoardNum, SendData);
                    int Result = SocketDic[el.Value.BoardNum].SocketSendRecv(SendData);
                    
                    if (Result != StaticDefine.NET_SUCCESS)
                        share.EventLog(el.Value.BoardNum, "[SEND]\t", "Failed to send Voltage Command.");
                }
            }

            if (!CheckSelectBoard)
            {
                MessageBox.Show("Please Select the Board", null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                share.EventLog(null, "[SHOW]\t", "MessageBox > Please Select the Board");
                return;
            }
        }
        
        private void TestCommandSend(int StatusOption)
        {
            DateTime NowTime = DateTime.Now;

            bool CheckSelectBoard = false;

            foreach (KeyValuePair<int, BoardInfo> el in share.BoardInfoDic)
            {
                if (el.Value.IsSelected)
                {
                    CheckSelectBoard = true;
                    
                    switch(StatusOption)
                    {
                        case StaticDefine.BOARD_STATUS_RUN:
                            {
                                // Pass by Testing Slot
                                //if (StaticDefine.TEST_STATE_TESTSTATE_START < el.Value.TestState && el.Value.TestState < StaticDefine.TEST_STATE_TESTSTATE_END)
                                if (el.Value.TestState == StaticDefine.TEST_STATE_TEST_ING)
                                {
                                    el.Value.EventMessage = "Already Running";
                                    continue;
                                }
                                else if (el.Value.TestState == StaticDefine.TEST_STATE_TEST_PAUSE)
                                    break;

                                TestInitProc(el.Value);

                                // Make Result File 
                                //string ResultFilePath = StaticDefine.PATH_RESULT + string.Format("\\{0:D2}\\", el.Value.BoardNum);
                                //
                                //if (!Directory.Exists(ResultFilePath))
                                //    Directory.CreateDirectory(ResultFilePath);
                                //
                                //el.Value.ResultFileName = string.Format(el.Value.TestID + NowTime.ToString("_yyyyMMdd_HHmm") + ".txt");
                            }
                            break;
                        case StaticDefine.BOARD_STATUS_FINISH:
                            {
                                if (!(StaticDefine.TEST_STATE_TESTSTATE_START < el.Value.TestState && el.Value.TestState < StaticDefine.TEST_STATE_TESTSTATE_END))
                                {
                                    el.Value.EventMessage = "Not Under Test";
                                    continue;
                                }
                            }
                            break;
                        case StaticDefine.BOARD_STATUS_PAUSE:
                            {
                                if(el.Value.TestState != StaticDefine.TEST_STATE_TEST_ING)
                                {
                                    if (el.Value.TestState == StaticDefine.TEST_STATE_TEST_PAUSE)
                                        el.Value.EventMessage = "Already Paused";
                                    else
                                        el.Value.EventMessage = "Not Under Test";
                                    
                                    continue;
                                }
                            }
                            break;
                    }

                    System.Threading.Thread.Sleep(150);

                    DataPacket SendData = new DataPacket(StaticDefine.MSG_CATEGORY_TEST, StaticDefine.MSG_ACTION_SET, StaticDefine.MSG_CONTENT_STATUS, StatusOption);
                    //int Result = SendSocketData(el.Value.BoardNum, SendData);
                    int Result = SocketDic[el.Value.BoardNum].SocketSendRecv(SendData);

                    if (Result != StaticDefine.NET_SUCCESS)
                        share.EventLog(el.Value.BoardNum, "[SEND]\t", "Failed to send Test Command.");
                }
            }

            if (!CheckSelectBoard)
            {
                MessageBox.Show("Please Select the Board", null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                share.EventLog(null, "[SHOW]\t", "MessageBox > Please Select the Board");
                return;
            }
        }

        private int SendSocketData(int BoardNum, DataPacket SendMsg)
        {
            int Result = StaticDefine.NET_FAIL;

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                try
                {
                    foreach (KeyValuePair<int, ClientNet> el in SocketDic)
                    {
                        if (el.Value.ClientNum != BoardNum)
                            continue;

                        Result = el.Value.SocketSendRecv(SendMsg);
                    }
                }
                catch (Exception ex)
                {
                    Result = StaticDefine.NET_FAIL;

                    share.EventLog(null, string.Empty, string.Format("\t{0} > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);
                }
            }));

            //System.Threading.Thread.Sleep(20);

            return Result;
        }


        #endregion


        #region 0. Receive Command

        private void ReceiveSocketData(int ClientNum, DataPacket RecvCmd)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                switch (RecvCmd.MSGCategory)
                {
                    case StaticDefine.MSG_CATEGORY_SYSTEM:
                        SystemCommandRecv(ClientNum, RecvCmd);
                        break;
                    case StaticDefine.MSG_CATEGORY_VOLTAGE:
                        VoltageCommandRecv(ClientNum, RecvCmd);
                        break;
                    case StaticDefine.MSG_CATEGORY_TEST:
                        TestCommandRecv(ClientNum, RecvCmd);
                        break;
                }
            }));
        }

        private void SystemCommandRecv(int ClientNum, DataPacket RecvPacket)
        {
            int BoardNum = ClientNum;

            try
            {
                switch (RecvPacket.MSGContent)
                {
                    // CPU
                    case StaticDefine.MSG_CONTENT_CPU:
                        {
                            if (RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                            {
                                share.EventLog(ClientNum, "[RECV]\t", "Failed to get CPU information.");
                                break;
                            }

                            string CpuInfo = string.Empty;
                            string[] RecvData = RecvPacket.MSGData.Split(' ');

                            for (int i = 2; i < RecvData.Length; i++)
                            {
                                CpuInfo += (RecvData[i] + ' ');
                            }

                            share.BoardInfoDic[BoardNum].Cpu = CpuInfo.Trim();
                        }
                        break;
                    // MEMORY
                    case StaticDefine.MSG_CONTENT_MEMORY:
                        {
                            if (RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                            {
                                share.EventLog(ClientNum, "[RECV]\t", "Failed to get MEMORY information.");
                                break;
                            }

                            string MemoryInfo = string.Empty;
                            string[] RecvData = RecvPacket.MSGData.Split(' ');
                            
                            for(int i = 2; i < RecvData.Length; i++)
                            {
                                MemoryInfo += (RecvData[i] + ' ');
                            }

                            share.BoardInfoDic[BoardNum].Memory = MemoryInfo.Trim();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                share.EventLog(BoardNum, string.Empty, string.Format("\t{0} > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);
            }
        }

        private void VoltageCommandRecv(int ClientNum, DataPacket RecvPacket)
        {
            int BoardNum = ClientNum;
            BoardInfo RecvBoardInfo = share.BoardInfoDic[BoardNum];

            try
            {
                if (RecvPacket.MSGAction == StaticDefine.MSG_ACTION_SET)
                {
                    if (RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                    {
                        RecvBoardInfo.EventMessage = "VOLT SET FAIL";
                        share.EventLog(ClientNum, "[RECV]\t", "Failed to set the Voltage.");
                        return;
                    }

                    int ReadVolt = Convert.ToInt32(RecvPacket.MSGData);
                    RecvBoardInfo.VddQ = ReadVolt;

                    RecvBoardInfo.EventMessage = "VOLT SET SUCCESS";
                    share.EventLog(ClientNum, "[RECV]\t", "Successfully set the Voltage.");
                }
                else if (RecvPacket.MSGAction == StaticDefine.MSG_ACTION_GET)
                {
                    if (RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                    {
                        RecvBoardInfo.EventMessage = "VOLT GET FAIL";
                        share.EventLog(ClientNum, "[RECV]\t", "Failed to get the Voltage.");
                        return;
                    }

                    
                    int GetVolt = Convert.ToInt32(RecvPacket.MSGData);
                    RecvBoardInfo.VddQ = GetVolt;
                    
                    RecvBoardInfo.EventMessage = "VOLT GET SUCCESS";
                    share.EventLog(ClientNum, "[RECV]\t", "Successfully get the Voltage.");
                }
            }
            catch (Exception ex)
            {
                share.EventLog(BoardNum, string.Empty, string.Format("\t{0} > {1}", MethodBase.GetCurrentMethod().Name, ex.Message), false);
            }
        }
        
        private void TestCommandRecv(int ClientNum, DataPacket RecvPacket)
        {
            int BoardNum = ClientNum;

            if (!share.BoardInfoDic.ContainsKey(BoardNum))
                return;
            
            BoardInfo RecvBoardInfo = share.BoardInfoDic[BoardNum];
            
            try
            {
                if (RecvPacket.MSGAction == StaticDefine.MSG_ACTION_SET)
                {
                    //if (RecvPacket.MSGContent == StaticDefine.MSG_CONTENT_STATUS)
                    {
                        switch (Convert.ToInt32(RecvPacket.MSGData))
                        {
                            case StaticDefine.BOARD_STATUS_RUN_READY:
                                //break;
                            case StaticDefine.BOARD_STATUS_RUN:
                                {
                                    if (RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                                    {
                                        RecvBoardInfo.EventMessage = "RUN FAIL";
                                        RecvBoardInfo.TestState = StaticDefine.TEST_STATE_NULL;

                                        share.EventLog(BoardNum, "[RECV]\t", "Failed to run the Test.");
                                        break;
                                    }

                                    RecvBoardInfo.TestString = "RUNNING";
                                    RecvBoardInfo.TestState = StaticDefine.TEST_STATE_TEST_ING;
                                    RecvBoardInfo.EventMessage = "RUN SUCCESS";
                                    //RecvBoardInfo.IsTesting = true;
                                    share.EventLog(BoardNum, "[RECV]\t", "Successfully run the Test.");
                                }
                                break;
                            case StaticDefine.BOARD_STATUS_FINISH_READY:
                            case StaticDefine.BOARD_STATUS_FINISH:
                                {
                                    if (RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                                    {
                                        RecvBoardInfo.EventMessage = "STOP FAIL";

                                        share.EventLog(BoardNum, "[RECV]\t", "Failed to stop the Test.");
                                        break;
                                    }

                                    //RecvBoardInfo.IsTesting = false;
                                    RecvBoardInfo.TestString = "STOP";
                                    RecvBoardInfo.TestState = StaticDefine.TEST_STATE_FAIL;

                                    if (RecvBoardInfo.TestStep != -1)
                                        RecvBoardInfo.TestItemList[RecvBoardInfo.TestStep].ItemState = "FAIL";

                                    RecvBoardInfo.EventMessage = "STOP SUCCESS";

                                    share.EventLog(BoardNum, "[RECV]\t", "Successfully stop the Test.");
                                }
                                break;
                            case StaticDefine.BOARD_STATUS_PAUSE:
                                {
                                    if (RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                                    {
                                        RecvBoardInfo.EventMessage = "PAUSE FAIL";

                                        share.EventLog(BoardNum, "[RECV]\t", "Failed to pause the Test.");
                                        break;
                                    }

                                    RecvBoardInfo.TestString = "PAUSE";
                                    RecvBoardInfo.TestState = StaticDefine.TEST_STATE_TEST_PAUSE;

                                    if (RecvBoardInfo.TestStep != -1)
                                        RecvBoardInfo.TestItemList[RecvBoardInfo.TestStep].ItemState = "PAUSE";

                                    RecvBoardInfo.EventMessage = "PAUSE SUCCESS";
                                    
                                    share.EventLog(BoardNum, "[RECV]\t", "Successfully pause the Test.");
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                else if (RecvPacket.MSGAction == StaticDefine.MSG_ACTION_GET)
                {
                    // ITEM COUNT
                    if (RecvPacket.MSGContent == StaticDefine.MSG_CONTENT_ITEM_COUNT)
                    {
                        if(RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                        {
                            share.EventLog(BoardNum, "[RECV]\t", "Failed to get the number of items.");
                            return;
                        }

                        RecvBoardInfo.TestItemCount = Convert.ToInt32(RecvPacket.MSGData);
                     
                        share.EventLog(BoardNum, "[RECV]\t", "Successfully get the number of items.");
                    }
                    // ITEM NAME
                    else if (RecvPacket.MSGContent == StaticDefine.MSG_CONTENT_ITEM_NAME)
                    {
                        if (RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                        {
                            share.EventLog(BoardNum, "[RECV]\t", "Failed to get the name of each item.");
                            return;
                        }

                        string[] RecvData = RecvPacket.MSGData.Split(',');

                        TestItem TempItem = new TestItem();
                        TempItem.ItemNumber = Convert.ToInt32(RecvData[0]);
                        TempItem.ItemName = RecvData[1];
                        TempItem.ItemProgress = 0;
                        TempItem.ItemState = "WAIT";

                        RecvBoardInfo.TestItemList.Add(TempItem);

                        share.EventLog(BoardNum, "[RECV]\t", "Successfully get the name of each item.");
                    }
                    // TEST STATUS
                    else if (RecvPacket.MSGContent == StaticDefine.MSG_CONTENT_STATUS)
                    {
                        if (RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                        {
                            share.EventLog(BoardNum, "[RECV]\t", "Failed to get board status.");
                            return;
                        }

                        int ConvertIntData = Convert.ToInt32(RecvPacket.MSGData);

                        switch(ConvertIntData)
                        {
                            case StaticDefine.BOARD_STATUS_RUN_READY:
                                break;
                            case StaticDefine.BOARD_STATUS_RUN:
                                {
                                    //share.BoardInfoDic[BoardNum].IsTesting = true;
                                    if (RecvBoardInfo.TestState < StaticDefine.TEST_STATE_TESTSTATE_START)
                                        TestInitProc(RecvBoardInfo);

                                    RecvBoardInfo.TestString = "RUNNING";
                                    RecvBoardInfo.TestState = StaticDefine.TEST_STATE_TEST_ING;
                                }
                                break;
                            case StaticDefine.BOARD_STATUS_FINISH_READY:
                                break;
                            case StaticDefine.BOARD_STATUS_FINISH:
                                {
                                    // Initial
                                    if (RecvBoardInfo.TestStep == -1)
                                    {
                                        //RecvBoardInfo.EventMessage = "IDLE";
                                        // LOG
                                        //break;
                                    }
                                    // TEST END
                                    else if ((RecvBoardInfo.TestItemList.Count - 1) == RecvBoardInfo.TestStep)
                                    {
                                        ItemFinishProc(RecvBoardInfo);

                                        RecvBoardInfo.TestTotalProgress = CalculatorTotalProgress(RecvBoardInfo);

                                        TestFinishProc(RecvBoardInfo);
                                    }
                                    // TEST STOP (Manual STOP. In the Target Board.)
                                    else if ((RecvBoardInfo.TestItemList.Count - 1) > RecvBoardInfo.TestStep)
                                    {
                                        RecvBoardInfo.TestString = "STOP";
                                        RecvBoardInfo.TestState = StaticDefine.TEST_STATE_FAIL;

                                        if (RecvBoardInfo.TestStep != -1)
                                            RecvBoardInfo.TestItemList[RecvBoardInfo.TestStep].ItemState = "FAIL";
                                    }
                                }
                                break;
                            case StaticDefine.BOARD_STATUS_PAUSE:
                                RecvBoardInfo.TestString = "PAUSE";
                                RecvBoardInfo.TestState = StaticDefine.TEST_STATE_TEST_PAUSE;
                                //RecvBoardInfo.IsTesting = true;

                                if (RecvBoardInfo.TestStep != -1)
                                    RecvBoardInfo.TestItemList[RecvBoardInfo.TestStep].ItemState = "PAUSE";
                                break;
                            default:
                                break;
                        }
                    }
                    // PROGRESS
                    else if (RecvPacket.MSGContent == StaticDefine.MSG_CONTENT_PROGRESS)
                    {
                         if (RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                         {
                            share.EventLog(BoardNum, "[RECV]\t", "Failed to get test progress.");
                            return;
                         }

                        string[] RecvData = RecvPacket.MSGData.Split(',');
                         
                         int CurrentItemIndex = Convert.ToInt32(RecvData[0]);
                         int CurrentProgress = Convert.ToInt32(RecvData[1]);

                        if(RecvBoardInfo.TestStep == -1)
                        {
                            foreach(TestItem item in RecvBoardInfo.TestItemList)
                            {
                                if (item.ItemNumber > CurrentItemIndex)
                                    continue;
                                else if(item.ItemNumber == CurrentItemIndex)
                                {
                                    item.ItemProgress = CurrentItemIndex;
                                    item.ItemState = "TEST";


                                    RecvBoardInfo.TestStep = CurrentItemIndex;
                                    //RecvBoardInfo.TestItemList[RecvBoardInfo.TestStep].ItemProgress = CurrentProgress;
                                    RecvBoardInfo.TestTotalProgress = CalculatorTotalProgress(RecvBoardInfo);
                                }
                                else
                                {
                                    //ItemFinishProc(RecvBoardInfo);

                                    item.ItemProgress = 100;
                                    
                                    if (RecvBoardInfo.ErrorList.Any(x => x.ErrorIndex == item.ItemNumber))
                                        item.ItemState = "FAIL";
                                    else
                                        item.ItemState = "PASS";
                                }
                            }
                        }
                        //else if(share.BoardInfoDic[BoardNum].IsTesting)
                        else if ((RecvBoardInfo.TestState > StaticDefine.TEST_STATE_TESTSTATE_START) && (RecvBoardInfo.TestState < StaticDefine.TEST_STATE_TESTSTATE_END))
                        {
                            if (RecvBoardInfo.TestStep != -1)
                            {
                                if (RecvBoardInfo.TestStep != CurrentItemIndex)
                                {
                                    // Error 가 아닐 때
                                    ItemFinishProc(RecvBoardInfo);
                                }
                            }

                            RecvBoardInfo.TestStep = CurrentItemIndex;
                            RecvBoardInfo.TestItemList[RecvBoardInfo.TestStep].ItemProgress = CurrentProgress;
                            RecvBoardInfo.TestTotalProgress = CalculatorTotalProgress(RecvBoardInfo);

                            if (RecvBoardInfo.TestState == StaticDefine.TEST_STATE_TEST_ING)
                            {
                                if (RecvBoardInfo.TestItemList[RecvBoardInfo.TestStep].ItemState != "FAIL" && RecvBoardInfo.TestItemList[RecvBoardInfo.TestStep].ItemState != "PASS")
                                    RecvBoardInfo.TestItemList[RecvBoardInfo.TestStep].ItemState = "TEST";
                            }
                        }
                    }
                    // ERROR COUNT
                    else if (RecvPacket.MSGContent == StaticDefine.MSG_CONTENT_ERROR_COUNT)
                    {
                        if (RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                        {
                            share.EventLog(BoardNum, "[RECV]\t", "Failed to get test error count.");
                            return;
                        }

                        //int OccurErrorCount = 0;
                        int AfterTotalErrCnt = Convert.ToInt32(RecvPacket.MSGData);
                        int BeforTotalErrCnt = RecvBoardInfo.TestTotalErrorCount;

                        int OccurErrorCount = AfterTotalErrCnt - BeforTotalErrCnt;

                        RecvBoardInfo.TestTotalErrorCount = AfterTotalErrCnt;
                        RecvBoardInfo.TestOccurErrorCount = OccurErrorCount;

                        if(OccurErrorCount != 0)
                            share.EventLog(BoardNum, "[RECV]\t", string.Format("Successfully get test error count : {0}", OccurErrorCount));
                    }
                    // ERROR DATA
                    else if (RecvPacket.MSGContent == StaticDefine.MSG_CONTENT_ERROR_DATA)
                    {
                        if (RecvPacket.MSGData == StaticDefine.MSG_FAIL)
                        {
                            share.EventLog(BoardNum, "[RECV]\t", "Failed to get error data.");
                            return;
                        }

                        string[] RecvData = RecvPacket.MSGData.Split(',');

                        int ErrorItemIndex = Convert.ToInt32(RecvData[0]);
                        string ErrorAddr = RecvData[1];

                        ErrorItem errorItem = new ErrorItem();
                        errorItem.ErrorCount = RecvBoardInfo.ErrorList.Count + 1;
                        errorItem.ErrorIndex = ErrorItemIndex;
                        errorItem.ErrorAddress = ErrorAddr;

                        RecvBoardInfo.ErrorList.Add(errorItem);

                        share.EventLog(BoardNum, "[RECV]\t", string.Format("Successfully get error data : {0}, {1}", errorItem.ErrorCount, ErrorAddr));
                    }
                }
            }
            catch(Exception ex)
            {
                // {"입력 문자열의 형식이 잘못되었습니다."}
            }
        }

        private void TestInitProc(BoardInfo TempBoardInfo)
        {
            TempBoardInfo.TestStep = -1;
            TempBoardInfo.TestString = string.Empty;
            TempBoardInfo.TestTotalProgress = "0";
            TempBoardInfo.TestTotalErrorCount = 0;
            TempBoardInfo.TestOccurErrorCount = 0;
            TempBoardInfo.ErrorList.Clear();

            foreach (TestItem item in TempBoardInfo.TestItemList)
            {
                item.ItemProgress = 0;
                item.ItemState = "WAIT";
            }
        }

        private string CalculatorTotalProgress(BoardInfo boardInfo)
        {
            ObservableCollection<TestItem> testItemList = boardInfo.TestItemList;
            double TotalProgress = 0;

            foreach (TestItem el in testItemList)
            {
                double CalcProgress = ((double)el.ItemProgress / 100) / testItemList.Count;

                TotalProgress += CalcProgress;
            }

            string PercentProgress = (TotalProgress * 100).ToString("F2");

            return PercentProgress;
        }

        public void ItemFinishProc(BoardInfo TempBoardInfo)
        {
            TempBoardInfo.TestItemList[TempBoardInfo.TestStep].ItemProgress = 100;

            if (TempBoardInfo.ErrorList.Any(x => x.ErrorIndex == TempBoardInfo.TestStep))
                TempBoardInfo.TestItemList[TempBoardInfo.TestStep].ItemState = "FAIL";
            else
                TempBoardInfo.TestItemList[TempBoardInfo.TestStep].ItemState = "PASS";
        }

        private void TestFinishProc(BoardInfo TempBoardInfo)
        {
            if (TempBoardInfo.TestItemList.Any(x => x.ItemState == "FAIL"))
            {
                TempBoardInfo.TestString = "FAIL";
                TempBoardInfo.TestState = StaticDefine.TEST_STATE_FAIL;

                //share.EventLog(TempBoardInfo.BoardNum, "[RECV]\t", "Test Ended. (Fail)");
            }
            else
            {
                TempBoardInfo.TestString = "PASS";
                TempBoardInfo.TestState = StaticDefine.TEST_STATE_PASS;

                //share.EventLog(TempBoardInfo.BoardNum, "[RECV]\t", "Test Ended. (Pass)");
            }

            // TEST END
        }

        #endregion
        

        #region 99. Closing

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            share.EventLog(null, "[Click]\t", "Application Close");
            share.shutdown = true;

            try
            {
                foreach (KeyValuePair<int, ClientNet> el in SocketDic)
                {
                    //el.Value.StopThread();

                    if (el.Value.SocketThread != null)
                    {
                        el.Value.SocketThread.Abort();
                        el.Value.SocketThread = null;
                    }

                    el.Value.RemoveSocket(el.Value.ClientNum);
                }

                if (PowerDetectThread != null)
                {
                    PowerDetectThread.Abort();
                    PowerDetectThread = null;
                }

                if (serial != null)
                {
                    serial.SerialClose();
                    serial = null;
                }
                
                if (PowerFun != null)
                    PowerFun = null;
                
                if (VoltFun != null)
                    VoltFun = null;
                
                if (TestFun != null)
                    TestFun = null;
                
                if (KvmFun != null)
                    KvmFun = null;
                
                if (SerialConfigFun != null)
                    SerialConfigFun = null;
                
                share.EventLog(null, "[Closed]\t", "Application");
            }
            catch(Exception ex)
            {

            }
        }

        #endregion


    }


    #region 000. ETC

    public class PowerStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
                return null;

            bool PowerState = bool.Parse(value.ToString());
            string strPowerState;

            if (PowerState)
                strPowerState = "ON";
            else
                strPowerState = "OFF";

            return strPowerState;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConnectStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
                return null;

            bool ConnectState = bool.Parse(value.ToString());
            string strConnectState;

            if (ConnectState)
                strConnectState = "ONLINE";
            else
                strConnectState = "OFFLINE";

            return strConnectState;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    #endregion


}
