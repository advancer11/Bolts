using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BoltCalc2
{
    class ItemGammaC : INotifyPropertyChanged
    {
        private bool is_selected;
        public bool IsSelected
        {
            get { return is_selected; }
            set { is_selected = value; OnPropertyChanged("IsSelected"); }
        }
        public string Description { get; set; }
        public double Value { get; set; }

        /*
        public override string ToString()
        {
            return Value.ToString();
        }
        */


        //Код для поддержки MVVM:
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
