using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Xml.Serialization;

namespace BoltCalc2
{
    /// <summary>
    /// Логика взаимодействия для BaseWindow.xaml
    /// </summary>
    public partial class BaseWindow : Window
    {
        //Поля
        private DataSet sourceBase;
        private DataGrid selectedDg;
        private bool save_required;


        public BaseWindow(DataSet my_base)
        {
            InitializeComponent();
            sourceBase = my_base;
        }

        //Загрузить из файла
        /*
        public void TablesLoad()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(DataSet));
            if (File.Exists("data\\myBase.xml"))
            {
                using (FileStream fs = new FileStream("data\\myBase.xml", FileMode.Open))
                {
                    sourceBase = (DataSet)formatter.Deserialize(fs);
                }
            }
        }*/

            //Создать таблицы в окне
            private void CreateTables()
        {
            foreach (DataTable dt in sourceBase.Tables)
            {
                spTables.Children.Add(CreateTableHeader(dt));
                spTables.Children.Add(CreateStandartTable(dt));
            }
        }

        //Показать заголовок таблицы
        private TextBlock CreateTableHeader(DataTable dt)
        {
            TextBlock tb = new TextBlock();
            tb.Text = dt.TableName;
            tb.TextWrapping = TextWrapping.WrapWithOverflow;
            tb.Margin = new Thickness(5, 5, 5, 0);
            return tb;
        }
        //Показать стандартную таблицу
        private DataGrid CreateStandartTable(DataTable dt)
        {
            DataGrid dg = new DataGrid();
            dg.ItemsSource = dt.AsDataView();
            dg.HeadersVisibility = DataGridHeadersVisibility.Column;
            dg.CanUserSortColumns = false;
            dg.CanUserAddRows = false;
            dg.CanUserDeleteRows = false;
            dg.CanUserReorderColumns = false;
            dg.Margin = new Thickness(5);
            dg.SelectionMode = DataGridSelectionMode.Extended;
            dg.MinColumnWidth = 80;
            dg.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
            dg.GotFocus += DataGrid_GotFocus;
            dg.PreparingCellForEdit += DataGrid_PreparingCellForEdit;
            return dg;
        }

