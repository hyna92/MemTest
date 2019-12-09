using System;
using System.Collections.Generic;
using System.IO.Ports;
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
    /// SerialConfigUC.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SerialConfigUC : UserControl
    {
        public event DelSerialSettingCommandSend DelSerialSettingCommandSendHandler = null;

        public Share share = null;

        public SerialConfigUC()
        {
            InitializeComponent();

            share = Share.Initialize;

            ComPortRefreshBtn_Click(null, null);
        }

        private void ComPortRefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            SerialPortCB.Items.Clear();

            ComboBoxItem TempPort = new ComboBoxItem();
            TempPort.Content = "----";
            SerialPortCB.Items.Add(TempPort);

            foreach (var port in SerialPort.GetPortNames())
            {
                ComboBoxItem COMPort = new ComboBoxItem();
                COMPort.Content = port;
                SerialPortCB.Items.Add(port);
                share.SerialPortList.Add(port);
            }

            SerialPortCB.SelectedIndex = 0;
        }

        private void SerialOpenBtn_Click(object sender, RoutedEventArgs e)
        {
            ComPortResultLB.Content = string.Empty;

            string ComPort = SerialPortCB.SelectedItem.ToString();

            if (DelSerialSettingCommandSendHandler != null)
                DelSerialSettingCommandSendHandler.Invoke(StaticDefine.COMMAND_SERIAL_OPEN, ComPort);
        }

        private void SerialCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            ComPortResultLB.Content = string.Empty;

            string ComPort = SerialPortCB.SelectedItem.ToString();

            if (DelSerialSettingCommandSendHandler != null)
                DelSerialSettingCommandSendHandler.Invoke(StaticDefine.COMMAND_SERIAL_CLOSE, ComPort);
        }

        public void ResultStringView(string result)
        {
            ComPortResultLB.Content = result;
        }
    }
}
