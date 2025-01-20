
namespace NetTopologySuite.Topo.UnitTests;

[TestClass]
public class FaceTest : BaseTest
{
    [TestMethod]
    [DataRow("edge")]
    public void FaceCreateInvalid(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Edges.Length);
        Assert.AreEqual(0, Topology.Faces.Length);
    }

    [TestMethod]
    [DataRow("edges-face")]
    public void FaceCreate(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(2, Topology.Edges.Length);
        Assert.AreEqual(1, Topology.Faces.Length);
    }
}