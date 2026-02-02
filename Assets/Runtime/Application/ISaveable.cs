namespace Runtime.Application
{
    public interface ISaveable
    {
        string SaveId { get; }
        string CaptureStateJson();
        void RestoreStateJson(string json);
        int RestoreOrder => 0;
    }
}
