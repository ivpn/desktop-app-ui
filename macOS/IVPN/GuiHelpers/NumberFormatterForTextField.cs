using System;
using System.Linq;
using Foundation;

namespace IVPN
{
    /// <summary>
    /// Usage example:
    ///     GuiTextBlock.Formatter = new NSNumberFormatter();
    /// </summary>
    public class NumberFormatterForTextField : NSNumberFormatter
    {
        private int __MaxDigitsCount = 0;
        private int? __MaxValue = 0;

        public NumberFormatterForTextField(int maxDigitsCount = 0, int? maxValue = null)
        {
            __MaxDigitsCount = maxDigitsCount;
            __MaxValue = maxValue;
        }

        public override bool IsPartialStringValid(string partialString, out string newString, out NSString error)
        {
            newString = partialString;
            error = new NSString("");
            if (partialString.Length == 0)
                return true;

            bool isOK = true;
            if (__MaxDigitsCount > 0 && partialString.Length > __MaxDigitsCount)
            {
                partialString = partialString.Remove(0, partialString.Length - __MaxDigitsCount);
                newString = partialString;
                isOK = false;
            }

            // you could allow use partialString.All(c => c >= '0' && c <= '9') if internationalization is not a concern
            if (!partialString.All(char.IsDigit))
            {
                newString = new string(partialString.Where(char.IsDigit).ToArray());
                return false;
            }

            if (__MaxValue!=null)
            {
                int value;
                bool isValueChanged = false;
                try
                {
                    value = int.Parse(newString);
                    if (value > (int)__MaxValue)
                    {
                        value = (int)__MaxValue;
                        isValueChanged = true;
                    }
                }
                catch
                {
                    value = 0;
                    isValueChanged = true;
                }

                if (isValueChanged)
                {
                    newString = $"{value}";
                    return false;
                }
            }

            return isOK;
        }
    }
}
