using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Practices.Mvvm.Converters
{
    /// <summary>
    /// Provides the ability to perform basic math on a value.
    /// <para>
    /// Use @VALUE in the parameter equation as a substitute for the value being converted.
    /// Order of operations is parenthesis first, then Left-To-Right (no operator precedence).
    /// </para>
    /// </summary>
    public class MathConverter : MarkupExtension, IValueConverter
    {
        private static readonly char[] AllOperators = { '+', '-', '*', '/', '%', '(', ')' };

        private static readonly List<string> Grouping = new List<string> { "(", ")" };
        private static readonly List<string> Operators = new List<string> { "+", "-", "*", "/", "%" };

        #region MarkupExtension Members

        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension. 
        /// </summary>
        /// <returns>
        /// The object value to set on the property where the extension is applied. 
        /// </returns>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        #endregion

        #region IValueConverter Members

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // Parse value into equation and remove spaces
                var mathEquation = parameter as string;
                mathEquation = mathEquation.Replace(" ", String.Empty);
                mathEquation = mathEquation.Replace("@VALUE", value.ToString());

                // Validate values and get list of numbers in equation
                var numbers = new List<double>();

                foreach (string s in mathEquation.Split(AllOperators))
                {
                    if (s != string.Empty)
                    {
                        double tmp;
                        if (double.TryParse(s, out tmp))
                        {
                            numbers.Add(tmp);
                        }
                        else
                        {
                            // Handle Error - Some non-numeric, operator, or grouping character found in string
                            throw new InvalidCastException();
                        }
                    }
                }

                // Begin parsing method
                EvaluateMathString(ref mathEquation, ref numbers, 0);

                // After parsing the numbers list should only have one value - the total
                return numbers[0];
            }
            catch (Exception ex)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Evaluates a mathematical string and keeps track of the results in a collection of numbers.
        /// </summary>
        /// <param name="mathEquation"></param>
        /// <param name="numbers"></param>
        /// <param name="index"></param>
        private void EvaluateMathString(ref string mathEquation, ref List<double> numbers, int index)
        {
            // Loop through each mathemtaical token in the equation
            string token = GetNextToken(mathEquation);

            while (token != string.Empty)
            {
                // Remove token from mathEquation
                mathEquation = mathEquation.Remove(0, token.Length);

                // If token is a grouping character, it affects program flow
                if (Grouping.Contains(token))
                {
                    switch (token)
                    {
                        case "(":
                            EvaluateMathString(ref mathEquation, ref numbers, index);
                            break;

                        case ")":
                            return;
                    }
                }

                // If token is an operator, do requested operation
                if (Operators.Contains(token))
                {
                    // If next token after operator is a parenthesis, call method recursively
                    string nextToken = GetNextToken(mathEquation);
                    if (nextToken == "(")
                    {
                        EvaluateMathString(ref mathEquation, ref numbers, index + 1);
                    }

                    // Verify that enough numbers exist in the List<double> to complete the operation
                    // and that the next token is either the number expected, or it was a ( meaning
                    // that this was called recursively and that the number changed
                    if (numbers.Count > (index + 1) &&
                        (double.Parse(nextToken) == numbers[index + 1] || nextToken == "("))
                    {
                        switch (token)
                        {
                            case "+":
                                numbers[index] = numbers[index] + numbers[index + 1];
                                break;
                            case "-":
                                numbers[index] = numbers[index] - numbers[index + 1];
                                break;
                            case "*":
                                numbers[index] = numbers[index] * numbers[index + 1];
                                break;
                            case "/":
                                numbers[index] = numbers[index] / numbers[index + 1];
                                break;
                            case "%":
                                numbers[index] = numbers[index] % numbers[index + 1];
                                break;
                        }
                        numbers.RemoveAt(index + 1);
                    }
                    else
                    {
                        // Handle Error - Next token is not the expected number
                        throw new FormatException("Next token is not the expected number");
                    }
                }

                token = GetNextToken(mathEquation);
            }
        }

        /// <summary>
        /// Gets the next mathematical token in the equation.
        /// </summary>
        /// <param name="mathEquation"></param>
        /// <returns></returns>
        private string GetNextToken(string mathEquation)
        {
            // If we're at the end of the equation, return string.empty
            if (mathEquation == string.Empty)
            {
                return string.Empty;
            }

            // Get next operator or numeric value in equation and return it
            string tmp = String.Empty;
            foreach (char c in mathEquation)
            {
                if (AllOperators.Contains(c))
                {
                    return (tmp == String.Empty ? c.ToString() : tmp);
                }

                tmp += c;
            }

            return tmp;
        }
    }
}