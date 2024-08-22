using System;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.ComponentModel;
using System.Drawing;
using UnityEngine.Experimental.GlobalIllumination;
using System.Runtime.CompilerServices;
using Assets.Models;
using JetBrains.Annotations;
using Assets.DungeonGeneratorAlgorithms;
using UnityEditor;
using System.Linq;
using Point = Assets.DungeonGeneratorAlgorithms.Point;
using Edge = Assets.DungeonGeneratorAlgorithms.Edge;
using System.Collections;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using UnityEngine.UIElements;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;
using static UnityEditor.PlayerSettings;
using UnityEngine.SocialPlatforms;
using UnityEngine.U2D;

public class DungeonGenerator
{
    private int mapSizeX = 150;
    private int mapSizeY = 150;

    private int numRoomsToGenerate = 30;

    private List<Room> rooms = new List<Room>();
    private List<GameObject> roomGameObjects = new List<GameObject>();

    private static System.Random random = new System.Random();

    //private float spawnCircleRadius = 10f;

    private int minRoomWidth = 3;
    private int maxRoomWidth = 12;

    private int minRoomHeight = 3;
    private int maxRoomHeight = 12;

    private float numRandomllyAddedProcentHallways = 10;

    public DungeonGenerator()
    {
    }

    public DungeonGenerator(int mapSizeX, int mapSizeY, int numRoomsToGenerate, int minRoomWidth, int maxRoomWidth, int minRoomHeight, int maxRoomHeight, float numRandomllyAddedProcentHallways)
    {
        this.mapSizeX = mapSizeX;
        this.mapSizeY = mapSizeY;
        this.numRoomsToGenerate = numRoomsToGenerate;
        this.minRoomWidth = minRoomWidth;
        this.maxRoomWidth = maxRoomWidth;
        this.minRoomHeight = minRoomHeight;
        this.maxRoomHeight = maxRoomHeight;
        this.numRandomllyAddedProcentHallways = numRandomllyAddedProcentHallways;
    }

    public Dungeon Start()
    {
        for (int i = 0; i < numRoomsToGenerate; i++)
        {
            rooms.Add(CreateRoom(GetRandomPointInEllipse(mapSizeX, mapSizeY)));
        }

        bool simulate = true;
        while (simulate)
        {
            CreateSceneParameters csp = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
            Scene simulatedScene = SceneManager.CreateScene("SimulatedScene", csp);

            PhysicsScene2D physicsScene2D = simulatedScene.GetPhysicsScene2D();
            float time = 0f;

            List<Rigidbody2D> rbs = new List<Rigidbody2D>();

            for (int j = 0; j < roomGameObjects.Count; j++)
            {
                SceneManager.MoveGameObjectToScene(roomGameObjects[j], simulatedScene);
                rbs.Add(roomGameObjects[j].GetComponent<Rigidbody2D>());
            }
            while (simulate)
            {
                time += 0.1f;
                if (physicsScene2D.IsValid())
                {
                    while (time >= 0.1f)
                    {
                        time -= 0.1f;

                        physicsScene2D.Simulate(0.1f);
                    }
                }
                else
                {
                    Debug.LogWarning("physicsSceneNotValid");
                    simulate = false;
                }

                if (rbs.All(rb => rb.IsSleeping()))
                {
                    for (int j = 0; j < rooms.Count; j++)
                    {
                        rooms[j].Origin = roomGameObjects[j].transform.position;
                    }

                    for (int i = 0; i < rooms.Count; i++)
                    {
                        Room room = rooms[i];
                        room.Origin = new Vector2(Mathf.Floor(room.Origin.x), Mathf.Floor(room.Origin.y));
                        roomGameObjects[i].transform.position = room.Origin;
                        simulate = false;
                    }
                }

                //for (int i = 0; i < rooms.Count; i++)
                //{
                //    Room room = rooms[i];
                //    Vector2 force = SteeringBehaviourOperation(room);
                //    room.Origin += force;
                //    VisualizeRoom(room);
                //    yield return new WaitForSeconds(0.0002f);
                //}
            }
        }

        int averageRoomWidth;
        int averageRoomHeight;

        List<Room> roomsOrderedByWidth = rooms.OrderBy(r => r.Width).ToList();
        List<Room> roomsOrderedByHeight = rooms.OrderBy(r => r.Height).ToList();
        averageRoomWidth = roomsOrderedByWidth[(int)(roomsOrderedByWidth.Count / 2)].Width;
        averageRoomHeight = roomsOrderedByHeight[(int)(roomsOrderedByHeight.Count / 2)].Height;

        float minRoomWidth = (float)averageRoomWidth * 1.20f;
        float minRoomHeight = (float)averageRoomHeight * 1.20f;

        List<Room> mainRooms = rooms.Where(r => r.Width > minRoomWidth && r.Height > minRoomHeight).ToList();

        Point[] points = new Point[mainRooms.Count];

        for (int i = 0; i < mainRooms.Count; i++)
        {
            Room room = mainRooms[i];
            room.MainRoom = true;
            float x = room.Origin.x + room.Width / 2;
            float y = room.Origin.y + room.Height / 2;
            Point point = new Point(x, y, room);
            points[i] = point;
        }

        TriangleMesh triangleMesh = BowyerWatson.Triangulate(points);

        Prim.GetMinimumSpanningTree(triangleMesh);

        Prim.RandomlySelectEdges(triangleMesh, numRandomllyAddedProcentHallways);

        List<Hallway> hallways = CreateHallways(triangleMesh);

        List<Room> addRooms = CheckIfRoomIntersectsHallways(hallways, mainRooms);

        Dungeon dungeon = new Dungeon(mainRooms, addRooms, hallways, new Dictionary<Vector2, Tile>());

        return TurnIntoMapTiles(dungeon);
    }

    //public IEnumerator Start()
    //{
    //    Vector2 leftB = Vector2.zero;
    //    Vector2 rightB = new Vector2(mapSizeX, 0);
    //    Vector2 leftT = new Vector2(0, mapSizeY);
    //    Vector2 rightT = new Vector2(mapSizeX, mapSizeY);
    //    Debug.DrawLine(leftB, rightB, UnityEngine.Color.magenta, 100f);
    //    Debug.DrawLine(leftB, leftT, UnityEngine.Color.magenta, 100f);
    //    Debug.DrawLine(rightT, rightB, UnityEngine.Color.magenta, 100f);
    //    Debug.DrawLine(rightT, leftT, UnityEngine.Color.magenta, 100f);

    //    for (int i = 0; i < numRoomsToGenerate; i++)
    //    {
    //        rooms.Add(CreateRoom(GetRandomPointInSquare(mapSizeX, mapSizeY)));
    //        yield return new WaitForSeconds(.0375f);
    //    }

    //    for (int i = 0; i < rooms.Count; i++)
    //    {
    //        Room room = rooms[i];
    //        List<Room> roomsToRemove = new List<Room>();

    //        foreach (Room other in rooms)
    //        {
    //            if (other != room && CheckForRoomOverlap(room, other))
    //            {
    //                roomsToRemove.Add(other);
    //            }
    //        }

    //        foreach (Room removeRoom in roomsToRemove)
    //        {
    //            removeRoom.Remove();
    //            rooms.Remove(removeRoom);
    //            yield return new WaitForSeconds(.1f);
    //        }
    //    }

    //    if (rooms.Count > numRoomsToReturn)
    //    {
    //        int numRoomsToRemove = rooms.Count - numRoomsToReturn;

    //        for (int i = 0; i < numRoomsToRemove; i++)
    //        {
    //            int rndNum = random.Next(rooms.Count);
    //            Room removeRoom = rooms[rndNum];
    //            removeRoom.Remove();
    //            rooms.Remove(removeRoom);
    //            yield return new WaitForSeconds(.1f);
    //        }
    //    }
    //    else if (rooms.Count < numRoomsToReturn)
    //    {
    //        int missingRooms = numRoomsToReturn - rooms.Count;

