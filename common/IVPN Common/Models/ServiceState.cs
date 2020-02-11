namespace IVPN.Models
{
    public enum ServiceState
    {
        Uninitialized,
        Disconnected,
        Connecting,
        CancellingConnection,
        Connected,
        ReconnectingOnService,
        ReconnectingOnClient,
        Disconnecting
    }
}