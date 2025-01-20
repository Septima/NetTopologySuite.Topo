using NetTopologySuite.Geometries;

namespace NetTopologySuite.Topo;

public static class Algorithms
{
    public static double Azimuth(Coordinate p1, Coordinate p2)
    {
        double deltaX = p1.X - p2.X;
        double deltaY = p1.Y - p2.Y;
        double azimuth = Math.Atan2(deltaY, deltaX);
        azimuth = (2 * Math.PI + Math.PI / 2 - azimuth) % (2 * Math.PI);
        return azimuth;
    }

    private static double IsLeft(Coordinate c0, Coordinate c1, Coordinate c2) =>
        ((c1.X - c0.X) * (c2.Y - c0.Y)) - ((c2.X - c0.X) * (c1.Y - c0.Y));

    private static bool IsOnSegment(Coordinate c, Coordinate c1, Coordinate c2)
    {
        const double epsilon = 1e-9; // Small value for floating-point comparison
        return Math.Abs(IsLeft(c, c1, c2)) < epsilon &&
               c.X >= Math.Min(c1.X, c2.X) && c.X <= Math.Max(c1.X, c2.X) &&
               c.Y >= Math.Min(c1.Y, c2.Y) && c.Y <= Math.Max(c1.Y, c2.Y);
    }

    public static int WindingNumber(Coordinate[] cs, Coordinate c)
    {
        int wn = 0;
        for (var i = 0; i < cs.Length - 1; i++)
        {
            var c1 = cs[i];
            var c2 = cs[i + 1];
            if (c1.Y <= c.Y)
            {
                if (c2.Y > c.Y && IsLeft(c1, c2, c) > 0.0)
                    wn++;
            }
            else if (c2.Y <= c.Y && IsLeft(c1, c2, c) < 0.0)
                wn--;
        }
        return wn;
    }

    public static bool Contains(Coordinate[] cs, Coordinate c)
    {
        int wn = 0;
        for (var i = 0; i < cs.Length - 1; i++)
        {
            var c1 = cs[i];
            var c2 = cs[i + 1];
            if (IsOnSegment(c, c1, c2))
                return false;
            if (c1.Y <= c.Y)
            {
                if (c2.Y > c.Y && IsLeft(c1, c2, c) > 0.0)
                    wn++;
            }
            else if (c2.Y <= c.Y && IsLeft(c1, c2, c) < 0.0)
                wn--;
        }
        return wn != 0;
    }


    public static bool IsOnSegment(Coordinate[] cs, Coordinate c)
    {
        int wn = 0;
        for (var i = 0; i < cs.Length - 1; i++)
        {
            var c1 = cs[i];
            var c2 = cs[i + 1];
            if (IsOnSegment(c, c1, c2))
                return true;
            if (c1.Y <= c.Y)
            {
                if (c2.Y > c.Y && IsLeft(c1, c2, c) > 0.0)
                    wn++;
            }
            else if (c2.Y <= c.Y && IsLeft(c1, c2, c) < 0.0)
                wn--;
        }
        return false;
    }

    public static double SignedArea(Coordinate[] cs)
    {
        if (cs.Length < 3)
            return 0.0;
        var c1 = cs[0];
        var c2 = cs[1];
        var x0 = c1.X;
        double sum = 0.0;
        for (var i = 1; i < cs.Length; i++)
        {
            var c3 = cs[i];
            var x = c2.X - x0;
            sum += x * (c1.Y - c3.Y);
            c1 = c2;
            c2 = c3;
        }
        return sum / 2.0;
    }

    public static bool IsCCW(Coordinate[] cs) =>
        SignedArea(cs) <= 0.0;
}