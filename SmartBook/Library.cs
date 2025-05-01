using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Logging;
using SmartBook.Extensions;

namespace SmartBook
{
    public class Library
    {
        public List<Book> Books { get; private set; }
        public Library()
        {
            // Initialize the library with an empty list of books
            Books = new List<Book>();  
            Log.Note("Library is now initialized");
        }

        public bool AddBook(Book book)
        {
            // Check if the book already exists in the library
            if (Contains(book.ISBN ?? string.Empty))
            {
                Log.Warning($"Book with ISBN {book.ISBN} already exists");
                return false;
            }
            // Add the book to the library
            Books.Add(book);
            Log.Note($"Added book with ISBN {book.ISBN}");
            return true;
        }

        public Book FindBook(string input)
        {
            input.NormalizeISBNForStorage();
            // Check if the input is a valid ISBN
            if (Book.ValidateISBN(input))
            {
                return Books.FirstOrDefault(b => b.ISBN == input);
            }
            // If not, check if it's a title
            return Books.FirstOrDefault(b => b.Title.Equals(input, StringComparison.OrdinalIgnoreCase));
        }

        public void Load()
        {
            // Load the library data from a file
            try
            {
                List<Book>? books = JsonSerializer.Deserialize<List<Book>>(File.ReadAllText("SmartBook.json"));
                Books = books ?? new List<Book>();
                Log.Note($"Loaded {Books.Count()} books from the storage");
            }
            catch (FileNotFoundException e)
            {
                Log.Warning("No storage file found, this is not a problem");
            }
            catch (JsonException e)
            {
                Log.Error($"Probable data corruption: {e.Message}");
            }   
        }

        public void Save()
        {
            // Save the library data to a file
            try
            {
                File.WriteAllText("SmartBook.json", JsonSerializer.Serialize(Books));
                Log.Note($"Saved {Books.Count()} books to the storage");
            }
            catch (Exception e)
            {
                // Most likely exceptions are:
                // IOException - file is in use
                // UnauthorizedAccessException - no permission to write to the file
                // PathTooLongException - file path is too long
                //
                // We'll handle all the same way, that's whu I use the general Exception catcher.
                Log.Error($"Error saving data: {e.Message}");
            }
        }

        /// <summary>
        /// Check if the library contains a book with the given ISBN
        /// </summary>
        /// <param name="input">The ISBN or Title to look for</param>
        /// <returns>True if the ISBN or Title is found, False otherwise</returns>
        public bool Contains(string input)
        {
            // Check if the input is a valid ISBN
            if (Book.ValidateISBN(input))
            {
                return Books.Any(b => b.ISBN == input);
            }
            // If not, check if it's a title
            return Books.Any(b => b.Title.Equals(input, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Remove a book from the library based on it's ISBN or Title
        /// </summary>
        /// <param name="input">ISBN or Title to remove</param>
        /// <returns>True if the book was removed, False if it did not exist</returns>
        public bool RemoveBook(string input)
        {
            if (Book.ValidateISBN(input))
            {
                // Remove the book by ISBN
                Book? book = Books.FirstOrDefault(b => b.ISBN == input);
                if (book != null)
                {
                    Books.Remove(book);
                    Log.Note($"Removed book with ISBN {input}");
                    return true;
                }
                else
                {
                    Log.Warning($"Book with ISBN {input} not found");
                    return false;
                }
            }
            else
            {
                // Remove the book by Title
                Book? book = Books.FirstOrDefault(b => b.Title.Equals(input, StringComparison.OrdinalIgnoreCase));
                if (book != null)
                {
                    Books.Remove(book);
                    Log.Note($"Removed book with title {input}");
                    return true;
                }
                else
                {
                    Log.Warning($"Book with title {input} not found");
                    return false;
                }
            }
        }
    }
}
