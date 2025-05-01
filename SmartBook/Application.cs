using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SBC = SmartBook.Console.Console;
using Logging;
using SmartBook.Types;

namespace SmartBook
{
    internal class Application
    {
        private Library? library = null;
        public void Execute()
        {
            // Initialize the logger    
            Log.OpenLogFilename("SmartBook.log");
            Log.Note("Start of execution");
            // First, load the existing library
            library = new Library();
            library.Load();
            SBC.ShowToast($"Library loaded. {library.Books.Count} book{(library.Books.Count > 1 ? "s" : "")} loaded");

            OrderedDictionary<string, OrderedDictionary<string, Action?>> menu = new()
            {
                { "Main menu", new()
                    {
                        { "Add a book", AddBook },
                        { "Remove a book", RemoveBook },
                        { "List books", null },
                        { "Search for a book", null },
                        { "Check out, or in, a book", null },
                    }
                },
                { "Search for a book", new()
                    {
                        { "Search by title", SearchForBooksByTitle },
                        { "Search by author", SearchForBooksByAuthor },
                        { "Search by ISBN", SearchForBooksByISBN },
                        { "Search by genre", SearchForBooksByGenre },
                    }
                },
                { "List books", new()
                    {
                        { "List by title", ListBooksByTitle },
                        { "List by author", ListBooksByAuthor },
                        { "List by ISBN", ListBooksByISBN },
                        { "List checked out books", ListCheckedOutBooks }
                    }
                },
                { "Check out, or in, a book", new()
                    {
                        { "Check out a book", CheckOutBook },
                        { "Check in a book", CheckInBook },
                    }
                },
            };

            SBC.DisplayWelcome();
            try
            {
                SBC.ExecuteMenu(menu);
            }
            finally
            {
                // Save the library when exiting
                library?.Save();
                Log.Note("End of execution");
                Log.Close();
            }
        }

        private void SearchForBooksByGenre()
        {
            string genre = SBC.QueryUser("Enter the genre of the book to search for:");
            var books = library.Books
                .Where(b => b.Category.Contains(genre, StringComparison.OrdinalIgnoreCase))
                .OrderBy(b => b.Author)
                .ThenBy(b => b.Title);
            SBC.DisplayBooks($"Books matching genre {genre}", books);
        }

        private void ListCheckedOutBooks()
        {
            var books = library.Books
                .Where(b => b.Status == AvailabilityStatus.CheckedOut)
                .OrderBy(b => b.Author.ToUpperInvariant())
                .ThenBy(b => b.Title.ToUpperInvariant());
            SBC.DisplayBooks("Checked out books", books);
        }

        private void CheckInBook()
        {
            string input = SBC.QueryUser("Enter the ISBN or title of the book to check in: ");
            Book book = library.FindBook(input);
            if (book == null)
            {
                if (!Book.ValidateISBN(input))
                {
                    Log.Warning($"Book with title {input} not found");
                    SBC.DisplayWarning($"Book with title {input} not found");
                    return;
                }
                Log.Warning($"Book with ISBN {input} not found");
                SBC.DisplayWarning($"Book with ISBN {input} not found");
                return;
            }
            if (book.Status == AvailabilityStatus.Available)
            {
                Log.Note($"Book with ISBN {book.ISBN} is already checked in");
                SBC.DisplayWarning($"Book with ISBN {book.ISBN} is already checked in");
                return;
            }
            book.Status = AvailabilityStatus.Available;
        }

        private void CheckOutBook()
        {
            string input = SBC.QueryUser("Enter the ISBN or title of the book to check out: ");
            Book book = library.FindBook(input);
            if (book == null)
            {
                if (!Book.ValidateISBN(input))
                {
                    Log.Warning($"Book with title {input} not found");
                    SBC.DisplayWarning($"Book with title {input} not found");
                    return;
                }
                Log.Warning($"Book with ISBN {input} not found");
                SBC.DisplayWarning($"Book with ISBN {input} not found");
                return;
            }
            if (book.Status == AvailabilityStatus.CheckedOut)
            {
                Log.Note($"Book with ISBN {book.ISBN} is already checked out");
                SBC.DisplayWarning($"Book with ISBN {book.ISBN} is already checked out");
                return;
            }
            book.Status = AvailabilityStatus.CheckedOut;
        }

