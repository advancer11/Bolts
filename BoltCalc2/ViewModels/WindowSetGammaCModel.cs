using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BoltCalc2
{
    class WindowSetGammaCModel : INotifyPropertyChanged
    {
        public ObservableCollection<ItemGammaC> ListGammaC { get; set; }
        private ItemGammaC item_gamma_c;
        public ItemGammaC ItemGammaC
        {
            get { return item_gamma_c; }
            set
            {
                
                if(item_gamma_c != null) item_gamma_c.IsSelected = false;
                item_gamma_c = value;
                if (item_gamma_c != null) item_gamma_c.IsSelected = true;
                OnPropertyChanged("ItemGammaC");
            }
        }



        //Снять галочки
        public void UncheckedItems()
        {
            foreach(ItemGammaC gc in ListGammaC)
            {
                gc.IsSelected = false;
            }
        }


        //Код для поддержки MVVM:
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
