namespace costs.net.integration.tests.Browser
{
    using System.IO;

    public class BrowserContextMultipartFormDataConfigurator
    {
        public string Name { get; private set; }

        public string FileName { get; private set; }

        public string ContentType { get; private set; }

        public Stream File { get; private set; }

        public void AddFile(string name, string fileName, string contentType, Stream file)
        {
            Name = name;
            FileName = fileName;
            ContentType = contentType;
            File = file;
        }

        public bool IsEmpty()
        {
            return File == null;
        }
    }
}