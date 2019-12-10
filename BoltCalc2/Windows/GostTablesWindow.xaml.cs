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
    /// Логика взаимодействия для GostTablesWindow.xaml
    /// </summary>
    public partial class GostTablesWindow : Window
    {
        //Поля
        private DataGrid selectedDg;
        private bool save_required;
        private DataSet gost_base;

        //Конструктор
        public GostTablesWindow()
        {
            InitializeComponent();
            gost_base = new DataSet();
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


        //Выбор текущего DataGrid
        private void Data_grid_GotFocus(object sender, RoutedEventArgs e)
        {
            selectedDg = (DataGrid)sender;
        }
        //Отметка об изменении данных
        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            save_required = true;
        }


        //Открыть базу
        private void OpenBase()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(DataSet));
            if (File.Exists("data\\gost7798_70.xml"))
            {
                using (FileStream fs = new FileStream("data\\gost7798_70.xml", FileMode.Open))
                {
                    gost_base = (DataSet)formatter.Deserialize(fs);
                }
            }
            BindTables();
        }


        //Меню Вставить
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = (ContextMenu)((MenuItem)sender).Parent;
            DataGrid dg = (DataGrid)cm.PlacementTarget;
            DataTable datatable = ((DataView)dg.ItemsSource).Table;
            string clipboardText = Clipboard.GetText();
            string[] clipboardRows = clipboardText.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            int indexSelectedItem = dg.Items.IndexOf(dg.SelectedItem);
            int iMax = Math.Min(clipboardRows.Length, datatable.Rows.Count - indexSelectedItem);
            for (int i = 0; i < iMax; i++)
            {
                string[] clipboardValues = clipboardRows[i].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                DataRow dr = datatable.Rows[indexSelectedItem + i];
                object[] values = dr.ItemArray;
                int indexColEnd = Math.Min(clipboardValues.Length, datatable.Columns.Count);
                for (int j = 0; j < indexColEnd; j++)
                {
                    values[j] = clipboardValues[j];
                }
                dr.ItemArray = values;
            }
            save_required = true;
        }

        //При загрузке окна
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OpenBase();
        }

        //При закрытии окна
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (save_required)
            {
                MessageBoxResult result = MessageBox.Show("Изменения вступят в силу после перезагрузки программы. Вы хотите сохранить изменения?", "Сохранение изменений", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        XmlSerializer formatter = new XmlSerializer(typeof(DataSet));
                        using (FileStream fs = new FileStream("data\\gost7798_70.xml", FileMode.Create))
                        {
                            formatter.Serialize(fs, gost_base);
                        }
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }


        //Привязать таблицы
        private void BindTables()
        {
            try
            {
                table1_1.ItemsSource = gost_base.Tables["Конструктивные параметры болтов"].DefaultView;
                table1_2.ItemsSource = gost_base.Tables["Длина резьбы"].DefaultView;
                table1_3.ItemsSource = gost_base.Tables["Масса болтов"].DefaultView;
                table1_4.ItemsSource = gost_base.Tables["Гайки"].DefaultView;
                table1_5.ItemsSource = gost_base.Tables["Шайбы"].DefaultView;
                table1_6.ItemsSource = gost_base.Tables["Шайбы пружинные"].DefaultView;
            }
            catch { }
        }


        //Добавить таблицу
        private void Button_Click_Create(object sender, RoutedEventArgs e)
        {
            DataTable dt;
            /*
            dt = new DataTable("Конструктивные параметры болтов");
            dt.Columns.Add("Bolt", typeof(string));
            dt.Columns.Add("Diameter", typeof(double));
            dt.Columns.Add("TurnkeySize", typeof(double));
            dt.Columns.Add("HeadHeight", typeof(double));
            dt.Columns.Add("ThreadPitch", typeof(double));
            dt.Rows.Add(dt.NewRow());
            gost_base.Tables.Add(dt);
            
            dt = new DataTable("Длина резьбы");
            dt.Columns.Add("Длина болта", typeof(double));
            dt.Columns.Add("М6", typeof(double));
            dt.Columns.Add("М8", typeof(double));
            dt.Columns.Add("М10", typeof(double));
            dt.Columns.Add("М12", typeof(double));
            dt.Columns.Add("М14", typeof(double));
            dt.Columns.Add("М16", typeof(double));
            dt.Columns.Add("М18", typeof(double));
            dt.Columns.Add("М20", typeof(double));
            dt.Columns.Add("М22", typeof(double));
            dt.Columns.Add("М24", typeof(double));
            dt.Columns.Add("М27", typeof(double));
            dt.Columns.Add("М30", typeof(double));
            dt.Columns.Add("М36", typeof(double));
            dt.Columns.Add("М40", typeof(double));
            dt.Columns.Add("М48", typeof(double));
            dt.Rows.Add(dt.NewRow());
            gost_base.Tables.Add(dt);

            dt = new DataTable("Масса болтов");
            dt.Columns.Add("Длина болта", typeof(double));
            dt.Columns.Add("М6", typeof(double));
            dt.Columns.Add("М8", typeof(double));
            dt.Columns.Add("М10", typeof(double));
            dt.Columns.Add("М12", typeof(double));
            dt.Columns.Add("М14", typeof(double));
            dt.Columns.Add("М16", typeof(double));
            dt.Columns.Add("М18", typeof(double));
            dt.Columns.Add("М20", typeof(double));
            dt.Columns.Add("М22", typeof(double));
            dt.Columns.Add("М24", typeof(double));
            dt.Columns.Add("М27", typeof(double));
            dt.Columns.Add("М30", typeof(double));
            dt.Columns.Add("М36", typeof(double));
            dt.Columns.Add("М40", typeof(double));
            dt.Columns.Add("М48", typeof(double));
            dt.Rows.Add(dt.NewRow());
            gost_base.Tables.Add(dt);

            dt = new DataTable("Гайки");
            dt.Columns.Add("Thread", typeof(string));
            dt.Columns.Add("Height", typeof(double));
            dt.Columns.Add("Weight", typeof(double));
            dt.Rows.Add(dt.NewRow());
            gost_base.Tables.Add(dt);

            dt = new DataTable("Шайбы");
            dt.Columns.Add("Thread", typeof(string));
            dt.Columns.Add("Height", typeof(double));
            dt.Columns.Add("Weight", typeof(double));
            dt.Rows.Add(dt.NewRow());
            gost_base.Tables.Add(dt);

            dt = new DataTable("Шайбы пружинные");
            dt.Columns.Add("Thread", typeof(string));
            dt.Columns.Add("Height", typeof(double));
            dt.Columns.Add("Weight", typeof(double));
            dt.Rows.Add(dt.NewRow());
            gost_base.Tables.Add(dt);
            */

            gost_base.Tables["Длина резьбы"].Columns.Add(new DataColumn("Не рекомендуется", typeof(bool)));
            save_required = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            save_required = true;
        }

        //Обеспечить нормальную прокрутку
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            if (e.Delta > 0) scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta + 70);
            else scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta - 70);
            e.Handled = true;
        }
    }
}
