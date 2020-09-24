namespace oauth2.helpers
{
    public interface ISerializer
    {
        string Serialize<T>(T obj) where T : class;
        T Deserialize<T>(string text) where T : class;
    }
}
