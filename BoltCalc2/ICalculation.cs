using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoltCalc2
{
    interface ICalculation : ICloneable
    {
        //Свойства

        Bolt SelectedBolt { get; set; }
        StrengthClass SelectedStrengthClass { get; set; }
        double GammaC { get; set; }
        int BoltQuantity { get; set; }
        double Force { get; set; }
        BoltUnit Unit { get; set; }

        event Action<string> ResultChangedEvent;

        //Методы
        void MakeCalculation();

    }
}
