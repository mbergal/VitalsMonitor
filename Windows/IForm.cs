namespace Monitor.Windows
{
    public interface IForm<in Model>
    {
        void SyncUI(Model model);
    }
}