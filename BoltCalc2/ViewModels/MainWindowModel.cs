using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BoltCalc2
{
    class MainWindowModel : INotifyPropertyChanged
    {
        //Поля


        //Свойства
        public ObservableCollection<BoltUnit> UnitList { get; set; }
        public ObservableCollection<ConnectionType> ConnectionTypes { get; set; }
        public ObservableCollection<Bolt> BoltList { get; set; }
        public ObservableCollection<StrengthClass> StrengthClassList { get; set; }
        public ObservableCollection<ItemGammaC> ListGammaC { get; set; }
        public ObservableCollection<SteelMark> SteelMarks { get; set; }
        public DataSet[] BaseSet { get; set; }
        


        //Свойства зависимости
        private BoltUnit selected_unit;
        public BoltUnit SelectedUnit
        {
            get { return selected_unit; }
            set
            {
                selected_unit = value;
                OnPropertyChanged("SelectedUnit");
            }
        }




        //Конструктор
        public MainWindowModel()
        {
            BaseSet = new DataSet[3];
            TablesLoad();
            ListGammaC = new ObservableCollection<ItemGammaC>();
            RefreshListGammaC();
            ConnectionTypes = new ObservableCollection<ConnectionType>()
            {
                new ConnectionType() {Name = "На срез", Description = "Соединение без контроллируемого натяжения болтов. Внешние усилия воспринимаются вследствие сопротивления болтов срезу и соединяемых элементов смятию."},
                new ConnectionType() {Name = "На растяжение", Description = "Соединение, в котором болты работают на растяжение (кроме фланцевых на высокопрочных болтах)"}
            };
            UnitList = new ObservableCollection<BoltUnit>();
            for(int i = 0; i < 1; i++)
            {
                AddItem();
            }
            SelectedUnit = UnitList[0];
            
        }

        //Загрузить из файла
        public void TablesLoad()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(DataSet));
            if (File.Exists("data\\myBase.xml"))
            {
                using (FileStream fs = new FileStream("data\\myBase.xml", FileMode.Open))
                {
                    BaseSet[0] = (DataSet)formatter.Deserialize(fs);
                }
            }
            if (File.Exists("data\\gost7798_70.xml"))
            {
                using (FileStream fs = new FileStream("data\\gost7798_70.xml", FileMode.Open))
                {
                    BaseSet[1] = (DataSet)formatter.Deserialize(fs);
                }
            }
        }

        //Инициализировать списки Гамма с
        public void RefreshListGammaC()
        {
            ListGammaC.Clear();
            foreach (DataRow dr in BaseSet[0].Tables["Коэффициенты условий работы γс"].Rows)
            {
                ItemGammaC igc = new ItemGammaC();
                igc.Description = TableReader.GetStringCell(dr, 0);
                igc.Value = TableReader.GetDoubleCell(dr, 1);
                ListGammaC.Add(igc);
            }
        }

        //Обновить табличные данные в узлах
        public void RefreshData()
        {
            foreach(BoltUnit bu in UnitList)
            {
                bu.RefreshBoltList();
                bu.RefreshStrengthClassList();
                bu.RefreshSteelMarkList();
                RefreshListGammaC();
            }
        }



        //Кнопки управления списком
        public void AddItem()
        {
            BoltUnit unit = new BoltUnit(BaseSet);
            unit.Number = UnitList.Count + 1;
            unit.ConnectionTypes = ConnectionTypes;
            unit.SelectedItemGammaC = ListGammaC[0];
            UnitList.Add(unit);
            SelectedUnit = unit;
        }
        public void CopyItem()
        {
            if (SelectedUnit != null)
            {
                BoltUnit unit = (BoltUnit)SelectedUnit.Clone();
                unit.Number = UnitList.Count + 1;
                UnitList.Add(unit);
                SelectedUnit = unit;
            }
        }
        public void RemoveItem()
        {
            if(SelectedUnit != null)
            {
                int selectedIndex = UnitList.IndexOf(SelectedUnit);
                UnitList.Remove(SelectedUnit);
                if (UnitList.Count >= selectedIndex + 1)
                {
                    SelectedUnit = UnitList[selectedIndex];
                    for(int i = selectedIndex; i<UnitList.Count; i++)
                    {
                        UnitList[i].Number = i + 1;
                    }
                }
                else if (UnitList.Count > 0)
                {
                    SelectedUnit = UnitList[selectedIndex - 1];
                }
                
            }
        }
        public void ItemUp()
        {
            if(SelectedUnit != null)
            {
                int selectedIndex = UnitList.IndexOf(SelectedUnit);
                if (selectedIndex > 0)
                {
                    BoltUnit tempItem = UnitList[selectedIndex];
                    UnitList[selectedIndex] = UnitList[selectedIndex - 1];
                    UnitList[selectedIndex].Number += 1;
                    UnitList[selectedIndex - 1] = tempItem;
                    UnitList[selectedIndex - 1].Number -= 1;
                    SelectedUnit = UnitList[selectedIndex - 1];
                }
            }

        }
        public void ItemDown()
        {
            if (SelectedUnit != null)
            {
                int selectedIndex = UnitList.IndexOf(SelectedUnit);
                if (selectedIndex < UnitList.Count - 1)
                {
                    BoltUnit tempItem = UnitList[selectedIndex];
                    UnitList[selectedIndex] = UnitList[selectedIndex + 1];
                    UnitList[selectedIndex].Number -= 1;
                    UnitList[selectedIndex + 1] = tempItem;
                    UnitList[selectedIndex + 1].Number += 1;
                    SelectedUnit = UnitList[selectedIndex + 1];
                }
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
