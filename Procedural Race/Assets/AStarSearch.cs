using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

// A* needs only a WeightedGraph and a location type L, and does *not*
// have to be a grid. However, in the example code I am using a grid.
public interface WeightedGraph<L>
{
    float Cost(Location a, Location b);
    IEnumerable<Location> Neighbors(Location id);
    int DecimationFactor { get; }
}


public class Location
{
    // Implementation notes: I am using the default Equals but it can
    // be slow. You'll probably want to override both Equals and
    // GetHashCode in a real project.

    public int x, y;
    public readonly float cost;
    public Location(int x, int y, float mapCost = 0f)
    {
        this.x = x;
        this.y = y;
        cost = mapCost;
    }

    public Location(Vector3 position)
    {
        this.x = Mathf.RoundToInt(position.x);
        this.y = Mathf.RoundToInt(position.y);
    }

    public override bool Equals(System.Object obj)
    {

        if (obj == null)
        {
            return false;
        }

        Location location = obj as Location;
        if ((System.Object)location == null)
        {
            return false;
        }

        return this.x == location.x && this.y == location.y;
    }

    public override int GetHashCode()
    {
        return x ^ y;
    }

    public Vector3 vector3()
    {
        return new Vector3(this.x, this.y, 0f);
    }
}


public class SquareGrid : WeightedGraph<Location>
{
    // Implementation notes: I made the fields public for convenience,
    // but in a real project you'll probably want to follow standard
    // style and make them private.

    public static readonly Location[] DIRS = new[]
    {
        new Location(1, 0), // to right of tile
        new Location(0, -1), // below tile
        new Location(-1, 0), // to left of tile
        new Location(0, 1), // above tile
        new Location(1, 1), // diagonal top right
        new Location(-1, 1), // diagonal top left
        new Location(1, -1), // diagonal bottom right
        new Location(-1, -1) // diagonal bottom left
    };

    public int width, height;
    public int DecimationFactor { get; }
    private float[,] _map;
    //public HashSet<Location> floor = new HashSet<Location>();
    //public HashSet<Location> objects = new HashSet<Location>();
    //public HashSet<Location> forests = new HashSet<Location>();

    public SquareGrid(float[,] heightMap, int decimationFactor = 1)
    {
        width = heightMap.GetLength(0) / decimationFactor;
        height = heightMap.GetLength(1) / decimationFactor;
        DecimationFactor = decimationFactor;
        if (decimationFactor == 1)
        {
            _map = heightMap;
        }
        else
        {
            var decimatedMap = new float[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    decimatedMap[x, y] = heightMap[x * decimationFactor, y * decimationFactor];
                }
            }

            _map = decimatedMap;
        }

    }

    public bool InBounds(int locationX, int locationY)
    {
        return 0 <= locationX && locationX < width
            && 0 <= locationY && locationY < height;
    }

    public bool Passable(int locationX, int locationY)
    {
        return _map[locationX,locationY] <= 1f;
    }

    public float Cost(Location a, Location b)
    {
        return Mathf.Approximately(Pathing.Heuristic(a, b),2f) ? 
            b.cost * 1.4f *10: 
            b.cost * 10;
    }

    public Location GetLocationAtCoordinates(int x, int y)
    {
        return new Location(x, y, _map[x,y]);
    }

    public IEnumerable<Location> Neighbors(Location id)
    {
        foreach (var dir in DIRS)
        {
            var newX = id.x + dir.x;
            var newY = id.y + dir.y;
           
            if (InBounds(newX, newY) && Passable(newX, newY))
            {
                yield return new Location(newX, newY, _map[newX, newY]); ;
            }
        }
    }
}

public class Tuple<T1, T2>
{
    public T1 Item1 { get; private set; }
    public T2 Item2 { get; private set; }
    internal Tuple(T1 first, T2 second)
    {
        Item1 = first;
        Item2 = second;
    }

}

public static class Tuple
{
    public static Tuple<T1, T2> Create<T1, T2>(T1 first, T2 second)
    {
        var tuple = new Tuple<T1, T2>(first, second);
        return tuple;
    }
}

public class PriorityQueue<T>
{
    // I'm using an unsorted array for this example, but ideally this
    // would be a binary heap. Find a binary heap class:
    // * https://bitbucket.org/BlueRaja/high-speed-priority-queue-for-c/wiki/Home
    // * http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
    // * http://xfleury.github.io/graphsearch.html
    // * http://stackoverflow.com/questions/102398/priority-queue-in-net

    private List<Tuple<T, float>> elements = new List<Tuple<T, float>>();

    public int Count
    {
        get { return elements.Count; }
    }

    public void Enqueue(T item, float priority)
    {
        elements.Add(Tuple.Create(item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].Item2 < elements[bestIndex].Item2)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Item1;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}


public class Pathing
{
    public Dictionary<Location, Location> cameFrom = new Dictionary<Location, Location>();
    public Dictionary<Location, float> costSoFar = new Dictionary<Location, float>();

    private Location start;
    private Location goal;

    private int _decimationFactor;
    // Note: a generic version of A* would abstract over Location and
    // also Heuristic
    static public int Heuristic(Location a, Location b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    public Pathing(WeightedGraph<Location> graph, Location start, Location goal)
    {
        this.start = start;
        this.goal = goal;
        _decimationFactor = graph.DecimationFactor;
        var frontier = new PriorityQueue<Location>();
        frontier.Enqueue(start, 0);

        cameFrom.Add(start, start);
        costSoFar.Add(start, 0);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current.Equals(goal))
            {
                break;
            }

            foreach (var next in graph.Neighbors(current))
            {
                var newCost = costSoFar[current] + graph.Cost(current, next);
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    if (costSoFar.ContainsKey(next))
                    {
                        cameFrom.Remove(next);
                        costSoFar.Remove(next);
                    }

                    costSoFar.Add(next, newCost);
                    var priority = newCost + Heuristic(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom.Add(next, current);
                }
            }
        }
    }

    public List<Location> WhereTo()
    {

        List<Location> path = new List<Location>();
        Location current = goal;

        while (!current.Equals(start))
        {
            if (!cameFrom.ContainsKey(current))
            {
                return new List<Location>();
            }
            Location from = cameFrom[current];
            path.Add(current);
            current = from;
        }

        path.Reverse();
        foreach (var location in path)
        {
            location.x *= _decimationFactor;
            location.y *= _decimationFactor;
        }
        return path;
    }
}