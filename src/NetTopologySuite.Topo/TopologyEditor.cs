using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay.Snap;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Operation.Relate;
using NetTopologySuite.Precision;

namespace NetTopologySuite.Topo;

public record TopologyEditor(ILogger<TopologyEditor> Logger, TopoFactory TopoFactory)
{
    const double tolerance = 0.001;

    private record EdgeEnd(Edge NextCW, bool IsNextCWForward, Face? FaceCW, Edge NextCCW, bool IsNextCCWForward, Face? FaceCCW, bool Isolated) { }

    // based on _lwt_FindAdjacentEdges
    private EdgeEnd FindAdjacentEdges(Topology topology, Edge edge, Node node, double az, double? otherAz)
    {
        var edgeEnd = new EdgeEnd(edge, true, null, edge, true, null, false);
        double azdiff;
        double minaz = -1;
        double maxaz = -1;

        if (otherAz != null)
        {
            azdiff = (double)otherAz - az;
            if (azdiff < 0) azdiff += 2 * Math.PI;
            minaz = maxaz = azdiff;
        }

        var edges = GetEdgesForNode(topology, node);
        Logger.LogDebug("[TopologyEditor:FindAdjacentEdges] getEdgeByNode returned {edgeCount} edges, minaz={minaz}, maxaz={maxaz}", 
            edges.Length, minaz, maxaz);
        
        if (edges.Length == 0)
            edgeEnd = edgeEnd with { Isolated = true };

        foreach (var other in edges)
        {
            if (other == edge) continue;

            var otherEdgeRel = topology.EdgeRels[other.Id];
            if (other.StartNode == node)
            {
                var p1 = other.LineString.Coordinates[0];
                var p2 = other.LineString.Coordinates[1];
                var otherEdgeAz = Algorithms.Azimuth(p1, p2);
                azdiff = otherEdgeAz - az;
                if (azdiff < 0) azdiff += 2 * Math.PI;
                
                Logger.LogDebug("[TopologyEditor:FindAdjacentEdges] edge {edgeId} starts on node {nodeId}, edgeend is [{p1} {p2}]", 
                    other.Id, node.Id, p1, p2);
                Logger.LogDebug("[TopologyEditor:FindAdjacentEdges] azimuth of edge {edgeId}: {azimuth} (diff: {azdiff})", 
                    other.Id, otherEdgeAz, azdiff);
                
                if (minaz == -1)
                {
                    edgeEnd = edgeEnd with { NextCW = other, IsNextCWForward = true, NextCCW = other, IsNextCCWForward = true, FaceCW = otherEdgeRel.FaceLeft, FaceCCW = otherEdgeRel.FaceRight };
                    minaz = maxaz = azdiff;
                    Logger.LogDebug("[TopologyEditor:FindAdjacentEdges] new nextCW and nextCCW edge is {edgeId}, outgoing, with face_left {faceLeft} and face_right {faceRight} (face_right is new ccwFace, face_left is new cwFace)", 
                        other.Id, otherEdgeRel.FaceLeft?.Id, otherEdgeRel.FaceRight?.Id);
                }
                else
                {
                    if (azdiff < minaz)
                    {
                        edgeEnd = edgeEnd with { NextCW = other, IsNextCWForward = true, FaceCW = otherEdgeRel.FaceLeft };
                        minaz = azdiff;
                        Logger.LogDebug("[TopologyEditor:FindAdjacentEdges] new nextCW edge is {edgeId}, outgoing, with face_left {faceLeft} and face_right {faceRight} (previous had minaz={prevMinaz}, face_left is new cwFace)", 
                            other.Id, otherEdgeRel.FaceLeft?.Id, otherEdgeRel.FaceRight?.Id, minaz);
                    }
                    else if (azdiff > maxaz)
                    {
                        edgeEnd = edgeEnd with { NextCCW = other, IsNextCCWForward = true, FaceCCW = otherEdgeRel.FaceRight };
                        maxaz = azdiff;
                        Logger.LogDebug("[TopologyEditor:FindAdjacentEdges] new nextCCW edge is {edgeId}, outgoing, from start point, with face_left {faceLeft} and face_right {faceRight} (previous had maxaz={prevMaxaz}, face_left is new ccwFace)", 
                            other.Id, otherEdgeRel.FaceLeft?.Id, otherEdgeRel.FaceRight?.Id, maxaz);
                    }
                }
            }
            if (other.EndNode == node)
            {
                var p1 = other.LineString.Coordinates[^1];
                var p2 = other.LineString.Coordinates[^2];
                var otherEdgeAz = Algorithms.Azimuth(p1, p2);
                azdiff = otherEdgeAz - az;
                if (azdiff < 0) azdiff += 2 * Math.PI;
                
                Logger.LogDebug("[TopologyEditor:FindAdjacentEdges] edge {edgeId} ends on node {nodeId}, edgeend is [{p1} {p2}]", 
                    other.Id, node.Id, p1, p2);
                Logger.LogDebug("[TopologyEditor:FindAdjacentEdges] azimuth of edge {edgeId}: {azimuth} (diff: {azdiff})", 
                    other.Id, otherEdgeAz, azdiff);
                
                if (minaz == -1)
                {
                    edgeEnd = edgeEnd with { NextCW = other, IsNextCWForward = false, NextCCW = other, IsNextCCWForward = false, FaceCW = otherEdgeRel.FaceRight, FaceCCW = otherEdgeRel.FaceLeft };
                    minaz = maxaz = azdiff;
                    Logger.LogDebug("[TopologyEditor:FindAdjacentEdges] new nextCW and nextCCW edge is {edgeId}, incoming, with face_left {faceLeft} and face_right {faceRight} (face_right is new cwFace, face_left is new ccwFace)", 
                        other.Id, otherEdgeRel.FaceLeft?.Id, otherEdgeRel.FaceRight?.Id);
                }
                else
                {
                    if (azdiff < minaz)
                    {
                        edgeEnd = edgeEnd with { NextCW = other, IsNextCWForward = false, FaceCW = otherEdgeRel.FaceRight };
                        minaz = azdiff;
                        Logger.LogDebug("[TopologyEditor:FindAdjacentEdges] new nextCW edge is {edgeId}, incoming, with face_left {faceLeft} and face_right {faceRight} (previous had minaz={prevMinaz}, face_right is new cwFace)", 
                            other.Id, otherEdgeRel.FaceLeft?.Id, otherEdgeRel.FaceRight?.Id, minaz);
                    }
                    else if (azdiff > maxaz)
                    {
                        edgeEnd = edgeEnd with { NextCCW = other, IsNextCCWForward = false, FaceCCW = otherEdgeRel.FaceLeft };
                        maxaz = azdiff;
                        Logger.LogDebug("[TopologyEditor:FindAdjacentEdges] new nextCCW edge is {edgeId}, incoming, with face_left {faceLeft} and face_right {faceRight} (previous had maxaz={prevMaxaz}, face_left is new ccwFace)", 
                            other.Id, otherEdgeRel.FaceLeft?.Id, otherEdgeRel.FaceRight?.Id, maxaz);
                    }
                }
            }
        }
        
        if (!edgeEnd.Isolated)
        {
            Logger.LogDebug("[TopologyEditor:FindAdjacentEdges] edges adjacent to azimuth {azimuth} (incident to node {nodeId}): CW:{cwEdge}({cwAz}) CCW:{ccwEdgeSign}{ccwEdge}({ccwAz})", 
                az, node.Id, 
                edgeEnd.NextCW.Id, minaz,
                edgeEnd.IsNextCCWForward ? "" : "-", edgeEnd.NextCCW.Id, maxaz);
        }
        
        return edgeEnd;
    }

