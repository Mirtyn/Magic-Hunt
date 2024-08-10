using System;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.ComponentModel;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine.Experimental.GlobalIllumination;
using System.Runtime.CompilerServices;
using Assets.Models;
using JetBrains.Annotations;
using Assets.DungeonGeneratorAlgorithms;
using UnityEditor;
using System.Linq;
using Point = Assets.DungeonGeneratorAlgorithms.Point;
using Edge = Assets.DungeonGeneratorAlgorithms.Edge;
using System.Threading;
using System.Collections;
using System.Security.Cryptography;

public class DungeonGenerator
{
    private int mapSizeX = 150;
    private int mapSizeY = 150;

    private int numRoomsToGenerate = 30;

    private int numRoomsToReturn = 0;

    private int minNumRooms = 8;
    private int maxNumRooms = 12;

    private int maxNumAttemptsGenerateFillInGapRooms = 50;

    private List<Room> rooms = new List<Room>();

    private static System.Random random = new System.Random();

    //private float spawnCircleRadius = 10f;

    private int minRoomWidth = 3;
    private int maxRoomWidth = 12;

    private int minRoomHeight = 3;
    private int maxRoomHeight = 12;

    private Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();

    public DungeonGenerator()
    {
        numRoomsToReturn = random.Next(minNumRooms, maxNumRooms);
    }

    public DungeonGenerator(int mapSizeX, int mapSizeY, int numRoomsToGenerate, int minNumRoomsToReturn, int maxNumRoomsToReturn, int maxNumAttemptsGenerateFillInGapRooms, int minRoomWidth, int maxRoomWidth, int minRoomHeight, int maxRoomHeight)
    {
        this.mapSizeX = mapSizeX;
        this.mapSizeY = mapSizeY;
        this.numRoomsToGenerate = numRoomsToGenerate;
        this.minNumRooms = minNumRoomsToReturn;
        this.maxNumRooms = maxNumRoomsToReturn;
        this.minRoomWidth = minRoomWidth;
        this.maxRoomWidth = maxRoomWidth;
        this.minRoomHeight = minRoomHeight;
        this.maxRoomHeight = maxRoomHeight;
        this.maxNumAttemptsGenerateFillInGapRooms = maxNumAttemptsGenerateFillInGapRooms;
        numRoomsToReturn = random.Next(minNumRooms, maxNumRooms);
    }

    public Dictionary<Vector2, Tile> Start()
    {
        Vector2 leftB = Vector2.zero;
        Vector2 rightB = new Vector2(mapSizeX, 0);
        Vector2 leftT = new Vector2(0, mapSizeY);
        Vector2 rightT = new Vector2(mapSizeX, mapSizeY);
        Debug.DrawLine(leftB, rightB, UnityEngine.Color.magenta, 100f);
        Debug.DrawLine(leftB, leftT, UnityEngine.Color.magenta, 100f);
        Debug.DrawLine(rightT, rightB, UnityEngine.Color.magenta, 100f);
        Debug.DrawLine(rightT, leftT, UnityEngine.Color.magenta, 100f);

        for (int i = 0; i < numRoomsToGenerate; i++)
        {
            rooms.Add(CreateRoom(GetRandomPointInSquare(mapSizeX, mapSizeY)));
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            List<Room> roomsToRemove = new List<Room>();

            foreach (Room other in rooms)
            {
                if (other != room && CheckForRoomOverlap(room, other))
                {
                    roomsToRemove.Add(other);
                }
            }

            foreach (Room removeRoom in roomsToRemove)
            {
                rooms.Remove(removeRoom);
            }
        }

        if (rooms.Count > numRoomsToReturn)
        {
            int numRoomsToRemove = rooms.Count - numRoomsToReturn;

            for (int i = 0; i < numRoomsToRemove; i++)
            {
                int rndNum = random.Next(rooms.Count);
                rooms.RemoveAt(rndNum);
            }
        }
        else if (rooms.Count < numRoomsToReturn)
        {
            int missingRooms = numRoomsToReturn - rooms.Count;

            for (int i = 0; i < maxNumAttemptsGenerateFillInGapRooms; i++)
            {
                Room room = CreateRoom(GetRandomPointInSquare(mapSizeX, mapSizeY));

                bool isOverlap = rooms.Any(r => CheckForRoomOverlap(room, r));

                if (!isOverlap)
                {
                    rooms.Add(room);
                    missingRooms--;
                }

                if (missingRooms <= 0)
                {
                    break;
                }
            }
        }

        List<Tile> tilesList = ConvertRoomsToTiles(rooms);

        foreach (Tile tile in tilesList)
        {
            if (!tiles.TryAdd(tile.Position, tile))
            {
                tile.Position = Vector2.zero;
                tile.SceneObject.transform.position = tile.Position;
                tile.SceneObject.transform.SetParent(null);
            }
        }

        Point[] points = new Point[rooms.Count];

        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            float x = room.Origin.x + room.Width / 2;
            float y = room.Origin.y + room.Height / 2;
            Point point = new Point(x, y);
            points[i] = point;
        }

