using System;
using System.Collections.Generic;
using System.Data;
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
    /// <summary>
    /// Логика взаимодействия для WindowSetGammaC.xaml
    /// </summary>
    public partial class WindowSetGammaC : Window
    {

        //Конструктор
        public WindowSetGammaC()
        {
            InitializeComponent();
        }

        //Применить Гамма С и закрыть
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((WindowSetGammaCModel)DataContext).UncheckedItems();
        }
    }
}