    // based on lwt_be_ExistsCoincidentNode
    private void ExistsCoincidentNode(Topology topology, Point point)
    {
        if (topology.Nodes.Any(n => n.Point.IsWithinDistance(point, 0)))
            throw new TopologyException("Spatial exception - coincident node");
    }

    // based on lwt_be_ExistsEdgeIntersectingPoint
    private void ExistsEdgeIntersectingPoint(Topology topology, Point point)
    {
        if (topology.Edges.Any(e => e.LineString.IsWithinDistance(point, 0)))
            throw new TopologyException("Spatial exception - edge crosses node");
    }

    private Edge GetClosestEdge(Topology topology, Point point)
    {
        return topology.Edges.OrderBy(e => e.LineString.Distance(point)).First();
    }

    // based on lwt_GetFaceGeometry
    public Polygon GetFaceGeometry(Topology topology, Face face)
    {
        var edges = topology.EdgeRels.Values
            .Where(er => er.FaceLeft == face || er.FaceRight == face)
            .Select(er => er.Edge).ToArray();
        if (edges.Length == 0) {
            Logger.LogError("Face {face} has no edges", face);
            throw new Exception("Invalid topology");
        }
        var lineStrings = edges.Select(e => e.LineString).ToArray();
        // TODO: skipping edges with same face on both sides ?
        var polygonize = new Polygonizer();
        polygonize.Add(lineStrings);
        var polygons = polygonize.GetPolygons();
        if (polygons.Count == 0)
        {
            Logger.LogError("Unable to form polygon");
            foreach (var edge in edges)
                Logger.LogError("Failed polygonization involving edge {edge}", edge);
            throw new TopologyException("Spatial exception - face has no geometry");
        }
        var polygon = polygons.First() as Polygon;
        if (polygons.Count > 1)
        {
            var holes = polygons.Skip(1).Select(p => (p as Polygon)!.Shell).ToArray();
            polygon = polygon!.Factory.CreatePolygon(polygon.Shell, holes);
        }
        return polygon!;
    }

    private Face GetFaceContainingPoint(Topology topology, Point point)
    {
        if (topology.Edges.Length == 0)
            return Face.Universe;
        var closestEdge = GetClosestEdge(topology, point);
        var closestEdgeRel = topology.EdgeRels[closestEdge.Id];

        // naive inefficient implementation;
        foreach (var face in topology.Faces.Where(f => f.Bbox!.Contains(point.Coordinate)))
            if (GetFaceGeometry(topology, face).Contains(point))
                return face;
        return Face.Universe;

        // TODO: PostGIS has some fancy math here to determine closeness to node
        //var containingFace = closestEdgeRel.FaceLeft;
        // TODO: Check other incident edges
        // TODO: If close to node determine via angle
        // TODO: If close to edge determine if left/right
        //return containingFace;
    }

    /// <summary>
    /// Adds an isolated node at point location to an existing face.
    /// 
    /// NOTE: Similar to ST_AddIsoNode
    /// SQL/MM specification. SQL-MM: Topo-Net Routines: X+1.3.1
    public Topology AddIsoNode(Topology topology, Face? face, Point point)
    {
        Logger.LogTrace("AddIsoNode for {point}", point);
        ExistsCoincidentNode(topology, point);
        ExistsEdgeIntersectingPoint(topology, point);
        var foundInFace = GetFaceContainingPoint(topology, point);
        if (face == null)
            face = foundInFace;
        else if (face != foundInFace)
            throw new TopologyException("Spatial exception - not within face");
        var node = TopoFactory.CreateNode(point);
        var nodes = topology.Nodes.Add(node);
        var nodeRels = topology.NodeRels.Add(node.Id, new NodeRel(node, face));
        Logger.LogTrace("AddIsoNode created {node} in containing face {face}", node, face);
        return topology with { Nodes = nodes, NodeRels = nodeRels };
    }

    /// <summary>
    /// Adds an isolated edge defined by linestring connecting two existing isolated nodes.
    /// 
    /// NOTE: Similar to ST_AddIsoEdge
    /// SQL/MM specification. SQL-MM: Topo-Geo and Topo-Net 3: Routine Details: X.3.4
    /// </summary>
    public Topology AddIsoEdge(Topology topology, Node startNode, Node endNode, LineString lineString, int? eid = null)
    {
        Logger.LogTrace("AddIsoEdge for {startNode} to {endNode}", startNode, endNode);
        if (endNode == startNode)
            throw new TopologyException($"Iso edge cannot start and end in the same node");
        var fromContainedFace = topology.NodeRels[startNode.Id].ContainedFace ?? throw new TopologyException($"Node {startNode.Id} is not isolated");
        var toContainedFace = topology.NodeRels[startNode.Id].ContainedFace ?? throw new TopologyException($"Node {endNode.Id} is not isolated");
        if (fromContainedFace != toContainedFace)
            throw new TopologyException($"Nodes are not contained by the same face");
        if (!startNode.Point.Coordinate.Equals2D(lineString.Coordinates.First()))
            throw new TopologyException($"Node {startNode} is not at the same location as lineString start");
        if (!endNode.Point.Coordinate.Equals2D(lineString.Coordinates.Last()))
            throw new TopologyException($"Node {endNode} is not at the same location as lineString start");
        if (!lineString.IsSimple)
            throw new TopologyException($"lineString is not simple");

        CheckEdgeCrossing(topology, startNode, endNode, lineString);
        // TODO: check for face containment
        var containedFace = fromContainedFace;
        var edge = TopoFactory.CreateEdge(startNode, endNode, lineString, eid);
        var edges = topology.Edges.Add(edge);
        var edgeRels = topology.EdgeRels.Add(edge.Id, new EdgeRel(edge, edge, false, edge, true, containedFace, containedFace));
        var nodeRels = topology.NodeRels.SetItem(startNode.Id, topology.NodeRels[startNode.Id] with { ContainedFace = null });
        nodeRels = nodeRels.SetItem(endNode.Id, topology.NodeRels[endNode.Id] with { ContainedFace = null });
        return topology with { NodeRels = nodeRels, Edges = edges, EdgeRels = edgeRels };
    }

