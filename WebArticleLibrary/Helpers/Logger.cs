using System;
using System.Diagnostics;

namespace WebArticleLibrary.Helpers
{ 
	public static class Logger
	{
		private static TraceSource source;

		static Logger()
		{
			source = new TraceSource("Logger");	
		}

		public static void Verbose(String format, params Object[] args)
		{
			source.TraceVerbose(format, args);
		}

		public static void Error(Exception ex)
		{
			source.TraceError(ex.ToString());
		}

		public static void Error(String format, params Object[] args)
		{
			source.TraceError(format, args);
		}

		public static void Warning(String format, params Object[] args)
		{
			source.TraceWarning(format, args);
		}
	}
}