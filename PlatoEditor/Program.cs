using System.Diagnostics;
using Ptarmigan.Utils;

namespace PlatoEditor
{
    public static class Program
    {
        public static WebServer Server;

        public static string LastUpdate { get; set; }
        public static DirectoryPath OutputFolder 
            = AssemblyData.Current.LocationDir.RelativeFolder("html");

        public static void Main(string[] args)
        {
            Console.WriteLine("Creating web-server");
            Server = new WebServer(Callback);
            Server.Start();
            Console.WriteLine($"Web server started on {Server.Uri}");
            Server.SleepWhileActive();
        }

        public static void Callback(string verb, string path, IDictionary<string, string> parameters, Stream inputStream, Stream outputStream)
        {
            if (verb.ToLowerInvariant() == "get")
            {
                if (path.StartsWith("api"))
                {
                    if (path == "api/code" && LastUpdate != null)
                    {
                        outputStream.Write(LastUpdate.ToBytesUtf8());
                    }
                }
                else
                {
                    if (path == "")
                        path = "index.html";

                    var file = OutputFolder.RelativeFile(path);
                    //outputStream.Write("Hello!".ToBytesUtf8());
                    if (file.Exists())
                    {
                        file.CopyToStreamAndClose(outputStream);
                    }
                    else
                    {
                        // TODO: switch this to a logging system
                        Debug.WriteLine($"File {path} was not found");
                    }
                }
            }

            if (verb.ToLowerInvariant() == "post")
            {
                var data = inputStream.ReadAllBytes().ToUtf8();
                Debug.WriteLine($"A change was posted: {data}");
                LastUpdate = data;
            }
        }
    }
}