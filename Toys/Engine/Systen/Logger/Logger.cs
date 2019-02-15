﻿using System;
using System.Collections.Generic;
namespace Toys
{
	public class Logger
	{
		public enum Output
		{
			None,
			Console,
			File,
			Internal,
		}

		public enum Level
		{
			Critical,
			Error,
			Warning,
			Info,
		}

		static List<LogEntry> loggs = new List<LogEntry>();
		static Output type;
		string Sender;

		
		public Logger(string sender)
		{
			Sender = sender;
		}

		static Logger()
		{
			type = Settings.GetInstance().System.LogOutput;
		}

		public void Warning(string Message, string path)
		{
			ProceedEntry(Level.Warning, Message, path);
		}

		public void Critical(string Message, string path)
		{
			ProceedEntry(Level.Critical, Message, path);
		}

		public void Error(string Message, string path)
		{
			ProceedEntry(Level.Error, Message, path);
		}

		public void Info(string Message, string path)
		{
			ProceedEntry(Level.Error, Message, path);
		}

		void ProceedEntry(Level severenety, string message, string path)
		{
			var entry = new LogEntry(Sender, severenety, message, path);
			if (type == Output.Internal)
				loggs.Add(entry);
			else if (type == Output.Console)
				Console.WriteLine("{0}:{1}--{2}",entry.Sender,entry.Severenety,entry.Message);
			//else if (type == Output.File)
		}

	}
}
