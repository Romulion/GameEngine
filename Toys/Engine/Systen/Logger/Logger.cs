﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
		//string sender;

		/*
		public Logger(string sender)
		{
			this.sender = sender;
		}
		*/

		static Logger()
		{
			var settings = Settings.GetInstance().System;
			type = settings.LogOutput;
		}

		public static void Warning(object Message, string path = "")
		{
			ProceedEntry(Level.Warning, Message, path);
		}

		public static void Critical(object Message, string path = "")
		{
			ProceedEntry(Level.Critical, Message, path);
		}

		public static void Error(object Message, string path = "")
		{
			ProceedEntry(Level.Error, Message, path);
		}

		public static void Info(object Message, string path = "")
		{
			ProceedEntry(Level.Info, Message, path);
		}

		//[MethodImpl(MethodImplOptions.NoInlining)]
		static void ProceedEntry(Level severenety, object message, string path)
		{
			var senderStack = new System.Diagnostics.StackTrace();
			var method = senderStack.GetFrame(2).GetMethod();
			var sender = String.Format("{0}.{1}", method.DeclaringType.Name, method.Name);
			var entry = new LogEntry(sender, severenety, message.ToString(), path);
			if (type == Output.Internal)
				loggs.Add(entry);
			else if (type == Output.Console)
				Console.WriteLine("{0}:{1}--{2}\n{3}",entry.Sender,entry.Severenety,entry.Message,entry.Location);
			//else if (type == Output.File)
		}

	}
}
