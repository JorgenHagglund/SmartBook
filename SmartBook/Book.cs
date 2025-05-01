using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook
{
    internal class Book
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? ISBN
        {
            get;
            set
            {
                if (ValidateISBN(value))
                    ISBN = value;
            }
        }
        public string Category { get; set; }

        public static bool ValidateISBN(string isbn)
        {
            // Validate the ISBN number
            if (string.IsNullOrEmpty(isbn))
            {
                throw new ArgumentException("ISBN cannot be null or empty");
            }
            string? test = isbn
                .Where(c => char.IsDigit(c) || c == 'X')
                .ToArray()
                .ToString();

            int sum, weight, digit, check, i;
            if (isbn.Length == 13)
            {
                sum = 0;
                for (i = 0; i < 12; i++)
                {
                    digit = int.Parse(isbn[i].ToString());
                    if (i % 2 == 0)
                        sum += digit;
                    else
                        sum += digit * 3;
                }
                check = (10 - (sum % 10)) % 10;
                return check == int.Parse(isbn[12].ToString().ToUpper());
            }
            else if (isbn.Length == 10)
            {
                weight = 10;
                sum = 0;
                for (i = 0; i < 9; i++)
                {
                    digit = int.Parse(isbn[i].ToString());
                    sum += digit * weight;
                    weight--;
                }
                check = (11 - (sum % 11) % 11);
                if (check == 10)
                    return isbn[9].ToString().ToUpper() == "X";
                else
                    return check == int.Parse(isbn[9].ToString());
            }
            return false;
        }
    }
}
