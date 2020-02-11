namespace IVPN.WiFi
{
    public class WiFiNetwork
    {
        public WiFiNetwork(string ssid)//, byte[] bssid)
        {
            SSID = ssid;

            if (string.IsNullOrEmpty(SSID))
            {
                //throw new IVPNInternalException("Not defined SSID for WiFi network");
                SSID = "";
            }
            //if (bssid == null)
            //    throw new IVPNInternalException("Not defined BSSID for WiFi network");
            //if (bssid.Length!=6)
            //    throw new IVPNInternalException("Unexpected length of BSSID for WiFi network");
            //BSSID = bssid;
        }

        /// <summary>
        /// WiFi network name
        /// </summary>
        public string SSID { get; }
        
        /// <summary>
        /// Unic WiFi identifier (e.g. AP MAC address)
        /// </summary>
        //public byte[] BSSID { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is WiFiNetwork))
                return false;

            return Equals((WiFiNetwork)obj);
        }

        protected bool Equals(WiFiNetwork other)
        {
            return string.Equals(SSID, other.SSID); // && Enumerable.SequenceEqual(BSSID, other.BSSID);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return SSID != null ? SSID.GetHashCode() : 0;
            }
        }

        public override string ToString()
        {
            return SSID;
        }
    }
}
