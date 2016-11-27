using System;
using System.Collections.Generic;

namespace tndwolf.ECS {
	public interface Logger {
		string LogsDirectory { get; set; }
		void BlackList(string module);
		void Debug(string module, object text);
		void Error(string module, object text);
		void Info(string module, object text);
		void Warn(string module, object text);
	}

	public class DefaultLogger: Logger {
		List<string> blackList = new List<string>();
		ConsoleColor defaultColor;

		public DefaultLogger() {
			defaultColor = Console.ForegroundColor; 
		}

		public void BlackList(string module) {
			blackList.Add(module);
		}

		public void Debug(string module, object text) {
			if(IsBlackListed(module)) return;
			Console.Write(DateTime.Now.ToLongTimeString() + " - DEBUG - " + module + ": ");
			Console.WriteLine(text);
		}

		public void Error(string module, object text) {
			if(IsBlackListed(module)) return;
			Console.Write(DateTime.Now.ToLongTimeString() + " - ");
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ERROR");
			Console.ForegroundColor = defaultColor;
			Console.WriteLine(" - " + module + ": " + text);
		}

		public void Info(string module, object text) {
			if(IsBlackListed(module)) return;
			Console.Write(DateTime.Now.ToLongTimeString() + " - INFO - " + module + ": ");
			Console.WriteLine(text);
		}

		protected bool IsBlackListed(string module) {
			return blackList.Find((o) => o.Contains(module)) != null;
		}

		public string LogsDirectory { get; set; }

		public void Warn(string module, object text) {
			if(IsBlackListed(module)) return;
			Console.Write(DateTime.Now.ToLongTimeString() + " - ");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("WARN ");
			Console.ForegroundColor = defaultColor;
			Console.WriteLine(" - " + module + ": " + text);
		}
	}
}

