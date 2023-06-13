using System.Diagnostics;
using System.Numerics;

namespace PlatoDrawingDemo
{
    public partial class Form1 : Form
    {
        public List<IShape> Shapes { get; } = new();
        public double ViewportWidth { get; }
        public double ViewportHeight { get; }
        public Bitmap Bitmap { get; }
        public int BitmapWidth = 500;
        public int BitmapHeight = 500;
        public byte[] Bytes { get; }
        public byte[] SynchronizedBytes { get; }
        public Thread DrawingThread { get; }
        public static Mutex Mutex = new ();
        public ParticleSystem Parts { get; } = new ();
        public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
        public Point Offset { get; set; }
        public Form1()
        {
            InitializeComponent();
            Offset = PointToScreen(Point.Empty);
            Bitmap = new Bitmap(BitmapWidth, BitmapHeight);
            Bytes = BitmapHelpers.GetBytes(Bitmap);
            SynchronizedBytes = new byte[Bytes.Length];
            CreateShapes();
            pictureBox1.Image = Bitmap;
            DrawingThread = new Thread(DrawLoop);
            Stopwatch.Restart();
            DrawingThread.Start();
        }

        public static Random Rng = new Random();
        
        public void CreateShapes()
        {
            for (var i = 0; i < 10; i++)
            {
                var x = Rng.NextSingle();
                var y = Rng.NextSingle();
                var r = Rng.NextSingle() / 2;
                Shapes.Add(Extensions.Circle(new Vector2(x, y), r, Color.Blue));
            }
        }

        public void DrawLoop()
        {
            /*
            while (true)
            {
                Update();
                Draw();
                Mutex.WaitOne();
                Buffer.BlockCopy(Bytes, 0, SynchronizedBytes, 0, Bytes.Length);
                Mutex.ReleaseMutex();
                Thread.Sleep(25);
            }
            */
        }

        public void Update()
        {
            var elapsed = Stopwatch.Elapsed.TotalSeconds * 100;
            var pt = MousePosition;
            var x = (float)(pt.X - Offset.X) / Width;
            var y = (float)(pt.Y - Offset.Y) / Height;
            var pos = new Vector2(x, y);
            var mouse = new Mouse()
            {
                Position = pos,
                LeftMouseDown = (MouseButtons & MouseButtons.Left) != 0
            };
            Parts.Update(elapsed, mouse);
            Stopwatch.Restart();
        }

        public void Draw()
        {
            var dim = new Vector2(BitmapWidth, BitmapHeight);
            for (var i = 0; i < BitmapWidth; i++)
            for (var j = 0; j < BitmapHeight; j++)
            {
                var pos = (new Vector2(i, j)) / dim;
                SetPixel(i, j, Parts.GetColor(pos));
            }
        }

        public void OldDraw()
        {
            var shapes = Shapes.ToArray();
            var dim = new Vector2(BitmapWidth, BitmapHeight);
            var maxR = Math.Min(dim.X, dim.Y) / 2; 
            for (var i = 0; i < BitmapWidth; i++)
            for (var j = 0; j < BitmapHeight; j++)
            {
                var pos = (new Vector2(i , j)) / dim ;
                var minDist = double.MaxValue;
                var closest = (IShape)null;
                foreach (var shape in shapes)
                {
                    var d = Math.Abs(shape.Field.GetDistance(pos));
                    if (d < minDist)
                    {
                        minDist = d; 
                        closest = shape; 
                    }
                }

                if (Math.Abs(minDist) < 0.001 && closest != null)
                {
                    var c = closest.Shader.GetColor(minDist, pos);
                    SetPixel(i, j, c);
                }
                else
                {
                    SetPixel(i, j, Color.White);
                }
            }
        }

        public int ByteOffset(int i, int j)
        {
            return (i + j * BitmapWidth) * 4;
        }

        public void SetPixel(int i, int j, Color c)
        {

            Bytes[ByteOffset(i, j) + 0] = c.A;
            Bytes[ByteOffset(i, j) + 1] = c.R;
            Bytes[ByteOffset(i, j) + 2] = c.G;
            Bytes[ByteOffset(i, j) + 3] = c.B;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {   
            /*
            var pos = new Vector2(e.X / (float)Bitmap.Width, e.Y / (float)Bitmap.Height);
            var shape = Extensions.Circle(pos, Rng.NextDouble() / 2, Color.BlueViolet);
            Shapes.Add(shape);
            Draw();
            pictureBox1.Image = Bitmap;
            */
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Offset = PointToScreen(Point.Empty);
            Update();
            Draw();
            BitmapHelpers.CopyDataIntoBitmap(Bitmap, Bytes);
            pictureBox1.Invalidate();
        }
    }
}