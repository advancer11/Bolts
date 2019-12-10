using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BoltCalc2
{
    class Calc_Tension : ICalculation, INotifyPropertyChanged
    {

        //Свойства
        public BoltUnit Unit { get; set; }
        private Bolt selected_bolt;
        public Bolt SelectedBolt
        {
            get { return selected_bolt; }
            set
            {
                selected_bolt = value;
                if (selected_bolt != null) AreaAbn = selected_bolt.Abn;
                MakeCalculation();
            }
        }
        private int bolt_quantity;
        public int BoltQuantity
        {
            get { return bolt_quantity; }
            set
            {
                if (value > 0)
                {
                    bolt_quantity = value;
                    MakeCalculation();
                }
            }
        }
        private StrengthClass selected_strength_class;
        public StrengthClass SelectedStrengthClass
        {
            get { return selected_strength_class; }
            set
            {
                selected_strength_class = value;
                if (selected_strength_class != null) R_bt = selected_strength_class.Rbt;
                MakeCalculation();
            }
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
        private double force_limit_for_tension;
        public double ForceLimitForTension
        {
            get { return force_limit_for_tension; }
            set { force_limit_for_tension = value; OnPropertyChanged("ForceLimitForTension"); }
        }
        private double util_rate_for_tension;
        public double UtilRateForTension
        {
            get { return util_rate_for_tension; }
            set { util_rate_for_tension = value; OnPropertyChanged("UtilRateForTension"); }
        }

        //Свойства зависимости
        private double area_abn;
        public double AreaAbn
        {
            get { return area_abn; }
            set { area_abn = value; OnPropertyChanged("AreaAbn"); }
        }
        private double r_bt;
        public double R_bt
        {
            get { return r_bt; }
            set { r_bt = value; OnPropertyChanged("R_bt"); }
        }
        public string util_rate;

        //Конструктор
        public Calc_Tension()
        {
            //Unit = unit;
            GammaC = 1;
        }


        //Произвести расчет
        public void MakeCalculation()
        {
            ForceLimitForTension = Math.Round(R_bt * AreaAbn * GammaC * BoltQuantity / 10, 2, MidpointRounding.AwayFromZero);
            if (ForceLimitForTension > 0 && Force > 0)
            {
                UtilRateForTension = Math.Round(Force / ForceLimitForTension, 3, MidpointRounding.AwayFromZero);
                util_rate = UtilRateForTension.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                UtilRateForTension = 0;
                util_rate = "";
            }
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
            return this.MemberwiseClone();
        }
    }
}