        TriangleMesh triangleMesh = BowyerWatson.Triangulate(points, mapSizeX, mapSizeY);

        Prim.GetMinimumSpanningTree(triangleMesh);

        //triangleMesh.Visualize();

        return tiles;
    }

    private Room CreateRoom(Vector2 startPoint)
    {
        int width = random.Next(minRoomWidth, maxRoomWidth + 1);
        int height = random.Next(minRoomHeight, maxRoomHeight + 1);

        Room room = new Room(width, height, startPoint);
        return room;
    }

    private Vector2 GetRandomPointInSquare(int width, int height)
    {
        int x = random.Next(width + 1);
        int y = random.Next(height + 1);

        return new Vector2(x, y);
    }

    private Vector2 GetRandomPointInCircle(float radius)
    {
        float t = 2 * Mathf.PI * (float)random.NextDouble();
        float u = (float)random.NextDouble() + (float)random.NextDouble();
        float r = u > 1 ? 2 - u : u;

        float x = radius * r * Mathf.Cos(t);
        float y = radius * r * Mathf.Sin(t);
        return new Vector2(x, y);
    }

    private bool CheckForRoomOverlap(Room room, Room other)
    {
        float roomMinX = room.Origin.x - 1;
        float roomMaxX = room.Origin.x + room.Width + 1;

        float roomMinY = room.Origin.y - 1;
        float roomMaxY = room.Origin.y + room.Height + 1;

        float otherRoomMinX = other.Origin.x;
        float otherRoomMaxX = other.Origin.x + other.Width;

        float otherRoomMinY = other.Origin.y;
        float otherRoomMaxY = other.Origin.y + other.Height;

        if (roomMaxX > otherRoomMinX && roomMinX < otherRoomMaxX && roomMaxY > otherRoomMinY && roomMinY < otherRoomMaxY)
        {
            return true;
        }

        return false;
    }

    private Room TryCreateRoomToFillInGaps()
    {
        Vector2 startPos = new Vector2(random.Next(mapSizeX), random.Next(mapSizeY));

        int width = random.Next(minRoomWidth, maxRoomWidth + 1);
        int height = random.Next(minRoomHeight, maxRoomHeight + 1);

        Room room = new Room(width, height, startPos);


        return room;
    }

    private List<Tile> ConvertRoomsToTiles(List<Room> rooms)
    {
        List<Tile> tiles = new List<Tile>();

        foreach (Room room in rooms)
        {
            for (int x = 0; x < room.Width; x++)
            {
                for (int y = 0; y < room.Height; y++)
                {
                    Vector2 pos = new Vector2(room.Origin.x + x, room.Origin.y + y);
                    GameObject obj = GameObject.Instantiate(ProjectBehaviour.GameManager.TilePrefab, pos, Quaternion.identity, ProjectBehaviour.GameManager.transform);
                    Tile tile = new Tile(pos, obj);
                    tiles.Add(tile);
                }
            }
        }

        return tiles;
    }
}

