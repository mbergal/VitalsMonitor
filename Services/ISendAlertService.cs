namespace Monitor.Services
{
    public interface ISendAlertService
    {
        void SendAlert(string title, string description);
    }
}