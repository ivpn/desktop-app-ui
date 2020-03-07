namespace IVPN.Models
{
    public delegate void OnSessionChangedDelegate(SessionInfo sessionInfo);
    public interface ISessionKeeper
    {        
        event OnSessionChangedDelegate OnSessionChanged;

        bool IsLoggedIn();
        SessionInfo Session { get; }
    }
}
