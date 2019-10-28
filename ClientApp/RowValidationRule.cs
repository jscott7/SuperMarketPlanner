using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

[assembly: InternalsVisibleTo("SuperMarketPlannerUnitTests")]
namespace SuperMarketPlanner
{
    public class RowValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            var element = (value as BindingGroup).Items[0] as XmlElement;

            string itemValue = element.GetAttribute("name");
            
            if (string.IsNullOrEmpty(itemValue))
            {
                itemValue = element.InnerText;
            }

            if (!IsValidXmlString(itemValue))
            {
                MessageBox.Show("Invalid Character", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return new ValidationResult(false, "Invalid XML Characters");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }

        internal static bool IsValidXmlString(string text)
        {
            IEnumerable<char> invalidCharQuery =
               from c in text
               where (c == '<' || c == '>' || c == '&' || c == '/' || c == '"')
               select c;

            if (invalidCharQuery.Count() > 0)
            {
                return false;
            }

            return true;
        }
      
    }
}
