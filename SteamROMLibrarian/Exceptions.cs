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

	public class TooManyUserIDsException : Exception
	{
		public TooManyUserIDsException() { }

		public TooManyUserIDsException(string message) : base(message) { }

		public TooManyUserIDsException(string message, Exception inner) : base(message, inner) { }
	}

	public class InvalidUserIDException : Exception
	{
		public InvalidUserIDException() { }

		public InvalidUserIDException(string message) : base(message) { }

		public InvalidUserIDException(string message, Exception inner) : base(message, inner) { }
	}

	public class InvalidVDFException : Exception
	{
		public InvalidVDFException() { }

		public InvalidVDFException(string message) : base(message) { }

		public InvalidVDFException(string message, Exception inner) : base(message, inner) { }
	}

	public class InvalidLibraryException : Exception
	{
		public InvalidLibraryException() { }

		public InvalidLibraryException(string message) : base(message) { }

		public InvalidLibraryException(string message, Exception inner) : base(message, inner) { }
	}
}
