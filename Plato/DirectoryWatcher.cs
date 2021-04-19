using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Plato
{
    public class DirectoryWatcher
    {
        public FileSystemWatcher Watcher;
        public IEnumerable<string> GetFiles()
        {
            return Directory.GetFiles(Watcher.Path, Watcher.Filter, Watcher.IncludeSubdirectories
                           ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        private Action OnChange { get; }

        public DirectoryWatcher(string dir, string filter, bool subDirectories, Action onChange, ISynchronizeInvoke syncObject)
        {
            Watcher = new FileSystemWatcher(dir)
            {
                NotifyFilter = NotifyFilters.Attributes 
                    | NotifyFilters.CreationTime 
                    | NotifyFilters.DirectoryName 
                    | NotifyFilters.FileName 
                    | NotifyFilters.LastWrite 
                    | NotifyFilters.Size
            };
            Watcher.SynchronizingObject = syncObject;
            Watcher.Filter = filter;
            Watcher.IncludeSubdirectories = subDirectories;
            Watcher.Changed += Watcher_Changed;
            Watcher.Created += Watcher_Created;
            Watcher.Deleted += Watcher_Deleted;
            Watcher.Renamed += Watcher_Renamed;
            Watcher.Error += Watcher_Error;
            Watcher.EnableRaisingEvents = true;
            OnChange = onChange;
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"Error occurred {e}");
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            Debug.WriteLine($"Renamed file from {e.OldName} to {e.Name}");
            OnChange();
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"Deleted file {e.Name}");
            OnChange();
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"Created file {e.Name}");
            OnChange();
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine($"File changed {e.Name}");
            OnChange();
        }
    }
}
