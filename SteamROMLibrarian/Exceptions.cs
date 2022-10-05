namespace SteamROMLibrarian
{
	public class SteamPathNotFoundException : Exception
	{
		public SteamPathNotFoundException() { }

		public SteamPathNotFoundException(string message) : base(message) { }

		public SteamPathNotFoundException(string message, Exception inner) : base(message, inner) { }
	}

	public class NoUserIDsFoundException : Exception
	{
		public NoUserIDsFoundException() { }

		public NoUserIDsFoundException(string message) : base(message) { }

		public NoUserIDsFoundException(string message, Exception inner) : base(message, inner) { }
	}

	public class InvalidUserIDException : Exception
	{
		public InvalidUserIDException() { }

		public InvalidUserIDException(string message) : base(message) { }

		public InvalidUserIDException(string message, Exception inner) : base(message, inner) { }
	}
}
