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
    /// <summary>
    /// Логика взаимодействия для WindowTable.xaml
    /// </summary>
    public partial class WindowTableImage : Window
    {
        public WindowTableImage(string fileName, string windowTitle)
        {
            InitializeComponent();
            img1.Source = new BitmapImage(new Uri("/BoltCalc2;component/images/" + fileName, UriKind.Relative));
            this.Title = windowTitle;
        }
    }
}
