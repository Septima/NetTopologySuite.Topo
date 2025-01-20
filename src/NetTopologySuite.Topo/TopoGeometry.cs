using System.Collections.Immutable;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNG;

namespace NetTopologySuite.Topo;

public record TopoGeometry(int Id, int? Eid, ImmutableArray<Face> Faces) : EntityBase(Id, Eid)
{
    /*public Geometry Materialize()
    {
        var polygons = Faces.Select(f => f.Polygonize());
        MultiPolygon multiPolygon = polygons.First().Factory.CreateMultiPolygon(polygons.ToArray());
        var coverage = CoverageUnion.Union(multiPolygon);
        if (coverage is not Polygon && coverage is not MultiPolygon)
            throw new TopologyException("Invalid coverage");
        return coverage;
    }*/
}