        /// <summary>
        /// Add a book to the library
        /// </summary>
        private void AddBook()
        {
            string title, author, isbn, category;
            (title, author, isbn, category) = SBC.QueryBookDetails();
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(isbn) || string.IsNullOrEmpty(category))
            {
                Log.Warning("Book details are not complete");
                SBC.DisplayWarning("Book details are not complete");
                return;
            }
            if (!library.AddBook(new Book(title, author, isbn, category)))
            {
                SBC.DisplayWarning($"Book with ISBN {isbn} already exists");
                return; 
            }
        }

        private void RemoveBook()
        {
            string input = SBC.QueryUser("Enter the ISBN or title of the book to remove:");
            if (string.IsNullOrEmpty(input))
            {
                Log.Warning("No input provided. Nothing removed");
                SBC.DisplayWarning("No input provided. Returning.");
                return;
            }
            if (!library.RemoveBook(input))
            {
                Log.Warning($"Book with ISBN, or title, \"{input}\" does not exist");
                SBC.DisplayWarning($"Book with ISBN, or title, \"{input}\" does not exist");
                return;
            }
        }

        private void SearchForBooksByTitle()
        {
            string query = SBC.QueryUser("Enter the title of the book to search for:");
            if (string.IsNullOrEmpty(query))
                return;

            var books = SearchForBooks(query, (book, query) => book.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(b => b.Title)
                .ThenBy(b => b.Author);
            SBC.DisplayBooks("Books matching title", books);    
        }

        private void SearchForBooksByAuthor()
        {
            string query = SBC.QueryUser("Enter the author of the book to search for:");
            if (string.IsNullOrEmpty(query))
                return;

            var books = SearchForBooks(query, (book, query) => book.Author.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(b => b.Author)
                .ThenBy(b => b.Title);
            SBC.DisplayBooks("Books matching author", books);   
        }

        private void SearchForBooksByISBN()
        {
            string query = SBC.QueryUser("Enter the ISBN of the book to search for:");
            if (string.IsNullOrEmpty(query))
                return;

            var books = SearchForBooks(query, (book, query) => book.ISBN.Equals(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(b => b.Author)
                .ThenBy(b => b.Title);  
            SBC.DisplayBooks("Books matching ISBN", books);
        }

        private IEnumerable<Book>? SearchForBooks(string query, Func<Book, string, bool> predicate)
        {
            if (string.IsNullOrEmpty(query))
            {
                Log.Warning("Query is not provided");
                SBC.DisplayWarning("Query is not provided");
                return null;
            }
            var books = library.Books.Where(b => predicate(b, query)).ToList();
            if (books.Count == 0)
            {
                Log.Warning($"No books found for query {query}");
                SBC.DisplayWarning($"No books found for query {query}");
                return null;
            }
            return books;
        }

        private void ListBooksByTitle()
        {
            var books = library.Books.OrderBy(b => b.Title.ToUpperInvariant()).ToList();
            SBC.DisplayBooks("Books sorted by title", books);
        }

        private void ListBooksByAuthor()
        {
            var books = library.Books
                .OrderBy(b => b.Author.ToUpperInvariant())
                .ThenBy(b => b.Title.ToUpperInvariant())
                .ToList();
            SBC.DisplayBooks("Books sorted by author", books);
        }

        private void ListBooksByISBN()
        {
            var books = library.Books.OrderBy(b => b.ISBN.ToUpperInvariant()).ToList();
            SBC.DisplayBooks("Books sorted by ISBN", books);
        }
    }
}
