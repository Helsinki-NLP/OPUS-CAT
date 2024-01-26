using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatMtEngine.UI
{
    internal class Validators
    {
    }

   

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class StringRangeAttribute : ValidationAttribute
    {
        private int min;
        private int max;

        public StringRangeAttribute(int min, int max)
        {
            this.min = min;
            this.max = max;
        }


        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }
            var numberString = ((String)value).Trim('_');
            if (String.IsNullOrEmpty(numberString))
            {
                return false;
            }

            int number;
            if (!int.TryParse(numberString,out number))
            {
                return false;
            }
            
            return number >= this.min && number <= this.max;
        }

       
    }
}


