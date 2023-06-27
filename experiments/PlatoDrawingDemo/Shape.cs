using System.Numerics;
using Plato;
using Color = System.Drawing.Color;

namespace PlatoDrawingDemo;

public class Shape : IShape
{
    public IDistanceFieldShader Shader { get; }
    public IDistance Field { get; }
    public Shape(IDistanceFieldShader shader, IDistance field)
    {
        Shader = shader;
        Field = field;
    }
}

public interface IDistanceFieldShader
{
    Color GetColor(double d, Vector2 v);
}

public interface IShader
{
    Color GetColor(Vector2 v);
}

public class ParticleSystem : IAnimation
{
    public class Particle
    {
        public Vector2 Position { get; }
        public Vector2 Velocity { get; }

        public Particle(Vector2 position, Vector2 velocity)
        {
            Position = position;
            Velocity = velocity;
        }
    }

    public System.Collections.Generic.List<Particle> Particles { get; set; } = new();

    public Random Random { get; } = new Random();

    public static Particle Update(Particle p, double time)
    {
        var pos = p.Position + p.Velocity * (float)time;
        var x = pos.X;
        var y = pos.Y;
        var offX = p.Velocity.X;
        var offY = p.Velocity.Y;
        if (x < 0 || x > 1)
            offX = -offX;
        if (y < 0 || y > 1)
            offY = -offY;
        x = Math.Clamp(x, 0, 1);
        y = Math.Clamp(y, 0, 1);
        return new Particle(new Vector2(x, y), new Vector2(offX, offY));

    }

    public void Update(double time, Mouse mouse)
    {
        if (mouse.LeftMouseDown)
        {
            var x = Random.NextSingle() - 0.5f;
            var y = Random.NextSingle() - 0.5f;
            var v = new Vector2(x / 200, y/ 200);
            var p = new Particle(mouse.Position, v);
            Particles.Add(p);
        }

        for (var i = 0; i < Particles.Count; i++)
        {
            Particles[i] = Update(Particles[i], time);
        }
    }

    public Color Old_GetColor(Vector2 v)
    {
        var sum = 255;
        foreach (var p in Particles)
        {
            var d = (p.Position - v).Length();
            if (d < 0.1)
            {
                sum = Math.Min(sum, (byte)((0.1f - d) * 10f * 255));
            }
        }

        return Color.FromArgb(sum, 255, 255);
    }

    public Color GetColor(Vector2 v)
    {
        var sum = 0.0;
        foreach (var p in Particles)
        {
            var d = (p.Position - v).Length();
            sum += 10 / d;
        }

        return Color.FromArgb((byte)sum, 255, 255);
    }
}

public interface IDistance
{
    double GetDistance(Vector2 v);
}

public interface IShape
{
    IDistanceFieldShader Shader { get; }
    IDistance Field { get; }
}

public class Distance : IDistance
{
    private Func<Vector2, double> _func;
    public Distance(Func<Vector2, double> func)
    {
        _func = func;
    }
    public double GetDistance(Vector2 v) => _func(v);
}

public class DistanceFieldShader : IDistanceFieldShader
{
    private Func<double, Vector2, Color> _func;
    public DistanceFieldShader(Func<double, Vector2, Color> func)
    {
        _func = func;
    }
    public Color GetColor(double d, Vector2 v) => _func(d, v);
}

public class Mouse
{
    public Vector2 Position { get; set; }
    public bool LeftMouseDown { get; set; }
    public bool RightMouseDown { get; set; }
    public bool CenterMouseDown { get; set; }
}

public interface IAnimation : IShader
{
    void Update(double time, Mouse mouse);
}

/*
public class Animation<TState> : IAnimation
{
    private TState _state { get; }
    private Func<TState, double, Mouse, TState> _stateFunc;
    private Func<TState, IArray<Shape>> _shapeFunc;

    public Animation(TState state, Func<TState, double, Mouse, TState> stateFunc, Func<TState, IArray<Shape>> shapeFunc)
    {
        _state = state;
        _shapeFunc = shapeFunc;
        _stateFunc = stateFunc;
    }
    
    public IArray<Shape> Shapes { get; }

    public IAnimation Update(double time, Mouse mouse)
        => new Animation<TState>(_stateFunc(_state, time, mouse), _stateFunc, _shapeFunc);

}
*/
public static class Extensions
{
    public static IDistanceFieldShader ToShader(this Func<double, Vector2, Color> func) 
        => new DistanceFieldShader(func);

    public static IDistance ToDistance(this Func<Vector2, double> func)
        => new Distance(func);

    public static IShape ToShape(this IDistance distance, IDistanceFieldShader isdfShader)
        => new Shape(isdfShader, distance);

    public static Color GetColor(this IShape shape, Vector2 v) 
        => shape.Shader.GetColor(shape.Field.GetDistance(v), v);

    public static IDistanceFieldShader ToShader(this Color color)
        => ToShader((d, p) => color);

    public static IShape Circle(Vector2 p, double radius, Color color)
        => ToShape(ToDistance(xy => (xy - p).Length() - radius), ToShader(color));
}
