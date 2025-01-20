using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Topo.UnitTests;

[TestClass]
public class TopologyTest : BaseTest
{
    [TestMethod]
    [DataRow("edges-topo")]
    public void TestTopologyCreateFromGeometry(string filename)
    {
        CreateTopologyFromFile(filename);
    }

    [TestMethod]
    [DataRow("edges-face")]
    public void TestTopology(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual(2, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.EdgeRels.Values.Count());
        Assert.AreEqual("1 2 -2 1 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 1 -1 1 0", Topology.EdgeRels[2].ToString());
    }

    [TestMethod]
    [DataRow("edges-face")]
    public void TestTopologySplit(string filename)
    {
        CreateTopologyFromFile(filename);
        var lineString = new WKTReader().Read("LINESTRING(0 5, 10 5)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(5, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((0 0, 0 5, 10 5, 10 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 5, 0 10, 10 10, 10 5, 0 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    [DataRow("edges-face")]
    public void TestTopologySplit2(string filename)
    {
        CreateTopologyFromFile(filename);
        var lineString = new WKTReader().Read("LINESTRING(5 0, 10 5)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(5, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((10 5, 10 0, 5 0, 10 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 0, 0 10, 10 10, 10 5, 5 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
        var lineString2 = new WKTReader().Read("LINESTRING(7.5 2.5, 10 2.5)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString2!);
    }

    [TestMethod]
    [DataRow("edges-face")]
    public void TestTopologySplit2Dangling(string filename)
    {
        CreateTopologyFromFile(filename);
        var lineString = new WKTReader().Read("LINESTRING(5 -1, 11 5)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(7, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((10 4, 10 0, 6 0, 10 4))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 0, 0 10, 10 10, 10 4, 6 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
        var lineString2 = new WKTReader().Read("LINESTRING(7.5 2.5, 11 2.5)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString2!);
        Assert.AreEqual(3, Topology.Faces.Length);
    }

    [TestMethod]
    [DataRow("edge-face")]
    public void TestTopologySplitSingleEdge(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Edges.Length);
        Assert.AreEqual(1, Topology.Faces.Length);
        var lineString = new WKTReader().Read("LINESTRING(0 5, 10 5)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(4, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((0 0, 0 5, 10 5, 10 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 5, 0 10, 10 10, 10 5, 0 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    [DataRow("edges-face")]
    public void TestTopologySplitDangling(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(2, Topology.Edges.Length);
        Assert.AreEqual(1, Topology.Faces.Length);
        var lineString = new WKTReader().Read("LINESTRING(-1 5, 10 5)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(6, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((0 0, 0 5, 10 5, 10 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 5, 0 10, 10 10, 10 5, 0 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    [DataRow("edges-face")]
    public void TestTopologySplitDangling2(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual(2, Topology.Edges.Length);
        var lineString = new WKTReader().Read("LINESTRING(-1 5, 11 5)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(7, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((0 0, 0 5, 10 5, 10 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 5, 0 10, 10 10, 10 5, 0 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    [DataRow("edges-face")]
    public void TestTopologySplitDangling3(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual(2, Topology.Edges.Length);
        var lineString = new WKTReader().Read("LINESTRING (-0.31 3.00, 10.5 6.47)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(7, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((0 0, 0 3.1, 10 6.31, 10 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 3.1, 0 10, 10 10, 10 6.31, 0 3.1))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    [DataRow("edges-face")]
    public void TestTopologySplitDangling4(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual(2, Topology.Edges.Length);
        var lineString = new WKTReader().Read("LINESTRING(0 0.31, 10 6.23)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(5, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((0 0, 0 0.31, 10 6.23, 10 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 0.31, 0 10, 10 10, 10 6.23, 0 0.31))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    [DataRow("edges-topo-adjacent")]
    public void TestTopologyAdjacentSplit(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(2, Topology.Faces.Length);
        var lineString = new WKTReader().Read("LINESTRING (5 0, 5 10)") as LineString;
        Topology = TopologyEditor.AddLineString(Topology, lineString!);
        Assert.AreEqual(3, Topology.Faces.Length);
    }

    [TestMethod]
    [DataRow("denmark")]
    public void TestTopologyDenmarkSplit(string filename)
    {
        CreateTopologyFromFile(filename);
        Assert.AreEqual(1, Topology.Faces.Length);
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[0], Read<Point>("POINT (8.141 56.298)"));
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[1], Read<Point>("POINT (10.317 56.724)"));
        Assert.AreEqual("2 4 -5 0 1", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("4 5 -2 0 1", Topology.EdgeRels[4].ToString());
        Assert.AreEqual("5 2 -4 0 1", Topology.EdgeRels[5].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[2], Read<LineString>("LINESTRING(8.141 56.298, 10.317 56.724)"));
        Assert.AreEqual("2 4 -5 0 2", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("4 5 6 0 3", Topology.EdgeRels[4].ToString());
        Assert.AreEqual("5 2 -6 0 2", Topology.EdgeRels[5].ToString());
        Assert.AreEqual("6 -4 -2 3 2", Topology.EdgeRels[6].ToString());
        Assert.AreEqual(2, Topology.Faces.Length);
        //Assert.AreEqual("POLYGON ((8.141 56.298, 8.108 56.634, 8.407 57.003, 8.642 57.097, 9.255 57.148, 9.648 57.199, 9.899 57.564, 10.213 57.606, 10.637 57.765, 10.464 57.521, 10.59 57.216, 10.276 56.9, 10.317 56.724, 8.141 56.298), (8.69 54.911, 8.611 55.405, 8.155 55.556, 8.155 56.156, 8.141 56.298, 10.317 56.724, 10.354 56.564, 10.936 56.478, 10.826 56.199, 10.59 56.164, 10.292 56.208, 10.323 55.892, 9.962 55.76, 9.82 55.538, 10.354 55.618, 10.998 55.725, 11.501 55.901, 11.768 56.015, 12.365 56.138, 12.553 56.024, 12.585 55.813, 12.663 55.627, 12.333 55.574, 12.161 55.405, 12.506 55.36, 12.412 55.235, 12.145 55.208, 12.208 55.037, 12.632 54.974, 12.49 54.884, 12.239 54.902, 12.098 54.73, 11.988 54.612, 11.501 54.63, 11.045 54.748, 10.778 54.757, 10.26 54.866, 10.056 54.857, 10.04 54.794, 8.69 54.911))", topologyEditor.GetFaceGeometry(topology, topology.Faces[0]).ToString());
        //Assert.AreEqual("POLYGON ((8.141 56.298, 8.108 56.634, 8.407 57.003, 8.642 57.097, 9.255 57.148, 9.648 57.199, 9.899 57.564, 10.213 57.606, 10.637 57.765, 10.464 57.521, 10.59 57.216, 10.276 56.9, 10.317 56.724, 8.141 56.298), (8.69 54.911, 8.611 55.405, 8.155 55.556, 8.155 56.156, 8.141 56.298, 10.317 56.724, 10.354 56.564, 10.936 56.478, 10.826 56.199, 10.59 56.164, 10.292 56.208, 10.323 55.892, 9.962 55.76, 9.82 55.538, 10.354 55.618, 10.998 55.725, 11.501 55.901, 11.768 56.015, 12.365 56.138, 12.553 56.024, 12.585 55.813, 12.663 55.627, 12.333 55.574, 12.161 55.405, 12.506 55.36, 12.412 55.235, 12.145 55.208, 12.208 55.037, 12.632 54.974, 12.49 54.884, 12.239 54.902, 12.098 54.73, 11.988 54.612, 11.501 54.63, 11.045 54.748, 10.778 54.757, 10.26 54.866, 10.056 54.857, 10.04 54.794, 8.69 54.911))", topologyEditor.GetFaceGeometry(topology, topology.Faces[1]).ToString());
    }
}