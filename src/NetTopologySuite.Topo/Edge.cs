using System.Collections.Immutable;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;

namespace NetTopologySuite.Topo;

public sealed record Edge(int Id, int? Eid, Node StartNode, Node EndNode, LineString LineString) : EntityBase(Id, Eid)
{
    public record SplitPart(LineString LineString, object Context);

    private static List<ISegmentString> ExtractNodedSegmentStrings(Geometry geom, object context)
    {
        List<ISegmentString> list = [];
        foreach (Geometry line in LinearComponentExtracter.GetLines(geom))
            list.Add(new NodedSegmentString(line.Coordinates, context));
        return list;
    }

    public static ImmutableArray<SplitPart> Split(ImmutableArray<Edge> edges, LineString lineString, double tolerance)
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory();
        var intAdder = new SnapRoundingIntersectionAdder(tolerance);
        var noder = new MCIndexNoder(intAdder, tolerance);
        List<ISegmentString> ss = [];
        foreach (var e in edges)
            ss.AddRange(ExtractNodedSegmentStrings(e.LineString, e));
        ss.AddRange(SegmentStringUtil.ExtractNodedSegmentStrings(lineString));
        noder.ComputeNodes(ss);
        var nodalResult = noder.GetNodedSubstrings();
        var splitParts = nodalResult
            .Select(r => new SplitPart(factory.CreateLineString(r.Coordinates), r.Context))
            .ToImmutableArray();
        return splitParts;
    }

    public override string ToString() =>
        $"{Id} {StartNode.Id}-{EndNode.Id}";
}
