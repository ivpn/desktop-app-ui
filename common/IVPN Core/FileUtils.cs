using System;
using System.Text;
using System.IO;

namespace IVPN
{
    public class FileUtils
    {
        public const int MaxLogSize = 1024 * 64;
        public static string TailOfLog(string fileName, int tailLength = MaxLogSize)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var reader = new StreamReader(fs))
                {
                    if (reader.BaseStream.Length > tailLength)
                    {
                        reader.BaseStream.Seek(-tailLength, SeekOrigin.End);
                    }

                    string line;
                    while ((line = reader.ReadLine()) != null)                    
                        builder.AppendLine(line);
                }

                return builder.ToString();

            } 
            catch (FileNotFoundException)
            {
                return $"File '{Path.GetFileName(fileName)}' not exists";
            }
            catch (Exception e)
            {
                return $"Error occured when reading from file {Path.GetFileName(fileName)}:\n{e}";
            }
        }
    }
}