    //        for (int i = 0; i < maxNumAttemptsGenerateFillInGapRooms; i++)
    //        {
    //            Room room = CreateRoom(GetRandomPointInSquare(mapSizeX, mapSizeY));
    //            rooms.Add(room);
    //            bool isOverlap = rooms.Any(r => CheckForRoomOverlap(room, r));

    //            if (!isOverlap)
    //            {
    //                missingRooms--;
    //                yield return new WaitForSeconds(.1f);
    //            }
    //            else
    //            {
    //                room.Remove();
    //                rooms.Remove(room);
    //            }

    //            if (missingRooms <= 0)
    //            {
    //                break;
    //            }
    //        }
    //    }



    //    Point[] points = new Point[rooms.Count];

    //    for (int i = 0; i < rooms.Count; i++)
    //    {
    //        Room room = rooms[i];
    //        float x = room.Origin.x + room.Width / 2;
    //        float y = room.Origin.y + room.Height / 2;
    //        Point point = new Point(x, y);
    //        points[i] = point;
    //    }

    //    TriangleMesh triangleMesh = BowyerWatson.Triangulate(points, mapSizeX, mapSizeY);

    //    Prim.GetMinimumSpanningTree(triangleMesh);

    //    Prim.RandomlySelectEdges(triangleMesh, numRandomllyAddedProcentHallways);

    //    triangleMesh.Visualize();

    //    yield return null;
    //}

    //private void SeperateRooms()
    //{
    //    while (CheckIfAnyRoomsAreOverlappingInList(rooms))
    //    {
    //        for (int i = 0; i < rooms.Count; i++)
    //        {
    //            Room room = rooms[i];
    //            Vector2 force = SteeringBehaviourOperation(room);
    //            room.Origin += force;
    //        }

    //        for (int i = 0; i < rooms.Count; i++)
    //        {
    //            Room room = rooms[i];
    //            room.Origin = new Vector2(Mathf.Round(room.Origin.x), Mathf.Round(room.Origin.y));
    //        }
    //    }
    //}

    //private Vector2 SteeringBehaviourOperation(Room room)
    //{
    //    Vector2 pushForce = Vector2.zero;
    //    int neighboursCount = 0;

    //    foreach (Room other in rooms)
    //    {
    //        if (other != room && CheckForRoomOverlap(room, other))
    //        {
    //            float thisRoomSize = Mathf.Max(room.Width, room.Height) + 1;
    //            float minDistanceForSeparation = (thisRoomSize / 2 + 2) + (Mathf.Max(other.Width, other.Height) / 2 + 2);

    //            float Distance = Vector2.Distance(room.Origin, other.Origin);

    //            if (Distance < minDistanceForSeparation)
    //            {
    //                Vector2 pushDir = room.Origin - other.Origin;

    //                pushForce += pushDir / thisRoomSize;
    //                neighboursCount++;
    //            }
    //        }
    //    }

    //    return pushForce;
    //}

    private Room CreateRoom(Vector2 startPoint)
    {
        int width = random.Next(minRoomWidth, maxRoomWidth + 1);
        int height = random.Next(minRoomHeight, maxRoomHeight + 1);

        Room room = new Room(width, height, startPoint);
        Vector2 pos = new Vector2(room.Origin.x, room.Origin.y);
        GameObject obj = GameObject.Instantiate(ProjectBehaviour.GameManager.TilePrefab, pos, Quaternion.identity);

        obj.transform.localScale = new Vector3(width + 1.1f, height + 1.1f, 1);
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.offset = new Vector2(0.5f, 0.5f);
        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        roomGameObjects.Add(obj);

        return room;
    }

    private Vector2 GetRandomPointInSquare(int width, int height)
    {
        int x = random.Next(width + 1);
        int y = random.Next(height + 1);

        return new Vector2(x, y);
    }

