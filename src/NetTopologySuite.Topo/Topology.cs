using System.Collections.Immutable;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Topo;

public record Topology(ImmutableArray<Node> Nodes, ImmutableArray<Edge> Edges, ImmutableArray<Face> Faces, ImmutableArray<TopoGeometry> TopoGeometries, ImmutableDictionary<int, NodeRel> NodeRels, ImmutableDictionary<int, EdgeRel> EdgeRels)
{
    public Topology() : this([], [], [], [], ImmutableDictionary<int, NodeRel>.Empty, ImmutableDictionary<int, EdgeRel>.Empty) { }

    public ImmutableArray<Edge> GetDWithinEdges(LineString lineString, double distance) =>
        Edges.Where(e => e.LineString.IsWithinDistance(lineString, distance)).ToImmutableArray();
    public ImmutableArray<Edge> GetDWithinEdges(Point point, double distance) =>
        Edges.Where(e => e.LineString.IsWithinDistance(point, distance)).ToImmutableArray();
    public ImmutableArray<Node> GetDWithinNodes(Point point, double distance) =>
        Nodes.Where(n => n.Point.IsWithinDistance(point, distance)).ToImmutableArray();
    public Node GetNodeByPoint(Point point) =>
        Nodes.Where(n => n.Point.Coordinate.Equals2D(point.Coordinate)).First();
}
