using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MemeBoard
{
    class MemeRepo
    {
        public Meme[] Memes => this.memes.Values.ToArray();
        public string Path { get; private set; }

        private readonly FileSystemWatcher watcher = new FileSystemWatcher();
        private readonly Dictionary<string, Meme> memes = new Dictionary<string, Meme>();

        public MemeRepo(string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                this.memes.Add(file, new Meme(file));
            }

            this.watcher.Path = path;

            watcher.Created += UpdateMemes;
            watcher.Renamed += UpdateMemes;
            watcher.Deleted += UpdateMemes;

            this.Path = path;

            watcher.EnableRaisingEvents = true;
        }

        public event Action Updated;

        private void UpdateMemes(object sender, FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    this.memes.Add(e.FullPath, new Meme(e.FullPath));
                    break;
                case WatcherChangeTypes.Deleted:
                    this.memes.Remove(e.FullPath);
                    break;
                case WatcherChangeTypes.Renamed when e is RenamedEventArgs r:
                    this.memes.Remove(r.OldFullPath);
                    this.memes.Add(r.FullPath, new Meme(r.FullPath));
                    break;
                default:
                    break;
            }
            
            this.Updated?.Invoke();
        }
    }
}
