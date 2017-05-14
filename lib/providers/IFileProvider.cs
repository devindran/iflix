namespace iflix.providers
{
    public interface IFileProvider
    {
        string read(string name);
        void write(string name, string content);
    }
}