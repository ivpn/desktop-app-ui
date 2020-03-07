using System;
using System.Collections.Generic;

namespace IVPN.Models.Session
{
    /// <summary>
    /// Information about user account
    /// </summary>
    public class AccountStatus
    {
        public AccountStatus(bool isActive, DateTime activeUtil, bool isRenewable, bool willAutoRebill, bool isOnFreeTrial, string[] capabilities)
        {
            IsActive = isActive;
            ActiveUtil = activeUtil;
            IsRenewable = isRenewable;
            WillAutoRebill = willAutoRebill;
            IsOnFreeTrial = isOnFreeTrial;

			if (capabilities==null || capabilities.Length<=0)
                Capabilities = new List<string> ();
            else
                Capabilities = new List<string> ( capabilities );
        }

        public bool IsActive { get; }
		public DateTime ActiveUtil { get; }
		public bool IsRenewable { get; }
		public bool WillAutoRebill { get; }
		public bool IsOnFreeTrial { get; }
		public List <string> Capabilities { get; }
    }
}
