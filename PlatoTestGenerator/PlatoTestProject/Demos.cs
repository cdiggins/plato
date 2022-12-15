using Plato;

namespace PlatoTestProject
{
    public static class Demos
    {
        [Test]
        public static void AngleDemo()
        {
            var angle = 1.Turns() / 4;
            Console.WriteLine($"{angle}");

            var degrees = angle.Degrees;
            Console.WriteLine($"Angle is {degrees:n2} degrees");

            var sin = angle.Sin();
            Console.WriteLine($"Sin(angle) = {sin}");
        }

        [Test]
        public static void LightDemo()
        {
            Velocity c = Velocity.Light;
            
            double value = c;
            Console.WriteLine($"Speed of light {value} {Velocity.Units}");

            Length length = c * 1.Hours();
            Console.WriteLine($"{length.Miles:n0} mph");
        }
    }
}
