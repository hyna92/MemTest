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
    /// VoltageUC.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VoltageUC : UserControl
    {
        public Share share = null;

        public event DelCommandSend DelCommandSendEventHandler = null;

        public VoltageUC()
        {
            InitializeComponent();

            share = Share.Initialize;
        }

        private void VoltValueTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(((Key.D0 <= e.Key) && (e.Key <= Key.D9)) || ((Key.NumPad0 <= e.Key) && (e.Key <= Key.NumPad9)) || e.Key == Key.Back))
            {
                e.Handled = true;
            }
        }

        private void VoltApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            //string selch = CHSelectCB.SelectionBoxItem.ToString();
            //string selvdd = VddSelectCB.SelectionBoxItem.ToString();
            //
            //string sendvalue = selch + " " + selvdd + " " + VoltValueTB.Text + " mV";

            if(VoltValueTB.Text == string.Empty)
            {
                MessageBox.Show("Please Enter a Voltage Value", null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                share.EventLog(null, "[Show]\t", "MessageBox > Please Enter a Voltage Value");
                return;
            }

            share.EventLog(null, "[Click]\t", string.Format("Apply {0} ", VoltValueTB.Text));

            //string SelectChannel = CHSelectCB.SelectedIndex.ToString() + VddSelectCB.SelectedIndex.ToString();

            if (DelCommandSendEventHandler != null)
                DelCommandSendEventHandler.Invoke(StaticDefine.COMMAND_VOLTAGE_SET, Convert.ToInt32(VoltValueTB.Text));
        }

        private void VoltMeasureBtn_Click(object sender, RoutedEventArgs e)
        {
            //string selch = CHSelectCB.SelectionBoxItem.ToString();
            //string selvdd = VddSelectCB.SelectionBoxItem.ToString();

            share.EventLog(null, "[Click]\t", string.Format("Measure "));

            //string SelectChannel = CHSelectCB.SelectedIndex.ToString() + VddSelectCB.SelectedIndex.ToString();

            if (DelCommandSendEventHandler != null)
                DelCommandSendEventHandler.Invoke(StaticDefine.COMMAND_VOLTAGE_GET, StaticDefine.MSG_OPTION_NULL);
        }


    }
}
