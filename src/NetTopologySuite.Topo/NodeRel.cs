namespace NetTopologySuite.Topo;

public sealed record NodeRel(Node Node, Face? ContainedFace)
{
    public override string ToString() =>
        $"{Node.Id} {ContainedFace?.Id ?? -1}";
}
