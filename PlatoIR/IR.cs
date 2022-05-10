using System;

namespace PlatoIR
{
    public abstract class IR
    {
        public int Id { get; } = NewId++;
        public static int NewId;
        public string Source { get; set; }
        public abstract void Visit(Func<IR, bool> action);
        public abstract IR Clone();
    }
}