    /// <summary>
    /// Split an edge by creating a new node with point location along the edge, deleting the original edge and replacing it with two new edges.
    /// Updates all existing joined edges and relationships accordingly.
    /// 
    /// NOTE: Similar to ST_NewEdgesSplit
    /// SQL/MM specification. SQL-MM: Topo-Geo and Topo-Net 3: Routine Details: X.3.12
    /// </summary>
    public Topology NewEdgesSplit(Topology topology, Edge edge, Point point)
    {
        point = edge.LineString.GetClosest(point, tolerance);

        Logger.LogTrace("NewEdgesSplit for edge {edge} on point {point}", edge, point);

        // check that point is within tolerance to be created as node
        if (point.Distance(edge.LineString) > tolerance)
            throw new TopologyException($"{point} not on edge (distance {point.Distance(edge.LineString)})");
        if (point.Distance(edge.StartNode.Point) <= tolerance)
            throw new TopologyException($"{point} exists as node");
        if (point.Distance(edge.EndNode.Point) <= tolerance)
            throw new TopologyException($"{point} exists as node");

        var node = TopoFactory.CreateNode(point);
        var nodes = topology.Nodes.Add(node);
        var nodeRels = topology.NodeRels.Add(node.Id, new NodeRel(node, null));

        // split edge linestring on new node point
        var (ls1, ls2) = edge.LineString.Split(node.Point);

        // create new edges from split linestring and add to current topology edges
        var e1 = TopoFactory.CreateEdge(edge.StartNode, node, ls1);
        var e2 = TopoFactory.CreateEdge(node, edge.EndNode, ls2);
        var edges = topology.Edges.AddRange(ImmutableArray.Create(e1, e2));

        // remove edge that has been split
        edges = edges.Remove(edge);
        var edgeRel = topology.EdgeRels[edge.Id];
        var edgeRels = topology.EdgeRels.Remove(edge.Id);

        // Define the first new edge (to new node)
        var nextLeftEdge = e2;
        Edge nextRightEdge;
        bool isNextRightForward;
        if (edgeRel.NextRight == edge && edgeRel.IsNextRightForward)
        {
            nextRightEdge = e1;
            isNextRightForward = true;
        }
        else if (edgeRel.NextRight == edge && !edgeRel.IsNextRightForward)
        {
            nextRightEdge = e2;
            isNextRightForward = false;
        }
        else
        {
            nextRightEdge = edgeRel.NextRight;
            isNextRightForward = edgeRel.IsNextRightForward;
        }
        edgeRels = SetEdgeRel(edgeRels, e1.Id, edgeRel with { Edge = e1, NextLeft = nextLeftEdge, IsNextLeftForward = true, NextRight = nextRightEdge, IsNextRightForward = isNextRightForward });

        // Define the second new edge (from new node)
        nextRightEdge = e1;
        isNextRightForward = false;
        bool isNextLeftForward;
        if (edgeRel.NextLeft == edge && !edgeRel.IsNextLeftForward)
        {
            nextLeftEdge = e2;
            isNextLeftForward = false;
        }
        else if (edgeRel.NextLeft == edge && edgeRel.IsNextLeftForward)
        {
            nextLeftEdge = e1;
            isNextLeftForward = true;
        }
        else
        {
            nextLeftEdge = edgeRel.NextLeft;
            isNextLeftForward = edgeRel.IsNextLeftForward;
        }
        edgeRels = SetEdgeRel(edgeRels, e2.Id, edgeRel with { Edge = e2, NextLeft = nextLeftEdge, IsNextLeftForward = isNextLeftForward, NextRight = nextRightEdge, IsNextRightForward = isNextRightForward });

        foreach (var o in edgeRels.Values.Where(e => e.NextRight == edge && e.IsNextRightForward && e.Edge.StartNode == edge.StartNode))
            edgeRels = SetEdgeRel(edgeRels, o.Edge.Id, o with { NextRight = e1, IsNextRightForward = true });
        foreach (var o in edgeRels.Values.Where(e => e.NextRight == edge && !e.IsNextRightForward && e.Edge.StartNode == edge.EndNode))
            edgeRels = SetEdgeRel(edgeRels, o.Edge.Id, o with { NextRight = e2, IsNextRightForward = false });
        foreach (var o in edgeRels.Values.Where(e => e.NextLeft == edge && e.IsNextLeftForward && e.Edge.EndNode == edge.StartNode))
            edgeRels = SetEdgeRel(edgeRels, o.Edge.Id, o with { NextLeft = e1, IsNextLeftForward = true });
        foreach (var o in edgeRels.Values.Where(e => e.NextLeft == edge && !e.IsNextLeftForward && e.Edge.EndNode == edge.EndNode))
            edgeRels = SetEdgeRel(edgeRels, o.Edge.Id, o with { NextLeft = e2, IsNextLeftForward = false });

        return topology with { Nodes = nodes, NodeRels = nodeRels, Edges = edges, EdgeRels = edgeRels };
    }

    // based on _lwt_CheckEdgeCrossing
    private static void CheckEdgeCrossing(Topology topology, Node startNode, Node endNode, LineString lineString)
    {
        var nodes = topology.Nodes.Where(n => n.Point.Intersects(lineString.Envelope));
        foreach (var node in nodes)
        {
            if (node == startNode || node == endNode) continue;
            var contains = Algorithms.IsOnSegment(lineString.Coordinates, node.Point.Coordinate);
            if (contains)
                throw new TopologyException("geometry crosses a node");
        }
        var edges = topology.Edges.Where(e => e.LineString.Intersects(lineString));
        foreach (var edge in edges)
        {
            var relate = RelateOp.Relate(edge.LineString, lineString, BoundaryNodeRules.EndpointBoundaryRule);
            var match = relate.Matches("FF*F*****");
            if (match)
                continue; // no interior intersection
            match = relate.Matches("1FFF*FFF2");
            if (match)
                throw new TopologyException($"Spatial exception - coincident edge {edge}");
            match = relate.Matches("1********");
            if (match)
                throw new TopologyException($"Spatial exception - geometry intersects edge {edge}");
            match = relate.Matches("T********");
            if (match)
                throw new TopologyException($"Spatial exception - geometry crosses edge {edge}");
            match = relate.Matches("*T*******");
            if (match)
                throw new TopologyException($"Spatial exception - geometry boundary touches interior of edge {edge}");
            match = relate.Matches("***T*****");
            if (match)
                throw new TopologyException($"Spatial exception - boundary of edge {edge} touches interior of geometry");
        }
    }

