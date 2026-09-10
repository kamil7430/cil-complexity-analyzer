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
            var pointsList = new LinkedList<Point>(points);

            int removedFirst = 0;
            try
            {
                while (pointsList.First().y <= points.ElementAt(1).y) // first->next O(1)
                {
                    pointsList.RemoveFirst();
                    removedFirst++;
                }

                int removedLast = 0;
                while (pointsList.Last().y <= pointsList.ElementAt(pointsList.Count - 2).y) // last->prev O(1)
                {
                    pointsList.RemoveLast();
                    removedLast++;
                }
            }
            catch (InvalidOperationException)
            {
                return new double[n];
            }
            catch (ArgumentOutOfRangeException)
            {
                return new double[n];
            }

            var pts = pointsList.ToArray();
            var lakes = new List<(int index1, int index2)>();

            int highest = 0;
            int? secondHighest = null;
            for (int i = 1; i < pts.Length; i++)
            {
                if (i == pts.Length - 1 || !leftTurn(pts[i - 1], pts[i], pts[i + 1]))
                {
                    if (pts[i].y >= pts[highest].y)
                    {
                        secondHighest = highest;
                        highest = i;
                        lakes.Add((secondHighest.Value, highest));
                        secondHighest = null;
                    }
                    else if (secondHighest is null || pts[i].y >= pts[secondHighest.Value].y)
                    {
                        secondHighest = i;
                        if (i == pts.Length - 1)
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
                var y = Math.Min(pts[lake.index1].y, pts[lake.index2].y);
                for (int i = lake.index1 + 1; i < lake.index2; i++)
                    toReturn[i] = y - pts[i].y;
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
            return -1;
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
