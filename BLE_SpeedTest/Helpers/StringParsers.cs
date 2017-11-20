using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class StringParsers
    {

        /// <summary>
        /// Разбор строки во float
        /// </summary>
        /// <param name="data">Строка</param>
        /// <param name="result">Возвращаемый результат</param>
        /// <returns>если расшифровать невозможно, то возвращает false</returns>
        public static bool ToFloat(string data, out float result)
        {
            if (!float.TryParse(data.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = 0;
                return false;
            }
            return true;
        }
    }
}