        //Переименовать и отформатировать столбцы
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.Column.Header.ToString())
            {
                case "Rbun": { e.Column.Header = "Rbun, Н/мм2"; break; }
                case "Rbyn": { e.Column.Header = "Rbyn, Н/мм2"; break; }
                case "Rbs": { e.Column.Header = "Rbs, Н/мм2"; break; }
                case "Rbt": { e.Column.Header = "Rbt, Н/мм2"; break; }
                case "Ryn": { e.Column.Header = "Ryn, Н/мм2"; break; }
                case "Run": { e.Column.Header = "Run, Н/мм2"; break; }
                case "Rbp A": { e.Column.Header = "Rbp (A), Н/мм2"; break; }
                case "Rbp B": { e.Column.Header = "Rbp (B), Н/мм2"; break; }
            }
            if (e.Column.Header.ToString() == "Элементы конструкций")
            {
                e.Column.Width = 600;
                Style column_style = new Style();
                column_style.Setters.Add(new Setter { Property = TextBlock.TextWrappingProperty, Value = TextWrapping.Wrap });
                ((DataGridTextColumn)e.Column).ElementStyle = column_style;
            }
        }

        private void DataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            selectedDg = (DataGrid)sender;
        }
        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            save_required = true;
        }



        //Кнопка Добавить строку
        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            if (selectedDg == null) return;
            DataTable dt = ((DataView)selectedDg.ItemsSource).Table;
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);
            save_required = true;
        }

        //Кнопка Удалить строку
        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            if (selectedDg == null || selectedDg.SelectedItem == null) return;
            DataTable dt = ((DataView)selectedDg.ItemsSource).Table;
            DataRow dr = ((DataRowView)selectedDg.SelectedItem).Row;
            int i = dt.Rows.IndexOf(dr);
            if (dt.Rows.Count > 1) dt.Rows.Remove(dr);
            if (selectedDg.Items.Count > i)
            {
                selectedDg.SelectedItem = selectedDg.Items[i];
            }
            save_required = true;
        }

        //Кнопка Строку вверх
        private void Button_Click_Up(object sender, RoutedEventArgs e)
        {
            if (selectedDg == null) return;
            DataTable dt = ((DataView)selectedDg.ItemsSource).Table;
            DataRow dr = ((DataRowView)selectedDg.SelectedItem).Row;
            int index = dt.Rows.IndexOf(dr);
            if (index > 0)
            {
                object[] values = dt.Rows[index - 1].ItemArray;
                dt.Rows.Remove(dt.Rows[index - 1]);
                dt.Rows.InsertAt(dt.NewRow(), index);
                dt.Rows[index].ItemArray = values;
            }
            save_required = true;
        }

        //Кнопка Строку вниз
        private void Button_Click_Down(object sender, RoutedEventArgs e)
        {
            if (selectedDg == null) return;
            DataTable dt = ((DataView)selectedDg.ItemsSource).Table;
            DataRow dr = ((DataRowView)selectedDg.SelectedItem).Row;
            int index = dt.Rows.IndexOf(dr);
            if (index < dt.Rows.Count - 1)
            {
                object[] values = dt.Rows[index + 1].ItemArray;
                dt.Rows.Remove(dt.Rows[index + 1]);
                dt.Rows.InsertAt(dt.NewRow(), index);
                dt.Rows[index].ItemArray = values;
            }
            save_required = true;
        }


        //При закрытии окна
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (save_required)
            {
                MessageBoxResult result = MessageBox.Show("Сохранить изменения?", "Сохранение изменений", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes :
                        XmlSerializer formatter = new XmlSerializer(typeof(DataSet));
                        using (FileStream fs = new FileStream("data\\myBase.xml", FileMode.Create))
                        {
                            formatter.Serialize(fs, sourceBase);
                        }
                        DialogResult = true;
                        break;
                    case MessageBoxResult.No :
                        DialogResult = false;
                        break;
                    case MessageBoxResult.Cancel :
                        e.Cancel = true;
                        break;
                }
            }
        }



        //Обеспечить нормальную прокрутку
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            if (e.Delta > 0) scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta + 70);
            else scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta - 70);
            e.Handled = true;
        }




        //Добавить таблицу
        private void Button_Click_Create(object sender, RoutedEventArgs e)
        {
            /*DataTable dt;
            dt = new DataTable("Площади сечения болтов");
            dt.Columns.Add("Болт", typeof(string));
            dt.Columns.Add("d, мм", typeof(double));
            dt.Columns.Add("Ab, см2", typeof(double));
            dt.Columns.Add("Abn, см2", typeof(double));
            dt.Rows.Add(dt.NewRow());
            sourceBase.Tables.Add(dt);*/

            /*dt = new DataTable("Нормативные сопротивления стали болтов и расчётные сопротивления одноболтовых соединений срезу и растяжению");
            dt.Columns.Add("Класс прочности болтов", typeof(string));
            dt.Columns.Add("Rbun", typeof(double));
            dt.Columns.Add("Rbyn", typeof(double));
            dt.Columns.Add("Rbs", typeof(double));
            dt.Columns.Add("Rbt", typeof(double));
            dt.Columns.Add("Высокопрочный", typeof(bool));
            dt.Rows.Add(dt.NewRow());
            sourceBase.Tables.Add(dt);*/

            /*dt = new DataTable("Нормативные сопротивления при растяжении, сжатии и изгибе фасонного проката");
            dt.Columns.Add("Марка стали", typeof(string));
            dt.Columns.Add("t min, мм", typeof(double));
            dt.Columns.Add("t max, мм", typeof(double));
            dt.Columns.Add("Ryn", typeof(double));
            dt.Columns.Add("Run", typeof(double));
            dt.Rows.Add(dt.NewRow());
            sourceBase.Tables.Add(dt);*/

            /*dt = new DataTable("Расчетные сопротивления смятию элементов, соединяемых болтами");
            dt.Columns.Add("Run", typeof(double));
            dt.Columns.Add("Rbp A", typeof(double));
            dt.Columns.Add("Rbp B", typeof(double));
            dt.Rows.Add(dt.NewRow());
            sourceBase.Tables.Add(dt);*/

            /*dt = new DataTable("Коэффициенты условий работы γс");
            dt.Columns.Add("Элементы конструкций", typeof(string));
            dt.Columns.Add("Коэф-т γс", typeof(double));
            dt.Rows.Add(dt.NewRow());
            sourceBase.Tables.Add(dt);*/


            spTables.Children.Clear();
            CreateTables();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateTables();
        }

        private void Button_Click_Delete(object sender, RoutedEventArgs e)
        {/*
            sourceBase.Tables.Remove("Нормативные сопротивления стали болтов и расчётные сопротивления одноболтовых соединений срезу и растяжению");
            spTables.Children.Clear();
            CreateTables();*/
        }
    }
}
