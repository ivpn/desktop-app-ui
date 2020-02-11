using System;
using System.Collections;
using System.Collections.Generic;
using IVPN.Models.Configuration;
using Newtonsoft.Json;
using SharpRaven;
using SharpRaven.Data;

namespace IVPN
{
    internal class DiagnosticReport : Exception
    {
        public DiagnosticReport() : base("Diagnostic report") {}
    }

    public class ErrorReporter
    {
        private const int MaxParameterSize = 1024 * 16; // Sentry trims the end of parameter if it > 16KB

        private static readonly RavenClient __RavenClient = null;

        static ErrorReporter()
        {
            try
            {
                __RavenClient = new RavenClient(ErrorReporterSentryKey.SentryKey,
                    new SecureJsonPacketFactory(), null, new SecureUserFactory())
                {
#if DEBUG
                    Environment = "Debug",
                    Release = $"{Platform.ShortPlatformName}_v{Platform.Version}_[Debug]"
#else
                Release = $"{Platform.ShortPlatformName}_v{Platform.Version}"
#endif
                };
            }
            catch (Exception ex)
            {
                __RavenClient = null;
            }
        }

        public static ErrorReporterEvent PrepareEventToSend(Exception ex, AppSettings appSettings, string ivpnLog, string ivpnLogOld, string vpnProtocolLog, string vpnProtocolLogOld, bool? isServiceError = null)
        {
            if (ex == null)
                return null;

            IDictionary<string, string> parameters = InitializeParameters(appSettings);
            foreach(var item in parameters)
                ex.Data.Add(item.Key, item.Value);

            if (!string.IsNullOrEmpty(ivpnLog))
                ex.Data.Add(GetFieldName(FieldsPositions.ivpnLog), ivpnLog);
            if (!string.IsNullOrEmpty(ivpnLogOld))
                ex.Data.Add(GetFieldName(FieldsPositions.ivpnLogOld), ivpnLogOld);
            if (!string.IsNullOrEmpty(vpnProtocolLog))
                ex.Data.Add(GetFieldName(FieldsPositions.vpnProtocolLog), vpnProtocolLog);
            if (!string.IsNullOrEmpty(vpnProtocolLogOld))
                ex.Data.Add(GetFieldName(FieldsPositions.vpnProtocolLogOld), vpnProtocolLogOld);

            var logEvt = new SharpRaven.Data.SentryEvent(ex);
            logEvt.Tags = InitializeTags(appSettings);

            if (isServiceError != null)
            {
                ex.Data.Add(GetFieldName(FieldsPositions.IsService), ((bool)isServiceError).ToString());
                logEvt.Tags.Add($"{FieldsPositions.IsService}", isServiceError.ToString());
            }

            logEvt.Message = ex.Message;
            logEvt.Contexts.Device.Name = ""; // do not send real device name

            ErrorReporterEvent ret = new ErrorReporterEvent(logEvt);
            //DivideBigParameters(ret);

            return ret;
        }

        public static ErrorReporterEvent PrepareDiagReportToSend(AppSettings appSettings, string environmentLog, string ivpnLog, string ivpnLogOld, string vpnProtocolLog, string vpnProtocolLogOld)
        {
            var diagReport = new DiagnosticReport();
            diagReport.Data.Add(GetFieldName(FieldsPositions.Environment), environmentLog);
            diagReport.Data.Add(GetFieldName(FieldsPositions.IsDiagnosticReport), "True");

            ErrorReporterEvent evt = PrepareEventToSend(diagReport, appSettings, ivpnLog, ivpnLogOld, vpnProtocolLog, vpnProtocolLogOld);
            evt.Event.Tags.Add($"{FieldsPositions.IsDiagnosticReport}", "True");
            evt.Event.Level = ErrorLevel.Debug;
            //DivideBigParameters(evt);

            return evt;
        }

        public static void SendReport(ErrorReporterEvent evt, string userComments = null)
        {
            if (evt?.Event == null)
                return;

            if (!string.IsNullOrEmpty(userComments))
            {
                AddParameter(evt.Event.Exception.Data, GetFieldName(FieldsPositions.UserComments), userComments);
                evt.Event.Message = userComments;
            }

            DivideBigParameters(evt);

            Exception sendError = null;
            void OnError(Exception ex)
            {
                sendError = ex;
                Logging.Info($"[ERROR]: Unbale to send report-log to server {ex}");
            }

            if (__RavenClient == null)
                throw new Exception("Unable to send diagnostic report. Report sender not initialized (RavenClient).");

            __RavenClient.ErrorOnCapture += OnError;
            try
            {
                string eventId = __RavenClient.Capture(evt.Event);
                if (!string.IsNullOrEmpty(eventId))
                    Logging.Info($"Sent report. ID:{eventId}");

                if (sendError != null)
                    throw sendError;
            }
            finally
            {
                __RavenClient.ErrorOnCapture -= OnError;
            }
        }

        #region Private functionality

