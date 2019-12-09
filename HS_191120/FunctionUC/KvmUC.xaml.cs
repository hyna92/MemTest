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
using ShareLib;
using DefineLib;
using System.IO.Ports;

namespace FunctionUC
{
    /// <summary>
    /// KvmUC.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class KvmUC : UserControl
    {
        public Share share = null;

        public event DelSerialCommandSend DelSerialCommandSendEventHandler = null;
        
        int ColumnCount;
        int RowCount;

        public KvmUC()
        {
            InitializeComponent();

            share = Share.Initialize;

            int StandardNum = 5;

            RowCount = share.SettingBoardRowCount / StandardNum;
            ColumnCount = StandardNum;

            KvmGridDivision();

            //SerialPortSearchBtn_Click(null, null);
        }

        private void KvmGridDivision()
        {
            RowDefinition RowTemp = null;
            ColumnDefinition ColTemp = null;

            int BoardNum = 1;

            for (int r = 0; r < RowCount; r++)
            {
                RowTemp = new RowDefinition();
                RowTemp.Height = new GridLength(45, GridUnitType.Pixel);

                KVMGrid.RowDefinitions.Add(RowTemp);
            }

            for (int c = 0; c < ColumnCount; c++)
            {
                ColTemp = new ColumnDefinition();
                ColTemp.Width = new GridLength(1, GridUnitType.Star);

                KVMGrid.ColumnDefinitions.Add(ColTemp);
            }

            for (int r = 0; r < RowCount; r++)
            {
                for (int c = 0; c < ColumnCount; c++)
                {
                    Button KvmBtn = new Button();
                    KvmBtn.Name = "KvmBtn" + BoardNum.ToString();
                    KvmBtn.Margin = new Thickness(3);
                    KvmBtn.Content = BoardNum.ToString();
                    KvmBtn.BorderThickness = new Thickness(0);
                    KvmBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBE2EF"));
                    KvmBtn.Click += KvmBtn_Click;

                    Grid.SetRow(KvmBtn, r);
                    Grid.SetColumn(KvmBtn, c);

                    KVMGrid.Children.Add(KvmBtn);
                    BoardNum++;
                }
            }
        }

        public void KvmBtn_Click(object sender, RoutedEventArgs e)
        {
            Button KVMbutton = sender as Button;

            //Console.WriteLine("Click " + KVMbutton.Name);

            string TargetKVM = KVMbutton.Name.Substring(KVMbutton.Name.Length - 1);

            byte BdNumCode = 0;

            switch(TargetKVM)
            {
                case "1": // Board 1
                    BdNumCode = 0x30;
                    break;
                case "2": // Board 2
                    BdNumCode = 0x31;
                    break;
                case "3": // Board 3
                    BdNumCode = 0x32;
                    break;
                case "4": // Board 4
                    BdNumCode = 0x33;
                    break;
                case "5": // Board 5
                    BdNumCode = 0x34;
                    break;
                case "6": // Board 6
                    BdNumCode = 0x35;
                    break;
                case "7": // Board 7
                    BdNumCode = 0x36;
                    break;
                case "8": // Board 8
                    BdNumCode = 0x37;
                    break;
                case "9": // Board 9
                    BdNumCode = 0x38;
                    break;
                case "0":  // Board 10
                    BdNumCode = 0x39;
                    break;
                case "H":   // Host PC
                    BdNumCode = 0x3A;
                    break;
                default:
                    break;
            }

            if (DelSerialCommandSendEventHandler != null)
                DelSerialCommandSendEventHandler.Invoke(StaticDefine.COMMAND_KVM_SELECT, BdNumCode);

        }
    }
}
