using System.Collections.Immutable;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Topo.UnitTests;

[TestClass]
public class TopoEditTest : BaseTest
{
    [TestMethod]
    public void TestTriangle1()
    {
        /* Corresponding case in PostGIS
        select topology.droptopology('test_topo');
        select topology.createtopology('test_topo', 0, 0);
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(5 5)'));
        select topology.st_addedgenewfaces('test_topo', 1, 1, st_geomfromtext('LINESTRING(5 5, 10 0, 0 0, 5 5)'));
        select topology.st_newedgessplit('test_topo', 1, st_geomfromtext('POINT(5 0)'));
        select topology.st_addedgenewfaces('test_topo', 1, 2, st_geomfromtext('LINESTRING(5 5, 5 0)'));
        */
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(5 5)"));
        Assert.AreEqual("1 (5 5)", Topology.Nodes[0].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[0], Read<LineString>("LINESTRING(5 5, 10 0, 0 0, 5 5)"));
        Assert.AreEqual(1, Topology.Edges.Length);
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual("1 1-1", Topology.Edges[0].ToString());
        Assert.AreEqual("1 1 -1 0 1", Topology.EdgeRels[1].ToString());
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[0], Read<Point>("POINT(5 0)"));
        Assert.AreEqual(2, Topology.Nodes.Length);
        Assert.AreEqual(2, Topology.Edges.Length);
        Assert.AreEqual("2 1-2", Topology.Edges[0].ToString());
        Assert.AreEqual("3 2-1", Topology.Edges[1].ToString());
        Assert.AreEqual("2 3 -3 0 1", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 2 -2 0 1", Topology.EdgeRels[3].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[1], Read<LineString>("LINESTRING(5 5, 5 0)"));
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
        Topology = TopologyEditor.RemEdgeNewFace(Topology, Topology.Edges[2]);
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((5 5, 10 0, 5 0, 0 0, 5 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
    }

    [TestMethod]
    public void TestTriangle2()
    {
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(5 5)"));
        Assert.AreEqual("1 (5 5)", Topology.Nodes[0].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[0], Read<LineString>("LINESTRING(5 5, 0 0, 10 0, 5 5)"));
        Assert.AreEqual(1, Topology.Edges.Length);
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual("1 1-1", Topology.Edges[0].ToString());
        Assert.AreEqual("1 1 -1 1 0", Topology.EdgeRels[1].ToString());
    }

    [TestMethod]
    public void TestTriangle3()
    {
        /* Corresponding case in PostGIS
        select topology.droptopology('test_topo');
        select topology.createtopology('test_topo', 0, 0);
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 0)'));
        select topology.st_addedgenewfaces('test_topo', 1, 1, st_geomfromtext('LINESTRING(0 0, 10 0, 10 10, 0 0)'));
        select topology.st_newedgessplit('test_topo', 1, st_geomfromtext('POINT(10 5)'));
        select topology.st_addedgenewfaces('test_topo', 1, 2, st_geomfromtext('LINESTRING(0 0, 10 5)'));
        */
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Assert.AreEqual("1 (0 0)", Topology.Nodes[0].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[0], Read<LineString>("LINESTRING(0 0, 10 0, 10 10, 0 0)"));
        Assert.AreEqual(1, Topology.Edges.Length);
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual("1 1-1", Topology.Edges[0].ToString());
        Assert.AreEqual("1 1 -1 1 0", Topology.EdgeRels[1].ToString());
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[0], Read<Point>("POINT(10 5)"));
        Assert.AreEqual(2, Topology.Nodes.Length);
        Assert.AreEqual(2, Topology.Edges.Length);
        Assert.AreEqual("2 1-2", Topology.Edges[0].ToString());
        Assert.AreEqual("3 2-1", Topology.Edges[1].ToString());
        Assert.AreEqual("2 3 -3 1 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 2 -2 1 0", Topology.EdgeRels[3].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[1], Read<LineString>("LINESTRING(0 0, 10 5)"));
        Assert.AreEqual(2, Topology.Nodes.Length);
        Assert.AreEqual(3, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("2 1-2", Topology.Edges[0].ToString());
        Assert.AreEqual("3 2-1", Topology.Edges[1].ToString());
        Assert.AreEqual("4 1-2", Topology.Edges[2].ToString());
        Assert.AreEqual("2 -4 -3 2 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 4 -2 3 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 3 2 3 2", Topology.EdgeRels[4].ToString());
    }

    [TestMethod]
    public void TestSquare1()
    {
        /* Corresponding case in PostGIS
        select topology.droptopology('test_topo');
        select topology.createtopology('test_topo', 0, 0);
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 0)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(10 0)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(10 10)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 10)'));
        select topology.st_addisoedge('test_topo', 1, 2, st_geomfromtext('LINESTRING(0 0, 10 0)'));
        select topology.st_addedgenewfaces('test_topo', 2, 3, st_geomfromtext('LINESTRING(10 0, 10 10)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        select topology.st_addedgenewfaces('test_topo', 3, 4, st_geomfromtext('LINESTRING(10 10, 0 10)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        select topology.st_addedgenewfaces('test_topo', 4, 1, st_geomfromtext('LINESTRING(0 10, 0 0)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        */
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 10)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 10)"));
        Assert.AreEqual("1 (0 0)", Topology.Nodes[0].ToString());
        Assert.AreEqual("2 (10 0)", Topology.Nodes[1].ToString());
        Assert.AreEqual("3 (10 10)", Topology.Nodes[2].ToString());
        Assert.AreEqual("4 (0 10)", Topology.Nodes[3].ToString());
        Topology = TopologyEditor.AddIsoEdge(Topology, Topology.Nodes[0], Topology.Nodes[1], Read<LineString>("LINESTRING(0 0, 10 0)"));
        Assert.AreEqual("1 -1", Topology.NodeRels[1].ToString());
        Assert.AreEqual("2 -1", Topology.NodeRels[2].ToString());
        Assert.AreEqual("3 0", Topology.NodeRels[3].ToString());
        Assert.AreEqual("4 0", Topology.NodeRels[4].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[2], Read<LineString>("LINESTRING(10 0, 10 10)"));
        Assert.AreEqual("1 1-2", Topology.Edges[0].ToString());
        Assert.AreEqual("2 2-3", Topology.Edges[1].ToString());
        Assert.AreEqual("1 2 1 0 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 -2 -1 0 0", Topology.EdgeRels[2].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[2], Topology.Nodes[3], Read<LineString>("LINESTRING(10 10, 0 10)"));
        Assert.AreEqual("1 1-2", Topology.Edges[0].ToString());
        Assert.AreEqual("2 2-3", Topology.Edges[1].ToString());
        Assert.AreEqual("3 3-4", Topology.Edges[2].ToString());
        Assert.AreEqual("1 2 1 0 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 3 -1 0 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 -3 -2 0 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual(0, Topology.Faces.Length);
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[3], Topology.Nodes[0], Read<LineString>("LINESTRING(0 10, 0 0)"));
        Assert.AreEqual("1 1-2", Topology.Edges[0].ToString());
        Assert.AreEqual("2 2-3", Topology.Edges[1].ToString());
        Assert.AreEqual("3 3-4", Topology.Edges[2].ToString());
        Assert.AreEqual("4 4-1", Topology.Edges[3].ToString());
        Assert.AreEqual("1 2 -4 1 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 3 -1 1 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 4 -2 1 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 1 -3 1 0", Topology.EdgeRels[4].ToString());
        Assert.AreEqual(1, Topology.Faces.Length);
        Topology = TopologyEditor.RemEdgeNewFace(Topology, Topology.Edges[3]);
        Assert.AreEqual(3, Topology.Edges.Length);
        Assert.AreEqual(0, Topology.Faces.Length);
        Assert.AreEqual("1 2 1 0 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 3 -1 0 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 -3 -2 0 0", Topology.EdgeRels[3].ToString());
    }

