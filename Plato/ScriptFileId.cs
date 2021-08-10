using System;
using System.IO;

namespace Plato
{
    public struct ScriptFileId
    {
        public ScriptFileId(FileInfo file)
            => (Length, LastWriteTime, Path) = (file.Length, file.LastWriteTime, file.FullName);

        public static ScriptFileId Create(FileInfo file)
            => new ScriptFileId(file);

        public static ScriptFileId Create(string file)
            => new ScriptFileId(new FileInfo(file));

        public long Length { get; }
        public DateTime LastWriteTime { get; }
        public string Path { get; }

        public override bool Equals(object obj)
            => obj is ScriptFileId id && Equals(id);

        public bool Equals(ScriptFileId id)
            => Length == id.Length && LastWriteTime == id.LastWriteTime && Path == id.Path;

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + Length.GetHashCode();
                hash = hash * 31 + LastWriteTime.GetHashCode();
                hash = hash * 31 + Path.GetHashCode();
                return hash;
            }
        }
    }
}
