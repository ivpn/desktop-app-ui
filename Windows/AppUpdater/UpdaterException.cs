using System;
using System.Runtime.Serialization;
using System.Text;

namespace AppUpdater
{
    /// <inheritdoc />
    /// <summary>
    /// Generic class for updater exceptions
    /// </summary>
    class UpdaterException : Exception
    {
        /// <inheritdoc />
        public UpdaterException() : base("Installing update error") { }
        /// <inheritdoc />
        public UpdaterException(string description) : base(description){}
        /// <inheritdoc />
        public UpdaterException(string description, Exception ex) : base(description, ex){}
        /// <inheritdoc />
        public UpdaterException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <inheritdoc />
        public override string ToString()
        {
            return GetDetailedMessage(this);
        }

        /// <summary>
        ///  Get exception message string (including internal exceptions)
        /// </summary>
        ///  <param name="ex">current exception</param>
        ///  <param name="message">Prefix string</param>
        ///  <returns>exception message string</returns>
        public static string GetDetailedMessage(Exception ex, string message = "")
        {
            var ret = new StringBuilder();

            Exception exp = ex;
            if (!string.IsNullOrEmpty(message))
                ret.Append(message + " : ");

            string lastMes = "";

            if (!string.IsNullOrEmpty(exp.Message))
            {
                ret.Append(exp.Message);
                lastMes =   exp.Message.Trim();
            }

            exp = exp.InnerException;
            while (exp != null)
            {
                if (!string.IsNullOrEmpty(exp.Message))
                {
                    if (!lastMes.Equals(exp.Message.Trim()))
                    {
                        ret.Append("\n ");
                        ret.Append(exp.Message);
                        lastMes = exp.Message.Trim();
                    }
                }

                exp = exp.InnerException;
            }

            return ret.ToString();
        }
    }

    class UpdaterExceptionAppcastDownload : UpdaterException
    {
        /// <inheritdoc />
        public UpdaterExceptionAppcastDownload() : base("Appcast download error") {}
        /// <inheritdoc />
        public UpdaterExceptionAppcastDownload(string description) : base(description){}
        /// <inheritdoc />
        public UpdaterExceptionAppcastDownload(string description, Exception ex) : base(description, ex){}
        /// <inheritdoc />
        public UpdaterExceptionAppcastDownload(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    class UpdaterExceptionAppcastParsing : UpdaterException
    {
        /// <inheritdoc />
        public UpdaterExceptionAppcastParsing() : base("Appcast parsing error") { }
        /// <inheritdoc />
        public UpdaterExceptionAppcastParsing(string description) : base(description) { }
        /// <inheritdoc />
        public UpdaterExceptionAppcastParsing(string description, Exception ex) : base(description, ex) { }
        /// <inheritdoc />
        public UpdaterExceptionAppcastParsing(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    class UpdaterExceptionUpdateDownload : UpdaterException
    {
        /// <inheritdoc />
        public UpdaterExceptionUpdateDownload() : base("Update download error") { }
        /// <inheritdoc />
        public UpdaterExceptionUpdateDownload(string description) : base(description) { }
        /// <inheritdoc />
        public UpdaterExceptionUpdateDownload(string description, Exception ex) : base(description, ex) { }
        /// <inheritdoc />
        public UpdaterExceptionUpdateDownload(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    class UpdaterExceptionUpdateDownloadTimeout : UpdaterExceptionUpdateDownload
    {
        /// <inheritdoc />
        public UpdaterExceptionUpdateDownloadTimeout() : base("Download timeout") { }
        /// <inheritdoc />
        public UpdaterExceptionUpdateDownloadTimeout(string description) : base(description) { }
        /// <inheritdoc />
        public UpdaterExceptionUpdateDownloadTimeout(string description, Exception ex) : base(description, ex) { }
        /// <inheritdoc />
        public UpdaterExceptionUpdateDownloadTimeout(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    class UpdaterExceptionSignatureError : UpdaterException
    {
        /// <inheritdoc />
        public UpdaterExceptionSignatureError() : base("Signature error") { }
        /// <inheritdoc />
        public UpdaterExceptionSignatureError(string description) : base(description) { }
        /// <inheritdoc />
        public UpdaterExceptionSignatureError(string description, Exception ex) : base(description, ex) { }
        /// <inheritdoc />
        public UpdaterExceptionSignatureError(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
