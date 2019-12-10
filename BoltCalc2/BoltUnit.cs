using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BoltCalc2
{
    class BoltUnit : INotifyPropertyChanged, ICloneable
    {
        //Поля
        private DataSet[] base_set;
        private DataSet my_base;
        private DataSet gost_base;
        private List<ICalculation> calc_list;


        //Свойства-коллекции
        public ObservableCollection<ConnectionType> ConnectionTypes { get; set; }
        private ObservableCollection<Bolt> bolt_list;
        public ObservableCollection<Bolt> BoltList
        {
            get { return bolt_list; }
            set { bolt_list = value; OnPropertyChanged("BoltList"); }
        }
        private ObservableCollection<StrengthClass> strength_class_list;
        public ObservableCollection<StrengthClass> StrengthClassList
        {
            get { return strength_class_list; }
            set { strength_class_list = value; OnPropertyChanged("StrengthClassList"); }
        }
        public ObservableCollection<string> StandartList { get; set; }
        public ObservableCollection<Kit> KitList { get; set; }
        


        //Свойства зависимости
        private int number;
        public int Number
        {
            get { return number; }
            set { number = value; OnPropertyChanged("Number"); }
        }
        private ConnectionType selected_connection_type;
        public ConnectionType SelectedConnectionType
        {
            get { return selected_connection_type; }
            set
            {
                selected_connection_type = value;
                OnPropertyChanged("SelectedConnectionType");
                RefreshBoltList();
                ChangeCalc();
                RefreshStandartList();
                if (SelectedBolt != null) RefreshKitList();
                if (ConnectionTypes.IndexOf(SelectedConnectionType) != 0) Over2Det = false;
            }
        }          //Обновить списки: болтов, стандартов, комплектов, создать объект расчета
        private Bolt selected_bolt;
        public Bolt SelectedBolt
        {
            get { return selected_bolt; }
            set
            {
                selected_bolt = value;
                OnPropertyChanged("SelectedBolt");
                if (MyCalculation != null) MyCalculation.SelectedBolt = selected_bolt;
                RefreshStrengthClassList();
                if (KitList.Count == 0) RefreshKitList();
                if (selected_bolt != null && SelectedKit != null) SelectedKit.BoltName = selected_bolt.Name;
            }
        }                              //Обновить списки: классов прочности, 
        private int bolt_quantity;
        public int BoltQuantity
        {
            get { return bolt_quantity; }
            set
            {
                if (value > 0)
                {
                    bolt_quantity = value;
                    OnPropertyChanged("BoltQuantity");
                    if (MyCalculation != null) MyCalculation.BoltQuantity = bolt_quantity;
                }
            }
        }                               //Проверка на больше 0
        private StrengthClass selected_strength_class;
        public StrengthClass SelectedStrengthClass
        {
            get { return selected_strength_class; }
            set
            {
                selected_strength_class = value;
                OnPropertyChanged("SelectedStrengthClass");
                if (MyCalculation != null) MyCalculation.SelectedStrengthClass = selected_strength_class;
            }
        }            //Обновить списки: классов точности
        private double force;
        public double Force
        {
            get { return force; }
            set
            {
                force = value;
                OnPropertyChanged("Force");
                if (MyCalculation != null) MyCalculation.Force = force;
            }
        }
        private string selected_standart;
        public string SelectedStandart
        {
            get { return selected_standart; }
            set
            {
                selected_standart = value;
                OnPropertyChanged("SelectedStandart");
                if (selected_standart == "ГОСТ 7798-70") gost_base = base_set[1];
            }
        }                        //Присвоение базы ГОСТов
        private Kit selected_kit;
        public Kit SelectedKit
        {
            get { return selected_kit; }
            set
            {
                selected_kit = value;
                OnPropertyChanged("SelectedKit");
                if (selected_kit == null) return;
                if (SelectedBolt != null) selected_kit.BoltName = SelectedBolt.Name;
                selected_kit.Package = Package;
                selected_kit.ResultChangedEvent += UpdateLengthHandler;
                BoltLength = SelectedKit.BoltLength.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        private string util_rate;
        public string UtilRate
        {
            get { return util_rate; }
            set { util_rate = value; OnPropertyChanged("UtilRate"); }
        }
        private double package;
        public double Package
        {
            get { return package; }
            set
            {
                package = value;
                OnPropertyChanged("Package");
                if (SelectedKit != null) SelectedKit.Package = package;
            }
        }
        private string bolt_length;
        public string BoltLength
        {
            get { return bolt_length; }
            set
            {
                bolt_length = value;
                OnPropertyChanged("BoltLength");
            }
        }
        private bool over_2_det;
        public bool Over2Det
        {
            get { return over_2_det; }
            set
            {
                over_2_det = value;
                OnPropertyChanged("Over2Det");
            }
        }


        //Свойства
        public ICalculation MyCalculation { get; set; }
        private ItemGammaC selected_item_gamma_c;
        public ItemGammaC SelectedItemGammaC
        {
            get { return selected_item_gamma_c; }
            set
            {
                selected_item_gamma_c = value;
                if(MyCalculation != null) MyCalculation.GammaC = selected_item_gamma_c.Value;
            }
        }


        //Конструктор
        public BoltUnit(DataSet[] data_base)
        {
            base_set = data_base;
            this.my_base = base_set[0];
            BoltList = new ObservableCollection<Bolt>();
            StandartList = new ObservableCollection<string>();
            BoltQuantity = 1;
            StrengthClassList = new ObservableCollection<StrengthClass>();
            KitList = new ObservableCollection<Kit>();

            calc_list = new List<ICalculation>()
            {
                new Calc_SliceAndCrumple(base_set[0]),
                new Calc_Tension()
            };
            foreach(ICalculation calc in calc_list)
            {
                calc.ResultChangedEvent += UpdateResultHandler;
            }
        }


        //Изменить объект расчета
        private void ChangeCalc()
        {
            if (SelectedConnectionType != null)
            {
                int index = ConnectionTypes.IndexOf(SelectedConnectionType);
                MyCalculation = calc_list[index];
            }
            if (MyCalculation == null) return;
            MyCalculation.BoltQuantity = BoltQuantity;
            MyCalculation.SelectedBolt = SelectedBolt;
            MyCalculation.SelectedStrengthClass = SelectedStrengthClass;
            MyCalculation.GammaC = selected_item_gamma_c.Value;
            MyCalculation.Force = Force;
        }



        //Обновить список болтов
        public void RefreshBoltList()
        {
            string name;
            name = SelectedBolt == null ? "" : SelectedBolt.Name;
            BoltList.Clear();
            foreach (DataRow dr in my_base.Tables["Площади сечения болтов"].Rows)
            {
                Bolt bolt = new Bolt();
                bolt.Name = TableReader.GetStringCell(dr, 0);
                bolt.Diameter = TableReader.GetDoubleCell(dr, 1);
                bolt.Ab = TableReader.GetDoubleCell(dr, 2);
                bolt.Abn = TableReader.GetDoubleCell(dr, 3);
                BoltList.Add(bolt);
            }
            if (name == "") return;
            foreach (Bolt bolt in BoltList)
            {
                if (bolt.Name == name)
                {
                    SelectedBolt = bolt;
                    break;
                }
            }
        }
        //Обновить список классов прочности болтов
        public void RefreshStrengthClassList()
        {
            string name;
            name = SelectedStrengthClass == null ? "" : SelectedStrengthClass.Name;
            StrengthClassList.Clear();
            foreach (DataRow dr in my_base.Tables["Нормативные сопротивления стали болтов и расчётные сопротивления одноболтовых соединений срезу и растяжению"].Rows)
            {
                StrengthClass sc = new StrengthClass();
                sc.Name = TableReader.GetStringCell(dr, 0);
                sc.Rbun = TableReader.GetDoubleCell(dr, 1);
                sc.Rbyn = TableReader.GetDoubleCell(dr, 2);
                sc.Rbs = TableReader.GetDoubleCell(dr, 3);
                sc.Rbt = TableReader.GetDoubleCell(dr, 4);
                sc.IsHighStrength = TableReader.GetBoolCell(dr, 5);
                StrengthClassList.Add(sc);
            }
            if (name == "") return;
            foreach (StrengthClass sclass in StrengthClassList)
            {
                if (sclass.Name == name)
                {
                    SelectedStrengthClass = sclass;
                    break;
                }
            }
        }
        //Обновить список марок стали
        public void RefreshSteelMarkList()
        {
            (calc_list[0] as Calc_SliceAndCrumple).RefreshSteelMarkList();
        }
        //Обновить список Стандартов
        private void RefreshStandartList()
        {
            if (selected_connection_type != null)
            {
                if (ConnectionTypes.IndexOf(SelectedConnectionType) <= 1)
                {
                    StandartList.Clear();
                    StandartList.Add("ГОСТ 7798-70");
                    SelectedStandart = StandartList.First();
                }
            }
        }
        //Обновить список комплектов
        private void RefreshKitList()
        {
            if (SelectedConnectionType == null) return;
            KitList.Clear();
            string tempKitName = "";
            if (SelectedKit != null) tempKitName = SelectedKit.Name;
            switch (ConnectionTypes.IndexOf(SelectedConnectionType))
            {
                case 0 :
                    KitList.Add(new Kit(gost_base, "2Ш_2Г", true, true, false, 2, true));
                    KitList.Add(new Kit(gost_base, "Ш_Гр_Г", true, false, true, 1, true));
                    break;
                case 1:
                    KitList.Add(new Kit(gost_base, "2Ш_2Г", true, true, false, 2, false));
                    KitList.Add(new Kit(gost_base, "Ш_Гр_Г", true, false, true, 1, false));
                    break;
            }
            foreach (Kit kit in KitList)
            {
                if (kit.Name == tempKitName) SelectedKit = kit;
            }

        }




        //Обработать изменение результата расчета
        private void UpdateResultHandler(string rate)
        {
            UtilRate = rate;
        }

        //Обработать изменение длины
        private void UpdateLengthHandler(string length)
        {
            BoltLength = length;
        }

        //Код для поддержки MVVM:
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        //Код для поддержки копирования:
        public object Clone()
        {
            BoltUnit bu = (BoltUnit)this.MemberwiseClone();
            bu.BoltList = new ObservableCollection<Bolt>(this.BoltList);
            bu.StrengthClassList = new ObservableCollection<StrengthClass>(this.StrengthClassList);
            bu.StandartList = new ObservableCollection<string>(this.StandartList);
            bu.KitList = new ObservableCollection<Kit>(KitList);
            bu.calc_list = new List<ICalculation>();
            for (int i = 0; i < calc_list.Count; i++)
            {
                ICalculation calc = (ICalculation)calc_list[i].Clone();
                calc.ResultChangedEvent -= this.UpdateResultHandler;
                calc.ResultChangedEvent += bu.UpdateResultHandler;
                bu.calc_list.Add(calc);
            }
            bu.MyCalculation = bu.calc_list[calc_list.IndexOf(MyCalculation)];
            
            return bu;
        }
    }
}
