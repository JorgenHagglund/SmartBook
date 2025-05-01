namespace SmartBook.Extensions
{
    public static class Extensions
    {
        public static string NormalizeISBNForStorage(this string? str)
        {
            if (string.IsNullOrEmpty(str)) 
                return string.Empty;
            var intermediate = str
                .Where(c => char.IsDigit(c) || c == 'X');
            string? result = new string(intermediate.ToArray());
            return (result ?? string.Empty).ToUpper();  
        }

        public static string NormalizeISBNForDisplay(this string? str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            string result = str.NormalizeISBNForStorage();
            if (result.Length == 10)
                return $"{result.Substring(0, 1)}-{result.Substring(1, 3)}-{result.Substring(4, 5)}-{result.Substring(9, 1)}";
            else if (result.Length == 13)
                return $"{result.Substring(0, 3)}-{result.Substring(3, 1)}-{result.Substring(4, 4)}-{result.Substring(8, 4)}-{result.Substring(12, 1)}";
            else
                return result;
        }
    }
}
