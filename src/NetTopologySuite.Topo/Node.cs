using NetTopologySuite.Geometries;

namespace NetTopologySuite.Topo;

public sealed record Node(int Id, int? Eid, Point point) : EntityBase(Id, Eid)
{
    public Point Point { get; } = point;

    public override string ToString()
    {
        return $"{Id} ({Point.X} {Point.Y})";
    }
}
