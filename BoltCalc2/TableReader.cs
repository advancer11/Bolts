using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoltCalc2
{
    static class TableReader
    {
        //Получить строковое значение из таблицы
        public static string GetStringCell(DataRow dr, string col_name)
        {
            string result;
            try { result = (string)dr[col_name]; }
            catch { result = ""; }
            return result;
        }
        public static string GetStringCell(DataRow dr, int index)
        {
            string result;
            try { result = (string)dr[index]; }
            catch { result = ""; }
            return result;
        }

        //Получить численное значение из таблицы
        public static double GetDoubleCell(DataRow dr, string col_name)
        {
            double result;
            try { result = (double)dr[col_name]; }
            catch { result = 0; }
            return result;
        }
        public static double GetDoubleCell(DataRow dr, int index)
        {
            double result;
            try { result = (double)dr[index]; }
            catch { result = 0; }
            return result;
        }

        //Получить булево значение из таблицы
        public static bool GetBoolCell(DataRow dr, string col_name)
        {
            bool result;
            try { result = (bool)dr[col_name]; }
            catch { result = false; }
            return result;
        }
        public static bool GetBoolCell(DataRow dr, int index)
        {
            bool result;
            try { result = (bool)dr[index]; }
            catch { result = false; }
            return result;
        }
    }
}
