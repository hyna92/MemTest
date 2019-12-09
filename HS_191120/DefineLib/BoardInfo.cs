using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefineLib
{
    public class BoardInfo : INotifyPropertyChanged
    {
        public int BoardNum { get; set; }

        public bool IsSelected = false;
        //public bool IsTesting = false;

        public string ResultFileName = string.Empty;

        private string _EventMessage;
        public string EventMessage
        {
            get { return _EventMessage; }
            set
            {
                _EventMessage = value;
                OnPropertyChanged("EventMessage");
            }
        }



        #region 0. SYSTEM

        private bool _PowerState = false;
        public bool PowerState
        {
            get { return _PowerState; }
            set
            {
                _PowerState = value;
                OnPropertyChanged("PowerState");
            }
        }


        private bool _ConnectState = false;
        public bool ConnectState
        {
            get { return _ConnectState; }
            set
            {
                _ConnectState = value;
                OnPropertyChanged("ConnectState");
            }
        }

        //private string _IpAddr = string.Empty;
        //public string IpAddr
        //{
        //    get { return _IpAddr; }
        //    set
        //    {
        //        _IpAddr = value;
        //        OnPropertyChanged("IpAddr");
        //    }
        //}

        //private string _Cpu0 = string.Empty;
        //public string Cpu0
        //{
        //    get { return _Cpu0; }
        //    set
        //    {
        //        _Cpu0 = value;
        //        OnPropertyChanged("Cpu0");
        //    }
        //}

        private string _Cpu = string.Empty;
        public string Cpu
        {
            get { return _Cpu; }
            set
            {
                _Cpu = value;
                OnPropertyChanged("Cpu");
            }
        }

        private string _Memory = string.Empty;
        public string Memory
        {
            get { return _Memory; }
            set
            {
                _Memory = value;
                OnPropertyChanged("Memory");
            }
        }

        #endregion


        #region 0. VOLTAGE

        private int _VddQ = 0;
        public int VddQ
        {
            get { return _VddQ; }
            set
            {
                _VddQ = value;
                OnPropertyChanged("VddQ");
            }
        }

        //private int _Cpu0_Vdd;
        //public int Cpu0_Vdd
        //{
        //    get { return _Cpu0_Vdd; }
        //    set
        //    {
        //        _Cpu0_Vdd = value;
        //        OnPropertyChanged("Cpu0_Vdd");
        //    }
        //}
        //
        //private int _Cpu0_VddQ;
        //public int Cpu0_VddQ
        //{
        //    get { return _Cpu0_VddQ; }
        //    set
        //    {
        //        _Cpu0_VddQ = value;
        //        OnPropertyChanged("Cpu0_VddQ");
        //    }
        //}
        //
        //private int _Cpu0_VddQ2;
        //public int Cpu0_VddQ2
        //{
        //    get { return _Cpu0_VddQ2; }
        //    set
        //    {
        //        _Cpu0_VddQ2 = value;
        //        OnPropertyChanged("Cpu0_VddQ2");
        //    }
        //}
        //private int _Cpu1_Vdd;
        //public int Cpu1_Vdd
        //{
        //    get { return _Cpu1_Vdd; }
        //    set
        //    {
        //        _Cpu1_Vdd = value;
        //        OnPropertyChanged("Cpu1_Vdd");
        //    }
        //}
        //
        //private int _Cpu1_VddQ;
        //public int Cpu1_VddQ
        //{
        //    get { return _Cpu1_VddQ; }
        //    set
        //    {
        //        _Cpu1_VddQ = value;
        //        OnPropertyChanged("Cpu1_VddQ");
        //    }
        //}
        //
        //private int _Cpu1_VddQ2;
        //public int Cpu1_VddQ2
        //{
        //    get { return _Cpu1_VddQ2; }
        //    set
        //    {
        //        _Cpu1_VddQ2 = value;
        //        OnPropertyChanged("Cpu1_VddQ2");
        //    }
        //}

        #endregion


        #region 0. TEST

        private string _TestID;
        public string TestID
        {
            get { return _TestID; }
            set
            {
                _TestID = value;
                OnPropertyChanged("TestID");
            }
        }

        private int _TestStep = -1;
        public int TestStep
        {
            get { return _TestStep; }
            set
            {
                _TestStep = value;
                OnPropertyChanged("TestStep");
            }
        }

        private int _TestItemCount = 0;
        public int TestItemCount
        {
            get { return _TestItemCount; }
            set
            {
                _TestItemCount = value;
                OnPropertyChanged("TestItemCount");
            }
        }

        private int _TestState;
        public int TestState
        {
            get { return _TestState; }
            set
            {
                _TestState = value;
                OnPropertyChanged("TestState");
            }
        }

        private string _TestString = string.Empty;
        public string TestString
        {
            get { return _TestString; }
            set
            {
                _TestString = value;
                OnPropertyChanged("TestString");
            }
        }

        private string _TestTotalProgress = "0";
        public string TestTotalProgress
        {
            get { return _TestTotalProgress; }
            set
            {
                _TestTotalProgress = value;
                OnPropertyChanged("TestTotalProgress");
            }
        }


        private ObservableCollection<TestItem> _TestItemList = new ObservableCollection<TestItem>();
        public ObservableCollection<TestItem> TestItemList
        {
            get { return _TestItemList; }
            set
            {
                _TestItemList = value;
                OnPropertyChanged("TestItemList");
            }
        }



        private int _TestTotalErrorCount = 0;
        public int TestTotalErrorCount
        {
            get { return _TestTotalErrorCount; }
            set
            {
                _TestTotalErrorCount = value;
                OnPropertyChanged("TestTotalErrorCount");
            }
        }

        private int _TestOccurErrorCount = 0;
        public int TestOccurErrorCount
        {
            get { return _TestOccurErrorCount; }
            set
            {
                _TestOccurErrorCount = value;
                OnPropertyChanged("TestOccurErrorCount");
            }
        }
    
        private ObservableCollection<ErrorItem> _ErrorList = new ObservableCollection<ErrorItem>();
        public ObservableCollection<ErrorItem> ErrorList
        {
            get { return _ErrorList; }
            set
            {
                _ErrorList = value;
                OnPropertyChanged("ErrorList");
            }
        }



        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
