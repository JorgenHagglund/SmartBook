using System.Text;

namespace Logging
{
    public static class Log
    {
        private static FileStream? logFile = null;
        private enum LogLevel { NOTE, WARNING, ERROR };

        public static void OpenLogFilename(string fileName)
        {
            try
            {
                // Open the log file for appending
                logFile = File.OpenWrite(fileName);
                // Set the file stream to the end of the file
                logFile.Seek(0, SeekOrigin.End);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating log file: {e.Message}");
            }
        }

        public static void Close()
        {
            // Close the log file
            if (logFile != null)
            {
                logFile.Close();
                logFile = null;
            }
        }

        public static void Note(string message) => DoLog(LogLevel.NOTE, message);
        public static void Warning(string message) => DoLog(LogLevel.WARNING, message);
        public static void Error(string message) => DoLog(LogLevel.ERROR, message);

        private static void DoLog(LogLevel level, string message)
        {
            if (logFile == null)
                return;
            string logMessage = $"{DateTime.Now}: {level.ToString():-7}: {message}\n";
            byte[] data = Encoding.UTF8.GetBytes(logMessage);
            logFile.Write(data, 0, data.Length);
            logFile.Flush();
        }
    }
}
