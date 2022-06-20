namespace PlatoIR
{
    public abstract class IR
    {
        public int Id { get; } = NewId++;
        public IR Original { get; set; }
        public static int NewId;
        public string Source { get; set; }
    }
}
