using SmartBook;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void AddBook_ShouldAddBookToLibrary()
        {
            var lib = new Library();
            lib.Load();
            int precondition = lib.Books.Count; 

            var book = new Book("The Shining", "Stephen King", "9789174293920", "Horror");
            lib.AddBook(book);

            Assert.True(lib.Books.Contains(book), "Book was not added to the library");
            Assert.True(precondition + 1 == lib.Books.Count, "Book count did not increase as expected");
        }

        [Fact]
        public void BookValidateISBN_ShouldReturnTrueForValidISBN()
        {
            Assert.True(Book.ValidateISBN("9789174293920"), "Valid ISBN was not recognized as valid");
        }

        [Fact]
        public void BookValidateISBN_ShouldReturnFalseForInvalidISBN()
        {
            Assert.False(Book.ValidateISBN("1234567890123"), "Invalid ISBN was recognized as valid");
        }
    }
}
