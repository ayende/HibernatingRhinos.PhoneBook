namespace HibernatingRhinos.PhoneBook.Library
{
	public class Entry
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Type { get; set; }
		public string Number { get; set; }

		public long PositionInFile { get; set; }

		public bool Equals(Entry other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.FirstName, FirstName) && Equals(other.LastName, LastName) && Equals(other.Type, Type) && Equals(other.Number, Number) && other.PositionInFile == PositionInFile;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (Entry)) return false;
			return Equals((Entry) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (FirstName != null ? FirstName.GetHashCode() : 0);
				result = (result*397) ^ (LastName != null ? LastName.GetHashCode() : 0);
				result = (result*397) ^ (Type != null ? Type.GetHashCode() : 0);
				result = (result*397) ^ (Number != null ? Number.GetHashCode() : 0);
				result = (result*397) ^ PositionInFile.GetHashCode();
				return result;
			}
		}

		public override string ToString()
		{
			return string.Format("FirstName: {0}, LastName: {1}, Type: {2}, Number: {3}, PositionInFile: {4}", FirstName, LastName, Type, Number, PositionInFile);
		}
	}
}