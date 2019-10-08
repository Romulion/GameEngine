using System;
namespace Toys
{
	public class LogEntry
	{
		public string Sender { get; private set; }
		public DateTime Time { get; private set; }
		public Logger.Level Severenety { get; private set; }
		public string Message { get; private set; }
		public string Location { get; private set; }

		public LogEntry(string sender, Logger.Level severenety, string message, string location)
		{
			Sender = sender;
			Severenety = severenety;
			Message = message;
			Location = location;
			Time = DateTime.Now;
		}
	}
}
