using System;
using System.IO;

namespace iflix.providers
{
    internal class FileProvider : IFileProvider
    {
        public string read(string name)
        {
            return File.ReadAllText(name);
        }

        public void write(string name, string content)
        {
            File.WriteAllText(name, content);
        }
    }
}