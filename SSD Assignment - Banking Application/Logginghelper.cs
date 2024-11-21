using System;
using System.Diagnostics;

namespace Banking_Application
{
    public static class LoggingHelper
    {
        private const string SourceName = "SSD Banking Application";
        private const string LogName = "Application";

        static LoggingHelper()
        {
            if (!EventLog.SourceExists(SourceName))
            {
                EventLog.CreateEventSource(SourceName, LogName);
            }
        }

        public static void LogTransaction(string message, EventLogEntryType type = EventLogEntryType.Information)
        {
            try
            {
                using (EventLog eventLog = new EventLog(LogName))
                {
                    eventLog.Source = SourceName;
                    eventLog.WriteEntry(message, type);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log event: {ex.Message}");
            }
        }
    }
}