    private Vector2 GetRandomPointInEllipse(float ellipseWidth, float ellipseHeight)
    {
        float t = 2 * Mathf.PI * (float)random.NextDouble(); 
        float u = (float)random.NextDouble() + (float)random.NextDouble();
        float r = u > 1 ? 2 - u : u; 
        float x = ellipseWidth * r * Mathf.Cos(t) / 2;
        float y = ellipseHeight * r * Mathf.Sin(t) / 2;
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

    private bool CheckIfAnyRoomsAreOverlappingInList(List<Room> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];

            for (int j = 0; j < rooms.Count; j++)
            {
                Room other = rooms[j];

                if (other != room && CheckForRoomOverlap(other, room))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool CheckForRoomOverlap(Room room, Room other)
    {
        float roomMinX = room.Origin.x;
        float roomMaxX = room.Origin.x + room.Width;

        float roomMinY = room.Origin.y;
        float roomMaxY = room.Origin.y + room.Height;

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

    private void VisualizeRoomWithTiles(Room room, bool main)
    {
        List<Tile> newTiles = new List<Tile>();

        if (room.Visualized)
        {
            room.RoomMoved();
            return;
        }

        room.Visualized = true;

        for (int x = 0; x < room.Width; x++)
        {
            for (int y = 0; y < room.Height; y++)
            {
                Vector2 pos = new Vector2(room.Origin.x + x, room.Origin.y + y);
                Vector2 offset = new Vector2(x, y);
                GameObject obj = GameObject.Instantiate(ProjectBehaviour.GameManager.TilePrefab, pos, Quaternion.identity, ProjectBehaviour.GameManager.transform);

                if (!main)
                {
                    var t = obj.transform.GetChild(0);
                    var v = t.GetChild(0);
                    var c = t.GetComponent<SpriteRenderer>().color;
                    c.a /= 2;
                    t.GetComponent<SpriteRenderer>().color = c;

                    var c1 = v.GetComponent<SpriteRenderer>().color;
                    c1.a /= 2;
                    v.GetComponent<SpriteRenderer>().color = c1;
                }

                Tile tile = new Tile(pos, offset, obj, room, TileType.None);
                newTiles.Add(tile);
            }
        }

        //foreach (Tile tile in newTiles)
        //{
        //    if (!tiles.TryAdd(tile.Position, tile))
        //    {
        //        Tile other = tiles[tile.Position];
        //        other.
        //        //tile.Position = Vector2.zero;
        //        //tile.SceneObject.transform.position = tile.Position;
        //        //tile.SceneObject.transform.SetParent(null);
        //    }
        //}
    }

    private List<Hallway> CreateHallways(TriangleMesh triangleMesh)
    {
        List<Edge> edges = triangleMesh.Edges.Where(e => e.InTree).ToList();
        List<Hallway> hallways = new List<Hallway>();

        for (int i = 0; i < edges.Count; i++)
        {
            Edge edge = edges[i];

            Room room1 = edge.Start.Room;
            Room room2 = edge.End.Room;

            float room1MinX = room1.Origin.x;
            float room1MaxX = room1.Origin.x + room1.Width;

            float room1MinY = room1.Origin.y;
            float room1MaxY = room1.Origin.y + room1.Height;

            float room2MinX = room2.Origin.x;
            float room2MaxX = room2.Origin.x + room2.Width;

            float room2MinY = room2.Origin.y;
            float room2MaxY = room2.Origin.y + room2.Height;

            Vector2 midPoint = Vector2.Lerp(
                new Vector2(room1.Origin.x + room1.Width / 2, room1.Origin.y + room1.Height / 2),
                new Vector2(room2.Origin.x + room2.Width / 2, room2.Origin.y + room2.Height / 2), 0.5f);

            Hallway hallway;
            Vector2 start;
            Vector2 end;
            Vector2 bendPoint;

            if (midPoint.y > room1MinY && midPoint.y < room1MaxY && midPoint.y > room2MinY && midPoint.y < room2MaxY)
            {
                // Horizontal Hallway
                start = new Vector2(room1.Origin.x + room1.Width / 2, midPoint.y);
                end = new Vector2(room2.Origin.x + room2.Width / 2, midPoint.y);

                hallway = new Hallway(start, end);
                hallways.Add(hallway);
            }
            else if (midPoint.x > room1MinX && midPoint.x < room1MaxX && midPoint.x > room2MinX && midPoint.x < room2MaxX)
            {
                // Vertical Hallway
                start = new Vector2(midPoint.x, room1.Origin.y + room1.Height / 2); 
                end = new Vector2(midPoint.x, room2.Origin.y + room2.Height / 2);

                hallway = new Hallway(start, end);
                hallways.Add(hallway);
            }
            else
            {
                // L-Shaped Hallway
                if (room1.Width > room1.Height)
                {
                    // Start down or up

                    int rndNum1 = random.Next(-(room1.Width / 5), room1.Width / 5);
                    int rndNum2 = random.Next(-(room2.Height / 5), room2.Height / 5);

                    start = new Vector2((room1.Origin.x + room1.Width / 2) + rndNum1, room1.Origin.y + room1.Height / 2);
                    end = new Vector2(room2.Origin.x + room2.Width / 2, (room2.Origin.y + room2.Height / 2) + rndNum2);
                    bendPoint = new Vector2(start.x, end.y);

                    hallway = new Hallway(start, end, bendPoint);
                    hallways.Add(hallway);
                }
                else
                {
                    // Start left or right

                    int rndNum1 = random.Next(-(room1.Height / 5), room1.Height / 5);
                    int rndNum2 = random.Next(-(room2.Width / 5), room2.Width / 5);

                    start = new Vector2(room1.Origin.x + room1.Width / 2, (room1.Origin.y + room1.Height / 2) + rndNum1);
                    end = new Vector2((room2.Origin.x + room2.Width / 2) + rndNum2, room2.Origin.y + room2.Height / 2);
                    bendPoint = new Vector2(end.x, start.y);

                    hallway = new Hallway(start, end, bendPoint);
                    hallways.Add(hallway);
                }
            }

            //if (room1MaxX > room2MinX && room1MinX < room2MaxX)
            //{
            //    onX = true;
            //}
            //else if (room1MaxY > room2MinY && room1MinY < room2MaxY)
            //{
            //    onY = true;
            //}

            //Hallway hallway;

            //if (onX)
            //{
            //    //Horizontal Hallway
            //    if (room1.Origin.y == room2.Origin.y)
            //    {
            //        Vector2 start;
            //        Vector2 end;
            //        start = new Vector2 (room1.Origin.x + room1.Width, room1.Origin.y);
            //        end = new Vector2 (room2.Origin.x, room2.Origin.y);
            //        hallway = new Hallway(start, end);
            //    }
            //    else if (room1.Origin.y > room2.Origin.y)
            //    {

            //    }
            //}
            //else if (onY)
            //{
            //    // Vertical Hallway

            //}
            //else
            //{
            //    // L Shape Hallway

            //}
        }

        return hallways;
    }

    private List<Room> CheckIfRoomIntersectsHallways(List<Hallway> hallways, List<Room> mainRooms)
    {
        List<Room> addRooms = new List<Room>();
        List<Room> rooms = this.rooms.Except(mainRooms).ToList();

        foreach (Room room in rooms)
        {
            foreach (Hallway hallway in hallways)
            {
                if (hallway.L_Shape)
                {
                    if (LineIntersectsRoom(hallway.Start, hallway.BendPoint, room))
                    {
                        addRooms.Add(room);
                        break;
                    }
                    else if (LineIntersectsRoom(hallway.BendPoint, hallway.End, room))
                    {
                        addRooms.Add(room);
                        break;
                    }
                }
                else if (LineIntersectsRoom(hallway.Start, hallway.End, room))
                {
                    addRooms.Add(room);
                    break;
                }
            }
        }

        return addRooms;
    }

    public static bool LineIntersectsRoom(Vector2 p1, Vector2 p2, Room rect)
    {
        return LineIntersectsLine(p1, p2, new Vector2(rect.Origin.x, rect.Origin.y), new Vector2(rect.Origin.x + rect.Width, rect.Origin.y)) ||
               LineIntersectsLine(p1, p2, new Vector2(rect.Origin.x + rect.Width, rect.Origin.y), new Vector2(rect.Origin.x + rect.Width, rect.Origin.y + rect.Height)) ||
               LineIntersectsLine(p1, p2, new Vector2(rect.Origin.x + rect.Width, rect.Origin.y + rect.Height), new Vector2(rect.Origin.x, rect.Origin.y + rect.Height)) ||
               LineIntersectsLine(p1, p2, new Vector2(rect.Origin.x, rect.Origin.y + rect.Height), new Vector2(rect.Origin.x, rect.Origin.y)) ||
               (rect.Contains(p1) && rect.Contains(p2));
    }

    private static bool LineIntersectsLine(Vector2 line1p1, Vector2 line1p2, Vector2 line2p1, Vector2 line2p2)
    {
        float q = (line1p1.y - line2p1.y) * (line2p2.x - line2p1.x) - (line1p1.x - line2p1.x) * (line2p2.y - line2p1.y);
        float d = (line1p2.x - line1p1.x) * (line2p2.y - line2p1.y) - (line1p2.y - line1p1.y) * (line2p2.x - line2p1.x);

        if (d == 0)
        {
            return false;
        }

        float r = q / d;

        q = (line1p1.y - line2p1.y) * (line1p2.x - line1p1.x) - (line1p1.x - line2p1.x) * (line1p2.y - line1p1.y);
        float s = q / d;

        if (r < 0 || r > 1 || s < 0 || s > 1)
        {
            return false;
        }

        return true;
    }

    private Dungeon TurnIntoMapTiles(Dungeon dungeon)
    {
        foreach (Room mainRoom in dungeon.MainRooms)
        {
            for (int x = 0; x < mainRoom.Width; x++)
            {
                for (int y = 0; y < mainRoom.Height; y++)
                {
                    TilesHolderSO tilesSo = ProjectBehaviour.GameManager.FloorTiles;
                    Vector2 pos = new Vector2(mainRoom.Origin.x + x, mainRoom.Origin.y + y);
                    var obj = GameObject.Instantiate(tilesSo.Prefab, pos, Quaternion.identity, ProjectBehaviour.GameManager.transform);

                    int rndNum1 = UnityEngine.Random.Range(0, tilesSo.Tiles.Length);
                    int rndNum2 = UnityEngine.Random.Range(0, 4);

                    var t = obj.transform.GetChild(0);
                    var r = t.GetComponent<SpriteRenderer>();
                    r.sprite = tilesSo.Tiles[rndNum1];
                    r.color = ProjectBehaviour.GameManager.MapColor;


                    var v = t.eulerAngles;
                    v.z = rndNum2 * 90;
                    t.eulerAngles = v;

                    Tile tile = new Tile(pos, new Vector2(x, y), obj, mainRoom, TileType.FloorMain);

                    if (!dungeon.Tiles.TryAdd(pos, tile))
                    {
                        Debug.LogWarning("DoubleTile");
                    }

                    if (x == 0 && y == 0)
                    {
                        Vector2 wallPos1 = new Vector2(x - 1, y - 1) + mainRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, mainRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos2 = new Vector2(x, y - 1) + mainRoom.Origin;
                        Tile wallTile2 = CreateWall(wallPos2, mainRoom, wallPos2);

                        if (!dungeon.Tiles.TryAdd(wallPos2, wallTile2))
                        {
                            dungeon.Tiles.TryGetValue(wallPos2, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos3 = new Vector2(x - 1, y) + mainRoom.Origin;
                        Tile wallTile3 = CreateWall(wallPos3, mainRoom, wallPos3);

                        if (!dungeon.Tiles.TryAdd(wallPos3, wallTile3))
                        {
                            dungeon.Tiles.TryGetValue(wallPos3, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (x == mainRoom.Width - 1 && y == mainRoom.Height - 1)
                    {
                        Vector2 wallPos1 = new Vector2(x + 1, y + 1) + mainRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, mainRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos2 = new Vector2(x, y + 1) + mainRoom.Origin;
                        Tile wallTile2 = CreateWall(wallPos2, mainRoom, wallPos2);

                        if (!dungeon.Tiles.TryAdd(wallPos2, wallTile2))
                        {
                            dungeon.Tiles.TryGetValue(wallPos2, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos3 = new Vector2(x + 1, y) + mainRoom.Origin;
                        Tile wallTile3 = CreateWall(wallPos3, mainRoom, wallPos3);

                        if (!dungeon.Tiles.TryAdd(wallPos3, wallTile3))
                        {
                            dungeon.Tiles.TryGetValue(wallPos3, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (x == 0 && y == mainRoom.Height - 1)
                    {
                        Vector2 wallPos1 = new Vector2(x - 1, y + 1) + mainRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, mainRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos2 = new Vector2(x, y + 1) + mainRoom.Origin;
                        Tile wallTile2 = CreateWall(wallPos2, mainRoom, wallPos2);

                        if (!dungeon.Tiles.TryAdd(wallPos2, wallTile2))
                        {
                            dungeon.Tiles.TryGetValue(wallPos2, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos3 = new Vector2(x - 1, y) + mainRoom.Origin;
                        Tile wallTile3 = CreateWall(wallPos3, mainRoom, wallPos3);

                        if (!dungeon.Tiles.TryAdd(wallPos3, wallTile3))
                        {
                            dungeon.Tiles.TryGetValue(wallPos3, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (y == 0 && x == mainRoom.Width - 1)
                    {
                        Vector2 wallPos1 = new Vector2(x + 1, y - 1) + mainRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, mainRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos2 = new Vector2(x, y - 1) + mainRoom.Origin;
                        Tile wallTile2 = CreateWall(wallPos2, mainRoom, wallPos2);

                        if (!dungeon.Tiles.TryAdd(wallPos2, wallTile2))
                        {
                            dungeon.Tiles.TryGetValue(wallPos2, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos3 = new Vector2(x + 1, y) + mainRoom.Origin;
                        Tile wallTile3 = CreateWall(wallPos3, mainRoom, wallPos3);

                        if (!dungeon.Tiles.TryAdd(wallPos3, wallTile3))
                        {
                            dungeon.Tiles.TryGetValue(wallPos3, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (x == 0)
                    {
                        Vector2 wallPos1 = new Vector2(x - 1, y) + mainRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, mainRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (y == 0)
                    {
                        Vector2 wallPos1 = new Vector2(x, y - 1) + mainRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, mainRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (x == mainRoom.Width - 1)
                    {
                        Vector2 wallPos1 = new Vector2(x + 1, y) + mainRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, mainRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (y == mainRoom.Height - 1)
                    {
                        Vector2 wallPos1 = new Vector2(x, y + 1) + mainRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, mainRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                }
            }
        }

        foreach (Room normalRoom in dungeon.NormalRooms)
        {
            for (int x = 0; x < normalRoom.Width; x++)
            {
                for (int y = 0; y < normalRoom.Height; y++)
                {
                    TilesHolderSO tilesSO = ProjectBehaviour.GameManager.FloorTiles;
                    Vector2 pos = new Vector2(normalRoom.Origin.x + x, normalRoom.Origin.y + y);
                    var obj = GameObject.Instantiate(tilesSO.Prefab, pos, Quaternion.identity, ProjectBehaviour.GameManager.transform);

                    int rndNum1 = UnityEngine.Random.Range(0, tilesSO.Tiles.Length);
                    int rndNum2 = UnityEngine.Random.Range(0, 4);

                    var t = obj.transform.GetChild(0);
                    var r = t.GetComponent<SpriteRenderer>();
                    r.sprite = tilesSO.Tiles[rndNum1];
                    r.color = ProjectBehaviour.GameManager.MapColor;

                    var v = t.eulerAngles;
                    v.z = rndNum2 * 90;
                    t.eulerAngles = v;

                    Tile tile = new Tile(pos, new Vector2(x, y), obj, normalRoom, TileType.FloorNormal);

                    if (!dungeon.Tiles.TryAdd(pos, tile))
                    {
                        Debug.LogWarning("DoubleTile");
                    }

                    if (x == 0 && y == 0)
                    {
                        Vector2 wallPos1 = new Vector2(x - 1, y - 1) + normalRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, normalRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos2 = new Vector2(x, y - 1) + normalRoom.Origin;
                        Tile wallTile2 = CreateWall(wallPos2, normalRoom, wallPos2);

                        if (!dungeon.Tiles.TryAdd(wallPos2, wallTile2))
                        {
                            dungeon.Tiles.TryGetValue(wallPos2, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos3 = new Vector2(x - 1, y) + normalRoom.Origin;
                        Tile wallTile3 = CreateWall(wallPos3, normalRoom, wallPos3);

                        if (!dungeon.Tiles.TryAdd(wallPos3, wallTile3))
                        {
                            dungeon.Tiles.TryGetValue(wallPos3, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (x == normalRoom.Width - 1 && y == normalRoom.Height - 1)
                    {
                        Vector2 wallPos1 = new Vector2(x + 1, y + 1) + normalRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, normalRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos2 = new Vector2(x, y + 1) + normalRoom.Origin;
                        Tile wallTile2 = CreateWall(wallPos2, normalRoom, wallPos2);

                        if (!dungeon.Tiles.TryAdd(wallPos2, wallTile2))
                        {
                            dungeon.Tiles.TryGetValue(wallPos2, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos3 = new Vector2(x + 1, y) + normalRoom.Origin;
                        Tile wallTile3 = CreateWall(wallPos3, normalRoom, wallPos3);

                        if (!dungeon.Tiles.TryAdd(wallPos3, wallTile3))
                        {
                            dungeon.Tiles.TryGetValue(wallPos3, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (x == 0 && y == normalRoom.Height - 1)
                    {
                        Vector2 wallPos1 = new Vector2(x - 1, y + 1) + normalRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, normalRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos2 = new Vector2(x, y + 1) + normalRoom.Origin;
                        Tile wallTile2 = CreateWall(wallPos2, normalRoom, wallPos2);

                        if (!dungeon.Tiles.TryAdd(wallPos2, wallTile2))
                        {
                            dungeon.Tiles.TryGetValue(wallPos2, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos3 = new Vector2(x - 1, y) + normalRoom.Origin;
                        Tile wallTile3 = CreateWall(wallPos3, normalRoom, wallPos3);

                        if (!dungeon.Tiles.TryAdd(wallPos3, wallTile3))
                        {
                            dungeon.Tiles.TryGetValue(wallPos3, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (y == 0 && x == normalRoom.Width - 1)
                    {
                        Vector2 wallPos1 = new Vector2(x + 1, y - 1) + normalRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, normalRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos2 = new Vector2(x, y - 1) + normalRoom.Origin;
                        Tile wallTile2 = CreateWall(wallPos2, normalRoom, wallPos2);

                        if (!dungeon.Tiles.TryAdd(wallPos2, wallTile2))
                        {
                            dungeon.Tiles.TryGetValue(wallPos2, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }

                        Vector2 wallPos3 = new Vector2(x + 1, y) + normalRoom.Origin;
                        Tile wallTile3 = CreateWall(wallPos3, normalRoom, wallPos3);

                        if (!dungeon.Tiles.TryAdd(wallPos3, wallTile3))
                        {
                            dungeon.Tiles.TryGetValue(wallPos3, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (x == 0)
                    {
                        Vector2 wallPos1 = new Vector2(x - 1, y) + normalRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, normalRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (y == 0)
                    {
                        Vector2 wallPos1 = new Vector2(x, y - 1) + normalRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, normalRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (x == normalRoom.Width - 1)
                    {
                        Vector2 wallPos1 = new Vector2(x + 1, y) + normalRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, normalRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                    else if (y == normalRoom.Height - 1)
                    {
                        Vector2 wallPos1 = new Vector2(x, y + 1) + normalRoom.Origin;
                        Tile wallTile1 = CreateWall(wallPos1, normalRoom, wallPos1);

                        if (!dungeon.Tiles.TryAdd(wallPos1, wallTile1))
                        {
                            dungeon.Tiles.TryGetValue(wallPos1, out tile);
                            if (tile.Type == TileType.FloorNormal || tile.Type == TileType.FloorMain)
                            {
                                Debug.LogWarning("Wall on floor");
                            }
                        }
                    }
                }
            }
        }

        foreach (Hallway hallway in dungeon.Hallways)
        {
            if (hallway.L_Shape)
            {
                Vector2 diffrence1 = hallway.BendPoint - hallway.Start;

                bool x1 = false;
                if (Mathf.Abs(diffrence1.x) > Mathf.Abs(diffrence1.y))
                {
                    x1 = true;
                }

                int value1 = (int)Mathf.Max(Mathf.Abs(diffrence1.x), Mathf.Abs(diffrence1.y)) + 1;

                for (int i = 0; i < value1; i++)
                {
                    if (x1)
                    {
                        TilesHolderSO tilesSO = ProjectBehaviour.GameManager.FloorTiles;
                        Vector2 pos = new Vector2(i, 0) + hallway.Start;
                        var obj = GameObject.Instantiate(tilesSO.Prefab, pos, Quaternion.identity, ProjectBehaviour.GameManager.transform);

                        int rndNum1 = UnityEngine.Random.Range(0, tilesSO.Tiles.Length);
                        int rndNum2 = UnityEngine.Random.Range(0, 4);

                        var t = obj.transform.GetChild(0);
                        var r = t.GetComponent<SpriteRenderer>();
                        r.sprite = tilesSO.Tiles[rndNum1];
                        r.color = ProjectBehaviour.GameManager.MapColor;

                        var v = t.eulerAngles;
                        v.z = rndNum2 * 90;
                        t.eulerAngles = v;

                        Tile tile = new Tile(pos, obj, TileType.FloorHallway);

                        if (!dungeon.Tiles.TryAdd(pos, tile))
                        {
                            Debug.LogWarning("DoubleTile");
                        }
                    }
                    else
                    {
                        TilesHolderSO tilesSO = ProjectBehaviour.GameManager.FloorTiles;
                        Vector2 pos = new Vector2(0, i) + hallway.Start;
                        var obj = GameObject.Instantiate(tilesSO.Prefab, pos, Quaternion.identity, ProjectBehaviour.GameManager.transform);

                        int rndNum1 = UnityEngine.Random.Range(0, tilesSO.Tiles.Length);
                        int rndNum2 = UnityEngine.Random.Range(0, 4);

                        var t = obj.transform.GetChild(0);
                        var r = t.GetComponent<SpriteRenderer>();
                        r.sprite = tilesSO.Tiles[rndNum1];
                        r.color = ProjectBehaviour.GameManager.MapColor;

                        var v = t.eulerAngles;
                        v.z = rndNum2 * 90;
                        t.eulerAngles = v;

                        Tile tile = new Tile(pos, obj, TileType.FloorHallway);

                        if (!dungeon.Tiles.TryAdd(pos, tile))
                        {
                            Debug.LogWarning("DoubleTile");
                        }
                    }
                }

                Vector2 diffrence2 = hallway.End - hallway.BendPoint;

                bool x2 = false;
                if (Mathf.Abs(diffrence2.x) > Mathf.Abs(diffrence2.y))
                {
                    x2 = true;
                }

                int value2 = (int)Mathf.Max(Mathf.Abs(diffrence2.x), Mathf.Abs(diffrence2.y)) + 1;

                for (int i = 0; i < value2; i++)
                {
                    if (x2)
                    {
                        TilesHolderSO tilesSO = ProjectBehaviour.GameManager.FloorTiles;
                        Vector2 pos = new Vector2(i, 0) + hallway.BendPoint;
                        var obj = GameObject.Instantiate(tilesSO.Prefab, pos, Quaternion.identity, ProjectBehaviour.GameManager.transform);

                        int rndNum1 = UnityEngine.Random.Range(0, tilesSO.Tiles.Length);
                        int rndNum2 = UnityEngine.Random.Range(0, 4);

                        var t = obj.transform.GetChild(0);
                        var r = t.GetComponent<SpriteRenderer>();
                        r.sprite = tilesSO.Tiles[rndNum1];
                        r.color = ProjectBehaviour.GameManager.MapColor;

                        var v = t.eulerAngles;
                        v.z = rndNum2 * 90;
                        t.eulerAngles = v;

                        Tile tile = new Tile(pos, obj, TileType.FloorHallway);

                        if (!dungeon.Tiles.TryAdd(pos, tile))
                        {
                            Debug.LogWarning("DoubleTile");
                        }
                    }
                    else
                    {
                        TilesHolderSO tilesSO = ProjectBehaviour.GameManager.FloorTiles;
                        Vector2 pos = new Vector2(0, i) + hallway.BendPoint;
                        var obj = GameObject.Instantiate(tilesSO.Prefab, pos, Quaternion.identity, ProjectBehaviour.GameManager.transform);

                        int rndNum1 = UnityEngine.Random.Range(0, tilesSO.Tiles.Length);
                        int rndNum2 = UnityEngine.Random.Range(0, 4);

                        var t = obj.transform.GetChild(0);
                        var r = t.GetComponent<SpriteRenderer>();
                        r.sprite = tilesSO.Tiles[rndNum1];
                        r.color = ProjectBehaviour.GameManager.MapColor;

                        var v = t.eulerAngles;
                        v.z = rndNum2 * 90;
                        t.eulerAngles = v;

                        Tile tile = new Tile(pos, obj, TileType.FloorHallway);

                        if (!dungeon.Tiles.TryAdd(pos, tile))
                        {
                            Debug.LogWarning("DoubleTile");
                        }
                    }
                }
            }
            else
            {
                Vector2 diffrence = hallway.End - hallway.Start;

                bool x = false;
                if (Mathf.Abs(diffrence.x) > Mathf.Abs(diffrence.y))
                {
                    x = true;
                }

                int value = (int)Mathf.Max(Mathf.Abs(diffrence.x), Mathf.Abs(diffrence.y)) + 1;

                for (int i = 0; i < value; i++)
                {
                    if (x)
                    {
                        TilesHolderSO tilesSO = ProjectBehaviour.GameManager.FloorTiles;
                        Vector2 pos = new Vector2(i, 0) + hallway.Start;
                        var obj = GameObject.Instantiate(tilesSO.Prefab, pos, Quaternion.identity, ProjectBehaviour.GameManager.transform);

                        int rndNum1 = UnityEngine.Random.Range(0, tilesSO.Tiles.Length);
                        int rndNum2 = UnityEngine.Random.Range(0, 4);

                        var t = obj.transform.GetChild(0);
                        var r = t.GetComponent<SpriteRenderer>();
                        r.sprite = tilesSO.Tiles[rndNum1];
                        r.color = ProjectBehaviour.GameManager.MapColor;

                        var v = t.eulerAngles;
                        v.z = rndNum2 * 90;
                        t.eulerAngles = v;

                        Tile tile = new Tile(pos, obj, TileType.FloorHallway);

                        if (!dungeon.Tiles.TryAdd(pos, tile))
                        {
                            Debug.LogWarning("DoubleTile");
                        }
                    }
                    else
                    {
                        TilesHolderSO tilesSO = ProjectBehaviour.GameManager.FloorTiles;
                        Vector2 pos = new Vector2(0, i) + hallway.Start;
                        var obj = GameObject.Instantiate(tilesSO.Prefab, pos, Quaternion.identity, ProjectBehaviour.GameManager.transform);

                        int rndNum1 = UnityEngine.Random.Range(0, tilesSO.Tiles.Length);
                        int rndNum2 = UnityEngine.Random.Range(0, 4);

                        var t = obj.transform.GetChild(0);
                        var r = t.GetComponent<SpriteRenderer>();
                        r.sprite = tilesSO.Tiles[rndNum1];
                        r.color = ProjectBehaviour.GameManager.MapColor;

                        var v = t.eulerAngles;
                        v.z = rndNum2 * 90;
                        t.eulerAngles = v;

                        Tile tile = new Tile(pos, obj, TileType.FloorHallway);

                        if (!dungeon.Tiles.TryAdd(pos, tile))
                        {
                            Debug.LogWarning("DoubleTile");
                        }
                    }
                }
            }
        }

        return dungeon;
    }

    public Tile CreateWall(Vector2 pos, Room room, Vector2 offset)
    {
        TilesHolderSO tilesSO = ProjectBehaviour.GameManager.FrontWallTiles;
        var obj = GameObject.Instantiate(tilesSO.Prefab, pos, Quaternion.identity, ProjectBehaviour.GameManager.transform);

        int rndNum1 = UnityEngine.Random.Range(0, tilesSO.Tiles.Length);

        var t = obj.transform.GetChild(0);
        var r = t.GetComponent<SpriteRenderer>();
        r.sprite = tilesSO.Tiles[rndNum1];
        r.color = ProjectBehaviour.GameManager.MapColor;

        Tile tile = new Tile(pos, offset, obj, room, TileType.Wall);
        return tile;
    }
}

public class Dungeon
{
    public List<Room> MainRooms = new List<Room>();
    public List<Room> NormalRooms = new List<Room>();
    public List<Hallway> Hallways = new List<Hallway>();
    public Dictionary<Vector2, Tile> Tiles = new Dictionary<Vector2, Tile>();

    public Dungeon()
    {
        
    }

    public Dungeon(List<Room> mainRooms, List<Room> normalRooms, List<Hallway> hallways, Dictionary<Vector2, Tile> tiles)
    {
        MainRooms = mainRooms;
        NormalRooms = normalRooms;
        Hallways = hallways;
        Tiles = tiles;
    }
}

public enum TileType
{
    None,
    FloorMain,
    FloorNormal,
    FloorHallway,
    Wall,
}

public class Tile
{
    public Vector2 Position;
    public Vector2 OffsetPosition;
    public GameObject SceneObject;
    public Room Room;
    public TileType Type;

    public Tile(Vector2 position, Vector2 offset, GameObject sceneObject, Room room, TileType type)
    {
        Position = position;
        OffsetPosition = offset;
        SceneObject = sceneObject;
        room.OnRemove += OnRoomRemoved;
        room.OnRoomMoved += OnRoomMoved;
        Room = room;
        Type = type;
    }

    public Tile(Vector2 position, GameObject sceneObject, TileType type)
    {
        Position = position;
        SceneObject = sceneObject;
        Type = type;
    }

    private void OnRoomMoved(object sender, EventArgs e)
    {
        Position = Room.Origin + OffsetPosition;
        SceneObject.transform.position = Position;
    }

    private void OnRoomRemoved(object sender, EventArgs e)
    {
        GameObject.Destroy(SceneObject);
        Room.OnRoomMoved -= OnRoomMoved;
        Room.OnRemove -= OnRoomRemoved;
    }
}

public class Room
{
    public int Width;
    public int Height;

    public Vector2 Origin;

    public bool MainRoom = false;
    public bool Visualized = false;

    public event EventHandler OnRemove;
    public event EventHandler OnRoomMoved;

    public void RoomMoved()
    {
        OnRoomMoved?.Invoke(this, EventArgs.Empty);
    }

    public void Remove()
    {
        OnRemove?.Invoke(this, EventArgs.Empty);
    }

    public bool Contains(Vector2 point)
    {
        return Origin.x < point.x && Origin.y < point.y && (Origin.x + Width) > point.x && (Origin.y + Height) > point.y;
    }

    public Room(int width, int height, Vector2 origin)
    {
        Width = width;
        Height = height;
        Origin = origin;
    }
}

public class Hallway
{
    public Vector2 Start;
    public Vector2 End;

    public bool L_Shape;

    public Vector2 BendPoint;

    public Hallway(Vector2 start, Vector2 end)
    {
        this.Start = start;
        this.End = end;
        this.L_Shape = false;
    }

    public Hallway(Vector2 start, Vector2 end, Vector2 bendpoint)
    {
        this.Start = start;
        this.End = end;
        this.L_Shape = true;
        BendPoint = bendpoint;
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
        public Room Room;

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
            this.Room = null;
        }

        public Point(float x, float y, Room room)
        {
            this.x = x;
            this.y = y;
            this.Room = room;
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

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is Point))
            {
                return false;
            }

            return Equals((Point)other);
        }

        public bool Equals(Point other)
        {
            return x == other.x && y == other.y;
        }

        public static bool AlmostEqual(float x, float y)
        {
            return Mathf.Abs(x - y) <= float.Epsilon * Mathf.Abs(x + y) * 2
                || Mathf.Abs(x - y) < float.MinValue;
        }

        public static bool AlmostEqual(Point left, Point right)
        {
            return AlmostEqual(left.x, right.x) && AlmostEqual(left.y, right.y);
        }
    }

    public class Edge
    {
        public Point Start;
        public Point End;
        public float Weight;
        public bool InTree;
        public bool IsBad;

        public Edge(Point start, Point end)
        {
            this.IsBad = false;
            this.Start = start;
            this.End = end;
            this.Weight = this.Start.DistanceTo(end); // Euclidean distance
            this.InTree = false;
        }


        //public static bool operator ==(Edge lhs, Edge rhs)
        //{
        //    return lhs.Start == rhs.Start && lhs.End == rhs.End;
        //}

        //public static bool operator !=(Edge lhs, Edge rhs)
        //{
        //    return !(lhs == rhs);
        //}

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object other)
        {
            if (!(other is Edge))
            {
                return false;
            }

            return Equals((Edge)other);
        }

        public bool Equals(Edge other)
        {
            return this.Start == other.Start && this.End == other.End;
        }

        public static bool AlmostEqual(Edge left, Edge right)
        {
            return Point.AlmostEqual(left.Start, right.Start) && Point.AlmostEqual(left.End, right.End)
                || Point.AlmostEqual(left.Start, right.End) && Point.AlmostEqual(left.End, right.Start);
        }
    }

    //public class EdgedTriangle
    //{
    //    public Edge[] Edges;
    //    //public Edge Edge1;
    //    //public Edge Edge2;
    //    //public Edge Edge3;

    //    public EdgedTriangle(Edge edge1, Edge edge2, Edge edge3)
    //    {
    //        this.Edges = new Edge[3];
    //        this.Edges[0] = edge1;
    //        this.Edges[1] = edge2;
    //        this.Edges[2] = edge3;
    //        //this.Edge1 = edge1;
    //        //this.Edge2 = edge2;
    //        //this.Edge3 = edge3;
    //    }

    //    public void SetEdges(Edge[] edges)
    //    {
    //        this.Edges = edges;
    //    }

    //    public Point GetCircumcenter()
    //    {
    //        Point pointA = Edges[0].Start;
    //        Point pointB = Edges[1].Start;
    //        Point pointC = Edges[2].Start;

    //        LinearEquation lineAB = new LinearEquation(pointA, pointB);
    //        LinearEquation lineBC = new LinearEquation(pointB, pointC);

    //        Point midPointAB = Point.Lerp(pointA, pointB, 0.5f);
    //        Point midPointBC = Point.Lerp(pointB, pointC, 0.5f);

    //        LinearEquation perpendicularAB = lineAB.PerpendicularLineAt(midPointAB);
    //        LinearEquation perpendicularBC = lineBC.PerpendicularLineAt(midPointBC);

    //        Point circumcircle = LinearEquation.GetCrossingPoint(perpendicularAB, perpendicularBC);

    //        return circumcircle;
    //    }

    //    public float GetCircumRadius(Point circumcicle)
    //    {
    //        Point pointA = Edges[0].Start;

    //        float circumCircle = Point.Distance(circumcicle, pointA);

    //        return circumCircle;
    //    }

    //    private float TriangleArea(Point p1, Point p2, Point p3)
    //    {
    //        float det = ((p1.x - p3.x) * (p2.y - p3.y)) - ((p2.x - p3.x) * (p1.y - p3.y));
    //        return (det / 2.0f);
    //    }

    //    private bool TrianglesAreaApproach(Point point, Point p1, Point p2, Point p3)
    //    {
    //        float triangleArea = TriangleArea(p1, p2, p3);

    //        float areaSum = 0f;
    //        areaSum += TriangleArea(p1, p2, point);
    //        areaSum += TriangleArea(p1, p3, point);
    //        areaSum += TriangleArea(p2, p3, point);

    //        return (triangleArea == areaSum);
    //    }

    //    public bool ContainsEdge(Edge edge)
    //    {
    //        for (int i = 0; i < Edges.Length; i++)
    //        {
    //            if (Edges[i] == edge)
    //            {
    //                return true;
    //            }
    //        }

    //        return false;
    //    }

    //    public bool ContainsVertex(Point vertex)
    //    {
    //        return Edges[0].Start == vertex || Edges[1].Start == vertex || Edges[2].Start == vertex;
    //    }

    //    public static bool operator ==(EdgedTriangle lhs, EdgedTriangle rhs)
    //    {
    //        return lhs.Edges[0] == rhs.Edges[0] && lhs.Edges[1] == rhs.Edges[1] && lhs.Edges[2] == rhs.Edges[2];
    //    }

    //    public static bool operator !=(EdgedTriangle lhs, EdgedTriangle rhs)
    //    {
    //        return !(lhs == rhs);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return 0;
    //    }

    //    public override bool Equals(object other)
    //    {
    //        if (!(other is EdgedTriangle))
    //        {
    //            return false;
    //        }

    //        return Equals((EdgedTriangle)other);
    //    }

    //    public bool Equals(EdgedTriangle other)
    //    {
    //        return this.Edges[0] == other.Edges[0] && this.Edges[1] == other.Edges[1] && this.Edges[2] == other.Edges[2];
    //    }
    //}

    public class Triangle
    {
        public bool IsBad;

        public Point P1;
        public Point P2;
        public Point P3;

        public Triangle(Point p1, Point p2, Point p3)
        {
            IsBad = false;
            this.P1 = p1;
            this.P2 = p2;
            this.P3 = p3;
        }

        public Point GetCircumcenter()
        {
            Point pointA = P1;
            Point pointB = P2;
            Point pointC = P3;

            LinearEquation lineAB = new LinearEquation(pointA, pointB);
            LinearEquation lineBC = new LinearEquation(pointB, pointC);

            Point midPointAB = Point.Lerp(pointA, pointB, 0.5f);
            Point midPointBC = Point.Lerp(pointB, pointC, 0.5f);

            LinearEquation perpendicularAB = lineAB.PerpendicularLineAt(midPointAB);
            LinearEquation perpendicularBC = lineBC.PerpendicularLineAt(midPointBC);

            Point circumcircle = LinearEquation.GetCrossingPoint(perpendicularAB, perpendicularBC);

            return circumcircle;
        }

        public bool CircumCircleContains(Point point)
        {
            Point circlePoint = GetCircumcenter();
            float radius = GetCircumRadius(circlePoint);

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

        public float GetCircumRadius(Point circumcicle)
        {
            Point pointA = P1;

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
            return Point.Distance(vertex, P1) < 0.01f
                || Point.Distance(vertex, P2) < 0.01f
                || Point.Distance(vertex, P3) < 0.01f;
        }

        public static bool operator ==(Triangle lhs, Triangle rhs)
        {
            return lhs.P1 == rhs.P1 && lhs.P2 == rhs.P2 && lhs.P3 == rhs.P3;
        }

        public static bool operator !=(Triangle lhs, Triangle rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            return P1.GetHashCode() ^ P2.GetHashCode() ^ P3.GetHashCode();
        }

        public override bool Equals(object other)
        {
            if (!(other is Triangle))
            {
                return false;
            }

            return Equals((Triangle)other);
        }

        public bool Equals(Triangle other)
        {
            return this == other;
        }
    }

    public class TriangleMesh
    {
        public List<Triangle> Triangles = new List<Triangle>();
        public List<Edge> Edges = new List<Edge>();
        public List<Point> Points = new List<Point>();

        //public void SetTriangles(List<Triangle> triangles)
        //{
        //    this.Triangles = triangles;
        //}

        //public void SetEdgeAtIndex(int index, Edge edge)
        //{
        //    List<Edge> edges = GetUniqueEdges();
        //    edges[index] = edge;
        //}

        //public List<Point> GetUniqueVertices()
        //{
        //    List<Point> vertices = new List<Point>();

        //    for (int i = 0; i < Triangles.Count; i++)
        //    {
        //        Triangle triangle = Triangles[i];

        //        for (int j = 0; j < triangle.Edges.Length; j++)
        //        {
        //            if (!vertices.Contains(triangle.Edges[j].Start))
        //            {
        //                vertices.Add(triangle.Edges[j].Start);
        //            }
        //        }
        //    }

        //    return vertices;
        //}

        //public List<Edge> GetEdges()
        //{
        //    List<Edge> edges = new List<Edge>();

        //    for (int i = 0; i < Triangles.Count; i++)
        //    {
        //        Triangle triangle = Triangles[i];

        //        for (int j = 0; j < triangle.Edges.Length; j++)
        //        {
        //            edges.Add(triangle.Edges[j]);
        //        }
        //    }

        //    return edges;
        //}

        //public List<Edge> GetUniqueEdges()
        //{
        //    List<Edge> edges = new List<Edge>();

        //    for (int i = 0; i < Triangles.Count; i++)
        //    {
        //        Triangle triangle = Triangles[i];

        //        for (int j = 0; j < triangle.Edges.Length; j++)
        //        {
        //            if (!edges.Contains(triangle.Edges[j]))
        //            {
        //                edges.Add(triangle.Edges[j]);
        //            }
        //            else
        //            {

        //            }
        //        }
        //    }

        //    return edges;
        //}

        public void Visualize()
        {
            foreach (Edge edge in Edges.Where(e => !e.InTree))
            {
                Vector2 start = new Vector2(edge.Start.x, edge.Start.y);
                Vector2 end = new Vector2(edge.End.x, edge.End.y);

                Debug.DrawLine(start, end, UnityEngine.Color.black, 3.5f);
            }

            foreach (Edge edge in Edges.Where(e => e.InTree))
            {
                Vector2 start = new Vector2(edge.Start.x, edge.Start.y);
                Vector2 end = new Vector2(edge.End.x, edge.End.y);

                Debug.DrawLine(start, end, UnityEngine.Color.cyan, 100f);
            }
        }
    }

    public static class BowyerWatson
    {
        public static float minSuperTriangleSize;

        //public static TriangleMesh Triangulate(Point[] points, float xSize, float ySize)
        //{
        //    TriangleMesh triangulation = new TriangleMesh();
        //    triangulation.Points = points.ToList();

        //    EdgedTriangle superTriangle = CreateSuperTriangle(xSize, ySize);

        //    triangulation.Triangles.Add(superTriangle);

        //    foreach (Point point in triangulation.Points)
        //    {
        //        List<EdgedTriangle> badTriangles = new List<EdgedTriangle>();

        //        foreach (EdgedTriangle triangle in triangulation.Triangles)
        //        {
        //            Point circumcenter = triangle.GetCircumcenter();
        //            float circumRadius = triangle.GetCircumRadius(circumcenter);
        //            if (CheckIfPointLiesInCircle(point, circumcenter, circumRadius))
        //            {
        //                badTriangles.Add(triangle);
        //            }
        //        }

        //        List<Edge> polygon = new List<Edge>();

        //        foreach (EdgedTriangle triangle in badTriangles)
        //        {
        //            foreach(Edge edge in triangle.Edges)
        //            {
        //                bool isShared = badTriangles.Any(t => t != triangle && t.ContainsEdge(edge));
        //                if (!isShared)
        //                {
        //                    polygon.Add(edge);
        //                }
        //            }
        //        }

        //        foreach (EdgedTriangle triangle in badTriangles)
        //        {
        //            triangulation.Triangles.Remove(triangle);
        //        }

        //        foreach (Edge edge in polygon)
        //        {
        //            Edge edge1 = new Edge(edge.Start, edge.End);
        //            Edge edge2 = new Edge(edge.End, point);
        //            Edge edge3 = new Edge(point, edge.Start);

        //            EdgedTriangle newTriangle = new EdgedTriangle(edge1, edge2, edge3);
        //            triangulation.Triangles.Add(newTriangle);
        //        }
        //    }

        //    triangulation.Triangles = triangulation.Triangles.Where(t => 
        //        !t.ContainsVertex(superTriangle.Edges[0].Start) &&
        //        !t.ContainsVertex(superTriangle.Edges[1].Start) &&
        //        !t.ContainsVertex(superTriangle.Edges[2].Start)).ToList();

        //    HashSet<Edge> edgeSet = new HashSet<Edge>();

        //    foreach (EdgedTriangle triangle in triangulation.Triangles)
        //    {
        //        Edge ab = new Edge(triangle.Edges[0].Start, triangle.Edges[0].End);
        //        Edge bc = new Edge(triangle.Edges[1].Start, triangle.Edges[1].End);
        //        Edge ca = new Edge(triangle.Edges[2].Start, triangle.Edges[2].End);

        //        if (edgeSet.Add(ab))
        //        {
        //            triangulation.Edges.Add(ab);
        //        }

        //        if (edgeSet.Add(bc))
        //        {
        //            triangulation.Edges.Add(bc);
        //        }

        //        if (edgeSet.Add(ca))
        //        {
        //            triangulation.Edges.Add(ca);
        //        }
        //    }

        //    return triangulation;
        //}

        public static TriangleMesh Triangulate(Point[] points)
        {
            TriangleMesh triangulation = new TriangleMesh();
            triangulation.Points = points.ToList();

            float xMinSize = 0;
            float xMaxSize = 0;
            float yMinSize = 0;
            float yMaxSize = 0;

            for (int i = 0; i < points.Length; i++)
            {
                Point point = points[i];
                if (xMinSize > point.x) xMinSize = point.x;
                if (xMaxSize < point.x) xMaxSize = point.x;
                if (yMinSize > point.y) yMinSize = point.y;
                if (yMaxSize < point.y) yMaxSize = point.y;
            }

            Triangle superTriangle = CreateSuperTriangle(xMinSize, xMaxSize, yMinSize, yMaxSize);

            triangulation.Triangles.Add(superTriangle);

            foreach (Point point in triangulation.Points)
            {
                List<Edge> polygon = new List<Edge>();

                foreach (Triangle triangle in triangulation.Triangles)
                {
                    if (triangle.CircumCircleContains(point))
                    {
                        triangle.IsBad = true;
                        polygon.Add(new Edge(triangle.P1, triangle.P2));
                        polygon.Add(new Edge(triangle.P2, triangle.P3));
                        polygon.Add(new Edge(triangle.P3, triangle.P1));
                    }
                }

                triangulation.Triangles.RemoveAll((Triangle t) => t.IsBad);

                for (int i = 0; i < polygon.Count; i++)
                {
                    for (int j = i + 1; j < polygon.Count; j++)
                    {
                        if (Edge.AlmostEqual(polygon[i], polygon[j]))
                        {
                            polygon[i].IsBad = true;
                            polygon[j].IsBad = true;
                        }
                    }
                }

                polygon.RemoveAll((Edge e) => e.IsBad);

                foreach (Edge edge in polygon)
                {
                    triangulation.Triangles.Add(new Triangle(edge.Start, edge.End, point));
                }
            }

            triangulation.Triangles.RemoveAll((Triangle t) => t.ContainsVertex(superTriangle.P1) || t.ContainsVertex(superTriangle.P2) || t.ContainsVertex(superTriangle.P3));

            HashSet<Edge> edgeSet = new HashSet<Edge>();

            foreach (Triangle triangle in triangulation.Triangles)
            {
                var ab = new Edge(triangle.P1, triangle.P2);
                var bc = new Edge(triangle.P2, triangle.P3);
                var ca = new Edge(triangle.P3, triangle.P1);

                if (edgeSet.Add(ab))
                {
                    triangulation.Edges.Add(ab);
                }

                if (edgeSet.Add(bc))
                {
                    triangulation.Edges.Add(bc);
                }

                if (edgeSet.Add(ca))
                {
                    triangulation.Edges.Add(ca);
                }
            }

            return triangulation;
        }

        private static Triangle CreateSuperTriangle(float xMinSize, float xMaxSize, float yMinSize, float yMaxSize)
        {
            float xLength = xMaxSize - xMinSize;
            float yLength = yMaxSize - yMinSize;

            Point minCorner = new Point(xMinSize, yMinSize);
            //Point maxCorner = new Point(xMaxSize, yMaxSize);

            Point p1 = new Point(-xLength -1, -yLength -1) + minCorner;
            Point p2 = new Point(xLength * 2 +1, -yLength -1) + minCorner;
            Point p3 = new Point(xLength / 2, yLength * 1.5f + 1) + minCorner;

            p1 *= 5;
            p2 *= 5;
            p3 *= 5;

            Triangle triangle = new Triangle(p1, p2, p3);
            return triangle;
        }

        //private static bool CheckIfPointLiesInCircle(Point point, Point circlePoint, float radius)
        //{
        //    if ((point.x - circlePoint.x) * (point.x - circlePoint.x) + 
        //        (point.y - circlePoint.y) * (point.y - circlePoint.y)
        //        <= radius * radius)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

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
            List<Edge> edges = triangleMesh.Edges;

            for (int i = 1; i < VerticesCount; i++)
            {
                int startIndex = parent[i];
                int endIndex = i;

                Point start = points[startIndex];
                Point end = points[endIndex];

                Edge edge = edges.Where(e => e.Start == start && e.End == end || e.Start == end && e.End == start).FirstOrDefault();

                //for (int j = 0; j < edges.Count; j++)
                //{
                //    Edge currentEdge = edges[j];
                //    if (currentEdge.Start == start && currentEdge.End == end)
                //    {
                //        edge = currentEdge;
                //        break;
                //    }
                //}

                Debug.Log($"after: {startIndex} - {endIndex} \t{GetEdgeWeight(startIndex, endIndex)}");

                if (edge != null)
                {
                    edge.InTree = true;
                }
                else
                {
                    Debug.Log("Ole! ");
                }

                //GraphEdge graphEdge = GetEdge(parent[i], i);

                //if (graphEdge != null)
                //{
                //    edges.Contains(graphEdge.Edge);

                //    Edge edge = edges.Find(e => e == graphEdge.Edge);
                //}

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
            List<Point> points = triangleMesh.Points;
            List<Edge> edges = triangleMesh.Edges;

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

        public static void RandomlySelectEdges(TriangleMesh triangleMesh, float numAddedProcentEdges)
        {
            int totalEdges = triangleMesh.Edges.Count;

            int numEdges = 0;

            numEdges = (int)((float)totalEdges / 100f * numAddedProcentEdges);

            List<Edge> edges = triangleMesh.Edges.Where(e => !e.InTree).ToList();

            for (int i = 0; i < numEdges; i++)
            {
                int r = UnityEngine.Random.Range(0, edges.Count);

                Edge edge = edges[r];

                edge.InTree = true;

                edges.Remove(edge);
            }
        }
    }
}
