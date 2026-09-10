using System;
using System.Collections.Generic;

namespace ASD
{
    public class WaterCalculator : MarshalByRefObject
    {

        /*
         * Metoda sprawdza, czy przechodząc p1->p2->p3 skręcamy w lewo 
         * (jeżeli idziemy prosto, zwracany jest fałsz).
         */
        private bool leftTurn(Point p1, Point p2, Point p3)
        {
            Point w1 = new Point(p2.x - p1.x, p2.y - p1.y);
            Point w2 = new Point(p3.x - p2.x, p3.y - p2.y);
            double vectProduct = w1.x * w2.y - w2.x * w1.y;
            return vectProduct > 0;
        }


        /*
         * Metoda wyznacza punkt na odcinku p1-p2 o zadanej współrzędnej y.
         * Jeżeli taki punkt nie istnieje (bo cały odcinek jest wyżej lub niżej), zgłaszany jest wyjątek ArgumentException.
         */
        private Point getPointAtY(Point p1, Point p2, double y)
        {
            if (p1.y != p2.y)
            {
                double newX = p1.x + (p2.x - p1.x) * (y - p1.y) / (p2.y - p1.y);
                if ((newX - p1.x) * (newX - p2.x) > 0)
                    throw new ArgumentException("Odcinek p1-p2 nie zawiera punktu o zadanej współrzędnej y!");
                return new Point(p1.x + (p2.x - p1.x) * (y - p1.y) / (p2.y - p1.y), y);
            }
            else
            {
                if (p1.y != y)
                    throw new ArgumentException("Odcinek p1-p2 nie zawiera punktu o zadanej współrzędnej y!");
                return new Point((p1.x + p2.x) / 2, y);
            }
        }

        /// <summary>
        /// Funkcja zwraca tablice t taką, że t[i] jest głębokością, na jakiej znajduje się punkt points[i].
        /// 
        /// Przyjmujemy, że pierwszy punkt z tablicy points jest lewym krańcem, a ostatni - prawym krańcem łańcucha górskiego.
        /// </summary>
        public double[] PointDepths(Point[] points)
        {
            int n = points.Length;
            var toReturn = new double[n];
            bool fragmentToProcess = false;
            int? startOfSegment = null;

            for (int i = 0; i < n - 1; i++)
            {
                if (points[i].x <= points[i + 1].x)
                {
                    fragmentToProcess = true;
                    startOfSegment ??= i;
                }
                else
                {
                    if (fragmentToProcess) 
                    {
                        //Console.WriteLine($"rozpatruje {startOfSegment.Value}, {i+1}");
                        var depths = PointDepthsSimple(points[startOfSegment.Value..(i+1)]);
                        for (int j = startOfSegment.Value; j < i+1; j++)
                            toReturn[j] = depths[j - startOfSegment.Value];
                        fragmentToProcess = false;
                        startOfSegment = null;
                    }
                }
            }

            if (fragmentToProcess)
            {
                //Console.WriteLine($"rozpatruje {startOfSegment.Value}, {n - 1}");
                var depths = PointDepthsSimple(points[startOfSegment.Value..]);
                for (int j = startOfSegment.Value; j < n; j++)
                    toReturn[j] = depths[j - startOfSegment.Value];
            }

            return toReturn;
        }

        public double[] PointDepthsSimple(Point[] points)
        {
            int n = points.Length;
            if (n <= 2)
                return new double[n];

            int removedFirst = 0;
            int removedLast = 0;
            try
            {
                while (points[removedFirst].y <= points[removedFirst + 1].y)
                    removedFirst++;

                while (points[n - removedLast - 1].y <= points[n - removedLast - 2].y)
                    removedLast++;
            }
            catch (IndexOutOfRangeException)
            {
                return new double[n];
            }
            catch (InvalidOperationException)
            {
                return new double[n];
            }
            catch (ArgumentOutOfRangeException)
            {
                return new double[n];
            }

            var lakes = new List<(int index1, int index2)>();

            int highest = removedFirst;
            int? secondHighest = null;
            for (int i = removedFirst + 1; i < points.Length - removedLast; i++)
            {
                if (i == points.Length - removedLast - 1 || !leftTurn(points[i - 1], points[i], points[i + 1]))
                {
                    if (points[i].y >= points[highest].y)
                    {
                        secondHighest = highest;
                        highest = i;
                        lakes.Add((secondHighest.Value, highest));
                        secondHighest = null;
                    }
                    else if (secondHighest is null || points[i].y >= points[secondHighest.Value].y)
                    {
                        secondHighest = i;
                        if (i == points.Length - removedLast - 1)
                            lakes.Add((highest, secondHighest.Value));
                    }
                    else
                    {
                        lakes.Add((highest, secondHighest.Value));
                        highest = secondHighest.Value;
                        secondHighest = i;
                    }
                }
            }

            var toReturn = new double[n];
            foreach (var lake in lakes)
            {
                var y = Math.Min(points[lake.index1].y, points[lake.index2].y);
                for (int i = lake.index1 + 1; i < lake.index2; i++)
                    toReturn[i] = y - points[i].y;
            }
            
            return toReturn;
        }

        /// <summary>
        /// Funkcja zwraca objętość wody, jaka zatrzyma się w górach.
        /// 
        /// Przyjmujemy, że pierwszy punkt z tablicy points jest lewym krańcem, a ostatni - prawym krańcem łańcucha górskiego.
        /// </summary>
        public double WaterVolume(Point[] points)
        {
            var depths = PointDepths(points).Select(p => Math.Abs(p)).ToArray();
            int n = points.Length;
            double vol = 0.0;

            for (int i = 0; i < n - 1; i++)
            {
                try
                {

                    if (depths[i] == 0 && depths[i + 1] == 0)
                        continue;
                    if (depths[i + 1] == 0 && points[i + 1].y > points[i].y + depths[i])
                        vol += (depths[i] + depths[i + 1]) * (getPointAtY(points[i], points[i + 1], depths[i] + points[i].y).x - points[i].x) / 2;
                    else if (depths[i] == 0 && points[i].y > points[i + 1].y + depths[i + 1])
                        vol += (depths[i] + depths[i + 1]) * (points[i + 1].x - getPointAtY(points[i], points[i + 1], depths[i + 1] + points[i + 1].y).x) / 2;
                    else
                        vol += (depths[i] + depths[i + 1]) * (points[i + 1].x - points[i].x) / 2;

                }
                catch (Exception)
                {
                    throw new Exception();
                }
            }

            return vol;
        }
    }

    [Serializable]
    public struct Point
    {
        public double x, y;
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
