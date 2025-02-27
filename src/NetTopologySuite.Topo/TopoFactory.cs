using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Valid;

namespace NetTopologySuite.Topo;

#pragma warning disable CS9113 // Parameter is unread.
public class TopoFactory(ILogger<TopoFactory> Logger)
#pragma warning restore CS9113 // Parameter is unread.
{
    int nodeId = 1;
    readonly Dictionary<int, Node> nodes = [];
    int edgeId = 1;
    readonly Dictionary<int, Edge> edges = [];
    int faceId = 1;
    readonly Dictionary<int, Face> faces = [];

    public void ResetIds(int nodeId, int edgeId, int faceId)
    {
        this.nodeId = nodeId;
        this.edgeId = edgeId;
        this.faceId = faceId;
    }

    public Node CreateNode(Point point)
    {
        var node = new Node(nodeId++, null, point);
        //Logger.LogTrace("Created node {node}", node);
        return node;
    }

    public Edge CreateEdge(Node startNode, Node endNode, LineString lineString, int? eid = null)
    {
        if (startNode.Point != lineString.StartPoint)
            throw new TopologyException($"LineString startpoint not at node startpoint {startNode.Point}");
        if (endNode.Point != lineString.EndPoint)
            throw new TopologyException($"LineString endpoint not at node endpoint {endNode.Point}");
        var isSimpleOp = new IsSimpleOp(lineString);
        if (!isSimpleOp.IsSimple())
            throw new TopologyException("Non simple at location " + isSimpleOp.NonSimpleLocation);
        var edge = new Edge(edgeId++, eid, startNode, endNode, lineString);
        //Logger.LogTrace("Creating edge {edge}", edge);
        return edge;
    }

    public Face CreateFace(Envelope bbox)
    {
        var face = new Face(faceId++, null, bbox);
        //Logger.LogTrace("Creating face {face}", face);
        return face;
    }
}
