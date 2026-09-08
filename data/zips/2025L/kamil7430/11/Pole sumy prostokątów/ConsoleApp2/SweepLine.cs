
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD;

class SweepLine
{
    /// <summary>
    /// Struktura pomocnicza opisująca zdarzenie
    /// </summary>
    /// <remarks>
    /// Można jej użyć, przerobić, albo w ogóle nie używać i zrobić po swojemu
    /// </remarks>
    struct SweepEvent
    {
        /// <summary>
        /// Współrzędna zdarzenia
        /// </summary>
        public double Coord;

        /// <summary>
        /// Czy zdarzenie oznacza początek odcinka/prostokąta
        /// </summary>
        public bool IsStartingPoint;

        /// <summary>
        /// Indeks odcinka/prodtokąta w odpowiedniej tablicy
        /// </summary>
        public int Idx;

        public SweepEvent(double c, bool sp, int i = -1)
        {
            Coord = c;
            IsStartingPoint = sp;
            Idx = i;
        }
    }

    /// <summary>
    /// Funkcja obliczająca długość teoriomnogościowej sumy pionowych odcinków
    /// </summary>
    /// <returns>Długość teoriomnogościowej sumy pionowych odcinków</returns>
    /// <param name="segments">Tablica z odcinkami, których teoriomnogościowej sumy długość należy policzyć</param>
    /// Każdy odcinek opisany jest przez dwa punkty: początkowy i końcowy
    /// </param>
    public double VerticalSegmentsUnionLength(Geometry.Segment[] segments)
    {
        List<SweepEvent> events = [];
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            var start = segment.ps.y < segment.pe.y ? segment.ps.y : segment.pe.y;
            var end = segment.ps.y > segment.pe.y ? segment.ps.y : segment.pe.y;
            events.Add(new SweepEvent(start, true, i));
            events.Add(new SweepEvent(end, false, i));
        }
        
        events.Sort((a, b) => a.Coord.CompareTo(b.Coord));

        int segmentsEntered = 0;
        double? startOfSegment = null;
        double length = 0;
        foreach (var eventItem in events)
        {
            if (eventItem.IsStartingPoint)
            {
                segmentsEntered++;
                if (segmentsEntered == 1)
                    startOfSegment = eventItem.Coord;
            }
            else
            {
                segmentsEntered--;
                if (segmentsEntered == 0)
                    length += eventItem.Coord - startOfSegment.Value;
            }
        }

        return length;
    }

    /// <summary>
    /// Funkcja obliczająca pole teoriomnogościowej sumy prostokątów
    /// </summary>
    /// <returns>Pole teoriomnogościowej sumy prostokątów</returns>
    /// <param name="rectangles">Tablica z prostokątami, których teoriomnogościowej sumy pole należy policzyć</param>
    /// Każdy prostokąt opisany jest przez cztery wartości: minimalna współrzędna X, minimalna współrzędna Y, 
    /// maksymalna współrzędna X, maksymalna współrzędna Y.
    /// </param>
    public double RectanglesUnionArea(Geometry.Rectangle[] rectangles)
    {
        List<SweepEvent> events = [];

        for (int i = 0; i < rectangles.Length; i++)
        {
            var rc = rectangles[i];
            var start = rc.MinX < rc.MaxX ? rc.MinX : rc.MaxX;
            var end = rc.MinX > rc.MaxX ? rc.MinX : rc.MaxX;
            events.Add(new SweepEvent(start, true, i));
            events.Add(new SweepEvent(end, false, i));
        }
        
        events.Sort((a, b) => a.Coord.CompareTo(b.Coord));

        List<Geometry.Segment> segments = [];
        double area = 0;
        double? prevX = null;
        foreach (var eventItem in events)
        {
            if (prevX != null)
                area += VerticalSegmentsUnionLength(segments.ToArray()) * (eventItem.Coord - prevX.Value);

            prevX = eventItem.Coord;
            
            if (eventItem.IsStartingPoint)
                segments.Add(new Geometry.Segment(new Geometry.Point(0, rectangles[eventItem.Idx].MinY),
                    new Geometry.Point(0, rectangles[eventItem.Idx].MaxY)));
            else
            {
                var toRemove = segments.First(segment =>
                    segment.ps.y == rectangles[eventItem.Idx].MinY && segment.pe.y == rectangles[eventItem.Idx].MaxY
                );
                segments.Remove(toRemove);
            }
        }

        return area;
    }

}


