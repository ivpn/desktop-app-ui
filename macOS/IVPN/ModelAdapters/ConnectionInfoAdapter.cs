using IVPN.Models;

namespace IVPN
{
    public class ConnectionInfoAdapter: ObservableObject
    {
        public ConnectionInfoAdapter(ConnectionInfo connectionInfo): base(connectionInfo)
        {
            if (connectionInfo != null) {
                
                connectionInfo.PropertyChanged += (sender, e) => {
                    DidChangeValue(e.PropertyName);
                };

                connectionInfo.PropertyWillChange += (sender, e) => {
                    WillChangeValue(e.PropertyName);
                };
            }
        }

        public ConnectionInfo ConnectionInfo
        {
            get {
                return (ConnectionInfo)ObservedObject;
            }
        }
    }
}

