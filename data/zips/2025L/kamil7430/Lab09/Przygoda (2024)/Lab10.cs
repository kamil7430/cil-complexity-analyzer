using System;
using System.Linq;
using System.Collections.Generic;
using ASD.Graphs;

namespace ASD
{
    public class Lab10 : MarshalByRefObject
    {

        /// <param name="labyrinth">Graf reprezentujący labirynt</param>
        /// <param name="startingTorches">Ilość pochodni z jaką startują bohaterowie</param>
        /// <param name="roomTorches">Ilość pochodni w poszczególnych pokojach</param>
        /// <param name="debt>">Ilość złota jaką bohaterowie muszą zebrać</param>
        /// <param name="roomGold">Ilość złota w poszczególnych pokojach</param>
        /// <returns>Informację czy istnieje droga przez labirytn oraz tablicę reprezentującą kolejne wierzchołki na drodze. W przypadku, gdy zwracany jest false, wartość tego pola powinna być null.</returns>
        public (bool routeExists, int[] route) FindEscape(Graph labyrinth, int startingTorches, int[] roomTorches, int debt, int[] roomGold)
        {
            int n = labyrinth.VertexCount;
            var visited = new bool[n];
            visited[0] = true;
            LinkedList<int> r = new();
            r.AddLast(0);
            int torches = startingTorches + roomTorches[0],
                goldToGo = debt - roomGold[0];
            bool exists = false;

            RUN(0);

            return exists ? (true, r.ToArray()) : (false, null!);

            void RUN(int v)
            {
                if (exists)
                    return;
                if (torches <= 0 && v != n - 1)
                    return;
                if (v == n - 1 && goldToGo <= 0)
                {
                    exists = true;
                    return;
                }
                foreach (var neigh in labyrinth.OutNeighbors(v))
                {
                    if (!visited[neigh])
                    {
                        visited[neigh] = true;
                        r.AddLast(neigh);
                        torches += roomTorches[neigh] - 1;
                        goldToGo -= roomGold[neigh];
                        RUN(neigh);
                        if (exists)
                            return;
                        goldToGo += roomGold[neigh];
                        torches -= roomTorches[neigh] - 1;
                        r.RemoveLast();
                        visited[neigh] = false;
                    }
                }
            }
        }

        /// <param name="labyrinth">Graf reprezentujący labirynt</param>
        /// <param name="startingTorches">Ilość pochodni z jaką startują bohaterowie</param>
        /// <param name="roomTorches">Ilość pochodni w poszczególnych pokojach</param>
        /// <param name="debt">Ilość złota jaką bohaterowie muszą zebrać</param>
        /// <param name="roomGold">Ilość złota w poszczególnych pokojach</param>
        /// <param name="dragonDelay">Opóźnienie z jakim wystartuje smok</param>
        /// <returns>Informację czy istnieje droga przez labirynt oraz tablicę reprezentującą kolejne wierzchołki na drodze. W przypadku, gdy zwracany jest false, wartość tego pola powinna być null.</returns>

        public (bool routeExists, int[] route) FindEscapeWithHeadstart(Graph labyrinth, int startingTorches, int[] roomTorches, int debt, int[] roomGold, int dragonDelay)
        {
            int n = labyrinth.VertexCount;
            var destroyed = new bool[n];
            LinkedList<int> r = new();
            r.AddLast(0);
            int torches = startingTorches + roomTorches[0],
                goldToGo = debt - roomGold[0];
            roomTorches[0] = 0;
            roomGold[0] = 0;
            int turn = 0;
            int dragonPosition = -1;
            bool exists = false;

            RUN(0);

            return exists ? (true, r.ToArray()) : (false, null!);

            void IncUpdateDestroyed()
            {
                if (dragonPosition < 0)
                {
                    if (turn == dragonDelay + 1)
                        dragonPosition = 0;
                    else
                        return;
                }
                else
                {
                    dragonPosition = r.FindLast(dragonPosition).Next.Value;
                }
                destroyed[dragonPosition] = true;
            }

            void DecUpdateDestroyed()
            {
                if (dragonPosition >= 0)
                {
                    destroyed[dragonPosition] = false;
                    if (dragonPosition == 0)
                        dragonPosition = -1;
                    else
                        dragonPosition = r.Find(dragonPosition).Previous.Value;
                }
            }

            bool ToDestroy(int v)
            {
                if (dragonPosition < 0)
                {
                    if (turn < dragonDelay)
                        return false;
                    else if (v == 0)
                        return true;
                    return false;
                }
                else
                {
                    return v == r.FindLast(dragonPosition).Next.Value;
                }
            }

            void RUN(int v)
            {
                if (exists)
                    return;
                if (torches <= 0 && v != n - 1)
                    return;
                if (v == n - 1 && goldToGo <= 0)
                {
                    exists = true;
                    return;
                }
                foreach (var neigh in labyrinth.OutNeighbors(v))
                {
                    if (!destroyed[neigh] /*&& dragonPosition != neigh
                        && !ToDestroy(neigh)*/)
                    {
                        turn++;
                        int index = turn - dragonDelay;
                        r.AddLast(neigh);
                        //IncUpdateDestroyed();
                        if (index >= 0)
                            destroyed[r.ElementAt(index)] = true;
                        torches += roomTorches[neigh] - 1;
                        goldToGo -= roomGold[neigh];
                        (int t, int g) = (roomTorches[neigh], roomGold[neigh]);
                        roomTorches[neigh] = 0;
                        roomGold[neigh] = 0;
                        RUN(neigh);
                        if (exists)
                            return;
                        roomGold[neigh] = g;
                        roomTorches[neigh] = t;
                        goldToGo += roomGold[neigh];
                        torches -= roomTorches[neigh] - 1;
                        //DecUpdateDestroyed();
                        if (index >= 0)
                            destroyed[r.ElementAt(index)] = false;
                        r.RemoveLast();
                        turn--;
                    }
                }
            }
        }
    }
}
