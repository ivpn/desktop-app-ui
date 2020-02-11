using System;

namespace IVPN.Shell
{
    public class ShellCommandResult
    {
        private bool __IsAborted;
        private int __ExitCode;
        private string __Output;
        private string __ErrorOutput;
        
        public ShellCommandResult(bool isAborted, int exitCode, string output, string errorOutput)
        {
            __IsAborted = isAborted;
            __ExitCode = exitCode;
            __Output = output;
            __ErrorOutput = errorOutput;
        }

        public bool IsAborted
        {
            get {
                return __IsAborted;
            }
        }

        public int ExitCode 
        {
            get {
                return __ExitCode;
            }
        }

        public string Output
        {
            get {
                return __Output;
            }
        }

        public string ErrorOutput
        {
            get {
                return __ErrorOutput;
            }
        }

    }
}

