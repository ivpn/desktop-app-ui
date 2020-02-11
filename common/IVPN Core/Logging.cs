using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace IVPN
{
    public class Logging
    {
        public static bool OmitDate = false;

        private static bool __IsEnabled = false;

        private static string __LogFile = null;
        private static StreamWriter __StreamWriter;

        private static bool __AppendToExistingFile;

        public static void SetLogFile(string logFile, bool appendToExistingFile = false)
        {
            __AppendToExistingFile = appendToExistingFile;
            __LogFile = logFile;
        }

        private static void _Debug(string memberName, string sourceFilePath, int sourceLineNumber, string line)
        {
#if !DEBUG
            if (!IsEnabled)
                return;
#endif
            var prefix = OmitDate ? "" : DateTime.Now.ToString("MMM dd HH:mm:ss ");
            var message = getLogMessage(memberName, sourceFilePath, line, prefix);

#if DEBUG
            System.Diagnostics.Debug.WriteLine(message);
            //System.Console.WriteLine(message);

            if (!IsEnabled)
                return;
#endif
            lock (__StreamWriter) {
                __StreamWriter.WriteLine (message);
            }
        }

        private static string getLogMessage(string memberName, string sourceFilePath, string line, string prefix)
        {
            return Thread.CurrentThread.ManagedThreadId.ToString() + ":" + string.Format(
                "{0}{1}.{2}: {3}",
                prefix,
                Path.GetFileNameWithoutExtension(sourceFilePath),
                memberName,
                line);
        }

        public static void Info(
            string line,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _Debug(memberName, sourceFilePath, sourceLineNumber, line);
        }

        public static void Enable()
        {
            if (__LogFile == null)
                throw new Exception("Log File is not specified. Cannot enable logging");

            if (__IsEnabled)
                return;

            RotateLogFile();

            try
            {
                // This will ensure that all required directories are created.
                Directory.CreateDirectory(Path.GetDirectoryName(__LogFile));

                __StreamWriter = new StreamWriter(
                    new FileStream(
                        __LogFile,
                        __AppendToExistingFile ? FileMode.Append : FileMode.Create,
                        FileAccess.Write,
                        FileShare.ReadWrite
                    )
                );

                __StreamWriter.AutoFlush = true;

                Logging.IsEnabled = true;

            }
            catch
            {

            }
        }

        static void DeleteLogFiles()
        {
            SafeDeleteFile(__LogFile + ".0");
            SafeDeleteFile(__LogFile);
        }

        public static void DisableAndDeleteLogs()
        {

            try
            {
                DeleteLogFiles();

                if (!__IsEnabled)
                    return;

                Logging.IsEnabled = false;


                if (__StreamWriter != null)
                    __StreamWriter.Close();

            }
            catch { }
        }

        private static void SafeDeleteFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch { }
            }
        }

        public static void RotateLogFile()
        {
            if (__LogFile == null)
                return;

            if (File.Exists(__LogFile))
            {
                try
                {
                    File.Delete(__LogFile + ".0");
                    File.Move(__LogFile, __LogFile + ".0");
                }
                catch { }
            }
        }

        public static bool IsEnabled
        {
            get
            {
                return __IsEnabled;
            }
            private set
            {
                __IsEnabled = value;
            }
        }
    }
}
