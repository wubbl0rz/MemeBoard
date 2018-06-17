using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemeBoard
{
    public class Meme
    {
        public string Path { get; private set; }
        public string BindingKey { get; private set; }
        public bool IsAnimated { get; private set; }

        public Meme(string file)
        {
            var fileInfo = new FileInfo(file);

            this.Path = fileInfo.FullName;
            this.IsAnimated = fileInfo.Extension.ToLower() == ".gif";
            this.BindingKey = fileInfo.Name.Split('_').First();
        }

        public static List<Meme> Load(string path)
        {
            //var watcher = new FileSystemWatcher(path);

            //watcher.Created += Changed;
            //watcher.Renamed += Changed;
            //watcher.Deleted += Changed;

            //watcher.EnableRaisingEvents = true;

            return Directory.GetFiles(path).Select(file => new Meme(file)).ToList();
        }

        private static void Changed(object sender, FileSystemEventArgs e)
        {

        }
    }

    public class MemeWatcher
    {
        public MemeWatcher(string path)
        {

        }
    }
}