    /// <summary>
    /// Add a new edge and, if in doing so it splits a face, delete the original face and replace it with two new faces.
    /// 
    /// NOTE: Similar to ST_AddEdgeNewFaces
    /// SQL/MM specification. SQL-MM: Topo-Geo and Topo-Net 3: Routine Details: X.3.12
    /// </summary>
    public Topology AddEdgeNewFaces(Topology topology, Node startNode, Node endNode, LineString lineString, int? eid = null)
    {
        Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] Calling AddEdgeNewFaces");
        Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] edge's start node is {startNode}", startNode);
        Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] edge's end node is {endNode}", endNode);

        if (!startNode.Point.Coordinate.Equals2D(lineString.Coordinates.First()))
            throw new TopologyException($"Node {startNode} is not at the same location as lineString start");
        if (!endNode.Point.Coordinate.Equals2D(lineString.Coordinates.Last()))
            throw new TopologyException($"Node {endNode} is not at the same location as lineString start");
        if (!lineString.IsSimple)
            throw new TopologyException($"Spatial exception - curve not simple");
        CheckEdgeCrossing(topology, startNode, endNode, lineString);

        var isClosed = startNode == endNode;
        Face? leftFace = null;
        Face? rightFace = null;

        var edge = TopoFactory.CreateEdge(startNode, endNode, lineString, eid);

        /* Compute azimuth of first edge end on start node */
        double saz = Algorithms.Azimuth(edge.LineString.Coordinates[0], edge.LineString.Coordinates[1]);
        /* Compute azimuth of last edge end on end node */
        double eaz = Algorithms.Azimuth(edge.LineString.Coordinates[^1], edge.LineString.Coordinates[^2]);

        Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] edge azimuth on start node: {saz}, end node: {eaz}", saz, eaz);

        // Check endpoints existence, match with Curve geometry and get face information (if any)
        if (topology.NodeRels[startNode.Id].ContainedFace != null)
            leftFace = rightFace = topology.NodeRels[startNode.Id].ContainedFace;
        if (topology.NodeRels[endNode.Id].ContainedFace != null && leftFace != null && topology.NodeRels[endNode.Id].ContainedFace != leftFace)
            throw new TopologyException($"Spatial exception - geometry crosses an edge (endnodes in faces {topology.NodeRels[startNode.Id].ContainedFace} and {topology.NodeRels[endNode.Id].ContainedFace})");

        /* Find adjacent edges to each endpoint */
        Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] Looking for edges incident to node {startNode} and adjacent to azimuth {saz}", startNode, saz);
        var span = FindAdjacentEdges(topology, edge, startNode, saz, isClosed ? eaz : null);
        Edge nextRightEdge;
        bool isNextRightEdgeForward;
        Edge prevLeftEdge;
        bool isPrevLeftEdgeForward;
        if (!span.Isolated)
        {
            nextRightEdge = span.NextCW;
            isNextRightEdgeForward = span.NextCW != edge ? span.IsNextCWForward : false;
            prevLeftEdge = span.NextCCW != edge ? span.NextCCW : edge;
            isPrevLeftEdgeForward = span.NextCCW != edge ? !span.IsNextCCWForward : true;
            rightFace ??= span.FaceCW;
            leftFace ??= span.FaceCCW;
            Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] New edge {edge} is connected on start node, next_right is {nextRightEdge}, prev_left is {prevLeftEdge}", 
                edge, nextRightEdge, prevLeftEdge);
        }
        else
        {
            nextRightEdge = edge;
            isNextRightEdgeForward = !isClosed;
            prevLeftEdge = edge;
            isPrevLeftEdgeForward = isClosed;
        }

        Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] Looking for edges incident to node {endNode} and adjacent to azimuth {eaz}", endNode, eaz);
        var epan = FindAdjacentEdges(topology, edge, endNode, eaz, isClosed ? saz : null);
        Edge nextLeftEdge;
        bool isNextLeftEdgeFoward;
        Edge prevRightEdge;
        bool isPrevRightEdgeForward;
        if (!epan.Isolated)
        {
            nextLeftEdge = epan.NextCW;
            isNextLeftEdgeFoward = epan.NextCW != edge ? epan.IsNextCWForward : true;
            prevRightEdge = epan.NextCCW;
            isPrevRightEdgeForward = epan.NextCCW != edge ? !epan.IsNextCCWForward : false;
            if (rightFace == null)
                rightFace = span.FaceCCW;
            else if (rightFace != epan.FaceCCW)
                throw new TopologyException($"Side-location conflict: new edge starts in face {rightFace} and ends in face {epan.FaceCCW}");
            if (leftFace == null)
                leftFace = span.FaceCW;
            else if (leftFace != epan.FaceCW)
                throw new TopologyException($"Side-location conflict: new edge starts in face {leftFace} and ends in face {epan.FaceCW}");
            Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] New edge {edge} is connected on end node, next_left is {nextLeftEdge}, prev_right is {prevRightEdge}", 
                edge, nextLeftEdge, prevRightEdge);
        }
        else
        {
            nextLeftEdge = edge;
            isNextLeftEdgeFoward = isClosed;
            prevRightEdge = edge;
            isPrevRightEdgeForward = !isClosed;
        }

        if (leftFace == null || rightFace == null)
            throw new TopologyException("Invalid topology");

        var edges = topology.Edges.Add(edge);
        var edgeRels = topology.EdgeRels;
        var edgeRel = new EdgeRel(edge, nextLeftEdge, isNextLeftEdgeFoward, nextRightEdge, isNextRightEdgeForward, leftFace, rightFace);
        edgeRels = SetEdgeRel(edgeRels, edge.Id, edgeRel);

        Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] Inserted new edge {edgeId}", edge.Id);

        // link prevLeftEdge to us
        if (prevLeftEdge != edge)
        {
            var prevLeftEdgeRel = edgeRels[prevLeftEdge.Id];
            if (isPrevLeftEdgeForward)
            {
                edgeRels = SetEdgeRel(edgeRels, prevLeftEdge.Id, prevLeftEdgeRel with { NextLeft = edge, IsNextLeftForward = true });
                Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] Updated edge {edgeId} next_left_edge = {nextLeftEdge}", prevLeftEdge.Id, edge.Id);
            }
            else
            {
                edgeRels = SetEdgeRel(edgeRels, prevLeftEdge.Id, prevLeftEdgeRel with { NextRight = edge, IsNextRightForward = true });
                Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] Updated edge {edgeId} next_right_edge = {nextRightEdge}", prevLeftEdge.Id, edge.Id);
            }
        }

        // link prevRightEdge to us
        if (prevRightEdge != edge)
        {
            var prevRightEdgeRel = edgeRels[prevRightEdge.Id];
            if (isPrevRightEdgeForward)
            {
                edgeRels = SetEdgeRel(edgeRels, prevRightEdge.Id, prevRightEdgeRel with { NextLeft = edge, IsNextLeftForward = false });
                Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] Updated edge {edgeId} next_left_edge = -{nextLeftEdge}", prevRightEdge.Id, edge.Id);
            }
            else
            {
                edgeRels = SetEdgeRel(edgeRels, prevRightEdge.Id, prevRightEdgeRel with { NextRight = edge, IsNextRightForward = false });
                Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] Updated edge {edgeId} next_right_edge = -{nextRightEdge}", prevRightEdge.Id, edge.Id);
            }
        }

        // set containing_face = null for start_node and end_node if they where isolated
        var nodeRels = topology.NodeRels;
        if (span.Isolated)
            nodeRels = nodeRels.SetItem(startNode.Id, nodeRels[startNode.Id] with { ContainedFace = null });
        if (epan.Isolated)
            nodeRels = nodeRels.SetItem(endNode.Id, nodeRels[endNode.Id] with { ContainedFace = null });

        // face splitting
        var faces = topology.Faces;
        topology = topology with { NodeRels = nodeRels, Edges = edges, EdgeRels = edgeRels, Faces = faces };
        if (!isClosed && (span.Isolated || epan.Isolated))
        {
            Logger.LogTrace("New edge is dangling, so it cannot split any face");
            return topology;
        }
        // attempt face split by traversing edge backward
        var facesCount = topology.Faces.Length;
        topology = AddFaceSplit(topology, edge, edgeRel.FaceLeft, false, out bool leftIsUniverse);
        if (topology.Faces.Length == facesCount && !leftIsUniverse)
            return topology;

        edgeRel = topology.EdgeRels[edge.Id];

        // attempt face split by traversing edge forward
        topology = AddFaceSplit(topology, edge, edgeRel.FaceLeft, true, out leftIsUniverse);

        // TODO: update topogeoms

        // drop old face
        faces = topology.Faces;
        if (edgeRel.FaceLeft != Face.Universe)
            faces = topology.Faces.Remove(edgeRel.FaceLeft);

        Logger.LogDebug("[TopologyEditor:AddEdgeNewFaces] lwt_AddEdgeNewFaces returned");

        return topology with { Faces = faces };
    }

    /// <summary>
    /// Removes an edge and, if the removed edge separated two faces, delete the original faces and replace them with a new face.
    /// 
    /// NOTE: Similar to ST_RemEdgeNewFace
    /// SQL/MM specification. SQL-MM: Topo-Geo and Topo-Net 3: Routine Details: X.3.14
    /// </summary>
    public Topology RemEdgeNewFace(Topology topology, Edge edge)
    {
        Logger.LogTrace("RemEdgeNewFace for {edge}", edge);
        var edges = topology.Edges;
        var faces = topology.Faces;
        var edgeRels = topology.EdgeRels;
        var nodeRels = topology.NodeRels;
        if (!edges.Contains(edge))
            throw new TopologyException("Spatial exception - non-existent edge");
        var edgeRel = edgeRels[edge.Id];
        var edgesForNode = GetEdgesForNode(topology, edge.StartNode).Concat(GetEdgesForNode(topology, edge.EndNode));
        int fnodeEdges = 0;
        int lnodeEdges = 0;
        Face newFace;
        Face floodface = Face.Universe;
        foreach (var e in edgesForNode)
        {
            if (e == edge) continue;
            if (e.StartNode == edge.StartNode || e.EndNode == edge.StartNode) fnodeEdges++;
            if (e.StartNode == edge.EndNode || e.EndNode == edge.EndNode) lnodeEdges++;
            var eRel = topology.EdgeRels[e.Id];
            if (eRel.NextLeft == edge && !eRel.IsNextLeftForward)
            {
                if (!(edgeRel.NextLeft == edge && edgeRel.IsNextLeftForward))
                    edgeRels = SetEdgeRel(edgeRels, e.Id, eRel with { NextLeft = edgeRel.NextLeft, IsNextLeftForward = edgeRel.IsNextLeftForward });
                else
                    edgeRels = SetEdgeRel(edgeRels, e.Id, eRel with { NextLeft = edgeRel.NextRight, IsNextLeftForward = edgeRel.IsNextRightForward });
            }
            else if (eRel.NextLeft == edge && eRel.IsNextLeftForward)
            {
                if (!(edgeRel.NextRight == edge && edgeRel.IsNextRightForward))
                    edgeRels = SetEdgeRel(edgeRels, e.Id, eRel with { NextLeft = edgeRel.NextRight, IsNextLeftForward = edgeRel.IsNextRightForward });
                else
                    edgeRels = SetEdgeRel(edgeRels, e.Id, eRel with { NextLeft = edgeRel.NextLeft, IsNextLeftForward = edgeRel.IsNextLeftForward });
            }

            if (eRel.NextRight == edge && !eRel.IsNextRightForward)
            {
                if (!(edgeRel.NextLeft == edge && !edgeRel.IsNextLeftForward))
                    edgeRels = SetEdgeRel(edgeRels, e.Id, eRel with { NextRight = edgeRel.NextLeft, IsNextRightForward = edgeRel.IsNextLeftForward });
                else
                    edgeRels = SetEdgeRel(edgeRels, e.Id, eRel with { NextRight = edgeRel.NextRight, IsNextRightForward = edgeRel.IsNextRightForward });
            }
            else if (eRel.NextRight == edge && eRel.IsNextRightForward)
            {
                if (!(edgeRel.NextRight == edge && edgeRel.IsNextRightForward))
                    edgeRels = SetEdgeRel(edgeRels, e.Id, eRel with { NextRight = edgeRel.NextRight, IsNextRightForward = edgeRel.IsNextRightForward });
                else
                    edgeRels = SetEdgeRel(edgeRels, e.Id, eRel with { NextRight = edgeRel.NextLeft, IsNextRightForward = edgeRel.IsNextLeftForward });
            }
        }

        /* Find floodface, and update its mbr if != 0 */
        if (edgeRel.FaceLeft == edgeRel.FaceRight)
        {
            floodface = edgeRel.FaceRight;
        }
        else
        {
            if (edgeRel.FaceLeft == Face.Universe || edgeRel.FaceRight == Face.Universe)
            {
                floodface = Face.Universe;
            }
            else
            {
                floodface = edgeRel.FaceRight;
                var bbox = edgeRel.FaceLeft.Bbox!.ExpandedBy(edgeRel.FaceRight.Bbox);
                newFace = TopoFactory.CreateFace(bbox);
                faces = faces.Add(newFace);
                floodface = newFace;
            }

            if (edgeRel.FaceLeft != floodface)
            {
                foreach (var er in edgeRels.Values.Where(er => er.FaceLeft == edgeRel.FaceLeft))
                    edgeRels = SetEdgeRel(edgeRels, er.Edge.Id, er with { FaceLeft = floodface });
                foreach (var er in edgeRels.Values.Where(er => er.FaceRight == edgeRel.FaceLeft))
                    edgeRels = SetEdgeRel(edgeRels, er.Edge.Id, er with { FaceRight = floodface });
                foreach (var n in nodeRels.Values.Where(nr => nr.ContainedFace == edgeRel.FaceLeft))
                    nodeRels = nodeRels.SetItem(n.Node.Id, n with { ContainedFace = floodface });
            }

            if (edgeRel.FaceRight != floodface)
            {
                foreach (var er in edgeRels.Values.Where(er => er.FaceLeft == edgeRel.FaceRight))
                    edgeRels = SetEdgeRel(edgeRels, er.Edge.Id, er with { FaceLeft = floodface });
                foreach (var er in edgeRels.Values.Where(er => er.FaceRight == edgeRel.FaceRight))
                    edgeRels = SetEdgeRel(edgeRels, er.Edge.Id, er with { FaceRight = floodface });
                foreach (var n in nodeRels.Values.Where(nr => nr.ContainedFace == edgeRel.FaceRight))
                    nodeRels = nodeRels.SetItem(n.Node.Id, n with { ContainedFace = floodface });
            }

            // TODO: update topogeoms
        }

        edges = edges.Remove(edge);
        edgeRels = edgeRels.Remove(edge.Id);

        /* If any of the edge nodes remained isolated, set containing_face = floodface */
        if (fnodeEdges > 0)
            nodeRels = nodeRels.SetItem(edge.StartNode.Id, nodeRels[edge.StartNode.Id] with { ContainedFace = floodface });
        if (edge.EndNode != edge.StartNode && lnodeEdges > 0)
            nodeRels = nodeRels.SetItem(edge.EndNode.Id, nodeRels[edge.EndNode.Id] with { ContainedFace = floodface });

        // remove face
        if (edgeRel.FaceLeft != edgeRel.FaceRight)
        {
            if (edgeRel.FaceRight != floodface)
                faces = faces.Remove(edgeRel.FaceRight);
            if (edgeRel.FaceLeft != floodface)
                faces = faces.Remove(edgeRel.FaceLeft);
        }

        return topology with { NodeRels = nodeRels, Edges = edges, EdgeRels = edgeRels, Faces = faces };
    }

    // port of _lwt_AddFaceSplit
    private Topology AddFaceSplit(Topology topology, Edge edge, Face face, bool forward, out bool leftIsUniverse)
    {
        Logger.LogDebug("[TopologyEditor:AddFaceSplit] AddFaceSplit for edge {edge} on face {face} direction forward {forward}", edge, face, forward);

        var faces = topology.Faces;
        var nodeRels = topology.NodeRels;
        var edgeRels = topology.EdgeRels;
        leftIsUniverse = false;

        topology = topology with { NodeRels = nodeRels, EdgeRels = edgeRels, Faces = faces };

        var ringEdges = GetRingEdges(topology, edge, forward);
        Logger.LogDebug("[TopologyEditor:AddFaceSplit] getRingEdges returned {edgeCount} edges", ringEdges.Length);
        Logger.LogDebug("[TopologyEditor:AddFaceSplit] Edge {edgeDirection}{edgeId} split face {faceId} (mbr_only:0)", forward ? "" : "-", edge.Id, face.Id);

        // You can't get to the other side of an edge forming a ring
        foreach (var ring in ringEdges)
            if (ring.Edge == edge && ring.Forward == !forward)
                return topology;

        var coordinates = MakeRingShell(ringEdges);
        var bbox = CoordinateArrays.Envelope(coordinates);
        if (!coordinates.First().Equals2D(coordinates.Last()))
            throw new TopologyException($"Corrupted topology: ring of edge {edge} is geometrically not-closed");

        var isCCW = Algorithms.IsCCW(coordinates);

        Logger.LogDebug("[TopologyEditor:AddFaceSplit] Ring of edge {edgeDirection}{edgeId} is {orientation}", 
            forward ? "" : "-", edge.Id, isCCW ? "counterclockwise" : "clockwise");

        if (face == Face.Universe)
        {
            if (!isCCW)
            {
                Logger.LogDebug("[TopologyEditor:AddFaceSplit] The left face of this clockwise ring is the universe, won't create a new face there");
                leftIsUniverse = true;
                return topology;
            }
        }

        Envelope mbr = bbox;
        if (face != Face.Universe && !isCCW)
        {
            /* Face created an hole in an outer face */
            mbr = face.Bbox!;
        }

        /* Insert the new face */
        var newface = TopoFactory.CreateFace(mbr);
        faces = faces.Add(newface);
        Logger.LogDebug("[TopologyEditor:AddFaceSplit] Inserted new face {faceId}", newface.Id);

        /* Update side location of new face edges */

        /* We want the new face to be on the left, if possible */
        /* true = face shrunk, must update all non-contained edges and nodes */
        var newfaceOutside = face != Face.Universe && !isCCW;

        if (newfaceOutside)
        {
            Logger.LogDebug("[TopologyEditor:AddFaceSplit] New face is on the outside of the ring, updating rings in former shell");
        }
        else
        {
            Logger.LogDebug("[TopologyEditor:AddFaceSplit] New face is on the inside of the ring, updating forward edges in new ring");
        }

        /* Update edges bounding the old face */
        /* (1) fetch all edges where left_face or right_face is = oldface */
        var edges = edgeRels.Values
            .Where(er =>
                   er.FaceLeft == face || er.FaceRight == face &&
                   (er.FaceLeft == Face.Universe || bbox.Intersects(er.FaceLeft.Bbox) ||
                    er.FaceRight == Face.Universe || bbox.Intersects(er.FaceRight.Bbox))).ToList();
        Logger.LogDebug("[TopologyEditor:AddFaceSplit] _lwt_AddFaceSplit: lwt_be_getEdgeByFace({faceId}) returned {edgeCount} edges", face.Id, edges.Count);
                   
        List<int> forwardEdges = [];
        List<int> backwardEdges = [];
        /* (2) loop over the results and: */
        foreach (var e in edges)
        {
            var edgeRel = edgeRels[e.Edge.Id];
            int found = 0;
            /* (2.1) skip edges whose ID is in the list of boundary edges */
            foreach (var ringEdge in ringEdges)
            {
                if (ringEdge.Edge.Id == edgeRel.Edge.Id && ringEdge.Forward)
                {
                    forwardEdges.Add(e.Edge.Id);
                    found++;
                    Logger.LogDebug("[TopologyEditor:AddFaceSplit] Edge {edgeId} is a known forward edge of the new ring", e.Edge.Id);
                    if (found == 2) break; /* both edge sides are found on the ring */
                }
                else if (ringEdge.Edge.Id == edgeRel.Edge.Id && !ringEdge.Forward)
                {
                    backwardEdges.Add(e.Edge.Id);
                    found++;
                    Logger.LogDebug("[TopologyEditor:AddFaceSplit] Edge {edgeId} is a known backward edge of the new ring", e.Edge.Id);
                    if (found == 2) break; /* both edge sides are found on the ring */
                }
            }
            if (found > 0) 
            {
                continue;
            }

            Logger.LogDebug("[TopologyEditor:AddFaceSplit] Edge {edgeId} is not a known edge of the new ring", e.Edge.Id);
            
            var ep = e.Edge.StartNode.Point;
            var contains = bbox.Contains(ep.Coordinate) ? Algorithms.Location.Inside : Algorithms.Location.Outside;
            contains = contains == Algorithms.Location.Inside ? Algorithms.Contains(coordinates, ep.Coordinate) : contains;

            if (contains == Algorithms.Location.Inside)
            {
                Logger.LogDebug("[TopologyEditor:AddFaceSplit] Edge {edgeId} first point inside new ring", e.Edge.Id);
            }
            else if (contains == Algorithms.Location.OnBoundary)
            {
                Logger.LogDebug("[TopologyEditor:AddFaceSplit] Edge {edgeId} first point on boundary of new ring", e.Edge.Id);
            }
            else if (contains == Algorithms.Location.Outside)
            {
                Logger.LogDebug("[TopologyEditor:AddFaceSplit] Edge {edgeId} first point outside new ring", e.Edge.Id);
            }
            
            /* (2.2) skip edges (NOT, if newface_outside) contained in ring */
            if (newfaceOutside)
            {
                if (contains != Algorithms.Location.Outside)
                {
                    Logger.LogDebug("[TopologyEditor:AddFaceSplit] Edge {edgeId} not outside of the new ring, not updating it", e.Edge.Id);
                    continue;
                }
            }
            else
            {
                if (contains != Algorithms.Location.Inside)
                {
                    Logger.LogDebug("[TopologyEditor:AddFaceSplit] Edge {edgeId} not inside the new ring, not updating it", e.Edge.Id);
                    continue;
                }
            }

            /* (2.3) push to forward_edges if left_face = oface */
            if (e.FaceLeft == face)
            {
                forwardEdges.Add(e.Edge.Id);
                Logger.LogDebug("[TopologyEditor:AddFaceSplit] Edge {edgeId} has new face on the left side", e.Edge.Id);
            }
            /* (2.4) push to backward_edges if right_face = oface */
            if (e.FaceRight == face)
            {
                backwardEdges.Add(e.Edge.Id);
                Logger.LogDebug("[TopologyEditor:AddFaceSplit] Edge {edgeId} has new face on the right side", e.Edge.Id);
            }
        }

        /* Update forward edges */
        if (forwardEdges.Count > 0)
        {
            Logger.LogDebug("[TopologyEditor:AddFaceSplit] Updating {count} forward edges to new face {faceId}", forwardEdges.Count, newface.Id);
            foreach (var edgeId in forwardEdges)
                edgeRels = SetEdgeRel(edgeRels, edgeId, edgeRels[edgeId] with { FaceLeft = newface });
        }
        /* Update backward edges */
        if (backwardEdges.Count > 0)
        {
            Logger.LogDebug("[TopologyEditor:AddFaceSplit] Updating {count} backward edges to new face {faceId}", backwardEdges.Count, newface.Id);
            foreach (var edgeId in backwardEdges)
                edgeRels = SetEdgeRel(edgeRels, edgeId, edgeRels[edgeId] with { FaceRight = newface });
        }

        /* Update isolated nodes which are now in new face */
        var nodes = nodeRels.Values
            .Where(nr => nr.ContainedFace == face)
            .Where(nr => mbr.Contains(nr.Node.Point.Coordinate))
            .Select(nr => nr.Node);
        foreach (var node in nodes)
        {
            var contains = Algorithms.Contains(coordinates, node.Point.Coordinate);
            if (newfaceOutside)
            {
                if (contains == Algorithms.Location.Inside)
                {
                    Logger.LogDebug("[TopologyEditor:AddFaceSplit] Node {Id} contained in an hole of the new face", node.Id);
                    continue;
                }
            }
            else
            {
                if (contains != Algorithms.Location.Inside)
                {
                    Logger.LogDebug("[TopologyEditor:AddFaceSplit] Node {Id} not contained in the face shell", node.Id);
                    continue;
                }
            }
            nodeRels = nodeRels.SetItem(node.Id, nodeRels[node.Id] with { ContainedFace = newface });
        }

        topology = topology with { NodeRels = nodeRels, EdgeRels = edgeRels, Faces = faces };

        Logger.LogDebug("[TopologyEditor:AddFaceSplit] cb_updateTopoGeomFaceSplit signalled split of face {oldFaceId} into {newFaceId} and {remainingFaceId}", 
            face.Id, newface.Id, face.Id);

        return topology;
    }

    private ImmutableDictionary<int, EdgeRel> SetEdgeRel(ImmutableDictionary<int, EdgeRel> edgeRels, int id, EdgeRel edgeRel)
    {
        Logger.LogDebug("[TopologyEditor:SetEdgeRel] Setting edge rel: {edgeRel}", edgeRel);
        return edgeRels.SetItem(id, edgeRel);
    }

    private ImmutableArray<Edge> GetEdgesForNode(Topology topology, Node node)
    {
        var filtered = topology.Edges
            .Where(e => e.StartNode == node)
            .Concat(topology.Edges
                .Where(e => e.EndNode == node))
            .DistinctBy(e => e.Id)
            .ToImmutableArray();
        return filtered;
    }

    public record EdgeDirection(Edge Edge, bool Forward);
    public ImmutableArray<EdgeDirection> GetRingEdges(Topology topology, Edge edge, bool forward)
    {
        var initialDirection = forward;
        var currentEdge = edge;
        var ring = new List<EdgeDirection>();
        while (true)
        {
            var currentEdgeRel = topology.EdgeRels[currentEdge.Id];
            ring.Add(new EdgeDirection(currentEdge, forward));
            Logger.LogDebug("[TopologyEditor:GetRingEdges] Component {index} in ring of edge {edgeDirection}{edgeId} is edge {edgeDirection2}{edgeId2}", 
                ring.Count - 1, initialDirection ? "" : "-", edge.Id, forward ? "" : "-", currentEdge.Id);
            
            currentEdge = forward ? currentEdgeRel.NextLeft : currentEdgeRel.NextRight;
            forward = forward ? currentEdgeRel.IsNextLeftForward : currentEdgeRel.IsNextRightForward;
            if (currentEdge == edge && initialDirection == forward)
            {
                Logger.LogDebug("[TopologyEditor:GetRingEdges] Last component in ring of edge {edgeDirection}{edgeId} ({lastEdgeId}) has next_{direction}_edge {nextEdgeDirection}{nextEdgeId}", 
                    initialDirection ? "" : "-", edge.Id, ring.Last().Edge.Id, 
                    initialDirection ? "left" : "right", 
                    initialDirection == forward ? "" : "-", edge.Id);
                break;
            }
            if (ring.Count > topology.EdgeRels.Count * 2)
                throw new TopologyException("Suspect invalid topology (infinite traversal)");
        };
        return [.. ring];
    }

    // based on _lwt_MakeRingShell
    private Coordinate[] MakeRingShell(ImmutableArray<EdgeDirection> edges)
    {
        for (int i = 0; i < edges.Length; i++)
        {
            var edge = edges[i];
            Logger.LogDebug("[TopologyEditor:MakeRingShell] Edge {edgeId} in ring is edge {dir}{edgeId}", edge.Edge.Id, edge.Forward ? "" : "-", edge.Edge.Id);
        }
        
        var coordinates = edges
            .Distinct()
            .SelectMany(e => e.Forward ? e.Edge.LineString.Coordinates : e.Edge.LineString.Coordinates.Reverse());
        var coordinatesNoRepeated = CoordinateArrays.RemoveRepeatedPoints(coordinates.ToArray());
        return coordinatesNoRepeated;
    }

    public Topology AddLineString(Topology topology, LineString lineString)
    {
        Logger.LogTrace("AddLineString {lineString}", lineString);
        var isSimpleOp = new NetTopologySuite.Operation.Valid.IsSimpleOp(lineString);
        if (!isSimpleOp.IsSimple())
            throw new TopologyException("Non simple at location " + isSimpleOp.NonSimpleLocation);
        if (lineString.Length == 0)
            throw new TopologyException("Zero length");

        var snappedLineString = SnapToEdges(topology, lineString, tolerance);
        var foundEdges = topology.GetDWithinEdges(snappedLineString, tolerance);
        var splitParts = Edge.Split(foundEdges, snappedLineString, tolerance);
        var lineStringParts = splitParts
            .Where(sp => sp.Context == snappedLineString as object)
            .Select(sp => sp.LineString)
            .ToImmutableArray();

        // split where part touch edge (not at node)
        foreach (var lineStringPart in lineStringParts)
        {
            lineStringPart.Apply(new CoordinatePrecisionReducerFilter(new PrecisionModel(1 / tolerance)));
            if (lineStringPart.Length == 0)
                continue;
            if (!topology.GetDWithinNodes(lineStringPart.StartPoint, tolerance).Any())
            {
                var edge = topology.GetDWithinEdges(lineStringPart.StartPoint, tolerance).FirstOrDefault();
                if (edge != null)
                    topology = NewEdgesSplit(topology, edge, lineStringPart.StartPoint);
            }
            if (!topology.GetDWithinNodes(lineStringPart.EndPoint, tolerance).Any())
            {
                var edge = topology.GetDWithinEdges(lineStringPart.EndPoint, tolerance).FirstOrDefault();
                if (edge != null)
                    topology = NewEdgesSplit(topology, edge, lineStringPart.EndPoint);
            }
        }

        // add part as new edge potentially splitting faces
        foreach (var lineStringPart in lineStringParts)
        {
            lineStringPart.Apply(new CoordinatePrecisionReducerFilter(new PrecisionModel(1 / tolerance)));
            if (lineStringPart.Length == 0)
                continue;
            var startNode = topology.Nodes.FirstOrDefault(n => n.Point == lineStringPart.StartPoint);
            if (startNode == null)
                topology = AddIsoNode(topology, null, lineStringPart.StartPoint);
            startNode = topology.Nodes.First(n => n.Point == lineStringPart.StartPoint);
            var endNode = topology.Nodes.FirstOrDefault(n => n.Point == lineStringPart.EndPoint);
            if (endNode == null)
                topology = AddIsoNode(topology, null, lineStringPart.EndPoint);
            endNode = topology.Nodes.First(n => n.Point == lineStringPart.EndPoint);
            topology = AddEdgeNewFaces(topology, startNode, endNode, lineStringPart);
        }
        return topology;
    }

    private LineString SnapToEdges(Topology topology, LineString lineString, double tolerance)
    {
        // TODO: this should really snap to segments not only vertices
        var coordinates = topology.GetDWithinEdges(lineString, tolerance).SelectMany(e => e.LineString.Coordinates).ToArray();
        lineString.Factory.CreateLineString(new LineStringSnapper(lineString, tolerance).SnapTo(coordinates));
        return lineString;
    }

    /*public Topology Merge(ImmutableArray<TopoGeometry> topoGeometries)
    {
        var faces = topoGeometries.SelectMany(tg => tg.Faces).ToImmutableArray();
        var topoGeometry = new TopoGeometry(faces);
        var newTopoGeometries = TopoGeometries.RemoveRange(topoGeometries).Add(topoGeometry);
        var newTopology = new Topology(TopoFactory, Nodes, Edges, Faces, newTopoGeometries, NodeRels, EdgeRels);
        return newTopology;
    }*/
}
