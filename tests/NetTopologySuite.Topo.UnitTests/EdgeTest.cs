using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Topo.UnitTests;

[TestClass]
public class EdgeTest : BaseTest
{
    [TestMethod]
    [DataRow("edge")]
    public void TestEdgeCreate(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Edges.Length);
    }

    [TestMethod]
    [DataRow("edge-face")]
    public void TestEdgeFaceCreate(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Edges.Length);
        var e = Topology.Edges.First();
        Assert.AreEqual(1, Topology.Faces.Length);
    }

    [TestMethod]
    [DataRow("edge-face-triangle1")]
    public void TestEdgeSplitTriangle1(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Edges.Length);
        Assert.AreEqual("1 1 -1 1 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual(1, Topology.Faces.Length);
        var lineString = new WKTReader().Read("LINESTRING(0 0, 10 5)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(3, Topology.Edges.Length);
        Assert.AreEqual("2 -4 -3 2 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 4 -2 3 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 3 2 3 2", Topology.EdgeRels[4].ToString());
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((10 5, 10 0, 0 0, 10 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 0, 10 10, 10 5, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    [DataRow("edge-face-triangle2")]
    public void TestEdgeSplitTriangle2(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Edges.Length);
        Assert.AreEqual("1 1 -1 0 1", Topology.EdgeRels[1].ToString());
        Assert.AreEqual(1, Topology.Faces.Length);
        var lineString = new WKTReader().Read("LINESTRING(5 5, 5 0)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(2, Topology.Nodes.Length);
        Assert.AreEqual(3, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("2 1-2", Topology.Edges[0].ToString());
        Assert.AreEqual("3 2-1", Topology.Edges[1].ToString());
        Assert.AreEqual("4 1-2", Topology.Edges[2].ToString());
        Assert.AreEqual("2 3 4 0 3", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 2 -4 0 2", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 -2 -3 3 2", Topology.EdgeRels[4].ToString());
        Assert.AreEqual("POLYGON ((5 0, 0 0, 5 5, 5 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((5 5, 10 0, 5 0, 5 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    //[TestMethod]
    [DataRow("edge-face2")]
    public void TestEdgeSplit(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual(1, Topology.Edges.Length);
        var lineString = new WKTReader().Read("LINESTRING(5 0, 5 5)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual(3, Topology.Edges.Length);
        Assert.AreEqual("POLYGON ((10 10, 10 5, 0 0, 10 10))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((10 5, 10 0, 0 0, 10 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    //[TestMethod]
    [DataRow("edge-face")]
    public void TestEdgeSplitClose(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual(1, Topology.Edges.Length);
        var lineString = new WKTReader().Read("LINESTRING(0.000001 5, 10 5)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual(4, Topology.Edges.Length);
        Assert.AreEqual("POLYGON ((0.000001 5, 10 5, 10 0, 0 0, 0.000001 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 10, 10 10, 10 5, 0.000001 5, 0 10))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    //[TestMethod]
    [DataRow("edge-face")]
    public void TestEdgeSplitClose2(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual(1, Topology.Edges.Length);
        var lineString = new WKTReader().Read("LINESTRING (0 10.001, 5 5, 9.99 9.99)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual(4, Topology.Edges.Length);
        Assert.AreEqual("POLYGON ((0 10, 0.0009998000399914476 10, 10 10, 10 0, 0 0, 0 10))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
    }

    [TestMethod]
    [DataRow("edge-invalid")]
    public void TestEdgeCreateInvalid(string filename)
    {
        Assert.ThrowsException<TopologyException>(
            () => CreateTopologyFromFile(filename),
            "Non simple at location (0, 5)"
        );
    }

    [TestMethod]
    [DataRow("denmark")]
    public void TestEdgeDenmarkFaceCreate(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Edges.Length);
        Assert.AreEqual(1, Topology.Faces.Length);
    }
}