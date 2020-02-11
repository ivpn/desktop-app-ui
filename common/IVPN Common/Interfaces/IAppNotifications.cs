namespace IVPN.Interfaces
{
    public interface IAppNotifications
    {
        void TrayIconNotifyInsecureWiFi(string message);

        void ShowConnectedTrayBaloon(string baloonText);

        void ShowDisconnectedNotification();

        void UpdateConnectedTimeDuration(string serverName, string connectedTime);
    }
}