        /// <summary>
        /// Sentry limits message size to 100Kb
        /// Here we are trimming parameters size (if their common size is too big)
        /// </summary>
        /// <param name="evt"></param>
        private static void DivideBigParameters(ErrorReporterEvent evt)
        {
            try
            {
                DivideParameter(evt, FieldsPositions.vpnProtocolLogOld, MaxParameterSize, 2);
                DivideParameter(evt, FieldsPositions.vpnProtocolLog, MaxParameterSize, 3);
                DivideParameter(evt, FieldsPositions.ivpnLogOld, MaxParameterSize, 2);
                DivideParameter(evt, FieldsPositions.Environment, MaxParameterSize, 2);
                DivideParameter(evt, FieldsPositions.ivpnLog, MaxParameterSize, 6);
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Sentry have limit of parameter size - 16KB. So, here we dividing one big parameter to multiple
        /// </summary>
        private static void DivideParameter(ErrorReporterEvent evt, FieldsPositions param, int toSize, int maxItemsCount = 0)
        {
            string fieldName = GetFieldName(param);
            if (!evt.Event.Exception.Data.Contains(fieldName))
                return;

            string value = (string)evt.Event.Exception.Data[fieldName];
            if (value.Length <= toSize)
                return;

            int processedBytes = 0;
            int item = 0;

            List<string> items = new List<string>();

            try
            {
                while (processedBytes < value.Length)
                {
                    string newItem;
                    if (toSize * item + toSize >= value.Length)
                        newItem = value.Substring(toSize * item++);
                    else
                        newItem = value.Substring(toSize * item++, toSize);

                    processedBytes += newItem.Length;
                    newItem = newItem.Trim();
                    if (newItem.Length > 0)
                        items.Add(newItem);
                }

                int nameIdxOffset = 0;
                if (maxItemsCount > 0 && items.Count > maxItemsCount)
                    nameIdxOffset = items.Count - maxItemsCount;

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    evt.Event.Exception.Data.Add(fieldName + $" {i- nameIdxOffset}", items[i]);
                    if (maxItemsCount > 0 && items.Count - i >= maxItemsCount)
                        break;
                }

                // remove old big parameter
                evt.Event.Exception.Data.Remove(fieldName);
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Tail parametr to a site
        /// </summary>
        /// <returns>Saved data size</returns>
        /*private static int TailParameter(ErrorReporterEvent evt, FieldsPositions param, int toSize, int minSize)
        {
            string fieldName = GetFieldName(param);
            if (!evt.Event.Exception.Data.Contains(fieldName))
                return 0;

            string value = (string) evt.Event.Exception.Data[fieldName];

            int size = (toSize < minSize) ? minSize : toSize;
            if (value.Length <= size)
                return 0;

            int oldValLength = value.Length;

            int start = value.Length - size;
            value = "..." + value.Substring(start + 3);
            evt.Event.Exception.Data[fieldName] = value;

            return oldValLength - value.Length;
        }*/
        
        private static void AddParameter(IDictionary desDic, object key, object value)
        {
            if (desDic==null)
                return;

            if (desDic.Contains(key))
                desDic[key] = value;
            else
                desDic.Add(key, value);
        }

        private static IDictionary<string, string> InitializeParameters(AppSettings appSettings)
        {
            var parameters = new Dictionary<string, string>
            {
                { GetFieldName(FieldsPositions.Version), Platform.Version },
                { GetFieldName(FieldsPositions.Platform), Platform.ShortPlatformName },
                { GetFieldName(FieldsPositions.InstallationDirectory), Platform.InstallationDirectory },
                { GetFieldName(FieldsPositions.SettingsDirectory), Platform.SettingsDirectory }
            };

            try
            {
                if (appSettings != null)
                {
                    string settingsJson = JsonConvert.SerializeObject(
                            appSettings,
                            new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.None,
                                Formatting = Formatting.Indented
                            }
                        );

                    parameters.Add(GetFieldName(FieldsPositions.IVPNUser), appSettings.Username);
                    parameters.Add(GetFieldName(FieldsPositions.Settings), settingsJson);
                }
            }
            catch (Exception e)
            {
                Logging.Info($"[ERROR]: Failed to add Settings data a Sentry log {e}");
            }

            return parameters;
        }

        private static IDictionary<string, string> InitializeTags(AppSettings appSettings)
        {
            var parameters = new Dictionary<string, string>
            {
                { $"{FieldsPositions.Version}", Platform.Version },
                { $"{FieldsPositions.Platform}", Platform.ShortPlatformName }
            };

            if (appSettings != null)
            {
                if (appSettings.Username != null)
                {
                    parameters.Add($"{FieldsPositions.IVPNUser}", appSettings.Username);
                }
            }

            return parameters;
        }

        // block sending OS-username
        private class SecureUserFactory : ISentryUserFactory
        {
            SentryUser ISentryUserFactory.Create() { return null; }
        }

        // block sending ServerName (PS name)
        private class SecureJsonPacketFactory : IJsonPacketFactory
        {
            public JsonPacket Create(string project, SentryMessage message, ErrorLevel level = ErrorLevel.Info, IDictionary<string, string> tags = null, string[] fingerprint = null, object extra = null)
            {
                throw new NotImplementedException(); // obsolete
            }

            public JsonPacket Create(string project, Exception exception, SentryMessage message = null, ErrorLevel level = ErrorLevel.Error, IDictionary<string, string> tags = null, string[] fingerprint = null, object extra = null)
            {
                throw new NotImplementedException(); // obsolete
            }

            public JsonPacket Create(string project, SentryEvent @event)
            {
                JsonPacket ret = new JsonPacketFactory().Create(project, @event);
                ret.ServerName = "***";
                return ret;
            }
        }

        // Fields list order 
        private enum FieldsPositions
        {
            UserComments = 0,
            Version,
            Platform,
            IsService,
            IsDiagnosticReport,
            IVPNUser,
            InstallationDirectory,
            SettingsDirectory,
            Settings,
            ivpnLog,
            ivpnLogOld,
            Environment,
            vpnProtocolLog,
            vpnProtocolLogOld
        }

        private static string GetFieldName(FieldsPositions pos)
        {
            // We are using position number before fields name. 
            // This will help us to arrange fields position correctly (view on website)
            // (Sentry is sorting them in alphabetical order)
            return $"[{((int)pos).ToString("00")}] {pos}";
        }
        #endregion //Private functionality
    }
}
