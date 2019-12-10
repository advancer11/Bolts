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
    class Calc_SliceAndCrumple : ICalculation, INotifyPropertyChanged
    {
        //Поля
        private DataSet my_base;



        //Свойства
        public ObservableCollection<SteelMark> SteelMarkList { get; set; }
        public ObservableCollection<string> PrecisionClasses { get; set; }
        public BoltUnit Unit { get; set; }
        private Bolt selected_bolt;
        public Bolt SelectedBolt
        {
            get { return selected_bolt; }
            set
            {
                selected_bolt = value;
                if (selected_bolt != null) AreaAb = selected_bolt.Ab;
                MakeCalculation();
            }
        }
        private StrengthClass selected_strength_class;
        public StrengthClass SelectedStrengthClass
        {
            get { return selected_strength_class; }
            set
            {
                selected_strength_class = value;
                if(selected_strength_class != null) R_bs = selected_strength_class.Rbs;
                RefreshPrecisionClasses();
                MakeCalculation();
            }
        }   //Установить Rbs, Обновить список классов точности
        private int bolt_quantity;
        public int BoltQuantity
        {
            get { return bolt_quantity; }
            set
            {
                if (value > 0)
                {
                    if (bolt_quantity == 1 && value > 1 && SelectedPrecisionClass != "A")
                    {
                        GammaBs = GammaBm = 0.9;
                    }
                    else if (bolt_quantity > 1 && value == 1 && SelectedPrecisionClass != "A")
                    {
                        GammaBs = GammaBm = 1;
                    }
                    bolt_quantity = value;
                    SetMinDAS();
                    MakeCalculation();
                }
            }
        }                      //Сбросить Гамма с, Гамма b, DAS
        public double HoleDiameter { get; set; }
        public double DistanceA { get; set; }
        public double DistanceS { get; set; }
        private double force;
        public double Force
        {
            get { return force; }
            set
            {
                force = value;
                MakeCalculation();
            }
        }

        //Свойства зависимости
        private int quantity_slices;
        public int QuantitySlices
        {
            get { return quantity_slices; }
            set
            {
                if (value < 1) return;
                quantity_slices = value;
                OnPropertyChanged("QuantitySlices");
                MakeCalculation();
            }
        }                //Нельзя установить меньше 1
        private SteelMark selected_steel_mark;
        public SteelMark SelectedSteelMark
        {
            get { return selected_steel_mark; }
            set
            {
                selected_steel_mark = value;
                OnPropertyChanged("SelectedSteelMark");
                SetSteelCharact();
                SetRbp();
                SetMinDAS();
                MakeCalculation();
            }
        }       //Установить характ-ки стали, Rbp, DAS, 
        private string selected_precision_class;
        public string SelectedPrecisionClass
        {
            get { return selected_precision_class; }
            set
            {
                selected_precision_class = value;
                OnPropertyChanged("SelectedPrecisionClass");
                SetRbp();
                SetMinDAS();
                if (BoltQuantity > 1 && selected_precision_class != "A") GammaBs = GammaBm = 0.9;
                else GammaBs = GammaBm = 1;
            }
        }     //Установить Rbp, DAS, сбросить Гамма b (в зависимости от кол-ва и класса болтов)
        private double area_ab;
        public double AreaAb
        {
            get { return area_ab; }
            set { area_ab = value; OnPropertyChanged("AreaAb"); }
        }
        private double r_bs;
        public double R_bs
        {
            get { return r_bs; }
            set { r_bs = value; OnPropertyChanged("R_bs"); }
        }
        private double detail_thickness;
        public double DetailThickness
        {
            get { return detail_thickness; }
            set
            {
                if (value <= 0) return;
                detail_thickness = Math.Round(value, 1, MidpointRounding.AwayFromZero);
                OnPropertyChanged("DetailThickness");
                SetSteelCharact();
                SetRbp();
                MakeCalculation();
            }
        }            //Должно быть больше 0, округлить до 0.1, Установаить хар-ки стали, Rbp
        private double r_bp;
        public double R_bp
        {
            get { return r_bp; }
            set { r_bp = value; OnPropertyChanged("R_bp"); }
        }

        private double gamma_c;
        public double GammaC
        {
            get { return gamma_c; }
            set
            {
                gamma_c = value;
                OnPropertyChanged("GammaC");
                MakeCalculation();
            }
        }
        private double gamma_bs;
        public double GammaBs
        {
            get { return gamma_bs; }
            set
            {
                gamma_bs = value;
                OnPropertyChanged("GammaBs");
                MakeCalculation();
            }
        }
        private double gamma_bm;
        public double GammaBm
        {
            get { return gamma_bm; }
            set
            {
                gamma_bm = value;
                OnPropertyChanged("GammaBm");
                MakeCalculation();
            }
        }

        private double force_limit_for_slice;
        public double ForceLimitForSlice
        {
            get { return force_limit_for_slice; }
            set { force_limit_for_slice = value; OnPropertyChanged("ForceLimitForSlice"); }
        }
        private double util_rate_for_slice;
        public double UtilRateForSlice
        {
            get { return util_rate_for_slice; }
            set { util_rate_for_slice = value; OnPropertyChanged("UtilRateForSlice"); }
        }
        private double force_limit_for_crumple;
        public double ForceLimitForCrumple
        {
            get { return force_limit_for_crumple; }
            set { force_limit_for_crumple = value; OnPropertyChanged("ForceLimitForCrumple"); }
        }
        private double util_rate_for_crumple;
        public double UtilRateForCrumple
        {
            get { return util_rate_for_crumple; }
            set { util_rate_for_crumple = value; OnPropertyChanged("UtilRateForCrumple"); }
        }




        //Конструктор
        public Calc_SliceAndCrumple(DataSet my_base)
        {
            this.my_base = my_base;
            //Unit = unit;
            QuantitySlices = 1;
            SteelMarkList = new ObservableCollection<SteelMark>();
            RefreshSteelMarkList();
            PrecisionClasses = new ObservableCollection<string>();
            GammaC = 1;
        }



        //Обновить список марок стали
        public void RefreshSteelMarkList()
        {
            string name;
            name = SelectedSteelMark == null ? "" : SelectedSteelMark.Name;
            SteelMarkList.Clear();
            SteelMark sm = new SteelMark();
            foreach (DataRow dr in my_base.Tables["Нормативные сопротивления при растяжении, сжатии и изгибе фасонного проката"].Rows)
            {
                if (sm.Name == TableReader.GetStringCell(dr, 0)) continue;
                sm = new SteelMark();
                sm.Name = TableReader.GetStringCell(dr, 0);
                SteelMarkList.Add(sm);
            }
            if (name == "") return;
            foreach (SteelMark smark in SteelMarkList)
            {
                if (smark.Name == name)
                {
                    SelectedSteelMark = smark;
                    break;
                }
            }
        }

        //Обновить список классов точности
        private void RefreshPrecisionClasses()
        {
            if (SelectedStrengthClass == null) return;
            if (!SelectedStrengthClass.IsHighStrength)
            {
                PrecisionClasses.Clear();
                PrecisionClasses.Add("A");
                PrecisionClasses.Add("B");
                SelectedPrecisionClass = PrecisionClasses[1];
            }
            else
            {
                PrecisionClasses.Clear();
                PrecisionClasses.Add("Нет");
                SelectedPrecisionClass = PrecisionClasses[0];
            }
        }

        //Выбрать характеристики стали
        public void SetSteelCharact()
        {
            foreach (DataRow dr in my_base.Tables[2].Rows)
            {
                if (SelectedSteelMark == null) return;
                if (SelectedSteelMark.Name == TableReader.GetStringCell(dr, 0) && DetailThickness >= TableReader.GetDoubleCell(dr, 1) && DetailThickness <= TableReader.GetDoubleCell(dr, 2))
                {
                    SelectedSteelMark.Ryn = TableReader.GetDoubleCell(dr, 3);
                    SelectedSteelMark.Run = TableReader.GetDoubleCell(dr, 4);
                    break;
                }
                else
                {
                    SelectedSteelMark.Ryn = 0;
                    SelectedSteelMark.Run = 0;
                }
            }
        }

        //Установить Rbp
        public void SetRbp()
        {
            if (SelectedSteelMark == null) return;
            foreach (DataRow dr in my_base.Tables[3].Rows)
            {
                if (SelectedSteelMark.Run == TableReader.GetDoubleCell(dr, 0))
                {
                    if (SelectedPrecisionClass == "A") R_bp = TableReader.GetDoubleCell(dr, 1);
                    else R_bp = TableReader.GetDoubleCell(dr, 2);
                    return;
                }
                else R_bp = 0;
            }
        }

        //Установить значения D A S
        public void SetMinDAS()
        {
            if (SelectedBolt == null || SelectedSteelMark == null) return;
            if (SelectedPrecisionClass == "A") HoleDiameter = SelectedBolt.Diameter;
            else HoleDiameter = SelectedBolt.Diameter + 3;
            if (SelectedSteelMark.Ryn <= 375)
            {
                DistanceA = Math.Round(HoleDiameter * 2, 0, MidpointRounding.AwayFromZero);
                DistanceS = Math.Round(HoleDiameter * 2.5, 0, MidpointRounding.AwayFromZero);
            }
            else
            {
                DistanceA = Math.Round(HoleDiameter * 2.5, 0, MidpointRounding.AwayFromZero);
                DistanceS = Math.Round(HoleDiameter * 3, 0, MidpointRounding.AwayFromZero);
            }
            if (BoltQuantity == 1) DistanceS = 0;
        }


        //Произвести расчет на срез и смятие
        public void MakeCalculation()
        {
            if (SelectedBolt == null) return;
            ForceLimitForSlice = Math.Round(R_bs * AreaAb * QuantitySlices * GammaBs * GammaC * BoltQuantity / 10, 2, MidpointRounding.AwayFromZero);
            if (ForceLimitForSlice > 0)
            {
                UtilRateForSlice = Math.Round(Force / ForceLimitForSlice, 3, MidpointRounding.AwayFromZero);
            }
            else UtilRateForSlice = 0;
            if (SelectedSteelMark != null)
            {
                ForceLimitForCrumple = Math.Round(R_bp * SelectedBolt.Diameter * DetailThickness * GammaBm * GammaC * BoltQuantity / 1000, 2, MidpointRounding.AwayFromZero);
                if (ForceLimitForCrumple > 0)
                {
                    UtilRateForCrumple = Math.Round(Force / ForceLimitForCrumple, 3, MidpointRounding.AwayFromZero);
                }
                else UtilRateForCrumple = 0;
            }
            string util_rate;
            if (UtilRateForSlice > 0 && UtilRateForCrumple > 0)
            {
                util_rate = Math.Max(UtilRateForSlice, UtilRateForCrumple).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else util_rate = "";
            //Unit.UtilRate = util_rate;
            if (ResultChangedEvent != null) ResultChangedEvent(util_rate);
        }



        public event Action<string> ResultChangedEvent;
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
            /*
            Calc_SliceAndCrumple calc = new Calc_SliceAndCrumple(my_base);
            calc.SteelMarkList = new ObservableCollection<SteelMark>(SteelMarkList);
            calc.PrecisionClasses = new ObservableCollection<string>(PrecisionClasses);
            */
            return this.MemberwiseClone();
        }
    }
}
