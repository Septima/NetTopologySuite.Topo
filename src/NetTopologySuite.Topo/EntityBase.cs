namespace NetTopologySuite.Topo;

public abstract record EntityBase(int Id, int? Eid)
{
    public virtual bool Equals(EntityBase? other) =>
        Id == other?.Id;
    public override int GetHashCode()
        => Id;
}
