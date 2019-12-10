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
using System.Windows.Shapes;

namespace BoltCalc2
{
    public partial class WindowSetGammaB : Window
    {
        private WindowTableImage wt40;
        private WindowTableImage wt41;


        //Конструктор
        internal WindowSetGammaB()
        {
            InitializeComponent();
        }

        //Показать таблицу
        private void Button_Click_Open_Table(object sender, RoutedEventArgs e)
        {
            if (sender == bt1)
            {
                if (wt40 != null && wt40.IsLoaded) return;
                wt40 = new WindowTableImage("table40.jpg", "Таблица 40");
                wt40.Owner = this;
                wt40.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                wt40.Show();
            }
            else if (sender == bt2)
            {
                if (wt41 != null && wt41.IsLoaded) return;
                wt41 = new WindowTableImage("table41.jpg", "Таблица 41");
                wt41.Owner = this;
                wt41.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                wt41.Show();
            }
        }

        //ПРименить и закрыть
        private void Button_Click_Apply(object sender, RoutedEventArgs e)
        {
            ((WindowSetGammaBModel)this.DataContext).DataSave();
            this.Close();
        }
        //Отмена
        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //При загрузке
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!((WindowSetGammaBModel)this.DataContext).IsMultiBolt) tbS.IsEnabled = false;
            //((WindowSetGammaBModel)this.DataContext).ShowReport();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = ((TextBox)sender);
                tb.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                tb.SelectAll();
            }
        }

        
    }
}
