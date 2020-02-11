
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace IVPN.Shell
{
    public class ShellCommand
    {
        //public static async Task<ShellCommandResult> RunCommandAsync(string path, string arguments, string inputStreamText = null, int timeoutMs = 5000, bool isDisableLogging = false)
        //{
        //    return await Task.Run(() => RunCommand(path, arguments, inputStreamText, timeoutMs, isDisableLogging));
        //}

        public static ShellCommandResult RunCommand(string path, string arguments, string inputStreamText = null, int timeoutMs = 5000, bool isDisableLogging = false, string textToHideFromLog = null)
        {
            void Log(string text)
            {
                if (isDisableLogging)
                    return;

                if (!string.IsNullOrEmpty(textToHideFromLog))
                    Logging.Info(text.Replace(textToHideFromLog, "***"));
                else
                    Logging.Info(text);
            }

            try
            {
                Log(String.Format("Running: {0} {1}", Path.GetFileName(path), arguments));

                var scriptName = Path.GetFileName(path);

                bool isWriteToInputStream = !string.IsNullOrEmpty(inputStreamText);

                if (!Directory.Exists(Platform.LogDirectory))
                    Directory.CreateDirectory(Platform.LogDirectory);

                ProcessStartInfo processStartinfo = new ProcessStartInfo(path, arguments)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = isWriteToInputStream,
                    WorkingDirectory = Platform.LogDirectory,
                    CreateNoWindow = true
            };

                bool isCanProcessEvents = true;

                using (Process process = new Process())
                {
                    process.StartInfo = processStartinfo;

                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (!isCanProcessEvents) // ensure that outputWaitHandle is not Disposed
                                return;

                            if (e.Data == null)
                                outputWaitHandle.Set();
                            else
                                output.AppendLine(e.Data);
                        };

                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (!isCanProcessEvents) // ensure that errorWaitHandle is not Disposed
                                return;

                            if (e.Data == null)
                                errorWaitHandle.Set();
                            else
                                error.AppendLine(e.Data);
                        };

                        process.Start();

                        if (isWriteToInputStream)
                        {
                            process.StandardInput.Write(inputStreamText);
                            process.StandardInput.Flush();
                            process.StandardInput.Close();
                        }

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        if (process.WaitForExit(timeoutMs) &&
                            outputWaitHandle.WaitOne(timeoutMs) &&
                            errorWaitHandle.WaitOne(timeoutMs))
                        {
                            // Ensure that all asynchronous events are completed.
                            // It also avoid situation to call 'OutputDataReceived' or 'ErrorDataReceived' 
                            // when 'outputWaitHandle' or 'errorWaitHandle' has already been disposed.
                            //
                            // https://msdn.microsoft.com/en-us/library/ty0d8k56(v=vs.110)
                            // "When standard output has been redirected to asynchronous event handlers, 
                            // it is possible that output processing will not have completed when this method returns. 
                            // To ensure that asynchronous event handling has been completed, 
                            // call the WaitForExit() overload that takes no parameter after receiving a true from this overload."
                            process.WaitForExit();

                            // disable processing events of rceiving data or error 
                            // probably, we do not need it here (because of previous call of 'WaitForExit()')
                            isCanProcessEvents = false;

                            // Process completed. Check process.ExitCode here.
                            if (process.ExitCode != 0)
                                Log(string.Format("{0} exited with status {1}", scriptName, process.ExitCode));

                            string outputString = output.ToString();
                            string errorString = error.ToString();

                            if (!String.IsNullOrWhiteSpace(outputString))
                                Log(string.Format("Process {0} output: \n {1}", scriptName, outputString));

                            if (!String.IsNullOrWhiteSpace(errorString))
                                Log(string.Format("Process {0} error output: \n {1}", scriptName, errorString));

                            return new ShellCommandResult(false, process.ExitCode, outputString, errorString);
                        }
                        else
                        {
                            // Timed out.
                            Log(string.Format("{0} taking too long, killing", scriptName));
                            try
                            {
                                // disable processing events of rceiving data or error 
                                isCanProcessEvents = false;

                                process.Kill();
                            }
                            catch (Exception ex)
                            {
                                Log(string.Format("task killing error: {0}", ex));
                            }

                            return new ShellCommandResult(true, 255, "", "Process execution timeout");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (isDisableLogging)
                    Logging.Info("Shell command execution failed.");

                Log($"[EXCEPTION] Shell command execution failed: '{path} {arguments}'. Error: {ex}");

                return new ShellCommandResult(true, 256, "", $"Internal exception: {ex}");
                //IVPNException exp = new IVPNException($"[EXCEPTION] Shell command execution failed: '{path} {arguments}'",ex);
                //throw exp;
            }
        }        
    }
}
