using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DefineLib
{
    public class ErrorItem : INotifyPropertyChanged
    {
        private int _ErrorCount;
        private int _ErrorIndex;
        private string _ErrorAddress;

        public int ErrorCount
        {
            get { return _ErrorCount; }
            set
            {
                _ErrorCount = value;
                OnPropertyChanged("ErrorCount");
            }
        }

        public int ErrorIndex
        {
            get { return _ErrorIndex; }
            set
            {
                _ErrorIndex = value;
                OnPropertyChanged("ErrorIndex");
            }
        }

        public string ErrorAddress
        {
            get { return _ErrorAddress; }
            set
            {
                _ErrorAddress = value;
                OnPropertyChanged("ErrorAddress");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

    }
}
