using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Precision;

namespace NetTopologySuite.Topo;

public static class LineStringExtensions
{
    public static IEnumerable<LineString> Split(this LineString ls1, Geometry ls2, double tolerance)
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory();
        var intersection = ls1.Intersection(ls2);
        var intersectionPoints = intersection.Coordinates;
        if (intersectionPoints.Length == 0)
            return [ls1];
        var intAdder = new SnapRoundingIntersectionAdder(tolerance);
        var noder = new MCIndexNoder(intAdder, tolerance);
        var ss1 = SegmentStringUtil.ExtractNodedSegmentStrings(ls1);
        var ss2 = SegmentStringUtil.ExtractNodedSegmentStrings(ls2);
        var ss = ss1.Concat(ss2).ToList();
        noder.ComputeNodes(ss);
        var nodalResult = noder.GetNodedSubstrings();
        var result = nodalResult
            .Where(r => r.Context as Geometry != ls2)
            .Select(r => factory.CreateLineString(r.Coordinates));
        return result;
    }

    public static Point GetClosest(this LineString ls, Point p, double tolerance)
    {
        if (!ls.IsWithinDistance(p, tolerance))
            throw new TopologyException($"Point {p} not within tolerance {tolerance} of LineString {ls}");
        var closestVertex = ls.Coordinates.FirstOrDefault(c => p.IsWithinDistance(p.Factory.CreatePoint(c), tolerance));
        if (closestVertex != null)
            return p.Factory.CreatePoint(closestVertex);
        LocationIndexedLine indexedLine = new(ls);
        LinearLocation location = indexedLine.Project(p.Coordinate);
        Coordinate closestCoordinate = indexedLine.ExtractPoint(location);
        var closestCoordinate2D = new Coordinate(closestCoordinate.X, closestCoordinate.Y);
        var closestPoint = p.Factory.CreatePoint(closestCoordinate2D);
        closestPoint.Apply(new CoordinatePrecisionReducerFilter(new PrecisionModel(1 / tolerance)));
        return closestPoint;
    }

    public static (LineString ls1, LineString ls2) Split(this LineString ls, Point p)
    {
        LocationIndexedLine indexedLine = new(ls);
        LinearLocation location = indexedLine.IndexOf(p.Coordinate);
        LineString ls1 = (indexedLine.ExtractLine(new LinearLocation(), location) as LineString)!;
        LineString ls2 = (indexedLine.ExtractLine(location, LinearLocation.GetEndLocation(ls)) as LineString)!;
        var force2D = new GeometryEditor.CoordinateSequenceOperation((seq, g) =>
        {
            var factory = g.Factory.CoordinateSequenceFactory;
            var newSeq = factory.Create(seq.Count, Ordinates.Y | Ordinates.Y);
            for (int i = 0; i < seq.Count; i++)
            {
                newSeq.SetOrdinate(i, 0, seq.GetX(i));
                newSeq.SetOrdinate(i, 1, seq.GetY(i));
            }
            return newSeq;
        });
        GeometryEditor editor = new();
        ls1 = (editor.Edit(ls1, force2D) as LineString)!;
        Coordinate[] coordinates = ls1.Coordinates;
        coordinates[^1] = p.Coordinate;
        ls1 = new LineString(coordinates);
        coordinates = ls2.Coordinates;
        coordinates[0] = p.Coordinate;
        ls2 = new LineString(coordinates);
        return (ls1, ls2);
    }

    private class Force2DFilter : ICoordinateFilter
    {
        public void Filter(Coordinate coord) => coord.Z = Coordinate.NullOrdinate;
    }

}
