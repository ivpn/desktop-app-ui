//
//  IVPN Client Desktop
//  https://github.com/ivpn/desktop-app-ui
//
//  Created by Stelnykovych Alexandr.
//  Copyright (c) 2020 Privatus Limited.
//
//  This file is part of the IVPN Client Desktop.
//
//  The IVPN Client Desktop is free software: you can redistribute it and/or
//  modify it under the terms of the GNU General Public License as published by the Free
//  Software Foundation, either version 3 of the License, or (at your option) any later version.
//
//  The IVPN Client Desktop is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
//  details.
//
//  You should have received a copy of the GNU General Public License
//  along with the IVPN Client Desktop. If not, see <https://www.gnu.org/licenses/>.
//

﻿using System;
using System.Text;
using System.Runtime.Serialization;

namespace IVPN.Exceptions
{
    /// <summary>
    /// Base class for all IVPN exceptions
    /// </summary>
    public class IVPNException : Exception
    {
        /// <summary>
        /// Constructor of the class 
        /// </summary>
        public IVPNException() : base("IVPN exception")
        {}

        /// <summary>
        /// Constructor of the class 
        /// </summary>
        /// <param name="description"></param>
        public IVPNException(string description) : base(description)
        {}

        /// <summary>
        /// Constructor of the class 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="ex"></param>
        public IVPNException(string description, Exception ex) : base(description, ex)
        {}

        /// <summary>
        /// Constructor of the class 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public IVPNException(SerializationInfo info, StreamingContext context) : base(info, context) { }

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

        public static Exception GetInnerExceptionOfType(Exception ex, Type exceptionType)
        {
            if (ex == null)
                return null;

            if (ex.GetType() == exceptionType)
                return ex;

            Exception exp = ex.InnerException;
            while (exp != null)
            {
                if (exp.GetType() == exceptionType)
                    return exp;

                if (exp is AggregateException aggrExp)
                {
                    foreach (Exception expInnerException in aggrExp.InnerExceptions)
                    {
                        if (expInnerException.GetType() == exceptionType)
                            return expInnerException;
                    }
                }

                exp = exp.InnerException;
            }

            return null;
        }
    }

    public class IVPNServiceCrash : IVPNException
    {
        public IVPNServiceCrash(string description) : base(description) { }
    }

    public class NotSupportedOpenVPNParameter : IVPNException
    {
        public NotSupportedOpenVPNParameter() : base("Not supported OpenVPN parameter") {}
        public NotSupportedOpenVPNParameter(string description) : base(description) {}
        public NotSupportedOpenVPNParameter(string description, Exception ex) : base(description, ex) {}
        public NotSupportedOpenVPNParameter(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class IVPNInternalException : IVPNException
    {
        public IVPNInternalException () : base ("Internal error") { }
        public IVPNInternalException (string description) : base (description) { }
        public IVPNInternalException (string description, Exception ex) : base (description, ex) { }
        public IVPNInternalException (SerializationInfo info, StreamingContext context) : base (info, context) { }
    }

    public class ServersNotLoaded : IVPNException
    {
        public ServersNotLoaded() : base("Unable to load servers list") { }
        public ServersNotLoaded(string description) : base(description) { }
        public ServersNotLoaded(string description, Exception ex) : base(description, ex) { }
        public ServersNotLoaded(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class DNSChangeFailed : IVPNException
    {
        public DNSChangeFailed() : base("Failed to change DNS") { }
        public DNSChangeFailed(string description) : base(description) { }
        public DNSChangeFailed(string description, Exception ex) : base(description, ex) { }
        public DNSChangeFailed(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class IVPNExceptionUserErrorMsg : IVPNException
    {
        public IVPNExceptionUserErrorMsg(string caption, string message) : base(message)
        {
            Caption = caption;
        }
        public string Caption { get; }
    }

}
