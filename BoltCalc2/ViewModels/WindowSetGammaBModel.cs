using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace BoltCalc2
{
    class WindowSetGammaBModel : INotifyPropertyChanged
    {
        //Поля
        private double diameter_bolt;
        private bool is_multi_bolt;
        private bool is_high_strength_bolt;
        private string tb_multi_bolt;
        private string tb_high_strength_bolt;
        private double r_yn;
        private double d;
        private double a;
        private double s;
        private FlowDocument doc_gamma_b;

        //Свойства
        public double DiameterBolt
        {
            get { return diameter_bolt; }
            set { diameter_bolt = value; OnPropertyChanged(); }
        }
        public bool IsMultiBolt
        {
            get { return is_multi_bolt; }
            set
            {
                is_multi_bolt = value;
                if (is_multi_bolt) TbMultiBolt = "Да";
                else TbMultiBolt = "Нет";
                OnPropertyChanged("IsMultiBolt");
            }
        }
        public bool IsHighStrengthBolt
        {
            get { return is_high_strength_bolt; }
            set
            {
                is_high_strength_bolt = value;
                if (is_high_strength_bolt) TbHighStrengthBolt = "Да";
                else TbHighStrengthBolt = "Нет";
                OnPropertyChanged("IsHighStrengthBolt");
            }
        }
        public string TbMultiBolt
        {
            get { return tb_multi_bolt; }
            set { tb_multi_bolt = value; OnPropertyChanged("TbMultiBolt"); }
        }
        public string TbHighStrengthBolt
        {
            get { return tb_high_strength_bolt; }
            set { tb_high_strength_bolt = value; OnPropertyChanged("TbHighStrengthBolt"); }
        }
        public string PrecisionClass { get; set; }
        public double R_yn
        {
            get { return r_yn; }
            set { r_yn = value; OnPropertyChanged(); }
        }
        public double D
        {
            get { return d; }
            set
            {
                d = value;
                if (R_yn == 0) return;
                IsApplicable = true;
                if (DocGammaB != null) ShowReport();
                switch (PrecisionClass)
                {
                    case "A":
                        if (d != DiameterBolt)
                        {
                            DocGammaB.Blocks.Clear();
                            DocGammaB.Blocks.Add(new Paragraph(new Run("При классе точности \"А\" диаметр отверстия должен быть равен диаметру болта.") { Foreground = Brushes.Red }));
                            IsApplicable = false;
                        }
                        break;
                    case "B":
                        if (d < DiameterBolt + 1 || d > DiameterBolt + 3)
                        {
                            DocGammaB.Blocks.Clear();
                            DocGammaB.Blocks.Add(new Paragraph(new Run("При классе точности \"B\" диаметр отверстия должен быть больше диаметра болта на 1, 2 или 3 мм.") { Foreground = Brushes.Red }));
                            IsApplicable = false;
                        }
                        break;
                }

            }
        }
        public double A
        {
            get { return a; }
            set
            {
                a = value;
                if (R_yn == 0) return;
                if (DocGammaB != null) ShowReport();
            }
        }
        public double S
        {
            get { return s; }
            set
            {
                s = value;
                if (R_yn == 0) return;
                if (DocGammaB != null) ShowReport();
            }
        }
        public FlowDocument DocGammaB
        {
            get { return doc_gamma_b; }
            set { doc_gamma_b = value; OnPropertyChanged(); }
        }
        public double GammaBs;
        public double GammaBm;
        public Calc_SliceAndCrumple Calc { get; set; }

        private bool is_applicable = true;
        public bool IsApplicable
        {
            get { return is_applicable; }
            set { is_applicable = value; OnPropertyChanged("IsApplicable"); }
        }



        //Конструктор
        public WindowSetGammaBModel(Calc_SliceAndCrumple selected_calc)
        {
            Calc = selected_calc;
            DocGammaB = new FlowDocument();
            DocGammaB.PagePadding = new Thickness(10, 0, 10, 0);
            DocGammaB.FontSize = 14;
            DocGammaB.FontFamily = new FontFamily("Times New Roman");
            DiameterBolt = Calc.SelectedBolt.Diameter;
            int bolt_quantity = Calc.BoltQuantity;
            if (bolt_quantity > 1) IsMultiBolt = true;
            else IsMultiBolt = false;
            PrecisionClass = Calc.SelectedPrecisionClass;
            IsHighStrengthBolt = Calc.SelectedStrengthClass.IsHighStrength;
            R_yn = Calc.SelectedSteelMark.Ryn;
            if (R_yn == 0)
            {
                DocGammaB.Blocks.Add(new Paragraph(new Run("Не найден предел текучести стали Ryn для указанной марки стали и толщины детали") { Foreground = Brushes.Red }));
                GammaBm = 0;
                IsApplicable = false;
            }
            D = Calc.HoleDiameter;
            A = Calc.DistanceA;
            S = Calc.DistanceS;
        }

        //Отобразть расчет Гаама b
        public void ShowReport()
        {
            IsApplicable = true;
            DocGammaB.Blocks.Clear();
            Paragraph pr = new Paragraph();
            pr.Margin = new Thickness(0);
            pr.Inlines.Add(new Run("Коэффициент условий работы болтового соединения (принимается не более 1):"));
            Paragraph pr_gbs = new Paragraph();
            pr_gbs.Margin = new Thickness(0);
            pr_gbs.Inlines.Add(new Run(" - при расчете на срез:  "));
            Paragraph pr_gbm = new Paragraph();
            pr_gbm.Margin = new Thickness(0);
            pr_gbm.Inlines.Add(new Run(" - при расчете на смятие:  "));
            pr_gbs.Inlines.Add(new Run("γ") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
            pr_gbs.Inlines.Add(new Run("b") { FontSize = DocGammaB.FontSize - 4, FontStyle = FontStyles.Italic, BaselineAlignment = BaselineAlignment.Subscript });
            pr_gbs.Inlines.Add(new Run(" = "));
            pr_gbm.Inlines.Add(new Run("γ") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
            pr_gbm.Inlines.Add(new Run("b") { FontSize = DocGammaB.FontSize - 4, FontStyle = FontStyles.Italic, BaselineAlignment = BaselineAlignment.Subscript });
            pr_gbm.Inlines.Add(new Run(" = "));
            if (!IsMultiBolt)
            {
                GammaBs = 1;
                pr_gbs.Inlines.Add(new Run(GammaBs.ToString()));
                if (R_yn <= 285)
                {
                    if (A / D >= 2)
                    {
                        GammaBm = 1;
                        pr_gbm.Inlines.Add(new Run(GammaBs.ToString()));
                    }
                    else if (A / D >= 1.5)
                    {
                        pr_gbm.Inlines.Add(new Run("0,4"));
                        pr_gbm.Inlines.Add(new Run("a/d") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
                        pr_gbm.Inlines.Add(new Run(" + 0,2 = "));
                        GammaBm = Math.Round(0.4 * A / D + 0.2, 2, MidpointRounding.AwayFromZero);
                        pr_gbm.Inlines.Add(new Run(GammaBm.ToString()));
                    }
                    else if (A / D >= 1.35)
                    {
                        pr_gbm.Inlines.Add(new Run("a/d") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
                        pr_gbm.Inlines.Add(new Run(" - 0,7 = "));
                        GammaBm = Math.Round(A / D - 0.7, 2, MidpointRounding.AwayFromZero);
                        pr_gbm.Inlines.Add(new Run(GammaBm.ToString()));
                    }
                    else
                    {
                        GammaBm = 0;
                        pr_gbm.Inlines.Add(new Run("[Слишком малое значение a. Допустимо не меньше чем 1.35d]") { Foreground = Brushes.Red });
                        IsApplicable = false;
                    }
                }
                else if (R_yn <= 375)
                {
                    if (A / D >= 2)
                    {
                        GammaBm = 1;
                        pr_gbm.Inlines.Add(new Run(GammaBs.ToString()));
                    }
                    else if (A / D >= 1.5)
                    {
                        pr_gbm.Inlines.Add(new Run("0,5"));
                        pr_gbm.Inlines.Add(new Run("a/d") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
                        pr_gbm.Inlines.Add(new Run(" = "));
                        GammaBm = Math.Round(0.5 * A / D, 2, MidpointRounding.AwayFromZero);
                        pr_gbm.Inlines.Add(new Run(GammaBm.ToString()));
                    }
                    else if (A / D >= 1.35)
                    {
                        pr_gbm.Inlines.Add(new Run("0,67"));
                        pr_gbm.Inlines.Add(new Run("a/d") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
                        pr_gbm.Inlines.Add(new Run(" - 0,25 = "));
                        GammaBm = Math.Round(0.67 * A / D - 0.25, 2, MidpointRounding.AwayFromZero);
                        pr_gbm.Inlines.Add(new Run(GammaBm.ToString()));
                    }
                    else
                    {
                        GammaBm = 0;
                        pr_gbm.Inlines.Add(new Run("[Слишком малое значение a. Допустимо не меньше чем 1.35d]") { Foreground = Brushes.Red });
                        IsApplicable = false;
                    }
                }
                else
                {
                    if (A / D >= 2.5)
                    {
                        GammaBm = 1;
                        pr_gbm.Inlines.Add(new Run(GammaBs.ToString()));
                    }
                    else
                    {
                        GammaBm = 0;
                        pr_gbm.Inlines.Add(new Run("[Слишком малое значение a. Допустимо не меньше чем 2.5d (при Ryn > 375 Н/мм2)]") { Foreground = Brushes.Red });
                        IsApplicable = false;
                    }
                }
            }
            else
            {
                if (PrecisionClass == "A") GammaBs = 1;
                else GammaBs = 0.9;
                pr_gbs.Inlines.Add(new Run(GammaBs.ToString()));
                if (R_yn <= 285)
                {
                    if (A / D < 1.5 || S / D < 2)
                    {
                        GammaBm = 0;
                        pr_gbm.Inlines.Add(new Run("[Слишком малое значение a или s. Допустимо a >= 1.5d, s >= 2d]") { Foreground = Brushes.Red });
                        IsApplicable = false;
                    }
                    else if (A / D < 2 || S / D < 2.5)
                    {
                        if (0.4 * A / D + 0.2 <= 0.4 * S / D)
                        {
                            if (PrecisionClass == "A")
                            {
                                pr_gbm.Inlines.Add(new Run("0,4"));
                                pr_gbm.Inlines.Add(new Run("a/d") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
                                pr_gbm.Inlines.Add(new Run(" + 0,2 = "));
                                GammaBm = Math.Round(0.4 * A / D + 0.2, 2, MidpointRounding.AwayFromZero);
                                pr_gbm.Inlines.Add(new Run(GammaBm.ToString()));
                            }
                            else
                            {
                                pr_gbm.Inlines.Add(new Run("(0,4"));
                                pr_gbm.Inlines.Add(new Run("a/d") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
                                pr_gbm.Inlines.Add(new Run(" + 0,2) ∙ 0.9 = "));
                                GammaBm = Math.Round((0.4 * A / D + 0.2) * 0.9, 2, MidpointRounding.AwayFromZero);
                                pr_gbm.Inlines.Add(new Run(GammaBm.ToString()));
                            }
                        }
                        else
                        {
                            if (PrecisionClass == "A")
                            {
                                pr_gbm.Inlines.Add(new Run("0,4"));
                                pr_gbm.Inlines.Add(new Run("s/d") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
                                pr_gbm.Inlines.Add(new Run(" = "));
                                GammaBm = Math.Round(0.4 * S / D, 2, MidpointRounding.AwayFromZero);
                                pr_gbm.Inlines.Add(new Run(GammaBm.ToString()));
                            }
                            else
                            {
                                pr_gbm.Inlines.Add(new Run("0,4"));
                                pr_gbm.Inlines.Add(new Run("s/d") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
                                pr_gbm.Inlines.Add(new Run(" ∙ 0.9 = "));
                                GammaBm = Math.Round(0.4 * S / D * 0.9, 2, MidpointRounding.AwayFromZero);
                                pr_gbm.Inlines.Add(new Run(GammaBm.ToString()));
                            }
                        }
                    }
                    else
                    {
                        if (PrecisionClass == "A") GammaBm = 1;
                        else GammaBm = 0.9;
                        pr_gbm.Inlines.Add(new Run(GammaBs.ToString()));

                    }
                }
                else if (R_yn <= 375)
                {
                    if (A / D < 1.5 || S / D < 2)
                    {
                        GammaBm = 0;
                        pr_gbm.Inlines.Add(new Run("[Слишком малое значение a или s. Допустимо a >= 1.5d, s >= 2d]") { Foreground = Brushes.Red });
                        IsApplicable = false;
                    }
                    else if (A / D <= 2 || S / D < 2.5)
                    {
                        if (0.4 * A / D + 0.2 <= 0.4 * S / D)
                        {
                            if (PrecisionClass == "A")
                            {
                                pr_gbm.Inlines.Add(new Run("0,5"));
                                pr_gbm.Inlines.Add(new Run("a/d") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
                                pr_gbm.Inlines.Add(new Run(" = "));
                                GammaBm = Math.Round(0.5 * A / D, 2, MidpointRounding.AwayFromZero);
                                pr_gbm.Inlines.Add(new Run(GammaBm.ToString()));
                            }
                            else
                            {
                                pr_gbm.Inlines.Add(new Run("0,5"));
                                pr_gbm.Inlines.Add(new Run("a/d") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
                                pr_gbm.Inlines.Add(new Run(" ∙ 0.9 = "));
                                GammaBm = Math.Round(0.5 * A / D * 0.9, 2, MidpointRounding.AwayFromZero);
                                pr_gbm.Inlines.Add(new Run(GammaBm.ToString()));
                            }
                        }
                        else
                        {
                            if (PrecisionClass == "A")
                            {
                                pr_gbm.Inlines.Add(new Run("0,5"));
                                pr_gbm.Inlines.Add(new Run("s/d") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
                                pr_gbm.Inlines.Add(new Run(" - 0.25 = "));
                                GammaBm = Math.Round(0.5 * S / D - 0.25, 2, MidpointRounding.AwayFromZero);
                                pr_gbm.Inlines.Add(new Run(GammaBm.ToString()));
                            }
                            else
                            {
                                pr_gbm.Inlines.Add(new Run("(0,5"));
                                pr_gbm.Inlines.Add(new Run("s/d") { FontSize = DocGammaB.FontSize + 2, FontStyle = FontStyles.Italic });
                                pr_gbm.Inlines.Add(new Run(" - 0.25) ∙ 0.9 = "));
                                GammaBm = Math.Round((0.5 * S / D - 0.25) * 0.9, 2, MidpointRounding.AwayFromZero);
                                pr_gbm.Inlines.Add(new Run(GammaBm.ToString()));
                            }
                        }
                    }
                    else
                    {
                        if (PrecisionClass == "A") GammaBm = 1;
                        else GammaBm = 0.9;
                        pr_gbm.Inlines.Add(new Run(GammaBs.ToString()));
                    }
                }
                else
                {
                    if (A / D < 2.5 || S / D < 3)
                    {
                        GammaBm = 0;
                        pr_gbm.Inlines.Add(new Run("[Слишком малое значение a или s. Допустимо a >= 2.5d, s >= 3d (при Ryn > 375 Н/мм2)]") { Foreground = Brushes.Red });
                        IsApplicable = false;
                    }
                    else
                    {
                        if (PrecisionClass == "A") GammaBm = 1;
                        else GammaBm = 0.9;
                        pr_gbm.Inlines.Add(new Run(GammaBs.ToString()));
                    }
                }
            }
            DocGammaB.Blocks.Add(pr);
            DocGammaB.Blocks.Add(pr_gbs);
            DocGammaB.Blocks.Add(pr_gbm);
            foreach (Paragraph paragraph in DocGammaB.Blocks)
            {
                paragraph.TextAlignment = TextAlignment.Left;
            }
        }

        //Сохранения данных
        public void DataSave()
        {
            //if (GammaBm == 0) return;
            Calc.HoleDiameter = D;
            Calc.DistanceA = A;
            Calc.DistanceS = S;
            Calc.GammaBs = GammaBs;
            Calc.GammaBm = GammaBm;
        }

        //Код для поддержки MVVM
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
