using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BoltCalc2
{

    class Kit : INotifyPropertyChanged
    {
        //Поля
        private DataSet gost_base;

        private int left_washers_count;
        private int right_washers_count;
        private int grover_count;
        private int index_length;
        private double min_protrusion;


        //Свойства
        public string Name { get; set; }
        private string bolt_name;
        public string BoltName
        {
            get { return bolt_name; }
            set
            {
                bolt_name = value;
                bolt_name = bolt_name.Replace("(", "");
                bolt_name = bolt_name.Replace(")", "");
                SetThicknessAndMass();
                SetBoltLength();
            }
        }               //Убрать скобки, если есть, установить толщины и массы гайк, шайб
        private double package;
        public double Package
        {
            get { return package; }
            set
            {
                package = value;
                SetBoltLength();
            }
        }                //Пересчитать длину болта
        public bool IsSlice { get; set; }




        public Visibility SliceVis = Visibility.Collapsed;
        public double NutWeight { get; set; }
        public double WasherWeight { get; set; }
        public double GroverWashersWeight { get; set; }

        public double Diameter { get; set; }
        public double TurnkeySize { get; set; }
        public double HeadHeight { get; set; }
        private double thread_pitch;
        public double ThreadPitch
        {
            get { return thread_pitch; }
            set
            {
                thread_pitch = value;
                min_protrusion = Math.Min(thread_pitch, 3);
            }
        }

        //Свойства зависимости

        private int count_nuts;
        public int CountNuts
        {
            get { return count_nuts; }
            set
            {
                count_nuts = value;
                OnPropertyChanged("CountNuts");
            }
        }
        private double nut_height;
        public double NutHeight
        {
            get { return nut_height; }
            set
            {
                nut_height = value;
                OnPropertyChanged("NutHeight");
            }
        }
        private double washer_height;
        public double WasherHeight
        {
            get { return washer_height; }
            set
            {
                washer_height = value;
                OnPropertyChanged("WasherHeight");
            }
        }
        private double grover_washers_height;
        public double GroverWashersHeight
        {
            get { return grover_washers_height; }
            set
            {
                grover_washers_height = value;
                OnPropertyChanged("GroverWashersHeight");
            }

        }

        public bool left_washer_existence;
        public bool LeftWashersExistence
        {
            get { return left_washer_existence; }
            set
            {
                left_washer_existence = value;
                OnPropertyChanged("LeftWashersExistence");

            }
        }
        public bool right_washer_existence;
        public bool RightWashersExistence
        {
            get { return right_washer_existence; }
            set
            {
                right_washer_existence = value;
                OnPropertyChanged("RightWashersExistence");
            }
        }
        private bool grover_existence;
        public bool GroverExistence
        {
            get { return grover_existence; }
            set
            {
                grover_existence = value;
                OnPropertyChanged("GroverExistence");
            }
        }

        private double bolt_length;
        public double BoltLength
        {
            get { return bolt_length; }
            set
            {
                bolt_length = value;
                OnPropertyChanged("BoltLength");
            }
        }
        private double thread_length;
        public double ThreadLength
        {
            get { return thread_length; }
            set
            {
                if (value == -1) thread_length = 0;
                else if (value == 0) thread_length = BoltLength;
                else thread_length = value;
                OnPropertyChanged("ThreadLength");
            }
        }         //Проверить на 0 и -1
        private double protrusion;
        public double Protrusion
        {
            get { return protrusion; }
            set
            {
                protrusion = value;
                OnPropertyChanged("Protrusion");
            }
        }

        private bool auto_length;
        public bool AutoLength
        {
            get { return auto_length; }
            set
            {
                auto_length = value;
                OnPropertyChanged("AutoLength");
                if (auto_length) SetBoltLength();
            }
        }


        //Конструктор
        public Kit(DataSet gost_base, string kit_name, bool left_washer_existence, bool right_washers_existence, bool grover_existence, int count_nuts, bool is_slice)
        {
            this.gost_base = gost_base;
            Name = kit_name;
            LeftWashersExistence = left_washer_existence;
            RightWashersExistence = right_washers_existence;
            GroverExistence = grover_existence;
            CountNuts = count_nuts;
            IsSlice = is_slice;

            if (LeftWashersExistence) left_washers_count = 1;
            else left_washers_count = 0;
            if (RightWashersExistence) right_washers_count = 1;
            else right_washers_count = 0;
            if (GroverExistence) grover_count = 1;
            else grover_count = 0;

            WasherHeight = 0;
            WasherWeight = 0;
            GroverWashersHeight = 0;
            GroverWashersWeight = 0;

            AutoLength = true;
        }

        //Назначить толщины и массы 
        private void SetThicknessAndMass()
        {
            foreach (DataRow dr in gost_base.Tables["Гайки"].Rows)
            {
                string diameter = TableReader.GetStringCell(dr, 0);
                if (diameter == bolt_name)
                {
                    NutHeight = TableReader.GetDoubleCell(dr, 1);
                    NutWeight = TableReader.GetDoubleCell(dr, 2);
                    break;
                }
            }
            if (left_washer_existence || right_washer_existence)
            {
                foreach (DataRow dr in gost_base.Tables["Шайбы"].Rows)
                {
                    string diameter = TableReader.GetStringCell(dr, 0);
                    if (diameter == bolt_name)
                    {
                        WasherHeight = TableReader.GetDoubleCell(dr, 1);
                        WasherWeight = TableReader.GetDoubleCell(dr, 2);
                        break;
                    }
                }
            }
            if (grover_existence)
            {
                foreach (DataRow dr in gost_base.Tables["Шайбы пружинные"].Rows)
                {
                    string diameter = TableReader.GetStringCell(dr, 0);
                    if (diameter == bolt_name)
                    {
                        GroverWashersHeight = TableReader.GetDoubleCell(dr, 1);
                        GroverWashersWeight = TableReader.GetDoubleCell(dr, 2);
                        break;
                    }
                }
            }
            foreach (DataRow dr in gost_base.Tables["Конструктивные параметры болтов"].Rows)
            {
                string diameter = TableReader.GetStringCell(dr, 0);
                if (diameter == bolt_name)
                {
                    Diameter = TableReader.GetDoubleCell(dr, "Diameter");
                    TurnkeySize = TableReader.GetDoubleCell(dr, "TurnkeySize");
                    HeadHeight = TableReader.GetDoubleCell(dr, "HeadHeight");
                    ThreadPitch = TableReader.GetDoubleCell(dr, "ThreadPitch");
                    break;
                }
            }
        }


        //Подобрать длину из условия общей толщины (или проверить длину, если не авто)
        private void SetBoltLength()
        {
            if (AutoLength)
            {
                DataTable dt = gost_base.Tables["Длина резьбы"];
                double length;
                string result = "";
                BoltLength = 0;
                ThreadLength = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    length = TableReader.GetDoubleCell(dt.Rows[i], 0);
                    if (length >= left_washers_count * WasherHeight + Package + right_washers_count * WasherHeight + grover_count * GroverWashersHeight + CountNuts * NutHeight + min_protrusion)
                    {
                        BoltLength = length;
                        ThreadLength = TableReader.GetDoubleCell(dt.Rows[i], BoltName);
                        index_length = i;
                        result = BoltLength.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    }
                }
                if (ResultChangedEvent != null) ResultChangedEvent(result);
            }
            else
            {
                BoltLength = TableReader.GetDoubleCell(gost_base.Tables["Длина резьбы"].Rows[index_length], 0);
                ThreadLength = TableReader.GetDoubleCell(gost_base.Tables["Длина резьбы"].Rows[index_length], BoltName);
                CheckBoltLengths();
            }
        }

        //Увеличить, уменьшить длину болта вручную
        public void UpLengthManually()
        {
            if (AutoLength) return;
            if (index_length + 1 >= gost_base.Tables["Длина резьбы"].Rows.Count) return;
            index_length++;
            BoltLength = TableReader.GetDoubleCell(gost_base.Tables["Длина резьбы"].Rows[index_length], 0);
            ThreadLength = TableReader.GetDoubleCell(gost_base.Tables["Длина резьбы"].Rows[index_length], BoltName);
            if (ResultChangedEvent != null) ResultChangedEvent(BoltLength.ToString(System.Globalization.CultureInfo.InvariantCulture));
            CheckBoltLengths();
        }
        public void DownLengthManually()
        {
            if (AutoLength) return;
            if (index_length == 0) return;
            index_length--;
            BoltLength = TableReader.GetDoubleCell(gost_base.Tables["Длина резьбы"].Rows[index_length], 0);
            ThreadLength = TableReader.GetDoubleCell(gost_base.Tables["Длина резьбы"].Rows[index_length], BoltName);
            if (ResultChangedEvent != null) ResultChangedEvent(BoltLength.ToString(System.Globalization.CultureInfo.InvariantCulture));
            CheckBoltLengths();
        }

        //Проверить длину болта
        private void CheckBoltLengths()
        {
            if (BoltLength < left_washers_count * WasherHeight + Package + right_washers_count * WasherHeight + grover_count * GroverWashersHeight + CountNuts * NutHeight + min_protrusion)
            {

            }
        }


        //Событие
        public event Action<string> ResultChangedEvent;

        //Код для поддержки MVVM:
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