public class Tile
{
    public Vector2 Position;
    public GameObject SceneObject;

    public Tile(Vector2 position, GameObject sceneObject)
    {
        Position = position;
        SceneObject = sceneObject;
    }
}

public class Room
{
    public int Width;
    public int Height;

    public Vector2 Origin;

    public Room(int width, int height, Vector2 origin)
    {
        Width = width;
        Height = height;
        Origin = origin;
    }
}
namespace Assets.DungeonGeneratorAlgorithms
{
    public class LinearEquation
    {
        float _A;
        float _B;
        float _C;

        //Ax + By = C
        public LinearEquation(Point pointA, Point pointB)
        {
            float deltaX = pointB.x - pointA.x;
            float deltaY = pointB.y - pointA.y;
            _A = deltaY; //y2-y1
            _B = -deltaX; //x1-x2
            _C = _A * pointA.x + _B * pointA.y;
        }

        public LinearEquation()
        {
            
        }

        public LinearEquation PerpendicularLineAt(Point point)
        {
            LinearEquation newLine = new LinearEquation();

            newLine._A = -_B;
            newLine._B = _A;
            newLine._C = newLine._A * point.x + newLine._B * point.y;

            return newLine;
        }

        public static Point GetCrossingPoint(LinearEquation line1, LinearEquation line2)
        {
            float A1 = line1._A;
            float A2 = line2._A;
            float B1 = line1._B;
            float B2 = line2._B;
            float C1 = line1._C;
            float C2 = line2._C;

            // Cramer's rule
            float determinant = A1 * B2 - A2 * B1;
            float determinantX = C1 * B2 - C2 * B1;
            float determinantY = A1 * C2 - A2 * C1;

            float x = determinantX / determinant;
            float y = determinantY / determinant;

            return new Point(x, y);
        }
    }

    public struct Point
    {
        public float x;
        public float y;

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        // Euclidean distance
        public float DistanceTo(Point other)
        {
            return Mathf.Sqrt(Mathf.Pow(x - other.x, 2) + Mathf.Pow(y - other.y, 2));
        }

        public static Point Lerp(Point a, Point b, float t)
        {
            if (t < 0 ) { t = 0; }
            if (t > 1 ) { t = 1; }

            return a + (b - a) * t;
        }

        public static float Distance(Point a, Point b)
        {
            float num = a.x - b.x;
            float num2 = a.y - b.y;
            return (float)Math.Sqrt(num * num + num2 * num2);
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.x + b.x, a.y + b.y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.x - b.x, a.y - b.y);
        }

        public static Point operator *(Point a, Point b)
        {
            return new Point(a.x * b.x, a.y * b.y);
        }

        public static Point operator /(Point a, Point b)
        {
            return new Point(a.x / b.x, a.y / b.y);
        }

        public static Point operator -(Point a)
        {
            return new Point(0f - a.x, 0f - a.y);
        }

        public static Point operator *(Point a, float d)
        {
            return new Point(a.x * d, a.y * d);
        }

        public static Point operator *(float d, Point a)
        {
            return new Point(a.x * d, a.y * d);
        }

        public static Point operator /(Point a, float d)
        {
            return new Point(a.x / d, a.y / d);
        }

        public static bool operator ==(Point lhs, Point rhs)
        {
            float num = lhs.x - rhs.x;
            float num2 = lhs.y - rhs.y;
            return num * num + num2 * num2 < 9.99999944E-11f;
        }

