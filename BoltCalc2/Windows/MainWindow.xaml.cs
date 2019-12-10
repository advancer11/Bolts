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

namespace BoltCalc2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowModel this_dc = new MainWindowModel();
        private List<GroupBox> calc_groupBoxes;

        //Конструктор
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this_dc;
            calc_groupBoxes = new List<GroupBox>()
            {
                groupBox_slice,
                groupBox_crumple,
                groupBox_tension
            };
        }



        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            this_dc.AddItem();
        }

        private void Button_Click_Copy(object sender, RoutedEventArgs e)
        {
            this_dc.CopyItem();
        }

        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            this_dc.RemoveItem();
        }

        private void Button_Click_Up(object sender, RoutedEventArgs e)
        {
            this_dc.ItemUp();
        }

        private void Button_Click_Down(object sender, RoutedEventArgs e)
        {
            this_dc.ItemDown();
        }


        //При смене выбранного узла
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            groupBox_main.DataContext = this_dc.SelectedUnit;
            if (this_dc.SelectedUnit == null) return;
            
            foreach(GroupBox gb in calc_groupBoxes)
            {
                gb.DataContext = this_dc.SelectedUnit.MyCalculation;
            }
            groupBox_kit.DataContext = this_dc.SelectedUnit.SelectedKit;
            tp_package.DataContext = this_dc.SelectedUnit;
            if (this_dc.SelectedUnit.SelectedKit == null) grid_button_length.IsEnabled = false;
        }

        //Увеличить/уменьшить кол-во болтов
        private void Button_Click_PlusBoltQuant(object sender, RoutedEventArgs e)
        {
            this_dc.SelectedUnit.BoltQuantity += 1;
        }
        private void Button_Click_MinusBoltQuant(object sender, RoutedEventArgs e)
        {
            this_dc.SelectedUnit.BoltQuantity -= 1;
        }

        //Открыть окно базы данных для расчета
        private void Menu_Click_OpenBase(object sender, RoutedEventArgs e)
        {
            BaseWindow baseWindow = new BaseWindow(this_dc.BaseSet[0]);
            baseWindow.Owner = this;
            baseWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if(baseWindow.ShowDialog() == true)
            {
                this_dc.RefreshData();
            }
        }
        //Открыть окно Гамма с
        private void Button_Click_Open_GammaC_Window(object sender, RoutedEventArgs e)
        {
            WindowSetGammaC window_gamma_c = new WindowSetGammaC();
            window_gamma_c.Owner = this;
            window_gamma_c.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            WindowSetGammaCModel dc = new WindowSetGammaCModel();
            window_gamma_c.DataContext = dc;
            dc.ListGammaC = this_dc.ListGammaC;
            dc.ItemGammaC = this_dc.SelectedUnit.SelectedItemGammaC;
            if(window_gamma_c.ShowDialog() == true)
            {
                this_dc.SelectedUnit.SelectedItemGammaC = dc.ItemGammaC;
            }
        }
        //Открыть окно Гамма b
        private void Button_Click_Open_GammaB_Window(object sender, RoutedEventArgs e)
        {
            if (this_dc.SelectedUnit == null) return;
            if (this_dc.SelectedUnit.MyCalculation == null) return;
            Calc_SliceAndCrumple calc = (Calc_SliceAndCrumple)this_dc.SelectedUnit.MyCalculation;
            if (calc.SelectedBolt == null || calc.SelectedStrengthClass == null || calc.SelectedSteelMark == null)
            {
                MessageBox.Show("Не заданы параметры болтового соединения", "Ошибка", MessageBoxButton.OK);
                return;
            }
            WindowSetGammaB window_gamma_b = new WindowSetGammaB();
            window_gamma_b.Owner = this;
            window_gamma_b.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window_gamma_b.DataContext = new WindowSetGammaBModel(calc);
            window_gamma_b.ShowDialog();
        }



        //Код для автоматического перехода на нужную строку в списке
        private void ComboBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            BoltUnit ci = (BoltUnit)cb.DataContext;
            this_dc.SelectedUnit = ci;
        }
        private void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            BoltUnit ci = (BoltUnit)tb.DataContext;
            this_dc.SelectedUnit = ci;
        }

        //Нажатие клавиши в поле ввода
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = ((TextBox)sender);
                tb.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                tb.SelectAll();
            }
        }

        //Изменить цвет Коэфф. использ.
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text == "" || tb.Text == "0")
            {
                tb.Background = SystemColors.WindowBrush;
                return;
            }
            double value = Double.Parse(tb.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
            if (value < 1) tb.Background = SystemColors.WindowBrush;
            else tb.Background = Brushes.PeachPuff;
        }
        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text == "" || tb.Text == "0") return;
            double value = Double.Parse(tb.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
            if (value >= 1) tb.Background = Brushes.PeachPuff;
        }

        //Показать только нужные расчеты и скрыть ненужные
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (GroupBox gb in calc_groupBoxes)
            {
                gb.Visibility = Visibility.Collapsed;
            }
            cb_over2.Visibility = Visibility.Collapsed;
            if (this_dc.SelectedUnit == null) return;
            switch (this_dc.SelectedUnit.ConnectionTypes.IndexOf(this_dc.SelectedUnit.SelectedConnectionType))
            {
                case 0:
                    groupBox_slice.Visibility = Visibility.Visible;
                    groupBox_crumple.Visibility = Visibility.Visible;
                    cb_over2.Visibility = Visibility.Visible;
                    break;
                case 1:
                    groupBox_tension.Visibility = Visibility.Visible;
                    break;
            }
        }

        //Открыть окно ГОСТ
        private void Menu_Click_OpenGost(object sender, RoutedEventArgs e)
        {
            GostTablesWindow gtw = new GostTablesWindow();
            gtw.Owner = this;
            gtw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            gtw.ShowDialog();
        }

        //При изменении размера окна
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth < 1250)
            {
                grid_calc.Width = 1220;
                scrollviever_calc.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            }
            else
            {
                grid_calc.Width = Double.NaN;
                scrollviever_calc.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
            if (this.ActualWidth > 1700)
            {
                grid_calc.Height = 350;
                grid_button_length.Height = 25;
            }
            else
            {
                grid_calc.Height = 380;
                grid_button_length.Height = 38;
            }
        }

        //Заблокировать список классов точности, если высокопрочный болт
        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (cb.Items.Count > 1) cb.IsEnabled = true;
            else cb.IsEnabled = false;
        }

        //Увеличить, уменьшить длину болта
        private void Button_Click_UpLength(object sender, RoutedEventArgs e)
        {
            this_dc.SelectedUnit.SelectedKit.UpLengthManually();
        }
        private void Button_Click_DownLength(object sender, RoutedEventArgs e)
        {
            this_dc.SelectedUnit.SelectedKit.DownLengthManually();
        }

        //Заблокировать кнопки управления длиной
        private void CheckBox_AutoLength_Checked(object sender, RoutedEventArgs e)
        {
            grid_button_length.IsEnabled = false;
        }
        private void CheckBox_AutoLength_Unchecked(object sender, RoutedEventArgs e)
        {
            grid_button_length.IsEnabled = true;
        }

        private void CheckBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.DataContext != null) cb.IsEnabled = true;
            else cb.IsEnabled = false;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Cb_over2_Checked(object sender, RoutedEventArgs e)
        {
            tb_ext.Visibility = Visibility.Visible;
            tbx_ext.Visibility = Visibility.Visible;
        }

        private void Cb_over2_Unchecked(object sender, RoutedEventArgs e)
        {
            tb_ext.Visibility = Visibility.Collapsed;
            tbx_ext.Visibility = Visibility.Collapsed;
        }
    }
}
