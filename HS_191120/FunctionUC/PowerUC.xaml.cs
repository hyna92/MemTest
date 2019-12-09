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
    /// SystemUC.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PowerUC : UserControl
    {
        //public event DelCommandSend DelCommandSendEventHandler = null;
        public event DelSerialCommandSend DelSerialCommandSendEventHandler = null;

        public Share share = null;

        public PowerUC()
        {
            InitializeComponent();

            share = Share.Initialize;
        }

        private void PowerOnBtn_Click(object sender, RoutedEventArgs e)
        {
            share.EventLog(null, "[Click]\t", "Power On");

            if (DelSerialCommandSendEventHandler != null)
                DelSerialCommandSendEventHandler.Invoke(StaticDefine.COMMAND_POWER_ON, StaticDefine.SERIAL_CMD_PW_ON);
        }

        private void PowerOffBtn_Click(object sender, RoutedEventArgs e)
        {
            share.EventLog(null, "[Click]\t", "Power Off");

            if (DelSerialCommandSendEventHandler != null)
                DelSerialCommandSendEventHandler.Invoke(StaticDefine.COMMAND_POWER_OFF, StaticDefine.SERIAL_CMD_PW_OFF);
        }

        //private void PowerResetBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    share.EventLog(null, "[Click]\t", "Power Reset");

        //    if (DelSerialCommandSendEventHandler != null)
        //        DelSerialCommandSendEventHandler.Invoke(StaticDefine.COMMAND_POWER_RESET, StaticDefine.MSG_OPTION_NULL);
        //}
    }
}