        public static bool operator !=(Point lhs, Point rhs)
        {
            return !(lhs == rhs);
        }
    }

    public class Edge
    {
        public Point Start;
        public Point End;
        public float Weight;
        public bool InTree;

        public Edge(Point start, Point end)
        {
            this.Start = start;
            this.End = end;
            this.Weight = this.Start.DistanceTo(end); // Euclidean distance
            this.InTree = false;
        }


        public static bool operator ==(Edge lhs, Edge rhs)
        {
            return lhs.Start == rhs.Start && lhs.End == rhs.End;
        }

        public static bool operator !=(Edge lhs, Edge rhs)
        {
            return !(lhs == rhs);
        }
    }

    public class Triangle
    {
        public Edge[] Edges;
        //public Edge Edge1;
        //public Edge Edge2;
        //public Edge Edge3;

        public Triangle(Edge edge1, Edge edge2, Edge edge3)
        {
            this.Edges = new Edge[3];
            this.Edges[0] = edge1;
            this.Edges[1] = edge2;
            this.Edges[2] = edge3;
            //this.Edge1 = edge1;
            //this.Edge2 = edge2;
            //this.Edge3 = edge3;
        }

        public void SetEdges(Edge[] edges)
        {
            this.Edges = edges;
        }

        public Point GetCircumcenter()
        {
            Point pointA = Edges[0].Start;
            Point pointB = Edges[1].Start;
            Point pointC = Edges[2].Start;

            LinearEquation lineAB = new LinearEquation(pointA, pointB);
            LinearEquation lineBC = new LinearEquation(pointB, pointC);

            Point midPointAB = Point.Lerp(pointA, pointB, 0.5f);
            Point midPointBC = Point.Lerp(pointB, pointC, 0.5f);

            LinearEquation perpendicularAB = lineAB.PerpendicularLineAt(midPointAB);
            LinearEquation perpendicularBC = lineBC.PerpendicularLineAt(midPointBC);

            Point circumcircle = LinearEquation.GetCrossingPoint(perpendicularAB, perpendicularBC);

            return circumcircle;
        }

        public float GetCircumRadius(Point circumcicle)
        {
            Point pointA = Edges[0].Start;

            float circumCircle = Point.Distance(circumcicle, pointA);

            return circumCircle;
        }

        private float TriangleArea(Point p1, Point p2, Point p3)
        {
            float det = ((p1.x - p3.x) * (p2.y - p3.y)) - ((p2.x - p3.x) * (p1.y - p3.y));
            return (det / 2.0f);
        }

        private bool TrianglesAreaApproach(Point point, Point p1, Point p2, Point p3)
        {
            float triangleArea = TriangleArea(p1, p2, p3);

            float areaSum = 0f;
            areaSum += TriangleArea(p1, p2, point);
            areaSum += TriangleArea(p1, p3, point);
            areaSum += TriangleArea(p2, p3, point);

            return (triangleArea == areaSum);
        }

        public bool ContainsVertex(Point vertex)
        {
            return Edges[0].Start == vertex || Edges[1].Start == vertex || Edges[2].Start == vertex;
        }

        public static bool operator ==(Triangle lhs, Triangle rhs)
        {
            return lhs.Edges[0] == rhs.Edges[0] && lhs.Edges[1] == rhs.Edges[1] && lhs.Edges[2] == rhs.Edges[2];
        }

        public static bool operator !=(Triangle lhs, Triangle rhs)
        {
            return !(lhs == rhs);
        }
    }

    public class TriangleMesh
    {
        public List<Triangle> Triangles = new List<Triangle>();

        public void SetTriangles(List<Triangle> triangles)
        {
            this.Triangles = triangles;
        }

        public void SetEdgeAtIndex(int index, Edge edge)
        {
            List<Edge> edges = GetUniqueEdges();
            edges[index] = edge;
        }

        public List<Point> GetUniqueVertices()
        {
            List<Point> vertices = new List<Point>();

            for (int i = 0; i < Triangles.Count; i++)
            {
                Triangle triangle = Triangles[i];

                for (int j = 0; j < triangle.Edges.Length; j++)
                {
                    if (!vertices.Contains(triangle.Edges[j].Start))
                    {
                        vertices.Add(triangle.Edges[j].Start);
                    }
                }
            }

            return vertices;
        }

        public List<Edge> GetEdges()
        {
            List<Edge> edges = new List<Edge>();

            for (int i = 0; i < Triangles.Count; i++)
            {
                Triangle triangle = Triangles[i];

                for (int j = 0; j < triangle.Edges.Length; j++)
                {
                    edges.Add(triangle.Edges[j]);
                }
            }

            return edges;
        }

        public List<Edge> GetUniqueEdges()
        {
            List<Edge> edges = new List<Edge>();

            for (int i = 0; i < Triangles.Count; i++)
            {
                Triangle triangle = Triangles[i];

                for (int j = 0; j < triangle.Edges.Length; j++)
                {
                    if (!edges.Contains(triangle.Edges[j]))
                    {
                        edges.Add(triangle.Edges[j]);
                    }
                    else
                    {

                    }
                }
            }

            return edges;
        }

        public void Visualize()
        {
            foreach (Edge edge in this.GetUniqueEdges())
            {
                Vector2 start = new Vector2(edge.Start.x, edge.Start.y);
                Vector2 end = new Vector2(edge.End.x, edge.End.y);

                if (edge.InTree)
                {
                    Debug.DrawLine(start, end, UnityEngine.Color.red, 100f);
                }
                else
                {
                    Debug.DrawLine(start, end, UnityEngine.Color.black, 5f);
                }
            }
        }
    }

    public static class BowyerWatson
    {
        public static float minSuperTriangleSize;

        public static TriangleMesh Triangulate(Point[] points, float xSize, float ySize)
        {
            TriangleMesh triangulation = new TriangleMesh();

            Triangle superTriangle = CreateSuperTriangle(xSize, ySize);

            triangulation.Triangles.Add(superTriangle);

            foreach (Point point in points)
            {
                List<Triangle> badTriangles = new List<Triangle>();

                foreach (Triangle triangle in triangulation.Triangles)
                {
                    Point circumcenter = triangle.GetCircumcenter();
                    float circumRadius = triangle.GetCircumRadius(circumcenter);
                    if (CheckIfPointLiesInCircle(point, circumcenter, circumRadius))
                    {
                        badTriangles.Add(triangle);
                    }
                }

                List<Edge> polygon = new List<Edge>();

                foreach (Triangle triangle in badTriangles)
                {
                    foreach(Edge edge in triangle.Edges)
                    {
                        bool isShared = badTriangles.Any(t => t != triangle && t.Edges.Contains(edge));
                        if (!isShared)
                        {
                            polygon.Add(edge);
                        }
                    }
                }

                foreach (Triangle triangle in badTriangles)
                {
                    triangulation.Triangles.Remove(triangle);
                }

                foreach (Edge edge in polygon)
                {
                    Edge edge1 = new Edge(edge.Start, edge.End);
                    Edge edge2 = new Edge(edge.End, point);
                    Edge edge3 = new Edge(point, edge.Start);

                    Triangle newTriangle = new Triangle(edge1, edge2, edge3);
                    triangulation.Triangles.Add(newTriangle);
                }
            }

            triangulation.Triangles = triangulation.Triangles.Where(t => 
                !t.ContainsVertex(superTriangle.Edges[0].Start) &&
                !t.ContainsVertex(superTriangle.Edges[1].Start) &&
                !t.ContainsVertex(superTriangle.Edges[2].Start)).ToList();

            return triangulation;
        }

        private static Triangle CreateSuperTriangle(float xSize, float ySize)
        {
            Point p1 = new Point(-xSize, 0);
            Point p2 = new Point(xSize * 2, 0);
            Point p3 = new Point(xSize / 2, ySize * 1.5f);

            p1 *= 2;
            p2 *= 2;
            p3 *= 2;

            Edge edge1 = new Edge(p1, p2);
            Edge edge2 = new Edge(p2, p3);
            Edge edge3 = new Edge(p3, p1);

            Triangle triangle = new Triangle(edge1, edge2, edge3);
            return triangle;
        }

        private static bool CheckIfPointLiesInCircle(Point point, Point circlePoint, float radius)
        {
            if ((point.x - circlePoint.x) * (point.x - circlePoint.x) + 
                (point.y - circlePoint.y) * (point.y - circlePoint.y)
                <= radius * radius)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

//        function BowyerWatson(pointList)
//// pointList is a set of coordinates defining the points to be triangulated
//    triangulation := empty triangle mesh data structure
//    add super-triangle to triangulation // must be large enough to completely contain all the points in pointList
//    for each point in pointList do // add all the points one at a time to the triangulation
//        badTriangles := empty set
//        for each triangle in triangulation do // first find all the triangles that are no longer valid due to the insertion
//            if point is inside circumcircle of triangle
//                add triangle to badTriangles
//        polygon := empty set
//        for each triangle in badTriangles do // find the boundary of the polygonal hole
//            for each edge in triangle do
//                if edge is not shared by any other triangles in badTriangles
//                    add edge to polygon
//        for each triangle in badTriangles do // remove them from the data structure
//            remove triangle from triangulation
//        for each edge in polygon do // re-triangulate the polygonal hole
//            newTri := form a triangle from edge to point
//            add newTri to triangulation
//    for each triangle in triangulation // done inserting points, now clean up
//        if triangle contains a vertex from original super-triangle
//            remove triangle from triangulation
//    return triangulation

    }

    public class GraphEdge
    {
        public int Source;
        public int Destination;
        public int Weight;

        public GraphEdge(int source, int destination, int weight)
        {
            this.Source = source;
            this.Destination = destination;
            this.Weight = weight;
        }
    }

    public class Graph
    {
        public int[] Parent;
        public int VerticesCount { get; private set; }
        public List<GraphEdge>[] AdjacencyList { get; private set; }

        public Graph(int verticesCount)
        {
            VerticesCount = verticesCount;
            AdjacencyList = new List<GraphEdge>[verticesCount];
            for (int i = 0; i < verticesCount; i++)
            {
                AdjacencyList[i] = new List<GraphEdge>();
            }
        }

        public void AddEdge(int source, int destination, int weight)
        {
            AdjacencyList[source].Add(new GraphEdge(source, destination, weight));
            AdjacencyList[destination].Add(new GraphEdge(destination, source, weight));
        }

        public void PrimMST(List<Point> points, TriangleMesh triangleMesh)
        {
            bool[] inMST = new bool[VerticesCount];
            int[] parent = new int[VerticesCount];
            int[] key = new int[VerticesCount];

            // Initialize all keys as infinity and parent as -1
            for (int i = 0; i < VerticesCount; i++)
            {
                key[i] = int.MaxValue;
                parent[i] = -1;
            }

            // Start with the first vertex
            key[0] = 0;

            for (int i = 0; i < VerticesCount - 1; i++)
            {
                int u = MinKey(key, inMST);
                inMST[u] = true;

                // Update the key value and parent index of the adjacent vertices
                foreach (GraphEdge edge in AdjacencyList[u])
                {
                    int v = edge.Destination;
                    int weight = edge.Weight;

                    if (!inMST[v] && weight < key[v])
                    {
                        key[v] = weight;
                        parent[v] = u;
                    }
                }
            }



            PrintMST(parent);
            SetTriangleMesh(parent, points, triangleMesh);
        }

        private int MinKey(int[] key, bool[] inMST)
        {
            int min = int.MaxValue;
            int minIndex = -1;

            for (int v = 0; v < VerticesCount; v++)
            {
                if (!inMST[v] && key[v] < min)
                {
                    min = key[v];
                    minIndex = v;
                }
            }

            return minIndex;
        }

        private void PrintMST(int[] parent)
        {
            Debug.Log("Edge \tWeight");
            for (int i = 1; i < VerticesCount; i++)
            {
                Debug.Log($"{parent[i]} - {i} \t{GetEdgeWeight(parent[i], i)}");
            }
        }

        private void SetTriangleMesh(int[] parent, List<Point> points, TriangleMesh triangleMesh)
        {
            List<Edge> edges = triangleMesh.GetUniqueEdges();

            for (int i = 1; i < VerticesCount; i++)
            {
                int startIndex = parent[i];
                int endIndex = i;
                GraphEdge graphEdge = GetEdge(parent[i], i);

                if (graphEdge != null)
                {
                    edges.Contains(graphEdge.Edge);

                    Edge edge = edges.Find(e => e == graphEdge.Edge);
                }

                //Debug.DrawLine(start, end, UnityEngine.Color.cyan, 100);
                //Debug.Log($"{parent[i]} - {i} \t{GetEdgeWeight(, i)}");
            }


            //List<Edge> newEdges = new List<Edge>();

            //for (int i = 0; i < edges.Count; i++)
            //{
            //    Edge edge = edges[i];
            //    bool startInTree = false;
            //    bool endInTree = false;
            //    bool isInTree = false;

            //    for (int j = 1; j < VerticesCount; j++)
            //    {
            //        if (points.Any(p => p == edge.Start) && points.Any(p => p == edge.End))
            //        {
            //            isInTree = true;
            //            break;
            //        }
            //    }

            //    if (isInTree)
            //    {
            //        edge.InTree = true;
            //    }
            //    else
            //    {
            //        edge.InTree = false;
            //    }

            //    newEdges.Add(edge);

            //    //int startIndex = points.FindIndex(p => p == edge.Start);
            //    //int endIndex = points.FindIndex(p => p == edge.End);
            //    //int weight = (int)edge.Weight;
            //}
            ////int edgeIndex = 0;

            //for (int i = 0; i < triangleMesh.Triangles.Count; i++)
            //{
            //    Triangle triangle = triangleMesh.Triangles[i];

            //    for (int j = 0; j < triangle.Edges.Length; j++)
            //    {
            //        triangle.Edges[j] = newEdges[i + j];
            //    }

            //    //Edge[] triangleEdges = new Edge[3];


            //    //triangleEdges[0] = newEdges[edgeIndex];
            //    //triangleEdges[1] = newEdges[edgeIndex + 1];
            //    //triangleEdges[2] = newEdges[edgeIndex + 2];

            //    //newEdges[1] = triangleEdges[2];

            //    //triangle.SetEdges(triangleEdges);

            //    triangleMesh.Triangles[i] = triangle;

            //    //edgeIndex += 3;
            //}
        }

        private GraphEdge GetEdge(int parent, int destination)
        {
            foreach (GraphEdge edge in AdjacencyList[parent])
            {
                if (edge.Destination == destination)
                    return edge;
            }
            return null;
        }

        private int GetEdgeWeight(int u, int v)
        {
            foreach (GraphEdge edge in AdjacencyList[u])
            {
                if (edge.Destination == v)
                    return edge.Weight;
            }
            return 0;
        }
    }

    public static class Prim
    {
        public static void GetMinimumSpanningTree(TriangleMesh triangleMesh)
        {
            List<Point> points = triangleMesh.GetUniqueVertices();
            List<Edge> edges = triangleMesh.GetUniqueEdges();

            Graph graph = new Graph(points.Count);

            for (int i = 0; i < edges.Count; i++)
            {
                Edge edge = edges[i];

                int startIndex = points.FindIndex(p => p == edge.Start);
                int endIndex = points.FindIndex(p => p == edge.End);
                int weight = (int)edge.Weight;

                graph.AddEdge(startIndex, endIndex, weight);
            }

            graph.PrimMST(points, triangleMesh);
        }
    }
}
