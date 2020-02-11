using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IVPN
{
    public class UriHelpers
    {
        internal static void Navigate(string httpLink)
        {
            Process.Start(new ProcessStartInfo(httpLink));
        }
    }
}
