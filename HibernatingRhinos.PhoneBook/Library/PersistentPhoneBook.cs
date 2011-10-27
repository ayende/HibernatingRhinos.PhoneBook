using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace HibernatingRhinos.PhoneBook.Library
{
	public class PersistentPhoneBook : IDisposable
	{
		private readonly Stream stream;
		private readonly bool streamOwner;

		private const int RecordSizeV1 = 51 * 4;
		private const int RecrodSizePlusRecordIndicatorV1 = RecordSizeV1 + 1;

		public enum EntryOptions : byte
		{
			InvalidEntryV1 = 0,
			StandardEntryV1 = 1
		}
		
		public PersistentPhoneBook(string path)
			:this(File.OpenWrite(path))
		{
		}

		public PersistentPhoneBook(Stream stream, bool streamOwner = true)
		{
			this.stream = stream;
			this.streamOwner = streamOwner;
			this.stream.Seek(0, SeekOrigin.End);
		}

		public void Create(Entry entry)
		{
			entry.PositionInFile = stream.Position;
			WriteToStream(entry);
		}

		public void Edit(Entry entry)
		{
			stream.Position = entry.PositionInFile;
			WriteToStream(entry);
		}

		public void Delete(Entry entry)
		{
			stream.Position = entry.PositionInFile;
			stream.WriteByte((byte)EntryOptions.InvalidEntryV1);
		}

		public IEnumerable<Entry> ReadEntries()
		{
			stream.Seek(0, SeekOrigin.Begin);
			while(stream.Position < stream.Length)
			{
				var entry = ReadEntry();
				if (entry != null)
					yield return entry;
			}
		}

		private Entry ReadEntry()
		{
			var positionInFile = stream.Position;
			var entryOptions = (EntryOptions)stream.ReadByte();
			switch (entryOptions)
			{
				case EntryOptions.InvalidEntryV1:
					stream.Seek(RecordSizeV1, SeekOrigin.Current);
					return null;
				case EntryOptions.StandardEntryV1:
					return new Entry
					{
						PositionInFile = positionInFile,
						FirstName = ReadString(),
						LastName = ReadString(),
						Number = ReadString(),
						Type = ReadString(),
					};
				default:
					throw new ArgumentOutOfRangeException(entryOptions.ToString());
			}
		}


		private string ReadString()
		{
			var len = stream.ReadByte();
			byte[] buffer = ReadBuffer(50);
			return Encoding.UTF8.GetString(buffer, 0, len);
		}

		private byte[] ReadBuffer(int len)
		{
			var buffer = new byte[len];
			var readCount = 0;
			while(readCount < len)
			{
				var count = stream.Read(buffer, readCount, len - readCount);
				if(count == 0)
					throw new InvalidDataException("Couldn't read fixed length buffer from stream");
				readCount += count;
			}
			return buffer;
		}

		private void WriteToStream(Entry entry)
		{
			stream.WriteByte((byte)EntryOptions.StandardEntryV1);
			WriteString(entry.FirstName);
			WriteString(entry.LastName);
			WriteString(entry.Number);
			WriteString(entry.Type);
		}

		private void WriteString(string val)
		{
			var bytes = Encoding.UTF8.GetBytes(val);
			if(bytes.Length > 50)
				throw new ArgumentException("Cannot write a string larger than 50 bytes, but got: " + val);
			stream.WriteByte((byte) bytes.Length);
			stream.Write(bytes, 0, bytes.Length);
			var remaining = new byte[50 - bytes.Length];
			stream.Write(remaining, 0, remaining.Length);
		}

		public void Dispose()
		{
			Compact();
			if (streamOwner)
				stream.Dispose();
		}

		private void Compact()
		{
			var entriesPositions = GetEntriesPositions();

			while (entriesPositions.InvalidEntries.Count > 0 &&
				entriesPositions.ValidEntries.Count > 0)
			{
				var firstInvalidEntry = entriesPositions.ConsumeFirstInvalid();
				var currentLastValidEntry = entriesPositions.ConsumeLastValid();

				// all the invalid entries are past the valid entries, we can just truncate
				if (firstInvalidEntry >= currentLastValidEntry)
				{
					stream.SetLength(currentLastValidEntry);
					break;
				}
				// copy to invalid entry position
				stream.Position = currentLastValidEntry;
				var buffer = ReadBuffer(RecrodSizePlusRecordIndicatorV1);
				stream.Position = firstInvalidEntry;
				stream.Write(buffer, 0, RecrodSizePlusRecordIndicatorV1);
				// remove the last valid entry
				stream.SetLength(currentLastValidEntry);
			}
			
		}

		public class EntriesPositions
		{
			public List<long> InvalidEntries { get; set; }
			public List<long> ValidEntries { get; set; }

			public long ConsumeFirstInvalid()
			{
				var invalidEntry = InvalidEntries[0];
				InvalidEntries.RemoveAt(0);
				return invalidEntry;

			}

			public long ConsumeLastValid()
			{
				var last = ValidEntries.Last();

				ValidEntries.RemoveAt(ValidEntries.Count - 1);

				return last;
			}
		}

		private EntriesPositions GetEntriesPositions()
		{
			var invalidEntries = new List<long>();
			var validEntries = new List<long>();
			stream.Seek(0, SeekOrigin.Begin);
			while (stream.Position < stream.Length)
			{
				var posInFile = stream.Position;
				var entryOptions = (EntryOptions)stream.ReadByte();
				switch (entryOptions)
				{
					case EntryOptions.InvalidEntryV1:
						invalidEntries.Add(posInFile);
						break;
					case EntryOptions.StandardEntryV1:
						validEntries.Add(posInFile);
						break;
				}
				stream.Seek(RecordSizeV1, SeekOrigin.Current);
			}
			return new EntriesPositions
			{
				InvalidEntries = invalidEntries, 
				ValidEntries = validEntries
			};
		}
	}
}