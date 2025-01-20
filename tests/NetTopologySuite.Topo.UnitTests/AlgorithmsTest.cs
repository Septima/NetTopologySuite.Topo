using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Topo.UnitTests;

[TestClass]
public class AlgorithmsTest
{
    [TestMethod]
    public void TestCalculateWindingNumber2()
    {
        var denmark = new WKTReader().Read("POLYGON ((7.87 54.69, 7.78 57.25, 9.63 58.08, 10.71 58.11, 12.05 56.69, 13.15 56.42, 14.2 55.47, 15.5 55.33, 15.28 54.64, 12.98 54.94, 12.29 54.35, 12.46 53.64, 11.41 53.42, 10.07 53.18, 8.78 53.52, 7.87 54.69))") as Polygon;
        var cs = denmark!.Coordinates;
        Assert.AreEqual(-1, Algorithms.WindingNumber(cs, new Coordinate(10, 56)));
        Assert.AreEqual(0, Algorithms.WindingNumber(cs, new Coordinate(56, 10)));
    }

    [TestMethod]
    public void TestCalculateWindingNumber3()
    {
        var triangle = new WKTReader().Read("POLYGON ((10 10, 0 10, 5 5, 10 10))") as Polygon;
        var cs = triangle!.Coordinates;
        Assert.AreEqual(1, Algorithms.WindingNumber(cs, new Coordinate(5, 8)));
        Assert.AreEqual(0, Algorithms.WindingNumber(cs, new Coordinate(5, 5)));
        Assert.AreEqual(0, Algorithms.WindingNumber(cs, new Coordinate(0, 0)));
    }
}
