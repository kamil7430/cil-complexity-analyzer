using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
    public class Lab11 : System.MarshalByRefObject
    {

        // iloczyn wektorowy
        private int Cross((double, double) o, (double, double) a, (double, double) b)
        {
            double value = (a.Item1 - o.Item1) * (b.Item2 - o.Item2) - (a.Item2 - o.Item2) * (b.Item1 - o.Item1);
            return Math.Abs(value) < 1e-10 ? 0 : value < 0 ? -1 : 1;
        }

        // Etap 1
        // po prostu otoczka wypukła
        public (double, double)[] ConvexHull((double x, double y)[] points)
        {
            if (points.Length <= 2)
                return points;

            var minimalPoint = 0;
            for (int i = 1; i < points.Length; i++)
            {
                if (points[i].y < points[minimalPoint].y ||
                    (points[i].y == points[minimalPoint].y && 
                    points[i].x < points[minimalPoint].x))
                    minimalPoint = i;
            }
            (points[0], points[minimalPoint]) = (points[minimalPoint], points[0]);

            var pointsList = new List<(double x, double y)>(points);

            pointsList.Sort((a, b) =>
            {
                if (a == points[0]) return -1;
                if (b == points[0]) return 1;
                if (a == b) return 0;
                return -Cross(points[0], a, b);
            });

            for (int i = pointsList.Count - 1; i >= 2; i--)
            {
                var o = pointsList[i - 2];
                var a = pointsList[i];
                var b = pointsList[i - 1];
                if (Cross(o, a, b) == 0)
                {
                    double distanceII = (o.x - a.x) * (o.x - a.x) + (o.y - a.y) * (o.y - a.y);
                    double distanceI = (o.x - b.x) * (o.x - b.x) + (o.y - b.y) * (o.y - b.y);
                    if (distanceI > distanceII)
                        pointsList.RemoveAt(i);
                    else
                        pointsList.RemoveAt(i - 1);
                }
            }

            Stack<(double x, double y)> S = [];
            S.Push(pointsList[0]);
            S.Push(pointsList[1]);
            for (int k = 2; k < pointsList.Count; k++)
            {
                var top = S.Pop();
                var belowTop = S.Pop();
                while (Cross(belowTop, top, pointsList[k]) <= 0)
                {
                    top = belowTop;
                    belowTop = S.Pop();
                }
                S.Push(belowTop);
                S.Push(top);
                S.Push(pointsList[k]);
            }

            return S.Reverse().ToArray();
        }

        // Etap 2
        // oblicza otoczkę dwóch wielokątów wypukłych
        public (double, double)[] ConvexHullOfTwo((double x, double y)[] poly1, (double x, double y)[] poly2)
        {
            var (lowerHull1, upperHull1) = DividePoly(poly1);
            var (lowerHull2, upperHull2) = DividePoly(poly2);

            var lowerHull = MergeHulls(lowerHull1, lowerHull2, false);
            var upperHull = MergeHulls(upperHull1, upperHull2, true);

            var cleanedUpLowerHull = CleanUpLowerHull(lowerHull);
            var cleanedUpUpperHull = CleanUpUpperHull(upperHull);

            var finalLowerHull = MakeHull(cleanedUpLowerHull);
            var finalUpperHull = MakeHull(cleanedUpUpperHull);

            //if (finalLowerHull.Count == 3 && finalUpperHull.Count == 3)
            //    return finalLowerHull.ToArray();

            //return null;
            return [.. finalLowerHull, .. finalUpperHull[1..(finalUpperHull.Count - 1)]];
        }

        private (List<(double x, double y)> lowerHull, List<(double x, double y)> upperHull) DividePoly((double x, double y)[] poly)
        {
            int smallestXIndex = 0, biggestXIndex = 0;
            for (int i = 0; i < poly.Length; i++)
            {
                if (poly[i].x < poly[smallestXIndex].x)
                    smallestXIndex = i;
                if (poly[i].x > poly[biggestXIndex].x)
                    biggestXIndex = i;
            }
            List<(double, double)> lowerHull = [], upperHull = [];
            if (smallestXIndex < biggestXIndex)
            {
                lowerHull = new(poly[smallestXIndex..(biggestXIndex + 1)]);
                upperHull = [.. poly[biggestXIndex..], .. poly[..(smallestXIndex + 1)]];
            }
            else
            {
                lowerHull = [.. poly[smallestXIndex..], .. poly[..(biggestXIndex + 1)]];
                upperHull = new(poly[biggestXIndex..(smallestXIndex + 1)]);
            }
            return (lowerHull, upperHull);
        }

        private List<(double x, double y)> MergeHulls(List<(double x, double y)> hull1, List<(double x, double y)> hull2, bool isUpper)
        {
            List<(double x, double y)> mergedHull = [];
            int n = hull1.Count + hull2.Count;
            int i = 0, j = 0;
            while (i + j < n)
            {
                if (i == hull1.Count)
                    mergedHull.Add(hull2[j++]);
                else if (j == hull2.Count)
                    mergedHull.Add(hull1[i++]);
                else
                {
                    if (hull1[i] == hull2[j])
                        i++;
                    else if (isUpper)
                    { 
                        if (hull1[i].x > hull2[j].x)
                            mergedHull.Add(hull1[i++]);
                        else
                            mergedHull.Add(hull2[j++]);
                    }
                    else
                    {
                        if (hull1[i].x < hull2[j].x)
                            mergedHull.Add(hull1[i++]);
                        else
                            mergedHull.Add(hull2[j++]);
                    }
                }
            }
            return mergedHull;
        }

        private List<(double x, double y)> MakeHull(List<(double x, double y)> givenHull)
        {
            List<(double x, double y)> hull = new(givenHull);
            for (int i = hull.Count - 1; i >= 2; i--)
            {
                var o = hull[i - 2];
                var a = hull[i - 1];
                var b = hull[i];
                if (Cross(o, a, b) == 0)
                {
                    double distanceII = (o.x - a.x) * (o.x - a.x) + (o.y - a.y) * (o.y - a.y);
                    double distanceI = (o.x - b.x) * (o.x - b.x) + (o.y - b.y) * (o.y - b.y);
                    if (distanceI > distanceII)
                        hull.RemoveAt(i - 1);
                    else
                        hull.RemoveAt(i);
                }
            }

            Stack<(double x, double y)> S = [];
            S.Push(hull[0]);
            S.Push(hull[1]);
            for (int k = 2; k < hull.Count; k++)
            {
                var top = S.Pop();
                var belowTop = S.Pop();
                while (Cross(belowTop, top, hull[k]) <= 0)
                {
                    top = belowTop;
                    belowTop = S.Pop();
                }
                S.Push(belowTop);
                S.Push(top);
                S.Push(hull[k]);
            }
            return S.Reverse().ToList();
        }

        private List<(double x, double y)> CleanUpLowerHull(List<(double x, double y)> givenHull)
        {
            List<(double x, double y)> hull = new(givenHull);
            double maxY = Math.Max(hull[hull.Count - 1].y, hull[0].y);
            for (int i = hull.Count - 2; i >= 1; i--)
                if (hull[i].y > maxY)
                    hull.RemoveAt(i);
            return hull;
        }

        private List<(double x, double y)> CleanUpUpperHull(List<(double x, double y)> givenHull)
        {
            List<(double x, double y)> hull = new(givenHull);
            double minY = Math.Min(hull[hull.Count - 1].y, hull[0].y);
            for (int i = hull.Count - 2; i >= 1; i--)
                if (hull[i].y < minY)
                    hull.RemoveAt(i);
            return hull;
        }
    }
}
