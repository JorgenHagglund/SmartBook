using SmartBook.Extensions;
using System;
using System.Diagnostics;
using SC = System.Console;

namespace SmartBook.Console
{
    /// <summary>
    /// A class to handle all console operations.
    /// </summary>
    public static class Console
    {
        public static void ClearToEndOfScreen()
        {
            /*int currentLine = SC.CursorTop;
            int currentColumn = SC.CursorLeft;
            int linesToClear = SC.WindowHeight - currentLine - 1;
            for (int i = 0; i < linesToClear; i++)
                SC.WriteLine(new string(' ', SC.WindowWidth - 1));
            SC.SetCursorPosition(currentColumn, currentLine);*/
            SC.Write("\x1B[0J");
        }

        /// <summary>
        /// Displays a welcome message after clearing the console.
        /// </summary>
        public static void DisplayWelcome()
        {
            SC.Clear();
            SC.WriteLine("Welcome to SmartBook!");
            SC.WriteLine("=====================");
        }

        public static void DisplayWarning(string message)
        {
            SC.Beep();
            SC.ForegroundColor = ConsoleColor.DarkYellow;
            SC.WriteLine($"Warning: {message}");
            SC.ResetColor();

            Thread.Sleep(2000);
        }

        public static void DisplayBooks(string prompt, IEnumerable<Book> books)
        {
            SC.WriteLine($"{Environment.NewLine}{prompt}:");
            SC.WriteLine(new string('=', prompt.Length));
            if (books.Count() == 0)
            {
                SC.WriteLine("There are no books to list");
                Thread.Sleep(2000);
                return;
            }
            int[] columnWidths =
            {
                books.Max(b => b.Title.Length),
                books.Max(b => b.Author.Length),
                books.Max(b => b.Status.ToString().Length),
                books.Max(b => b.ISBN.NormalizeISBNForDisplay().Length)
            };
            int totalWidth = columnWidths.Sum() + 2 * columnWidths.Length - 1 + 15;
            SC.WriteLine($"{"Title".PadRight(columnWidths[0])} | {"Author".PadRight(columnWidths[1])} | {"Status".PadRight(columnWidths[2])} | ISBN");
            SC.WriteLine(new string('-', totalWidth));
            foreach (var book in books)
                SC.WriteLine($"{book.Title.PadRight(columnWidths[0])} | {book.Author.PadRight(columnWidths[1])} | {book.Status.ToString().PadRight(columnWidths[2])} | {book.ISBN.NormalizeISBNForDisplay()}");

            Thread.Sleep(2000); 
        }

        private static OrderedDictionary<string, OrderedDictionary<String, Action?>>? _menu = null;

        public static void ExecuteMenu(OrderedDictionary<string, OrderedDictionary<String, Action?>>? menu = null, uint level = 0)
        {
            if (menu != null)
                _menu = menu;

            var it = _menu.GetEnumerator();
            int i = 0;
            do
            {
                it.MoveNext();
            } while (i++ < level);
            // it now points to the menu to show
            var currentMenu = it.Current;

            do
            {
                SC.SetCursorPosition(0, 3);
                ClearToEndOfScreen();
                SC.WriteLine(currentMenu.Key);
                SC.WriteLine(new string('-', currentMenu.Key.Length));
                var sub = currentMenu.Value;
                i = 1;
                foreach (var item in sub)
                    SC.WriteLine($"{i++}. {item.Key}");
                if (level > 0)
                    SC.WriteLine("0. Back");
                else 
                    SC.WriteLine("0. Exit");

                SC.Write($"{Environment.NewLine}Select an option: ");
                ConsoleKeyInfo key;
                key = SC.ReadKey(true);
                if (SC.KeyAvailable)
                    SC.ReadKey(true); // Clear the buffer, in case user pressed <enter> for example.
                switch (key.Key)
                {
                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:    
                        SC.WriteLine(key.KeyChar);
                        return;
                    case >= ConsoleKey.D1 and <= ConsoleKey.D9:
                    case >= ConsoleKey.NumPad1 and <= ConsoleKey.NumPad9:
                        int index = key.KeyChar - '1';
                        if (index >= 0 && index < sub.Count)
                        {
                            SC.WriteLine(key.KeyChar);  
                            var item = sub.ElementAt(index);
                            if (item.Value is null)
                                ExecuteMenu(null, (uint)_menu.Keys.ToList().IndexOf(item.Key));
                            else
                                item.Value.Invoke();
                        }
                        else
                            SC.Beep();
                        break;
                    default:
                        SC.Beep();
                        break;  
                }
            } while (true);
        }

        /// <summary>
        /// Questions the user about the book details.
        /// </summary>
        /// <returns>A Tuple containing five strings, Title, Author, ISBN and category</returns>
        public static Tuple<string, string, string, string> QueryBookDetails()
        {
            SC.WriteLine($"{Environment.NewLine}Enter book details:");
            string title = QueryUser("Title:");
            string author = QueryUser("Author:");
            string isbn = QueryUser("ISBN:").NormalizeISBNForStorage();
            string category = QueryUser("Category:");
            return new Tuple<string, string, string, string>(title, author, isbn, category);
        }

        public static string QueryISBN()
        {
            SC.Write("Enter ISBN: ");
            string isbn = SC.ReadLine() ?? string.Empty;
            return isbn.Trim();
        }

        public static string QueryUser(string prompt)
        {
            SC.Write($"{prompt.Trim()} ");
            string? input = SC.ReadLine();
            return input?.Trim() ?? string.Empty;
        }
    }

}
