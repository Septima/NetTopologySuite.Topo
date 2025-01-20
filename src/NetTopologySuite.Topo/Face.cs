using NetTopologySuite.Geometries;

namespace NetTopologySuite.Topo;

public sealed record Face(int Id, int? Eid, Envelope? Bbox) : EntityBase(Id, Eid)
{
    static readonly Face _universe = new(0, null, null);
    public static Face Universe => _universe;

    public override string ToString()
        => $"{Id}";
}