    [TestMethod]
    public void TestSquare2()
    {
        /* Corresponding case in PostGIS
        select topology.droptopology('test_topo');
        select topology.createtopology('test_topo', 0, 0);
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 0)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(10 0)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(10 10)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 10)'));
        select topology.st_addisoedge('test_topo', 2, 1, st_geomfromtext('LINESTRING(10 0, 0 0)'));
        select topology.st_addedgenewfaces('test_topo', 2, 3, st_geomfromtext('LINESTRING(10 0, 10 10)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        select topology.st_addedgenewfaces('test_topo', 3, 4, st_geomfromtext('LINESTRING(10 10, 0 10)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        select topology.st_addedgenewfaces('test_topo', 4, 1, st_geomfromtext('LINESTRING(0 10, 0 0)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        */
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 10)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 10)"));
        Assert.AreEqual("1 (0 0)", Topology.Nodes[0].ToString());
        Assert.AreEqual("2 (10 0)", Topology.Nodes[1].ToString());
        Assert.AreEqual("3 (10 10)", Topology.Nodes[2].ToString());
        Assert.AreEqual("4 (0 10)", Topology.Nodes[3].ToString());
        Topology = TopologyEditor.AddIsoEdge(Topology, Topology.Nodes[1], Topology.Nodes[0], Read<LineString>("LINESTRING(10 0, 0 0)"));
        Assert.AreEqual("1 -1", Topology.NodeRels[1].ToString());
        Assert.AreEqual("2 -1", Topology.NodeRels[2].ToString());
        Assert.AreEqual("3 0", Topology.NodeRels[3].ToString());
        Assert.AreEqual("4 0", Topology.NodeRels[4].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[2], Read<LineString>("LINESTRING(10 0, 10 10)"));
        Assert.AreEqual("1 2-1", Topology.Edges[0].ToString());
        Assert.AreEqual("2 2-3", Topology.Edges[1].ToString());
        Assert.AreEqual("1 -1 2 0 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 -2 1 0 0", Topology.EdgeRels[2].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[2], Topology.Nodes[3], Read<LineString>("LINESTRING(10 10, 0 10)"));
        Assert.AreEqual("1 2-1", Topology.Edges[0].ToString());
        Assert.AreEqual("2 2-3", Topology.Edges[1].ToString());
        Assert.AreEqual("3 3-4", Topology.Edges[2].ToString());
        Assert.AreEqual("1 -1 2 0 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 3 1 0 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 -3 -2 0 0", Topology.EdgeRels[3].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[3], Topology.Nodes[0], Read<LineString>("LINESTRING(0 10, 0 0)"));
        Assert.AreEqual("1 2-1", Topology.Edges[0].ToString());
        Assert.AreEqual("2 2-3", Topology.Edges[1].ToString());
        Assert.AreEqual("3 3-4", Topology.Edges[2].ToString());
        Assert.AreEqual("4 4-1", Topology.Edges[3].ToString());
        Assert.AreEqual("1 -4 2 0 1", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 3 1 1 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 4 -2 1 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 -1 -3 1 0", Topology.EdgeRels[4].ToString());
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((10 0, 0 0, 0 10, 10 10, 10 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
    }

    [TestMethod]
    public void TestSquare3()
    {
        /* Corresponding case in PostGIS
        select topology.droptopology('test_topo');
        select topology.createtopology('test_topo', 0, 0);
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 0)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(10 10)'));
        select topology.st_addisoedge('test_topo', 1, 2, st_geomfromtext('LINESTRING(0 0, 10 0, 10 10)'));
        select topology.st_addedgenewfaces('test_topo', 2, 1, st_geomfromtext('LINESTRING(10 10, 0 10, 0 0)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        select topology.st_newedgessplit('test_topo', 2, st_geomfromtext('POINT(0 5)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        select topology.st_newedgessplit('test_topo', 1, st_geomfromtext('POINT(10 5)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        select topology.st_addedgenewfaces('test_topo', 2, 1, st_geomfromtext('LINESTRING(0 5, 10 5)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        */
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 10)"));
        Assert.AreEqual("1 (0 0)", Topology.Nodes[0].ToString());
        Assert.AreEqual("2 (10 10)", Topology.Nodes[1].ToString());
        Topology = TopologyEditor.AddIsoEdge(Topology, Topology.Nodes[0], Topology.Nodes[1], Read<LineString>("LINESTRING(0 0, 10 0, 10 10)"));
        Assert.AreEqual("1 -1", Topology.NodeRels[1].ToString());
        Assert.AreEqual("2 -1", Topology.NodeRels[2].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[0], Read<LineString>("LINESTRING(10 10, 0 10, 0 0)"));
        Assert.AreEqual("1 1-2", Topology.Edges[0].ToString());
        Assert.AreEqual("2 2-1", Topology.Edges[1].ToString());
        Assert.AreEqual("1 2 -2 1 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 1 -1 1 0", Topology.EdgeRels[2].ToString());
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[1], Read<Point>("POINT(0 5)"));
        Assert.AreEqual("1 1-2", Topology.Edges[0].ToString());
        Assert.AreEqual("3 2-3", Topology.Edges[1].ToString());
        Assert.AreEqual("4 3-1", Topology.Edges[2].ToString());
        Assert.AreEqual("1 3 -4 1 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("3 4 -1 1 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 1 -3 1 0", Topology.EdgeRels[4].ToString());
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[0], Read<Point>("POINT(10 5)"));
        Assert.AreEqual("3 2-3", Topology.Edges[0].ToString());
        Assert.AreEqual("4 3-1", Topology.Edges[1].ToString());
        Assert.AreEqual("5 1-4", Topology.Edges[2].ToString());
        Assert.AreEqual("6 4-2", Topology.Edges[3].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[2], Topology.Nodes[3], Read<LineString>("LINESTRING(0 5, 10 5)"));
        Assert.AreEqual(4, Topology.Nodes.Length);
        Assert.AreEqual(5, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(-1 5)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[4], Topology.Nodes[2], Read<LineString>("LINESTRING(-1 5, 0 5)"));
        Assert.AreEqual(6, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(11 5)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[3], Topology.Nodes[5], Read<LineString>("LINESTRING(10 5, 11 5)"));
        Assert.AreEqual(7, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((0 0, 0 5, 10 5, 10 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 5, 0 10, 10 10, 10 5, 0 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    public void TestSquare4()
    {
        /* Corresponding case in PostGIS
        select topology.droptopology('test_topo');
        select topology.createtopology('test_topo', 0, 0);
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 0)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(10 10)'));
        select topology.st_addisoedge('test_topo', 1, 2, st_geomfromtext('LINESTRING(0 0, 10 0, 10 10)'));
        select topology.st_addedgenewfaces('test_topo', 2, 1, st_geomfromtext('LINESTRING(10 10, 0 10, 0 0)'));
        select topology.st_newedgessplit('test_topo', 1, st_geomfromtext('POINT(0 5)'));
        select topology.st_newedgessplit('test_topo', 1, st_geomfromtext('POINT(10 5)'));
        select topology.st_addedgenewfaces('test_topo', 2, 1, st_geomfromtext('LINESTRING(0 5, 10 5)'));
        */
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 10)"));
        Assert.AreEqual("1 (0 0)", Topology.Nodes[0].ToString());
        Assert.AreEqual("2 (10 10)", Topology.Nodes[1].ToString());
        Topology = TopologyEditor.AddIsoEdge(Topology, Topology.Nodes[0], Topology.Nodes[1], Read<LineString>("LINESTRING(0 0, 10 0, 10 10)"));
        Assert.AreEqual("1 -1", Topology.NodeRels[1].ToString());
        Assert.AreEqual("2 -1", Topology.NodeRels[2].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[0], Read<LineString>("LINESTRING(10 10, 0 10, 0 0)"));
        Assert.AreEqual("1 1-2", Topology.Edges[0].ToString());
        Assert.AreEqual("2 2-1", Topology.Edges[1].ToString());
        Assert.AreEqual("1 2 -2 1 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 1 -1 1 0", Topology.EdgeRels[2].ToString());
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[1], Read<Point>("POINT(0 0.31)"));
        Assert.AreEqual("1 1-2", Topology.Edges[0].ToString());
        Assert.AreEqual("3 2-3", Topology.Edges[1].ToString());
        Assert.AreEqual("4 3-1", Topology.Edges[2].ToString());
        Assert.AreEqual("1 3 -4 1 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("3 4 -1 1 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 1 -3 1 0", Topology.EdgeRels[4].ToString());
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[0], Read<Point>("POINT(10 6.23)"));
        Assert.AreEqual("3 2-3", Topology.Edges[0].ToString());
        Assert.AreEqual("4 3-1", Topology.Edges[1].ToString());
        Assert.AreEqual("5 1-4", Topology.Edges[2].ToString());
        Assert.AreEqual("6 4-2", Topology.Edges[3].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[2], Topology.Nodes[3], Read<LineString>("LINESTRING(0 0.31, 10 6.23)"));
        Assert.AreEqual(4, Topology.Nodes.Length);
        Assert.AreEqual(5, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((0 0, 0 0.31, 10 6.23, 10 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 0.31, 0 10, 10 10, 10 6.23, 0 0.31))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    public void TestSquare5()
    {
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[0], Read<LineString>("LINESTRING(0 0, 0 10, 10 10, 10 0, 0 0)"));
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[0], Read<Point>("POINT (0 5)"));
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[1], Read<Point>("POINT (10 5)"));
        Assert.AreEqual("2 4 -5 0 1", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("4 5 -2 0 1", Topology.EdgeRels[4].ToString());
        Assert.AreEqual("5 2 -4 0 1", Topology.EdgeRels[5].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[2], Read<LineString>("LINESTRING(0 5, 10 5)"));
        Assert.AreEqual("2 4 -5 0 2", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("4 5 6 0 3", Topology.EdgeRels[4].ToString());
        Assert.AreEqual("5 2 -6 0 2", Topology.EdgeRels[5].ToString());
        Assert.AreEqual("6 -4 -2 3 2", Topology.EdgeRels[6].ToString());
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((0 0, 0 5, 10 5, 10 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 5, 0 10, 10 10, 10 5, 0 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    public void TestSquare6()
    {
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[0], Read<LineString>("LINESTRING(0 0, 0 10, 10 10, 10 0, 0 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Topology.Faces[0], Read<Point>("POINT(5 5)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[0], Read<LineString>("LINESTRING(5 5, 0 0)"));
        Assert.AreEqual("1 1 -2 0 1", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 -1 2 1 1", Topology.EdgeRels[2].ToString());
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((0 0, 0 10, 10 10, 10 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
    }

    [TestMethod]
    public void TestSquare7()
    {
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[0], Read<LineString>("LINESTRING(0 0, 0 10, 10 10, 10 0, 0 0)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[0], Read<LineString>("LINESTRING(0 0, 5 5, 5 2, 0 0)"));
        Assert.AreEqual("1 1 2 0 3", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 -1 -2 3 2", Topology.EdgeRels[2].ToString());
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((0 0, 5 5, 5 2, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 0, 5 5, 5 2, 0 0), (0 0, 0 10, 10 10, 10 0, 0 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    public void TestSquare8()
    {
        /* Corresponding case in PostGIS
        select topology.droptopology('test_topo');
        select topology.createtopology('test_topo', 0, 0);
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 0)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(10 0)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(10 10)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 10)'));
        select topology.st_addisoedge('test_topo', 1, 2, st_geomfromtext('LINESTRING(0 0, 10 0)'));
        select topology.st_addedgenewfaces('test_topo', 2, 3, st_geomfromtext('LINESTRING(10 0, 10 10)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        select topology.st_addedgenewfaces('test_topo', 3, 4, st_geomfromtext('LINESTRING(10 10, 0 10)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        select topology.st_addedgenewfaces('test_topo', 4, 1, st_geomfromtext('LINESTRING(0 10, 0 0)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        select topology.st_addedgenewfaces('test_topo', 2, 2, st_geomfromtext('LINESTRING(10 0, 5 5, 5 2, 10 0)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        */
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 10)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 10)"));
        Topology = TopologyEditor.AddIsoEdge(Topology, Topology.Nodes[0], Topology.Nodes[1], Read<LineString>("LINESTRING(0 0, 10 0)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[2], Read<LineString>("LINESTRING(10 0, 10 10)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[2], Topology.Nodes[3], Read<LineString>("LINESTRING(10 10, 0 10)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[3], Topology.Nodes[0], Read<LineString>("LINESTRING(0 10, 0 0)"));
        Assert.AreEqual(1, Topology.Faces.Length);
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[1], Read<LineString>("LINESTRING(10 0, 5 5, 5 2, 10 0)"));
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("1 -5 -4 2 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 3 -1 2 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 4 -2 2 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 1 -3 2 0", Topology.EdgeRels[4].ToString());
        Assert.AreEqual("5 5 2 3 2", Topology.EdgeRels[5].ToString());
        Assert.AreEqual("POLYGON ((10 0, 0 0, 0 10, 10 10, 10 0), (10 0, 5 2, 5 5, 10 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((10 0, 5 2, 5 5, 10 0))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    public void TestCross1()
    {
        /* Corresponding case in PostGIS
        select topology.droptopology('test_topo');
        select topology.createtopology('test_topo', 0, 0);
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 0)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(10 10)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(10 0)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 10)'));
        select topology.st_addisoedge('test_topo', 1, 2, st_geomfromtext('LINESTRING(0 0, 10 10)'));
        select * from test_topo.edge_data order by edge_id;
        select topology.st_newedgessplit('test_topo', 1, st_geomfromtext('POINT(5 5)'));
        select * from test_topo.edge_data order by edge_id;
        select topology.st_addedgenewfaces('test_topo', 3, 5, st_geomfromtext('LINESTRING(10 0, 5 5)'));
        select * from test_topo.edge_data order by edge_id;
        select topology.st_addedgenewfaces('test_topo', 5, 4, st_geomfromtext('LINESTRING(5 5, 0 10)'));
        select * from test_topo.edge_data order by edge_id;
        select topology.st_addedgenewfaces('test_topo', 2, 4, st_geomfromtext('LINESTRING(10 10, 0 10)'));
        select * from test_topo.edge_data order by edge_id;
        */
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 10)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 10)"));
        Topology = TopologyEditor.AddIsoEdge(Topology, Topology.Nodes[0], Topology.Nodes[1], Read<LineString>("LINESTRING(0 0, 10 10)"));
        Assert.AreEqual("1 -1 1 0 0", Topology.EdgeRels[1].ToString());
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[0], Read<Point>("POINT(5 5)"));
        Assert.AreEqual("2 3 2 0 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 -3 -2 0 0", Topology.EdgeRels[3].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[2], Topology.Nodes[4], Read<LineString>("LINESTRING(10 0, 5 5)"));
        Assert.AreEqual("2 3 2 0 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 -3 -4 0 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 -2 4 0 0", Topology.EdgeRels[4].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[4], Topology.Nodes[3], Read<LineString>("LINESTRING(5 5, 0 10)"));
        Assert.AreEqual("2 5 2 0 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 -3 -4 0 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 -2 4 0 0", Topology.EdgeRels[4].ToString());
        Assert.AreEqual("5 -5 3 0 0", Topology.EdgeRels[5].ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[3], Read<LineString>("LINESTRING(10 10, 0 10)"));
        Assert.AreEqual("2 5 2 0 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 6 -4 1 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 -2 4 0 0", Topology.EdgeRels[4].ToString());
        Assert.AreEqual("5 -6 3 0 1", Topology.EdgeRels[5].ToString());
        Assert.AreEqual("6 -5 -3 1 0", Topology.EdgeRels[6].ToString());
        Assert.AreEqual(1, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((10 10, 5 5, 0 10, 10 10))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
    }

    [TestMethod]
    public void TestIsland1()
    {
        /* Corresponding case in PostGIS
        select topology.droptopology('test_topo');
        select topology.createtopology('test_topo', 0, 0);
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 0)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(10 0)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(10 10)'));
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 10)'));
        select topology.st_addisoedge('test_topo', 1, 2, st_geomfromtext('LINESTRING(0 0, 10 0)'));
        select topology.st_addedgenewfaces('test_topo', 2, 3, st_geomfromtext('LINESTRING(10 0, 10 10)'));
        select topology.st_addedgenewfaces('test_topo', 3, 4, st_geomfromtext('LINESTRING(10 10, 0 10)'));
        select topology.st_addedgenewfaces('test_topo', 4, 1, st_geomfromtext('LINESTRING(0 10, 0 0)'));
        select topology.st_addisonode('test_topo', 1, st_geomfromtext('POINT(2 2)'));
        select topology.st_addisonode('test_topo', 1, st_geomfromtext('POINT(8 2)'));
        select topology.st_addisonode('test_topo', 1, st_geomfromtext('POINT(8 8)'));
        select topology.st_addisonode('test_topo', 1, st_geomfromtext('POINT(2 8)'));
        select topology.st_addisoedge('test_topo', 5, 6, st_geomfromtext('LINESTRING(2 2, 8 2)'));
        select topology.st_addedgenewfaces('test_topo', 6, 7, st_geomfromtext('LINESTRING(8 2, 8 8)'));
        select topology.st_addedgenewfaces('test_topo', 7, 8, st_geomfromtext('LINESTRING(8 8, 2 8)'));
        select topology.st_addedgenewfaces('test_topo', 8, 5, st_geomfromtext('LINESTRING(2 8, 2 2)'));;
        select topology.st_newedgessplit('test_topo', 4, st_geomfromtext('POINT(0 5)'));
        select topology.st_newedgessplit('test_topo', 8, st_geomfromtext('POINT(2 5)'));
        select topology.st_addedgenewfaces('test_topo', 9, 10, st_geomfromtext('LINESTRING(0 5, 2 5)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        */
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 10)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 10)"));
        Topology = TopologyEditor.AddIsoEdge(Topology, Topology.Nodes[0], Topology.Nodes[1], Read<LineString>("LINESTRING(0 0, 10 0)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[2], Read<LineString>("LINESTRING(10 0, 10 10)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[2], Topology.Nodes[3], Read<LineString>("LINESTRING(10 10, 0 10)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[3], Topology.Nodes[0], Read<LineString>("LINESTRING(0 10, 0 0)"));
        // above is same as Square1
        Topology = TopologyEditor.AddIsoNode(Topology, Topology.Faces[0], Read<Point>("POINT(2 2)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Topology.Faces[0], Read<Point>("POINT(8 2)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Topology.Faces[0], Read<Point>("POINT(8 8)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Topology.Faces[0], Read<Point>("POINT(2 8)"));
        Topology = TopologyEditor.AddIsoEdge(Topology, Topology.Nodes[4], Topology.Nodes[5], Read<LineString>("LINESTRING(2 2, 8 2)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[5], Topology.Nodes[6], Read<LineString>("LINESTRING(8 2, 8 8)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[6], Topology.Nodes[7], Read<LineString>("LINESTRING(8 8, 2 8)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[7], Topology.Nodes[4], Read<LineString>("LINESTRING(2 8, 2 2)"));
        Assert.AreEqual(8, Topology.Edges.Length);
        Assert.AreEqual("1 2 -4 2 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 3 -1 2 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 4 -2 2 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 1 -3 2 0", Topology.EdgeRels[4].ToString());
        Assert.AreEqual("5 6 -8 3 2", Topology.EdgeRels[5].ToString());
        Assert.AreEqual("6 7 -5 3 2", Topology.EdgeRels[6].ToString());
        Assert.AreEqual("7 8 -6 3 2", Topology.EdgeRels[7].ToString());
        Assert.AreEqual("8 5 -7 3 2", Topology.EdgeRels[8].ToString());
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((10 0, 0 0, 0 10, 10 10, 10 0), (8 2, 2 2, 2 8, 8 8, 8 2))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((8 2, 2 2, 2 8, 8 8, 8 2))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[3], Read<Point>("POINT(0 5)"));
        Assert.AreEqual("1 2 -10 2 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 3 -1 2 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 9 -2 2 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("5 6 -8 3 2", Topology.EdgeRels[5].ToString());
        Assert.AreEqual("6 7 -5 3 2", Topology.EdgeRels[6].ToString());
        Assert.AreEqual("7 8 -6 3 2", Topology.EdgeRels[7].ToString());
        Assert.AreEqual("8 5 -7 3 2", Topology.EdgeRels[8].ToString());
        Assert.AreEqual("9 10 -3 2 0", Topology.EdgeRels[9].ToString());
        Assert.AreEqual("10 1 -9 2 0", Topology.EdgeRels[10].ToString());
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[6], Read<Point>("POINT(2 5)"));
        Assert.AreEqual("1 2 -10 2 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 3 -1 2 0", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 9 -2 2 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("5 6 -12 3 2", Topology.EdgeRels[5].ToString());
        Assert.AreEqual("6 7 -5 3 2", Topology.EdgeRels[6].ToString());
        Assert.AreEqual("7 11 -6 3 2", Topology.EdgeRels[7].ToString());
        Assert.AreEqual("9 10 -3 2 0", Topology.EdgeRels[9].ToString());
        Assert.AreEqual("10 1 -9 2 0", Topology.EdgeRels[10].ToString());
        Assert.AreEqual("11 12 -7 3 2", Topology.EdgeRels[11].ToString());
        Assert.AreEqual("12 5 -11 3 2", Topology.EdgeRels[12].ToString());
        Assert.AreEqual("POLYGON ((10 0, 0 0, 0 5, 0 10, 10 10, 10 0), (8 2, 2 2, 2 5, 2 8, 8 8, 8 2))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((8 2, 2 2, 2 5, 2 8, 8 8, 8 2))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[8], Topology.Nodes[9], Read<LineString>("LINESTRING(0 5, 2 5)"));
        Assert.AreEqual("POLYGON ((10 0, 0 0, 0 5, 0 10, 10 10, 10 0), (8 2, 2 2, 2 5, 2 8, 8 8, 8 2))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((8 2, 2 2, 2 5, 2 8, 8 8, 8 2))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    public void TestIsland2()
    {
        /* Corresponding case in PostGIS
        select topology.droptopology('test_topo');
        select topology.createtopology('test_topo', 0, 0);
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 0)'));
        select topology.st_addedgenewfaces('test_topo', 1, 1, st_geomfromtext('LINESTRING(0 0, 10 0, 10 10, 0 10, 0 0)'));
        select topology.st_addisonode('test_topo', 1, st_geomfromtext('POINT(2 2)'));
        select topology.st_addedgenewfaces('test_topo', 2, 2, st_geomfromtext('LINESTRING(2 2, 8 2, 8 8, 2 8, 2 2)'));
        select topology.st_newedgessplit('test_topo', 1, st_geomfromtext('POINT(0 5)'));
        select topology.st_newedgessplit('test_topo', 2, st_geomfromtext('POINT(2 5)'));
        select topology.st_addedgenewfaces('test_topo', 3, 4, st_geomfromtext('LINESTRING(0 5, 2 5)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        */
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[0], Read<LineString>("LINESTRING(0 0, 10 0, 10 10, 0 10, 0 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Topology.Faces[0], Read<Point>("POINT(2 2)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[1], Read<LineString>("LINESTRING(2 2, 8 2, 8 8, 2 8, 2 2)"));
        Assert.AreEqual("1 1 -1 2 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 2 -2 3 2", Topology.EdgeRels[2].ToString());
        Assert.AreEqual(2, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual("POLYGON ((0 0, 0 10, 10 10, 10 0, 0 0), (2 2, 2 8, 8 8, 8 2, 2 2))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((2 2, 2 8, 8 8, 8 2, 2 2))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[0], Read<Point>("POINT(0 5)"));
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[0], Read<Point>("POINT(2 5)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[2], Topology.Nodes[3], Read<LineString>("LINESTRING(0 5, 2 5)"));
        Assert.AreEqual("3 7 -4 2 0", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("4 3 -3 2 0", Topology.EdgeRels[4].ToString());
        Assert.AreEqual("5 6 -6 3 2", Topology.EdgeRels[5].ToString());
        Assert.AreEqual("6 5 -7 3 2", Topology.EdgeRels[6].ToString());
        Assert.AreEqual("7 -5 4 2 2", Topology.EdgeRels[7].ToString());
        Assert.AreEqual("POLYGON ((0 5, 0 10, 10 10, 10 0, 0 0, 0 5), (2 5, 2 8, 8 8, 8 2, 2 2, 2 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((2 5, 2 8, 8 8, 8 2, 2 2, 2 5))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
    }

    [TestMethod]
    public void TestIsland3()
    {
        /* Corresponding case in PostGIS
        select topology.droptopology('test_topo');
        select topology.createtopology('test_topo', 0, 0);
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 0)'));
        select topology.st_addedgenewfaces('test_topo', 1, 1, st_geomfromtext('LINESTRING(0 0, 10 0, 10 10, 0 10, 0 0)'));
        select topology.st_addisonode('test_topo', 1, st_geomfromtext('POINT(2 2)'));
        select topology.st_addedgenewfaces('test_topo', 2, 2, st_geomfromtext('LINESTRING(2 2, 8 2, 8 8, 2 8, 2 2)'));
        select topology.st_newedgessplit('test_topo', 1, st_geomfromtext('POINT(0 5)'));
        select topology.st_newedgessplit('test_topo', 2, st_geomfromtext('POINT(2 5)'));
        select topology.st_addedgenewfaces('test_topo', 3, 4, st_geomfromtext('LINESTRING(0 5, 2 5)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        */
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[0], Read<LineString>("LINESTRING(0 0, 10 0, 10 10, 0 10, 0 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Topology.Faces[0], Read<Point>("POINT(2 2)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[1], Read<LineString>("LINESTRING(2 2, 4 2, 4 4, 2 4, 2 2)"));
        Assert.AreEqual("1 1 -1 2 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 2 -2 3 2", Topology.EdgeRels[2].ToString());
        Assert.AreEqual(2, Topology.Edges.Length);
        Assert.AreEqual(2, Topology.Faces.Length);
        Assert.AreEqual(new Envelope(0, 10, 0, 10), Topology.Faces[0].Bbox);
        Assert.AreEqual(new Envelope(2, 4, 2, 4), Topology.Faces[1].Bbox);
        Assert.AreEqual("POLYGON ((0 0, 0 10, 10 10, 10 0, 0 0), (2 2, 2 4, 4 4, 4 2, 2 2))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((2 2, 2 4, 4 4, 4 2, 2 2))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
        Topology = TopologyEditor.AddIsoNode(Topology, Topology.Faces[0], Read<Point>("POINT(6 6)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[2], Topology.Nodes[2], Read<LineString>("LINESTRING(6 6, 8 6, 8 8, 6 8, 6 6)"));
        Assert.AreEqual(3, Topology.Edges.Length);
        Assert.AreEqual(3, Topology.Faces.Length);
        Assert.AreEqual("1 1 -1 4 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 2 -2 3 4", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 3 -3 5 4", Topology.EdgeRels[3].ToString());
        Assert.AreEqual("POLYGON ((2 2, 2 4, 4 4, 4 2, 2 2))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[0]).ToString());
        Assert.AreEqual("POLYGON ((0 0, 0 10, 10 10, 10 0, 0 0), (2 2, 2 4, 4 4, 4 2, 2 2), (6 6, 6 8, 8 8, 8 6, 6 6))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[1]).ToString());
        Assert.AreEqual("POLYGON ((6 6, 6 8, 8 8, 8 6, 6 6))", TopologyEditor.GetFaceGeometry(Topology, Topology.Faces[2]).ToString());
    }

    [TestMethod]
    public void TestDangling1()
    {
        /* Corresponding case in PostGIS
        select topology.droptopology('test_topo');
        select topology.createtopology('test_topo', 0, 0);
        select topology.st_addisonode('test_topo', 0, st_geomfromtext('POINT(0 0)'));
        select topology.st_addisonode('test_topo', 1, st_geomfromtext('POINT(5 0)'));
        select topology.st_addisonode('test_topo', 2, st_geomfromtext('POINT(10 0)'));
        select topology.st_addedgenewfaces('test_topo', 1, 2, st_geomfromtext('LINESTRING(0 0, 5 0)'));
        select topology.st_addedgenewfaces('test_topo', 2, 3, st_geomfromtext('LINESTRING(5 0, 10 0)'));
        select topology.st_addedgenewfaces('test_topo', 2, 2, st_geomfromtext('LINESTRING(5 0, 5 5, 15 5, 15 -5, 5 -5, 5 0)'));
        select topology.st_newedgessplit('test_topo', 3, st_geomfromtext('POINT(15 5)'));
        select topology.st_addedgenewfaces('test_topo', 2, 2, st_geomfromtext('LINESTRING(10 0, 15 5)'));
        select edge_id,next_left_edge,next_right_edge,left_face,right_face from test_topo.edge_data order by edge_id;
        */
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(0 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(5 0)"));
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(10 0)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[1], Read<LineString>("LINESTRING(0 0, 5 0)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[2], Read<LineString>("LINESTRING(5 0, 10 0)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[1], Topology.Nodes[1], Read<LineString>("LINESTRING(5 0, 5 5, 15 5, 15 -5, 5 -5, 5 0)"));
        Assert.AreEqual("1 3 1 0 0", Topology.EdgeRels[1].ToString());
        Assert.AreEqual("2 -2 -3 1 1", Topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 -1 2 0 1", Topology.EdgeRels[3].ToString());
        Topology = TopologyEditor.NewEdgesSplit(Topology, Topology.Edges[2], Read<Point>("POINT(15 5)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[2], Topology.Nodes[3], Read<LineString>("LINESTRING(10 0, 15 5)"));
    }

    [TestMethod]
    public void TestAddIslands(){
        // All three linestrings added are closed linestrings
        // The first of the linestrings contains the other two
        var node1 = new Node(1, null, Read<Point>("POINT (585479.12 6251629.84)"));
        var node2 = new Node(2, null, Read<Point>("POINT (597134.95 6254725.32)"));
        var node3 = new Node(3, null, Read<Point>("POINT (597816.89 6254498.89)"));
        var ls1 = Read<LineString>("LINESTRING (585479.12 6251629.84, 585499.54 6251653.89, 585510.87 6251668.97, 585518.53 6251675.03, 585714.95 6251891.13, 585719 6251896.05, 585727.45 6251900.2, 585728.93 6251900.93, 585739.6 6251910.03, 585740.07 6251910.43, 585748.41 6251912.48, 585775.12 6251914.89, 585803.45 6251916.93, 585828.56 6251919.08, 585833.28 6251919.49, 585838.28 6251919.92, 585839.72 6251920.05, 585847.57 6251920.73, 585872.78 6251923.45, 585893.96 6251925.42, 585918.36 6251927.32, 585928.04 6251927.97, 585932.32 6251928.25, 585937.5 6251928.6, 585950.29 6251929.84, 585961.67 6251930.95, 585985.05 6251933.17, 586000 6251934.58, 586015.09 6251936.01, 586030.2 6251937.46, 586041.72 6251938.57, 586053.97 6251941.09, 586065.04 6251940.91, 586074.46 6251940.76, 586094.32 6251938.81, 586122.21 6251934.92, 586146.9 6251931.4, 586166.92 6251929.12, 586176.57 6251928.02, 586201.52 6251924.84, 586224.37 6251921.73, 586230.05 6251920.88, 586240.19 6251919.36, 586247.7 6251919.51, 586251.42 6251919.58, 586256.6 6251921.79, 586261.53 6251923.89, 586282.99 6251931.07, 586292.03 6251933.87, 586298.38 6251932.5, 586306.74 6251930.7, 586315.22 6251928.85, 586339.1 6251923.08, 586342.09 6251922.38, 586362.27 6251917.54, 586370.74 6251915.3, 586376.05 6251911.25, 586381.77 6251906.89, 586385.71 6251905.91, 586387.66 6251905.65, 586401.4 6251903.85, 586415.32 6251902.03, 586422.4 6251901, 586449.65 6251894.78, 586485.59 6251887.07, 586487.06 6251886.75, 586491.14 6251886.06, 586494.57 6251885.56, 586494.87 6251885.51, 586521.28 6251881.38, 586525.36 6251880.44, 586525.69 6251880.36, 586534.91 6251878.85, 586546.71 6251880.35, 586559.4 6251887.48, 586569.32 6251898.42, 586575.57 6251901.06, 586597.42 6251903.3, 586609.1 6251904.89, 586609.38 6251904.85, 586613.91 6251904.17, 586619.46 6251903.46, 586635.81 6251903.65, 586635.83 6251903.65, 586637.54 6251903.67, 586643.82 6251904.54, 586651.35 6251910.64, 586652.26 6251911.38, 586652.94 6251912.9, 586652.95 6251912.91, 586655.13 6251917.72, 586659.52 6251922.08, 586661.26 6251923.26, 586662.39 6251924.03, 586663.43 6251924.74, 586666.29 6251926.53, 586672.35 6251930.33, 586680.88 6251937.97, 586683.83 6251939.29, 586683.84 6251939.29, 586684.77 6251939.69, 586702.75 6251939.58, 586729.14 6251938.09, 586743.4 6251938.1, 586747.23 6251938.15, 586748.41 6251938.16, 586764.53 6251938.27, 586777.56 6251938.35, 586786.67 6251938.78, 586805.94 6251932.04, 586827.97 6251924.51, 586842.73 6251919.32, 586849.07 6251917.1, 586854.59 6251915.3, 586888.68 6251902.92, 586893.68 6251901.09, 586896.26 6251900.27, 586900.37 6251898.97, 586902.24 6251898.38, 586906.26 6251897.09, 586913.25 6251894.45, 586929.91 6251888.51, 586940.32 6251885.66, 586952.39 6251883.02, 586952.42 6251883.01, 586957.49 6251881.76, 586960.22 6251880.95, 586970.24 6251877.96, 586973.47 6251877.25, 586990.09 6251880.15, 586995.16 6251880.9, 587008.09 6251880.52, 587025.63 6251879.03, 587057.09 6251877.01, 587069.75 6251876.03, 587083.61 6251876.92, 587105.56 6251878.64, 587110.69 6251878.65, 587112.45 6251878.85, 587115.88 6251880.7, 587121.96 6251885.49, 587125.52 6251888.24, 587125.53 6251888.24, 587128.46 6251890.51, 587133.76 6251893.61, 587138.62 6251894.29, 587143.93 6251896.29, 587151.57 6251898.81, 587162.1 6251902.06, 587173.28 6251905.72, 587178.18 6251905.24, 587184.87 6251904.58, 587184.88 6251904.58, 587192.01 6251903.3, 587208.2 6251900.3, 587218.87 6251897.71, 587224.14 6251902.28, 587230.65 6251910.55, 587237.66 6251912.92, 587251.67 6251921.14, 587254.03 6251923.62, 587257.78 6251927.56, 587263.94 6251935.3, 587275.48 6251947.33, 587288.29 6251963, 587295.15 6251969.9, 587301.17 6251975.96, 587314.25 6251990.23, 587320.09 6251995.75, 587332.55 6251992.83, 587333.72 6251994.38, 587338.58 6251997.63, 587342.12 6252000, 587360.64 6252012.94, 587380.72 6252026.97, 587395.36 6252038.22, 587417.73 6252055.42, 587430.27 6252065.06, 587446.38 6252077.45, 587457.52 6252086.02, 587477.37 6252101.28, 587492.07 6252112.58, 587500.44 6252119.01, 587509.81 6252126.22, 587520.54 6252121.5, 587528.61 6252117.96, 587532.03 6252119.31, 587541.13 6252122.9, 587557.91 6252129.52, 587570.85 6252134.63, 587578.93 6252137.82, 587593.54 6252143.59, 587604.36 6252147.86, 587606.15 6252146.28, 587616.88 6252155.75, 587623.69 6252164.38, 587627.37 6252171.42, 587629.96 6252176.37, 587635.84 6252193.42, 587637.67 6252203.65, 587633.37 6252210.96, 587628.98 6252222.83, 587627.3 6252228.58, 587627.25 6252235.42, 587627.26 6252238.97, 587627.27 6252245.88, 587627.27 6252251.67, 587627.97 6252262.08, 587629.22 6252274.66, 587632.09 6252286.93, 587638.09 6252296.25, 587639.47 6252299.46, 587645.1 6252312.6, 587657.07 6252341.32, 587665.27 6252356.82, 587669.47 6252365.54, 587679.01 6252384.66, 587692.51 6252412.84, 587701.79 6252431.64, 587705.93 6252443.86, 587706.23 6252444.75, 587715.17 6252456.32, 587721.57 6252462.78, 587734.78 6252469.05, 587740.43 6252471.73, 587744.09 6252473.47, 587759.73 6252483.43, 587769.81 6252485.52, 587777.66 6252491.19, 587781.73 6252491.29, 587786.43 6252490.8, 587789.46 6252490.39, 587791.21 6252490.15, 587796.85 6252490.93, 587811.01 6252498.01, 587822.16 6252505.03, 587827.66 6252511.6, 587829.71 6252514.04, 587834.89 6252520.21, 587855.51 6252529.61, 587858.36 6252531.53, 587861.36 6252533.55, 587869.57 6252539.46, 587887.54 6252553.93, 587910.57 6252571.74, 587931.31 6252589.14, 587942.21 6252599.12, 587959.35 6252616.15, 587972.69 6252631.18, 587983.91 6252643.59, 587994.32 6252653.51, 587995.6 6252655.34, 587986.32 6252685.5, 587981.09 6252702.43, 587980.57 6252703.78, 587975.94 6252716.18, 587974.42 6252720.22, 587967.2 6252746.08, 587966.2 6252750.77, 587960.84 6252775.99, 587959.57 6252782.45, 587955.36 6252798.28, 587938.46 6252861.55, 587923.32 6252918.21, 587898.11 6253012.5, 587890.06 6253042.59, 587884.53 6253063.31, 587882.26 6253071.79, 587885.56 6253073.56, 587874.47 6253122.85, 587878.48 6253124.73, 587873.05 6253140.33, 587867.67 6253168.06, 587869.75 6253227.89, 587870.79 6253244.82, 587868.92 6253334.94, 587868.07 6253381.24, 587867.49 6253403.15, 587866.85 6253427.17, 587868.54 6253445.57, 587887.78 6253493.6, 587919.75 6253570.77, 587917.29 6253595.93, 588082.29 6254093.57, 588086.11 6254113.61, 588089.29 6254181.47, 588089.67 6254186.48, 588456.37 6254233.15, 588544.14 6254244.26, 588631.37 6254255.3, 588698.59 6254263.8, 588754.9 6254269.43, 588827.98 6254299.99, 588899.5 6254329.89, 588910.54 6254334.51, 588962 6254337.92, 589005.64 6254341.89, 589085.16 6254349.14, 589245.74 6254363.77, 589577.3 6254392.61, 589675.74 6254401.17, 589679.24 6254402.15, 589692.65 6254382.8, 589717.29 6254349.88, 589736.12 6254325.49, 589747.45 6254311.27, 589756.7 6254299.65, 589767.02 6254287.93, 589767.49 6254287.38, 589768.01 6254286.35, 589769.02 6254284.36, 589769.81 6254282.78, 589773.28 6254275.91, 589800.48 6254222.47, 589823.4 6254178.25, 589824.31 6254175.37, 589824.8 6254171.62, 589821.71 6254153.47, 589818.51 6254134.83, 589811.91 6254086.85, 589861.39 6254045.54, 589914.87 6254000.71, 589930.07 6253987.56, 589932.41 6253987.66, 589964.96 6253996.23, 590005.74 6254007.57, 590042.73 6254018.31, 590057.78 6254020.83, 590079.34 6254024.2, 590086.76 6254031.68, 590106.25 6254049.55, 590128.1 6254070.12, 590134.19 6254075.98, 590146.02 6254087.4, 590162.46 6254103.38, 590180.26 6254120.37, 590189.21 6254128.67, 590198.43 6254137.05, 590217.34 6254153.8, 590237.5 6254171.31, 590257.42 6254188.44, 590289.23 6254215.77, 590315.55 6254238.4, 590330.93 6254251.61, 590346.16 6254264.69, 590364.8 6254280.7, 590377.51 6254291.51, 590380.82 6254294.45, 590400.47 6254311.2, 590422.91 6254330.35, 590446.35 6254350.36, 590471.65 6254371.95, 590489.41 6254387.1, 590502.16 6254397.9, 590556.15 6254443.48, 590564.17 6254447.94, 590564.48 6254448.11, 590566.69 6254449.34, 590568.81 6254450.51, 590571.76 6254452.15, 590580.76 6254457.15, 590600.56 6254468.15, 590631.8 6254485.5, 590658.28 6254500.19, 590681.1 6254512.9, 590681.96 6254513.45, 590684.92 6254512.61, 590714.51 6254531.61, 590763.31 6254563.32, 590782.95 6254575.98, 590794.91 6254583.69, 590805.34 6254591.16, 590814.13 6254597.27, 590834.75 6254611.62, 590854.21 6254620.73, 590881.5 6254633.5, 590931.51 6254651.09, 590940.41 6254655.07, 590958.65 6254661.6, 590960.15 6254662.14, 590963.29 6254663.26, 590964.84 6254663.82, 590969.73 6254665.57, 590981.11 6254669.63, 591006.79 6254678.63, 591074.97 6254700.8, 591092.45 6254707.22, 591145.42 6254726.68, 591176.17 6254737.5, 591179.14 6254738.67, 591189.9 6254742.93, 591236.19 6254759.8, 591286.44 6254775.9, 591300.53 6254780.27, 591319.02 6254785.85, 591362.45 6254796.14, 591407.18 6254807.87, 591471.41 6254821.27, 591508.91 6254828.76, 591519.18 6254830.76, 591560.09 6254838.66, 591605.35 6254846.4, 591627.62 6254848.17, 591636.56 6254849.33, 591652.46 6254857.12, 591676.14 6254869.67, 591698.05 6254882.98, 591729.87 6254902.67, 591751.96 6254916.01, 591761.12 6254921.54, 591788.81 6254939.07, 591823.38 6254959.82, 591839.59 6254968.12, 591901.82 6254987.33, 591924.86 6254996.81, 591940.05 6255000.62, 591950.39 6255002.92, 591976.14 6255006.97, 591997.34 6255013.3, 592000 6255022.01, 592003.16 6255037.43, 592021.61 6255033.1, 592051.95 6255026.29, 592059.91 6255024.8, 592067.79 6255023.42, 592114.23 6255015.12, 592116.87 6255014.53, 592122.82 6255013.2, 592132.49 6255011.08, 592158.46 6255006.16, 592181.69 6255001.75, 592217.66 6254995.67, 592269.95 6254990.25, 592342.86 6254984.03, 592342.87 6254984.03, 592408.52 6254978.57, 592413.5 6254978.17, 592417.24 6254977.97, 592482.19 6254972.32, 592555.62 6254966.31, 592611.21 6254961.61, 592636.72 6254959.3, 592678.94 6254955.48, 592701.4 6254952.88, 592722.6 6254951.56, 592757.3 6254949.39, 592825.76 6254943.38, 592868.85 6254936.81, 592909.51 6254927.11, 592939.91 6254919.24, 592945.13 6254917.81, 593006.7 6254901.33, 593038.57 6254893.03, 593078.67 6254882.3, 593135.26 6254867.49, 593143.06 6254865.68, 593201.03 6254850.26, 593226.23 6254843.5, 593248.37 6254840.17, 593273.54 6254837.52, 593321.32 6254836.21, 593402.07 6254835.11, 593489.2 6254833.21, 593525.99 6254832.83, 593592.35 6254832, 593676.05 6254831.11, 593682.54 6254831.01, 593717.82 6254830.25, 593799.13 6254828.83, 593874 6254827.6, 593935.48 6254827.06, 594001.64 6254825.85, 594048.95 6254825, 594137.82 6254823.4, 594201.1 6254822.8, 594275.49 6254821.56, 594338.17 6254821.35, 594381.74 6254820.49, 594401.01 6254820.69, 594411.47 6254820.5, 594417.87 6254820.57, 594483.56 6254819.09, 594546.85 6254818.09, 594600.98 6254817.05, 594687.48 6254815.86, 594800.06 6254814.05, 594839.27 6254813.64, 594895.89 6254813.55, 595000.82 6254811.54, 595045.82 6254810.97, 595147.06 6254809.36, 595200.34 6254809.32, 595230.25 6254810.71, 595256.52 6254814.26, 595285.99 6254817.98, 595303.02 6254821.16, 595315.34 6254822.92, 595332.07 6254825.18, 595344.61 6254826.88, 595351.59 6254827.63, 595367.37 6254830.34, 595397.97 6254834.02, 595412.32 6254827.84, 595418.21 6254828.9, 595414.21 6254835.83, 595410.62 6254841.65, 595407.26 6254847.1, 595403 6254854.03, 595399.13 6254854.63, 595395.18 6254855.27, 595374.3 6254858.35, 595371.82 6254860.19, 595369.55 6254862.41, 595354.51 6254863.88, 595315.69 6254868.6, 595307.43 6254868.67, 595303.07 6254871.99, 595298.96 6254875.89, 595295.31 6254878.67, 595290.56 6254878.7, 595296.79 6254906.52, 595295.46 6254923.19, 595297.42 6254948.98, 595301.39 6254996.28, 595304.97 6255032.71, 595307.94 6255078.15, 595310.38 6255109.7, 595311.68 6255126.73, 595315.56 6255182.43, 595319.62 6255233.69, 595323.97 6255300.38, 595326.2 6255343.08, 595327.54 6255366.58, 595329.18 6255395.51, 595337.42 6255491.22, 595337.55 6255492.3, 595337.76 6255495.03, 595337.88 6255496.48, 595339.78 6255530.53, 595342.69 6255573.39, 595344.89 6255597.08, 595346.07 6255624.57, 595350.2 6255683.15, 595351.82 6255706.22, 595354.47 6255735.37, 595361.12 6255805.69, 595362.89 6255831.28, 595365.28 6255860.47, 595366.69 6255879.21, 595364.95 6255879.45, 595374.53 6256000, 595379.77 6256066.35, 595381.57 6256085.79, 595383.41 6256108.43, 595384.93 6256128.38, 595386.77 6256151.07, 595388.54 6256175.31, 595390.04 6256197.84, 595394.25 6256248.03, 595396.07 6256278.33, 595399.31 6256316.98, 595402.64 6256357.11, 595408.32 6256431.72, 595412.11 6256484.54, 595416.45 6256539.42, 595419.25 6256580.75, 595420.4 6256594.32, 595420.51 6256612.84, 595421.76 6256618.99, 595421.95 6256620.79, 595422.68 6256627.82, 595424.53 6256649.55, 595426.23 6256670.58, 595428.26 6256701.71, 595430.89 6256736.86, 595433.14 6256764.86, 595436.54 6256811.94, 595439.06 6256844.94, 595442.8 6256887.49, 595462.56 6256950.32, 595470.99 6256954.52, 595484.09 6256960, 595503.81 6256967.69, 595529.44 6256976.8, 595566.51 6256990.12, 595598.36 6257001.09, 595612.55 6257005.69, 595617.07 6257007.49, 595642.45 6257017.61, 595674.35 6257029.73, 595704.14 6257039.54, 595739.65 6257052.14, 595757.11 6257058.39, 595761.7 6257055.5, 595836.43 6257022.23, 595884.38 6257001.86, 595994.64 6256953.75, 596146.14 6256948.64, 596151.19 6256948.46, 596205.42 6256938.9, 596227.04 6256926.74, 596228.82 6256887.53, 596227.1 6256877.48, 596225.48 6256868.73, 596227.39 6256860.67, 596227.9 6256844.52, 596228.91 6256829.36, 596229.07 6256823.03, 596229.26 6256811.39, 596230.07 6256786.14, 596230.13 6256771.22, 596230.14 6256768.81, 596234.27 6256766.09, 596236.78 6256765.75, 596260.07 6256763.02, 596277.32 6256759.44, 596286.67 6256757.32, 596284.92 6256746.72, 596296 6256744.65, 596346.81 6256735.13, 596409.09 6256724.81, 596459.55 6256715.42, 596519.38 6256701.52, 596600.29 6256681.68, 596644.24 6256667.96, 596705.31 6256649.54, 596786.26 6256618.93, 596844.71 6256594.31, 596900.59 6256570.02, 596904.44 6256555.63, 596918.82 6256501.79, 596933.72 6256454.65, 596953.38 6256403.74, 596970.27 6256350.91, 596981.14 6256325.54, 596992.07 6256300.8, 597002.19 6256273.45, 597005.32 6256262.45, 597007.39 6256240.96, 597007.73 6256225.18, 597007.15 6256192.83, 597006.89 6256162.08, 597007.38 6256134.51, 597009.48 6256107.96, 597011.3 6256089.86, 597013.48 6256068.15, 597015.79 6256068, 597018.45 6256065.39, 597020.48 6256060.26, 597021.24 6256053.91, 597027.65 6256000, 597027.69 6255999.65, 597033.57 6255950.2, 597036.96 6255920.67, 597041.36 6255885.87, 597045.95 6255844.3, 597049.35 6255810.8, 597051.05 6255790.91, 597039.83 6255755.84, 597029.61 6255723.88, 597023.06 6255707.67, 597023.06 6255707.66, 597023.06 6255707.65, 597012.46 6255632.84, 597022.4 6255628.02, 597039.32 6255603.37, 597070.96 6255560.33, 597076.1 6255550.95, 597078.15 6255545.03, 597085.9 6255522.62, 597093.84 6255500.2, 597104.91 6255471.26, 597113.82 6255444.2, 597126.17 6255410.44, 597129.61 6255401.03, 597133.98 6255383.4, 597138.42 6255353.65, 597140.14 6255345.52, 597170.71 6255350.04, 597260.02 6255363.25, 597263.99 6255364.2, 597269.15 6255364.97, 597291.47 6255368.68, 597317.2 6255372.19, 597335.2 6255372.97, 597363.98 6255377.21, 597408.64 6255383.8, 597460.41 6255391.18, 597503.02 6255397.25, 597511.93 6255398.52, 597561.68 6255406.47, 597609.54 6255413.52, 597640.57 6255417.83, 597656.41 6255418.2, 597656.42 6255418.2, 597663.03 6255413.59, 597677.19 6255405.61, 597679.82 6255404.11, 597701.55 6255394.19, 597724.53 6255385.09, 597734.76 6255378.08, 597747.85 6255367.78, 597757.9 6255358.63, 597816.03 6255301.83, 597814.98 6255288.01, 597816.24 6255288.07, 597823.06 6255288.4, 597833.6 6255288.91, 597836.59 6255289.05, 597864.9 6255292.12, 597868.2 6255291.76, 597873.71 6255288.66, 597883.01 6255289.08, 597915.18 6255293.71, 597918.96 6255294.25, 597934.76 6255298.23, 597944.74 6255300.6, 597955.62 6255306.2, 597979.59 6255322.6, 597984.22 6255324.96, 598000.57 6255329.48, 598019.97 6255335.85, 598030.04 6255334.69, 598038.42 6255331.94, 598046.44 6255331.62, 598100.17 6255330.81, 598127.22 6255335.01, 598143.66 6255341.85, 598152.93 6255348.16, 598173.38 6255349.32, 598180.07 6255354.13, 598228.22 6255362.99, 598271.97 6255376.12, 598283.25 6255358.61, 598308.05 6255321, 598314.79 6255310.87, 598316.16 6255309.22, 598318.33 6255305.26, 598320.99 6255300.41, 598321.5 6255298.94, 598331.73 6255282.87, 598349.41 6255256.03, 598363.79 6255234.19, 598371 6255223.35, 598376.01 6255215.8, 598392.93 6255190.29, 598410.22 6255160.57, 598412.34 6255156.93, 598424.63 6255138.31, 598438.08 6255117.99, 598438.1 6255117.96, 598439.3 6255114.97, 598449.35 6255089.88, 598455.55 6255074.4, 598468.81 6255041.3, 598485.67 6255006.81, 598485.73 6255006.7, 598485.81 6255006.54, 598494.95 6254987.84, 598502.75 6254971.89, 598511.65 6254953.68, 598519.3 6254938, 598583.17 6254815.79, 598615.1 6254763.51, 598617.41 6254764.87, 598623.01 6254768.16, 598629.78 6254772.13, 598630.2 6254772.38, 598631.41 6254777.18, 598638.27 6254785.5, 598639.18 6254790.5, 598699.72 6254794.93, 598754.06 6254797.37, 598823.89 6254800.5, 598824.45 6254800.52, 598830.91 6254800.81, 598830.92 6254800.81, 598855.21 6254801.14, 598896.49 6254801.7, 598896.5 6254801.7, 598895.73 6254678.56, 598895.73 6254678.05, 598895.6 6254657.75, 598894.98 6254559.04, 598912.32 6254559.21, 598918.7 6254525.13, 598900.78 6254470.19, 598906.55 6254469.41, 598925.9 6254468.08, 598936.09 6254465.35, 598960.84 6254462.11, 599003.9 6254459.17, 599026.18 6254459.67, 599035.41 6254459.88, 599049 6254460.28, 599054.85 6254458.05, 599060.69 6254458.58, 599061.74 6254456.69, 599063.24 6254453.99, 599064.13 6254452.38, 599065.54 6254451.05, 599066.59 6254449.95, 599070.04 6254452.15, 599072.32 6254452.51, 599075.18 6254452.97, 599120.23 6254444.79, 599125.87 6254443.5, 599165.51 6254438.09, 599211.53 6254429.41, 599274.66 6254419.26, 599290.39 6254416.05, 599300.69 6254414.66, 599320.39 6254411.88, 599346.91 6254407.47, 599363.72 6254405.01, 599368.61 6254404.22, 599376.92 6254403.01, 599382.24 6254400.91, 599400.06 6254397.77, 599422.74 6254396.71, 599453.61 6254395.95, 599481.3 6254393.95, 599506.38 6254392.92, 599548.06 6254389.58, 599559.16 6254388.7, 599560.32 6254388.62, 599560.84 6254388.62, 599565.42 6254388.62, 599587.7 6254387.66, 599623.47 6254386.86, 599654.02 6254384.4, 599667.89 6254383.28, 599761.66 6254375.87, 599931.71 6254354.86, 599999.32 6254346.51, 599957.97 6254113.54, 599986.04 6254096.07, 600031.88 6254084.9, 600090.81 6254064.58, 600098.56 6254061.19, 600116.82 6254056.2, 600146.39 6254048.09, 600160.45 6254041.71, 600162.71 6254040.17, 600168.91 6254037.43, 600180.56 6254034, 600195.88 6254029.48, 600200.6 6254028.1, 600214.52 6254022.54, 600197.05 6253968.89, 600149.28 6253819.9, 600188.89 6253804.41, 600160.28 6253701.35, 600202.99 6253673.89, 600209.7 6253672.18, 600230.07 6253667, 600242.53 6253669.71, 600322.07 6253715.39, 600357.31 6253737.8, 600374.74 6253749.27, 600404.17 6253766.35, 600409.31 6253769.15, 600417.17 6253772.41, 600440.17 6253778.82, 600466.3 6253787.73, 600485.38 6253794.94, 600502.59 6253801.02, 600522.37 6253809.52, 600550.18 6253819.35, 600561.96 6253822.17, 600570.39 6253823.54, 600576.85 6253825.26, 600624.21 6253843.73, 600634.36 6253847.05, 600654.44 6253855.46, 600681.43 6253867.17, 600712.68 6253877.71, 600717.79 6253879.51, 600735.72 6253884.2, 600769.89 6253892.26, 600800.66 6253900.68, 600816.06 6253905.38, 600826.68 6253910.07, 600849.66 6253916.58, 600872.6 6253921.57, 600897.73 6253925.1, 600918.36 6253930.11, 600929.96 6253933.99, 600940.38 6253938.45, 600956.19 6253945.63, 600967.2 6253949.5, 600982.06 6253953.41, 601003.16 6253954.94, 601020.32 6253953.36, 601040.54 6253950.54, 601050.54 6253948.46, 601068.86 6253903.08, 601166.14 6253671.05, 601188.7 6253525.4, 601394.86 6253372.74, 601714.51 6253283.51, 601812.35 6253298.82, 601795.58 6253156.54, 601805.05 6252959.11, 601829.65 6252918.58, 602089.61 6252891.56, 602163.22 6252748.33, 602320.34 6252732.25, 602384.94 6252709.5, 602701.31 6252598.11, 602670.25 6252334.13, 603071.45 6252215.67, 603124.25 6252159.61, 603129.68 6252156.04, 603167.55 6252089.44, 603155.36 6252041.1, 603194.41 6252014.1, 603182.13 6251928.47, 603177.76 6251919.03, 603162.64 6251900.32, 603136.22 6251876.29, 602845.29 6251803.4, 602798.34 6251783.33, 602557.17 6252046.99, 602425.99 6252006.9, 602398.99 6252021.31, 601886.06 6252295.02, 601858.64 6252291.35, 601805.71 6252666.49, 601791.99 6252668.65, 601538.9 6252594.95, 601534.15 6252625.49, 601736.93 6252744.24, 601743.55 6252778.5, 601743.58 6252779.34, 601497.48 6252847.9, 601471.69 6252849.96, 601456.08 6252829.92, 601245.85 6252846.48, 601135.68 6252832.68, 601110.35 6252836.28, 601027.49 6252829.93, 600979.2 6252779.26, 600728.51 6252665.06, 600728.5 6252665.06, 600604.92 6252678.34, 600561.62 6252671.15, 600458.71 6252683.47, 600435.88 6252349.6, 600344.73 6252224.85, 600343.59 6252183.93, 600282.88 6252142.2, 600091.23 6252151.92, 600090.06 6252150.92, 599914.47 6252103.98, 599880.64 6252083.78, 599734.55 6252097.55, 599618.23 6252057.24, 599377.43 6251878.99, 599264.17 6251791.85, 599231.45 6251678.72, 599215.69 6251675.57, 598968.15 6251485.37, 598783.8 6251354.05, 598769.9 6251344.15, 598769.89 6251344.15, 598758.8 6251347.74, 598446.27 6251448.91, 598201.82 6251351.52, 598215.48 6251258.33, 598282.2 6250803.24, 598240.91 6250707.72, 598152.59 6250503.38, 598104.9 6250511.92, 597866.92 6250584.11, 597745.01 6250626.26, 597689.84 6250650.79, 597654.2 6250666.63, 597635.82 6250675.61, 597507.86 6250635.51, 597188 6250584.61, 597150.76 6250519.14, 596871.71 6250028.53, 596823.21 6249949.95, 596803.09 6249761.42, 597065.85 6249628.66, 597096.86 6249469.56, 597167.2 6249254.99, 597172.71 6249243.35, 597101.49 6248986.89, 597027.16 6248950.18, 597009.83 6249035.38, 596981.16 6249031.03, 596973.26 6248976.38, 597026.78 6248949.77, 597021.41 6248922.63, 597313.02 6248662.6, 597230.84 6248533.44, 597209.44 6248506.1, 597082.14 6248236.11, 597070.44 6248211.3, 597065.22 6248210.99, 597049.25 6248239.52, 596954.08 6248409.53, 596779.51 6248647, 596773.16 6248658.86, 596764.31 6248696.45, 596779.01 6248749.33, 596843.81 6248758.16, 596833.49 6248800.07, 596857.72 6248829.35, 596866.82 6248834.72, 596873.18 6248845.63, 596865.7 6248878.32, 596868.83 6248889.51, 596884.57 6248902.64, 596887.35 6248910.38, 596875.34 6248949.84, 596900.87 6248985.56, 596886.79 6249021.36, 596861.37 6249038.81, 596855.8 6249039.75, 596812.45 6249051.94, 596799.69 6249063.55, 596698.26 6249706.25, 596490.11 6249631.86, 596142.85 6249507.75, 596142.83 6249507.75, 596066.03 6249530.85, 595889.82 6249487.64, 595589.25 6249333.27, 595564.63 6249309.38, 595803.33 6248453.48, 595687.86 6248379.59, 595687.25 6248379.19, 595684.42 6248377.38, 595543.26 6248287.04, 595505.14 6248262.65, 595501.3 6248260.19, 595495.84 6248261.23, 595275.2 6248303.25, 595272.85 6248305.37, 595142.98 6248359.32, 594968.06 6248432, 594816.13 6248444.22, 594681.22 6248528.66, 594605.02 6248548.08, 594438.06 6248527.03, 594438.05 6248527.03, 594362.15 6248514.84, 594125.64 6248395.2, 594116.22 6248390.44, 594279.84 6247735.31, 593790.86 6247664.21, 593785.2 6247661.27, 593679.86 6247530.46, 593326.12 6247607.76, 593200.65 6247302.02, 593126.38 6247259.62, 593070.74 6247105.98, 592901.72 6246989.13, 592865.71 6246925.08, 592725.91 6246846.83, 592676.97 6246829.85, 592311.42 6246944.29, 592094.36 6246576.02, 592072.08 6246556.8, 591667.77 6246737.62, 591585.26 6246774.52, 591584.02 6246775.08, 591584.12 6246775.34, 591617.97 6246862.89, 591628.1 6246889.1, 591614.73 6247165.74, 591398.43 6247354.14, 591305.17 6247332.41, 590979.78 6247256.58, 590976.33 6247255.78, 590845.82 6247363.68, 590850.43 6247452.9, 590826.85 6247608.98, 590826.56 6247610.96, 590808.07 6247733.35, 590443.96 6247941.51, 590310.93 6247788.78, 590309.46 6247787.1, 590155.2 6247609.99, 590010.41 6247699.9, 589951.63 6248074.36, 589909.81 6248099.86, 589864.89 6248140.98, 589864.17 6248141.63, 589844.32 6248159.8, 589834.84 6248168.82, 589765.13 6248274.74, 589676.22 6248270.98, 589673.18 6248270.85, 589458.27 6248261.76, 589337.3 6248276.1, 589247.91 6248297.21, 589361.71 6248578.15, 589362.05 6248578.98, 589406.67 6248689.14, 589180.68 6248878.42, 589180.67 6248878.42, 588966.05 6249058.18, 588967.76 6249335.58, 588940.56 6249360.13, 588926.87 6249372.12, 588926.48 6249372.46, 588887.79 6249406.34, 588879.1 6249414.68, 588794.69 6249438.1, 588270.28 6249841.34, 587931.7 6249708.17, 587910.94 6249694.92, 587398.68 6249055.88, 586917.47 6249213.64, 586634.22 6249100.77, 586352.13 6249293.1, 586349.33 6249295.01, 586140.31 6249437.52, 586064.04 6249489.52, 585781.99 6249681.82, 585797.32 6249713.38, 585876.66 6249891.01, 585864.03 6250027.18, 585853.73 6250138.3, 585840.48 6250171.43, 585780.78 6250320.7, 585769.54 6250338, 585803.77 6250375.92, 585819.07 6250393.23, 585838.48 6250415.18, 585861.28 6250440.39, 585868.15 6250447.98, 585881.07 6250462.29, 585881.85 6250462.97, 585875.84 6250469.6, 585873.36 6250472.52, 585866.49 6250480.6, 585850.69 6250496.28, 585847.57 6250502.69, 585858.15 6250513.18, 585858.22 6250514.81, 585856.37 6250525.13, 585809.24 6250597.78, 585737.65 6250637.46, 585688.18 6250704.18, 585679.39 6250716.01, 585678.65 6250742.83, 585678.4 6250751.98, 585679.61 6250762.42, 585681.12 6250775.67, 585681.23 6250776.57, 585701 6250843.34, 585703.54 6250847.01, 585705.28 6250849.51, 585740.07 6250974.39, 585729.2 6251079.63, 585719.49 6251173.54, 585717.81 6251189.75, 585716.07 6251206.61, 585713.07 6251240.99, 585712.53 6251247.21, 585712.06 6251252.61, 585702.71 6251359.56, 585701.47 6251363.73, 585689.41 6251374.29, 585682.52 6251380.18, 585669.32 6251391.45, 585648.72 6251406.5, 585645.93 6251414.93, 585624.46 6251433.27, 585610.01 6251449.6, 585587.16 6251472.73, 585565.85 6251498.14, 585546.16 6251526.53, 585528.35 6251554.63, 585518.42 6251570.91, 585517.56 6251572.32, 585502.88 6251595.35, 585489.29 6251616.83, 585486.82 6251620.89, 585483.71 6251624.51, 585479.12 6251629.84)");
        var ls2 = Read<LineString>("LINESTRING (597134.95 6254725.32, 597135.36 6254722.7, 597160.2 6254692.1, 597183.09 6254665.03, 597195.72 6254649.32, 597211.07 6254595.52, 597212.93 6254588.25, 597213.78 6254582.25, 597185.89 6254542.63, 597174.38 6254526.27, 597156.21 6254498.7, 597153.57 6254484.68, 597179.25 6254475.83, 597185.67 6254475.33, 597195.25 6254472.01, 597212.81 6254465.68, 597255.1 6254452.31, 597258.5 6254451.43, 597261.98 6254450.76, 597275.44 6254449.43, 597311.56 6254443.79, 597326.49 6254449.75, 597337.93 6254457.16, 597330.18 6254489.99, 597320.84 6254546.69, 597280.05 6254578.61, 597270.04 6254623.66, 597218.55 6254715.11, 597141.46 6254774.09, 597134.95 6254725.32)");
        var ls3 = Read<LineString>("LINESTRING (597816.89 6254498.89, 597931.87 6254482.83, 597941.19 6254512.88, 597983.39 6254544.82, 598027.11 6254545.78, 598105.91 6254547.51, 598137.97 6254543.67, 598167.99 6254426.31, 598060.97 6254414.09, 598020.54 6254403.48, 598000 6254396.83, 597963.64 6254385.24, 597951.41 6254382.72, 597851.72 6254375.54, 597845.85 6254383.83, 597840.66 6254391.8, 597835.57 6254401.72, 597829.53 6254414.16, 597825 6254427.17, 597821.32 6254436.95, 597816.89 6254498.89)");
        
        var nodes = new[] { node1, node2, node3 }.ToImmutableArray();
        var nodeRels = nodes.Select(n => new NodeRel(n, Face.Universe)).ToDictionary(k => k.Node.Id, v => v);
        var topology = new Topology() with { Nodes = nodes, NodeRels = nodeRels.ToImmutableDictionary() };

        // -----
        // Den "store ring" tilføjes først og får Face 1
        topology = TopologyEditor.AddEdgeNewFaces(topology, topology.Nodes.Where(n => n.Point == ls1.StartPoint).First(), topology.Nodes.Where(n => n.Point == ls1.EndPoint).First(), ls1);
        Assert.AreEqual(1, topology.Faces.Length);
        Assert.AreEqual("1 1 -1 0 1", topology.EdgeRels[1].ToString());
        Assert.AreEqual("1 -1", topology.NodeRels[1].ToString());
        // Noderne 2 og 3 ligger nu i Face 1
        Assert.AreEqual("2 1", topology.NodeRels[2].ToString());
        Assert.AreEqual("3 1", topology.NodeRels[3].ToString());

        // -----
        // Den ene ø tilføjes - får id 3, og den store Face får nu id 2
        topology = TopologyEditor.AddEdgeNewFaces(topology, topology.Nodes.Where(n => n.Point == ls2.StartPoint).First(), topology.Nodes.Where(n => n.Point == ls2.EndPoint).First(), ls2);
        Assert.AreEqual(2, topology.Faces.Length);
        Assert.AreEqual("1 1 -1 0 2", topology.EdgeRels[1].ToString()); // Stor face er nu id 2
        Assert.AreEqual("2 2 -2 3 2", topology.EdgeRels[2].ToString());
        Assert.AreEqual("1 -1", topology.NodeRels[1].ToString());
        // Node 2 is now non-isolated
        Assert.AreEqual("2 -1", topology.NodeRels[2].ToString());
        // Node 3 is now in face 2
        Assert.AreEqual("3 2", topology.NodeRels[3].ToString());

        // -----
        // Den anden ø tilføjes - får id 4, og den store Face får nu id 5
        topology = TopologyEditor.AddEdgeNewFaces(topology, topology.Nodes.Where(n => n.Point == ls3.StartPoint).First(), topology.Nodes.Where(n => n.Point == ls3.EndPoint).First(), ls3);
        Assert.AreEqual(3, topology.Faces.Length);
        Assert.AreEqual("1 1 -1 0 5", topology.EdgeRels[1].ToString()); // Stor face er nu id 5
        Assert.AreEqual("2 2 -2 3 5", topology.EdgeRels[2].ToString());
        Assert.AreEqual("3 3 -3 5 4", topology.EdgeRels[3].ToString());
        Assert.AreEqual("1 -1", topology.NodeRels[1].ToString());
        // Nodes 2 and 3 are now non-isolated
        Assert.AreEqual("2 -1", topology.NodeRels[2].ToString());
        Assert.AreEqual("3 -1", topology.NodeRels[3].ToString());
    }

        [TestMethod]
    public void TestAddEdgeWithEid()
    {
        Topology = TopologyEditor.AddIsoNode(Topology, Face.Universe, Read<Point>("POINT(5 5)"));
        Topology = TopologyEditor.AddEdgeNewFaces(Topology, Topology.Nodes[0], Topology.Nodes[0], Read<LineString>("LINESTRING(5 5, 0 0, 10 0, 5 5)"), 999);
        Assert.AreEqual(999, Topology.Edges[0].Eid);
    }
}