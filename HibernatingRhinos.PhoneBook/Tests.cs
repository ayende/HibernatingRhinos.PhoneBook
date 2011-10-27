using System.IO;
using HibernatingRhinos.PhoneBook.Library;
using Xunit;
using System.Linq;

namespace HibernatingRhinos.PhoneBook
{
	public class Tests
	{
		[Fact]
		public void CanSaveAndReadEntry()
		{
			var phoneBook = new PersistentPhoneBook(new MemoryStream());

			var expected = new Entry
			{
				FirstName = "ayende",
				LastName = "rahien",
				Number = "01231412312312",
				Type = "Home"
			};
			phoneBook.Create(expected);

			var actual = phoneBook.ReadEntries().First();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void CanCreateAndEndEntry()
		{
			var phoneBook = new PersistentPhoneBook(new MemoryStream());

			var expected = new Entry
			{
				FirstName = "ayende",
				LastName = "rahien",
				Number = "01231412312312",
				Type = "Home"
			};
			phoneBook.Create(expected);

			var fromPhoneBook = phoneBook.ReadEntries().First();

			fromPhoneBook.LastName = "eini";
			fromPhoneBook.FirstName = "oren";

			phoneBook.Edit(fromPhoneBook);

			var fromPhoneBook2 = phoneBook.ReadEntries().First();

			Assert.Equal(fromPhoneBook, fromPhoneBook2);
		}

		[Fact]
		public void CanReadMultipleValues()
		{
			var phoneBook = new PersistentPhoneBook(new MemoryStream());

			for (int i = 0; i < 5; i++)
			{
				var expected = new Entry
				{
					FirstName = "ayende",
					LastName = "rahien",
					Number = "01231412312312",
					Type = "Home"
				};
				phoneBook.Create(expected);
			}

			Assert.Equal(5, phoneBook.ReadEntries().Count());
		}

		[Fact]
		public void CanDeleteEntry()
		{
			var phoneBook = new PersistentPhoneBook(new MemoryStream());

			for (int i = 0; i < 5; i++)
			{
				var expected = new Entry
				{
					FirstName = "ayende",
					LastName = "rahien",
					Number = "01231412312312",
					Type = "Home"
				};
				phoneBook.Create(expected);
			}

			var entry = phoneBook.ReadEntries()
				.Skip(2).First();
			phoneBook.Delete(entry);

			Assert.Equal(4, phoneBook.ReadEntries().Count());
		}

		[Fact]
		public void OnDisposeWillCompact()
		{
			var memoryStream = new MemoryStream();
			var phoneBook = new PersistentPhoneBook(memoryStream, streamOwner:false);

			for (int i = 0; i < 5; i++)
			{
				var expected = new Entry
				{
					FirstName = "ayende",
					LastName = "rahien",
					Number = "01231412312312",
					Type = "Home"
				};
				phoneBook.Create(expected);
			}

			var before = memoryStream.Length;

			var entry = phoneBook.ReadEntries()
				.Skip(2).First();
			phoneBook.Delete(entry);

			Assert.Equal(before, memoryStream.Length); // shouldn't change

			phoneBook.Dispose();

			Assert.True(before > memoryStream.Length);

		}

		[Fact]
		public void CanReadAfterCompaction()
		{
			var memoryStream = new MemoryStream();
			var phoneBook = new PersistentPhoneBook(memoryStream, streamOwner: false);

			for (int i = 0; i < 5; i++)
			{
				var expected = new Entry
				{
					FirstName = "ayende",
					LastName = "rahien",
					Number = "01231412312312",
					Type = "Home"
				};
				phoneBook.Create(expected);
			}

			var before = memoryStream.Length;

			var entry = phoneBook.ReadEntries()
				.Skip(2).First();
			phoneBook.Delete(entry);

			Assert.Equal(before, memoryStream.Length); // shouldn't change

			phoneBook.Dispose();

			var phoneBook2 = new PersistentPhoneBook(memoryStream, false);

			Assert.Equal(4, phoneBook2.ReadEntries().Count());

		}
	}
}