using System;

namespace TrelloApi.Exceptions
{
	public class NoAccessException : Exception
	{
		public NoAccessException(string message) : base(message) { }
	}
}
