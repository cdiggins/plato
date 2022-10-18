using Plato.Geometry;

namespace Plato.Standard.Library.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var xs = new[] { 1, 2 }.ToIArray();
            var ys = new[] { 1, 2, 3 }.ToIArray();
            var zs = xs.CartesianProduct(ys, (x, y) => (x, y));
            Assert.AreEqual(zs.ToArray(), new[] { (1, 1), (1, 2), (1, 3), (2, 1), (2, 2), (2, 3) });
            Assert.Pass();
        }
    }
}