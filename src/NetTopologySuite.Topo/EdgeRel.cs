namespace NetTopologySuite.Topo;

public sealed record EdgeRel(Edge Edge, Edge NextLeft, bool IsNextLeftForward, Edge NextRight, bool IsNextRightForward, Face FaceLeft, Face FaceRight)
{
    public Edge GetNextEdge(bool isLeft) => isLeft ? NextLeft : NextRight;

    public override string ToString()
    {
        var sl = !IsNextLeftForward ? "-" : "";
        var sr = !IsNextRightForward ? "-" : "";
        return $"{Edge.Id} {sl}{NextLeft.Id} {sr}{NextRight.Id} {FaceLeft.Id} {FaceRight.Id}";
    }
}
