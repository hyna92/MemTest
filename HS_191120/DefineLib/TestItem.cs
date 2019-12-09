using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefineLib
{
    public class TestItem : INotifyPropertyChanged
    {
        private int _ItemNumber;
        private string _ItemName;
        private int _ItemProgress;
        private string _ItemState;

        public int ItemNumber
        {
            get { return _ItemNumber; }
            set
            {
                _ItemNumber = value;
                OnPropertyChanged("ItemNumber");
            }
        }
        public string ItemName
        {
            get { return _ItemName; }
            set
            {
                _ItemName = value;
                OnPropertyChanged("ItemName");
            }
        }

        public int ItemProgress
        {
            get { return _ItemProgress; }
            set
            {
                _ItemProgress = value;
                OnPropertyChanged("ItemProgress");
            }
        }

        public string ItemState
        {
            get { return _ItemState; }
            set
            {
                _ItemState = value;
                OnPropertyChanged("ItemState");
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
