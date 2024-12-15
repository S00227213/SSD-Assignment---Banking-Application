using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SSD_Assignment___Banking_Application
{
    public static class InputValidator
    {
        /// <summary>
        /// Validates that a string is not empty or null.
        /// </summary>
        public static bool ValidateString(string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        /// <summary>
        /// Validates that a string contains only letters and spaces.
        /// </summary>
        public static bool ValidateName(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z\s]+$");
        }

        /// <summary>
        /// Validates that a string contains only alphanumeric characters, spaces, or punctuation.
        /// </summary>
        public static bool ValidateAddress(string address)
        {
            return Regex.IsMatch(address, @"^[a-zA-Z0-9\s,.-]+$");
        }

        /// <summary>
        /// Validates that a double is non-negative.
        /// </summary>
        public static bool ValidatePositiveNumber(double number)
        {
            return number >= 0;
        }
    }
}
