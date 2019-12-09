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
using DefineLib;
using ShareLib;

namespace FunctionUC
{
    /// <summary>
    /// TestUC.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TestUC : UserControl
    {
        Share share = null;

        public event DelCommandSend DelCommandSendEventHandler = null;

        Dictionary<CheckBox, string> ItemDic = new Dictionary<CheckBox, string>();

        //public string TestId = string.Empty;

        public TestUC()
        {
            InitializeComponent();

            share = Share.Initialize;

            //TestItemListSetting();
        }

        //private void TestItemListSetting()
        //{
        //    string TempStr1 = "1111";
        //    string TempStr2 = "22";
        //    string TempStr3 = "333";
        //    string TempStr4 = "4444";
        //
        //    TestLB1.Content = "TEST #1 : " + TempStr1;
        //    TestLB2.Content = "TEST #2 : " + TempStr2;
        //    TestLB3.Content = "TEST #3 : " + TempStr3;
        //    TestLB4.Content = "TEST #4 : " + TempStr4;
        //
        //    ItemDic.Add(Test1CB, TempStr1);
        //    ItemDic.Add(Test2CB, TempStr2);
        //    ItemDic.Add(Test3CB, TempStr3);
        //    ItemDic.Add(Test4CB, TempStr4);
        //}
        //
        //public bool SelectItemSetting(BoardInfo boardInfo)
        //{
        //    //string SelectItem = string.Empty;  // ?? 협의 필요
        //
        //    boardInfo.TestItemList.Clear();
        //
        //    int itemcount = 0;
        //    bool ItemSelectCheck = false;
        //
        //    foreach (KeyValuePair<CheckBox, string> CB in ItemDic)
        //    {
        //        if (CB.Key.IsChecked.Value)
        //        {
        //            itemcount++;
        //
        //            TestItem TempTest = new TestItem();
        //            TempTest.ItemNumber = itemcount;
        //            TempTest.ItemName = CB.Value;
        //            TempTest.ItemState = "WAIT";
        //
        //            boardInfo.TestItemList.Add(TempTest);
        //            boardInfo.TestID = TestId;
        //
        //            //SelectItem += itemcount.ToString() + " ";
        //
        //            ItemSelectCheck = true; 
        //        }
        //    }
        //
        //    //if (SelectItem == string.Empty)
        //    //    return false; 
        //
        //    return ItemSelectCheck;
        //}

        private void TestRunBtn_Click(object sender, RoutedEventArgs e)
        {
            //string SelectItem = string.Empty;  // ?? 협의 필요

            //string TestId = TestIDTB.Text;

            // Test Setting
            //if (TestId == string.Empty)
            //{
            //    MessageBox.Show("Please Enter the Test ID", null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    share.EventLog(null, "[Show]\t", "MessageBox > Please Enter the Test ID");
            //    return;
            //}

            //foreach (KeyValuePair<int, BoardInfo> TempBD in share.BoardInfoDic)
            //{
            //    //// Pass by Non-Select Slot
            //    //if (!TempBD.Value.IsSelected)
            //    //    continue;
            //    //
            //    //// Pass by Disconnect Slot
            //    //if (!TempBD.Value.PowerState)
            //    //    continue;
            //    
            //    // Pass by Testing Slot
            //    if (StaticDefine.TEST_STATE_TESTSTATE_START < TempBD.Value.State && TempBD.Value.State < StaticDefine.TEST_STATE_TESTSTATE_END)
            //        continue;
            //
            //    TempBD.Value.TestItemList.Clear();
            //
            //    int itemcount = 0;
            //
            //    foreach (KeyValuePair<CheckBox, string> CB in ItemDic)
            //    {
            //        if (CB.Key.IsChecked.Value)
            //        {
            //            itemcount++;
            //
            //            TestItem TempTest = new TestItem();
            //            TempTest.ItemNumber = itemcount;
            //            TempTest.ItemName = CB.Value;
            //            TempTest.ItemState = "WAIT";
            //
            //            TempBD.Value.TestItemList.Add(TempTest);
            //            TempBD.Value.TestID = TestId;
            //
            //            SelectItem += itemcount.ToString() + " ";
            //        }
            //    }
            //
            //    if (SelectItem == string.Empty)
            //    {
            //        MessageBox.Show("Please Select the Test Item", null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //        share.EventLog(null, "[Show]\t", "MessageBox > Please Select the Test Item");
            //        return;  // continue???
            //    }
            //}

            ///////////////////////////////////////////////////////////////////////////////////////

            share.EventLog(null, "[Click]\t", "Test Run");

            if (DelCommandSendEventHandler != null)
                DelCommandSendEventHandler.Invoke(StaticDefine.COMMAND_TEST_RUN, StaticDefine.BOARD_STATUS_RUN);
        }

        private void TestPauseBtn_Click(object sender, RoutedEventArgs e)
        {
            share.EventLog(null, "[Click]\t", "Test Pause");

            if (DelCommandSendEventHandler != null)
                DelCommandSendEventHandler.Invoke(StaticDefine.COMMAND_TEST_PAUSE, StaticDefine.BOARD_STATUS_PAUSE);
        }

        private void TestStopBtn_Click(object sender, RoutedEventArgs e)
        {
            share.EventLog(null, "[Click]\t", "Test Stop");

            if (DelCommandSendEventHandler != null)
                DelCommandSendEventHandler.Invoke(StaticDefine.COMMAND_TEST_STOP, StaticDefine.BOARD_STATUS_FINISH);
        }
    }
}
