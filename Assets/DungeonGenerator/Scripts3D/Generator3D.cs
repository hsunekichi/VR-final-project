using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;

public class Generator3D : MonoBehaviour
{
    enum CellType
    {
        None,
        Room,
        Hallway,
        Stairs
    }

    class Room
    {
        public Bounds bounds;

        public Room(Vector3 location, Vector3 size)
        {
            bounds = new Bounds(location + size / 2f, size);
        }

        public static bool Intersect(Room a, Room b)
        {
            return a.bounds.Intersects(b.bounds);
        }
    }

    // DUNGEON CONFIGURATION
    [SerializeField]
    int seed;
    [SerializeField]
    Vector3Int size;
    [SerializeField]
    int roomCount;
    [SerializeField]
    Vector3Int roomMaxSize;
    [SerializeField]
    Vector3Int roomMinSize;
    public Transform PlayerRef;


    // SOLID PREFABS
    [SerializeField]
    GameObject[] floorPrefabs;
    [SerializeField]
    GameObject[] ceilingPrefabs;
    [SerializeField]
    GameObject[] wallPrefabs;
    [SerializeField]
    GameObject stairsPrefab;
    [SerializeField]
    GameObject[] columnPrefabs;
    [SerializeField]
    GameObject[] cornerPrefabs;
    [SerializeField]
    GameObject[] doorPrefabs;

    // OTHER PREFABS
    [SerializeField]
    GameObject colliderPrefab;
    [SerializeField]
    GameObject spawnerPrefab;
    [SerializeField]
    GameObject cubePrefab;

    //MATERIALS
    [SerializeField]
    Material redMaterial;
    [SerializeField]
    Material blueMaterial;
    [SerializeField]
    Material greenMaterial;

    Random random;
    Grid3D<CellType> grid;
    List<Room> rooms;
    Delaunay3D delaunay;
    HashSet<Prim.Edge> selectedEdges;
    [SerializeField]
    List<Vector3Int> path;

    void Start()
    {
        random = new Random(seed);
        grid = new Grid3D<CellType>(size, Vector3Int.zero);
        rooms = new List<Room>();

        PlaceInitialStructure();
        PlaceRooms();
        Triangulate();
        CreateHallways();
        path = PathfindHallways();
        RenderGrids();
        PutPillars();
    }

    void Update()
    {
        checkLocation();
    }

    void checkLocation()
    {
        if (PlayerRef != null && rooms != null)
        {
            Vector3 playerPos = PlayerRef.position;
            bool insideRoom = false;

            foreach (var room in rooms)
            {
                if (room.bounds.Contains(playerPos))
                {
                    insideRoom = true;
                    break;
                }
            }

            // if (insideRoom)
            // {
            //     Debug.Log("Player is inside a room.");
            // }
            // else
            // {
            //     Debug.Log("Player is NOT inside any room.");
            // }
        }
    }

    void PlaceInitialStructure()
    {
        // Crea una habitación de 2x2x2 que empieza en (0,0,0)
        Vector3Int location = new Vector3Int(0, 0, 0);
        Vector3Int roomSize = new Vector3Int(2, 1, 2);

        Room initialRoom = new Room(location, roomSize);
        rooms.Add(initialRoom);

        // Marca las celdas del grid como Room
        Vector3 min = initialRoom.bounds.min;
        Vector3 max = initialRoom.bounds.max;
        for (int x = Mathf.FloorToInt(min.x); x < Mathf.CeilToInt(max.x); x++)
        {
            for (int y = Mathf.FloorToInt(min.y); y < Mathf.CeilToInt(max.y); y++)
            {
                for (int z = Mathf.FloorToInt(min.z); z < Mathf.CeilToInt(max.z); z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    grid[pos] = CellType.Room;
                }
            }
        }
    }

    void PlaceRooms()
    {
        int maxAttempts = 1000;
        int attempt = 0;
        while (rooms.Count < roomCount && attempt < maxAttempts)
        {
            attempt++;
            Vector3Int location = new Vector3Int(
                random.Next(0, size.x),
                random.Next(0, size.y),
                random.Next(0, size.z)
            );

            Vector3Int roomSize = new Vector3Int(
                random.Next(roomMinSize.x, roomMaxSize.x + 1),
                random.Next(roomMinSize.y, roomMaxSize.y + 1),
                random.Next(roomMinSize.z, roomMaxSize.z + 1)
            );

            bool add = true;
            Room newRoom = new Room(location, roomSize);
            Room buffer = new Room(location + new Vector3Int(-1, 0, -1), roomSize + new Vector3Int(2, 0, 2));

            foreach (var room in rooms)
            {
                if (Room.Intersect(room, buffer))
                {
                    add = false;
                    break;
                }
            }

            if (newRoom.bounds.min.x < 0 || newRoom.bounds.max.x >= size.x
                || newRoom.bounds.min.y < 0 || newRoom.bounds.max.y >= size.y
                || newRoom.bounds.min.z < 0 || newRoom.bounds.max.z >= size.z)
            {
                add = false;
            }

            if (add)
            {
                rooms.Add(newRoom);
                if (rooms.Count < roomCount)
                    attempt = 0;
                else
                    attempt = maxAttempts;
                // PlaceRoom(newRoom.bounds.position, newRoom.bounds.size);

                // Iterate through all integer positions within the bounds
                Vector3 min = newRoom.bounds.min;
                Vector3 max = newRoom.bounds.max;
                for (int x = Mathf.FloorToInt(min.x); x < Mathf.CeilToInt(max.x); x++)
                {
                    for (int y = Mathf.FloorToInt(min.y); y < Mathf.CeilToInt(max.y); y++)
                    {
                        for (int z = Mathf.FloorToInt(min.z); z < Mathf.CeilToInt(max.z); z++)
                        {
                            Vector3Int pos = new Vector3Int(x, y, z);
                            grid[pos] = CellType.Room;
                        }
                    }
                }
            }
        }

        // Place objects/spawners etc
        foreach (var room in rooms)
        {
            // Place spawners
            GameObject tile = Instantiate(
            spawnerPrefab,
            room.bounds.center,
            Quaternion.identity
            );

            var enemySpawn = tile.GetComponent<EnemySpawn>();
            if (enemySpawn != null)
            {
                // Debug.Log("EnemySpawn component found!");
                var enemySpawnType = enemySpawn.GetType();
                var sizeProp = enemySpawnType.GetField("SpawnSize");
                if (sizeProp != null)
                {
                    sizeProp.SetValue(enemySpawn, room.bounds.size);
                }
                var refObjProp = enemySpawnType.GetField("ReferenceObject");
                if (refObjProp != null)
                {
                    refObjProp.SetValue(enemySpawn, PlayerRef);
                }
            }
        }
    }

    void Triangulate()
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in rooms)
        {
            vertices.Add(new Vertex<Room>(room.bounds.center, room));
        }

        delaunay = Delaunay3D.Triangulate(vertices);
    }

    void CreateHallways()
    {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges)
        {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        List<Prim.Edge> minimumSpanningTree = Prim.MinimumSpanningTree(edges, edges[0].U);

        selectedEdges = new HashSet<Prim.Edge>(minimumSpanningTree);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (var edge in remainingEdges)
        {
            if (random.NextDouble() < 0.125)
            {
                selectedEdges.Add(edge);
            }
        }

        // Visualize all selected edges (interconnections) as yellow lines
        foreach (var edge in selectedEdges)
        {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;
            Vector3 start = startRoom.bounds.center + new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 end = endRoom.bounds.center + new Vector3(0.5f, 0.5f, 0.5f);
            Debug.DrawLine(start, end, Color.yellow, 10000, false);
        }
    }

    List<Vector3Int> PathfindHallways()
    {
        List<Vector3Int> globalPath = new List<Vector3Int>();
        DungeonPathfinder3D aStar = new DungeonPathfinder3D(size);

        foreach (var edge in selectedEdges)
        {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector3Int((int)startPosf.x, (int)startPosf.y, (int)startPosf.z);
            var endPos = new Vector3Int((int)endPosf.x, (int)endPosf.y, (int)endPosf.z);

            List<Vector3Int> path = aStar.FindPath(startPos, endPos, (DungeonPathfinder3D.Node a, DungeonPathfinder3D.Node b) =>
            {
                var pathCost = new DungeonPathfinder3D.PathCost();

                var delta = b.Position - a.Position;

                if (delta.y == 0)
                {
                    //flat hallway
                    pathCost.cost = Vector3Int.Distance(b.Position, endPos);    //heuristic

                    if (grid[b.Position] == CellType.Stairs)
                    {
                        return pathCost;
                    }
                    else if (grid[b.Position] == CellType.Room)
                    {
                        pathCost.cost += 5;
                    }
                    else if (grid[b.Position] == CellType.Hallway)
                    {
                        pathCost.cost += 2; // Permitir pasar por pasillos existentes con bajo coste
                    }
                    else if (grid[b.Position] == CellType.None)
                    {
                        pathCost.cost += 1;
                    }

                    // Permitir pasar por pasillos existentes
                    pathCost.traversable = true;
                }
                else
                {
                    // Permitir seguir una escalera existente solo si la dirección es coherente (subir/bajar y avanzar)
                    if (false && grid[b.Position] == CellType.Stairs)
                    {
                        Vector3Int dir = new Vector3Int();
                        for (int i = 0; i < 8; i++)
                        {
                            dir = (i % 4 == 0 ? Vector3Int.forward : (i % 4 == 1 ? Vector3Int.back : (i % 4 == 2 ? Vector3Int.right : Vector3Int.left)));
                            dir += (i < 4 ? Vector3Int.up : Vector3Int.down);

                            Vector3Int current = b.Position;

                            // Avanzar por la escalera en la misma dirección mientras siga habiendo escalera
                            if (grid.InBounds(current + dir) && grid[current + dir] == CellType.Stairs)
                            {
                                current += dir;
                            }
                            else
                                continue;

                            Vector3Int afterStairs = current + new Vector3Int(dir.x, 0, dir.z);

                            // Solo permitimos si después de la escalera hay un espacio transitable
                            if (grid.InBounds(afterStairs) && grid[afterStairs] == CellType.Hallway)
                            {
                                pathCost.traversable = true;
                                pathCost.cost = 100 + Vector3Int.Distance(afterStairs, endPos); // base cost + heuristic
                                pathCost.isStairs = true;
                                return pathCost;
                            }
                            else
                                continue;
                        }
                        pathCost.traversable = false;
                        pathCost.cost = 10000;
                        return pathCost;
                    }

                    //staircase nueva
                    if ((grid[a.Position] != CellType.None && grid[a.Position] != CellType.Hallway)
                        || (grid[b.Position] != CellType.None && grid[b.Position] != CellType.Hallway)) return pathCost;

                    pathCost.cost = 100 + Vector3Int.Distance(b.Position, endPos);    //base cost + heuristic

                    int xDir = Mathf.Clamp(delta.x, -1, 1);
                    int zDir = Mathf.Clamp(delta.z, -1, 1);
                    Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                    Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                    if (!grid.InBounds(a.Position + verticalOffset)
                        || !grid.InBounds(a.Position + horizontalOffset)
                        || !grid.InBounds(a.Position + verticalOffset + horizontalOffset))
                    {
                        return pathCost;
                    }

                    if (grid[a.Position + horizontalOffset] != CellType.None
                        || grid[a.Position + horizontalOffset * 2] != CellType.None
                        || grid[a.Position + verticalOffset + horizontalOffset] != CellType.None
                        || grid[a.Position + verticalOffset + horizontalOffset * 2] != CellType.None)
                    {
                        return pathCost;
                    }

                    pathCost.traversable = true;
                    pathCost.isStairs = true;
                }

                return pathCost;
            });

            if (path != null)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    var current = path[i];

                    if (grid[current] == CellType.None)
                    {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0)
                    {
                        var prev = path[i - 1];

                        var delta = current - prev;

                        if (delta.y != 0)
                        {
                            int xDir = Mathf.Clamp(delta.x, -1, 1);
                            int zDir = Mathf.Clamp(delta.z, -1, 1);
                            Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                            Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                            grid[prev + horizontalOffset] = CellType.Stairs;
                            grid[prev + horizontalOffset * 2] = CellType.Stairs;
                            grid[prev + verticalOffset + horizontalOffset] = CellType.Stairs;
                            grid[prev + verticalOffset + horizontalOffset * 2] = CellType.Stairs;

                            // Save the stair position
                            float angle = Mathf.Atan2(horizontalOffset.x, horizontalOffset.z) * Mathf.Rad2Deg;
                            Quaternion rotation = Quaternion.Euler(0, (delta.y < 0 ? 180 : 0) + angle, 0);

                            Vector3 offset = Vector3.zero;
                            if (delta.y < 0)
                            {
                                if (delta.x < 0 || delta.z < 0)
                                    offset = new Vector3(xDir == 0f ? -zDir / 2f : 3 * xDir / 2f, delta.y, zDir == 0f ? -xDir / 2f : 3 * zDir / 2f);
                                else
                                    offset = new Vector3(xDir == 0f ? zDir / 2f : 5 * xDir / 2f, delta.y, zDir == 0f ? xDir / 2f : 5 * zDir / 2f);
                            }
                            else
                            {
                                if (delta.x < 0 || delta.z < 0)
                                    offset = new Vector3(xDir == 0f ? -zDir / 2f : xDir / 2f, 0f, zDir == 0f ? -xDir / 2f : zDir / 2f);
                                else
                                    offset = new Vector3(xDir == 0f ? zDir / 2f : 3 * xDir / 2f, 0f, zDir == 0f ? xDir / 2f : 3 * zDir / 2f);
                            }
                            PlaceStairs(prev + offset, offset, rotation, xDir, zDir, delta);
                        }

                        Debug.DrawLine(prev + new Vector3(0.5f, 0.5f, 0.5f), current + new Vector3(0.5f, 0.5f, 0.5f), grid[prev] == CellType.Room && grid[current] == CellType.Hallway ? Color.red : Color.blue, 10000, false);//grid[current] == CellType.Room ||
                        globalPath.Add(current);
                    }
                }

                foreach (var pos in path)
                {
                    if (grid[pos] == CellType.Hallway)
                    {
                        // PlaceHallway(pos);
                    }
                }
            }
        }
        return globalPath;
    }

    void PlaceCube(Vector3Int location, Vector3 size, Material material)
    {
        GameObject go = Instantiate(cubePrefab, location, Quaternion.identity);
        go.GetComponent<Transform>().localScale = size;
        go.GetComponent<MeshRenderer>().material = material;
    }
    void PlaceFloor(Vector3 location, Vector3 size)
    {
        // Tamaño de cada baldosa en XZ
        const float tileSize = 0.25f;

        // Número de baldosas en X y Z (redondeado al entero más cercano)
        int countX = Mathf.RoundToInt(size.x / tileSize);
        int countZ = Mathf.RoundToInt(size.z / tileSize);

        // Creamos un objeto padre para agrupar las baldosas
        GameObject parent = new GameObject("Floor");
        parent.transform.position = location;

        // Calculamos la posición de la esquina suroeste (inferior izquierda)
        // Partimos del centro, desplazamos la mitad del tamaño total, y luego ajustamos
        // para centrar la primera baldosa (tileSize/2)
        Vector3 totalOffset = new Vector3(countX * tileSize, 0f, countZ * tileSize) * 2;
        Vector3 startPos = location - totalOffset + new Vector3(tileSize * 2, 0f, tileSize * 2);

        // Instanciamos cada baldosa en la cuadrícula
        for (int x = 0; x < countX; x++)
        {
            for (int z = 0; z < countZ; z++)
            {
                // Posición de la baldosa actual
                Vector3 tilePos = startPos + new Vector3(x, 0f, z);

                // Instanciamos la baldosa como hija del objeto padre
                // Obtener todos los prefabs de la carpeta especificada

                for (int i = 0; i < 4; i++)
                {
                    var factor1 = i < 2 ? (i == 0 ? 1 : 1) : (i == 2 ? -1 : -1);
                    var factor2 = i < 2 ? (i == 0 ? 1 : -1) : (i == 2 ? -1 : 1);
                    Vector3 tilePosAux = tilePos + new Vector3(0.25f * factor1, 0, 0.25f * factor2);
                    GameObject prefabToUse = null;
                    if (floorPrefabs != null && floorPrefabs.Length > 0)
                    {
                        var idx = random.Next(0, floorPrefabs.Length);
                        prefabToUse = floorPrefabs[idx];
                    }

                    GameObject tile = Instantiate(
                        prefabToUse,
                        tilePosAux,
                        Quaternion.identity,
                        parent.transform
                    );

                    // Ajustamos escala: tileSize×size.y×tileSize
                    tile.transform.localScale = new Vector3(tileSize * 0.5f, size.y, tileSize * 0.5f);
                }

                // Instanciamos la pieza con la misma rotación que la pared
                GameObject collider = Instantiate(
                    colliderPrefab,
                    tilePos,
                    Quaternion.identity,
                    parent.transform
                );

                // Escalamos: ancho=tileWidth, alto=size.y, grosor=size.z
                collider.transform.localScale = new Vector3(1.05f, 0.1f, 1.05f);
            }
        }
    }

    void PlaceCeiling(Vector3 location, Vector3 size)
    {
        // Tamaño de cada baldosa en XZ
        const float tileSize = 0.25f;

        // Número de baldosas en X y Z (redondeado al entero más cercano)
        int countX = Mathf.RoundToInt(size.x / tileSize);
        int countZ = Mathf.RoundToInt(size.z / tileSize);

        // Creamos un objeto padre para agrupar las baldosas
        GameObject parent = new GameObject("Floor");
        parent.transform.position = location;

        // Calculamos la posición de la esquina suroeste (inferior izquierda)
        // Partimos del centro, desplazamos la mitad del tamaño total, y luego ajustamos
        // para centrar la primera baldosa (tileSize/2)
        Vector3 totalOffset = new Vector3(countX * tileSize, 0f, countZ * tileSize) * 2;
        Vector3 startPos = location - totalOffset + new Vector3(tileSize * 2, 0f, tileSize * 2);

        // Instanciamos cada baldosa en la cuadrícula
        for (int x = 0; x < countX; x++)
        {
            for (int z = 0; z < countZ; z++)
            {
                // Posición de la baldosa actual
                Vector3 tilePos = startPos + new Vector3(x, 0f, z);

                for (int i = 0; i < 4; i++)
                {
                    var factor1 = i < 2 ? (i == 0 ? 1 : 1) : (i == 2 ? -1 : -1);
                    var factor2 = i < 2 ? (i == 0 ? 1 : -1) : (i == 2 ? -1 : 1);
                    Vector3 tilePosAux = tilePos + new Vector3(0.25f * factor1, 0, 0.25f * factor2);
                    GameObject prefabToUse = null;
                    if (ceilingPrefabs != null && ceilingPrefabs.Length > 0)
                    {
                        var idx = random.Next(0, ceilingPrefabs.Length);
                        prefabToUse = ceilingPrefabs[idx];
                    }

                    GameObject tile = Instantiate(
                        prefabToUse,
                        tilePosAux,
                        Quaternion.identity,
                        parent.transform
                    );

                    // Ajustamos escala: tileSize×size.y×tileSize
                    tile.transform.localScale = new Vector3(tileSize * 0.5f, size.y, tileSize * 0.5f);
                }

                // Instanciamos la pieza con la misma rotación que la pared
                GameObject collider = Instantiate(
                    colliderPrefab,
                    tilePos,
                    Quaternion.identity,
                    parent.transform
                );

                // Escalamos: ancho=tileWidth, alto=size.y, grosor=size.z
                collider.transform.localScale = new Vector3(1.05f, 0.1f, 1.05f);
            }
        }
    }

    public void PlaceWall(Vector3 location, Vector3 size, Quaternion rotation)
    {

        const float tileWidth = 0.25f;                 // ancho de cada pieza en X
        int countColumns = Mathf.RoundToInt(size.x / tileWidth);

        // Creamos un padre vacío para agrupar las piezas
        GameObject parent = new GameObject("Wall");
        parent.transform.position = location;
        parent.transform.rotation = rotation;


        // 1) Detectamos si la pared “mira” al eje Z (forward apunta más en Z)
        //    o si “mira” al eje X (forward apunta más en X).
        Vector3 forward = rotation * Vector3.forward;
        bool facesZ = Mathf.Abs(forward.z) > Mathf.Abs(forward.x);

        // 2) Elegimos el eje de expansión:
        //    • Si facesZ, la pared está alineada con Z, así que expandimos en X.
        //    • Si no, está alineada con X, así que expandimos en Z.
        Vector3 axis = facesZ ? Vector3.right : Vector3.forward;
        bool axisPositive = facesZ ? forward.x > 0 : forward.z > 0;


        // Calculamos el offset inicial en local X:
        // partimos del centro (-size.x/2), luego + tileWidth/2 para centrar la primera
        float halfWidth = size.x * 2;
        float startOffset = -halfWidth + (tileWidth * 2);

        // 3) Instanciamos cada columna a lo largo del eje elegido
        for (int i = 0; i < countColumns; i++)
        {
            // Calculamos posición en mundo: centro + axis * (offset + paso)
            float step = startOffset + i * tileWidth * 4;
            Vector3 worldPos = location + axis * step;


            GameObject prefabToUse = null;
            if (wallPrefabs != null && wallPrefabs.Length > 0)
            {
                var idx = random.Next(0, wallPrefabs.Length);
                prefabToUse = wallPrefabs[idx];
            }

            // Instanciamos la pieza con la misma rotación que la pared
            GameObject piece = Instantiate(
                prefabToUse,
                worldPos,
                rotation,
                parent.transform
            );

            // Escalamos: ancho=tileWidth, alto=size.y, grosor=size.z
            piece.transform.localScale = new Vector3(tileWidth, size.y * 1.01f, size.z);

            if (wallPrefabs != null && wallPrefabs.Length > 0)
            {
                var idx = random.Next(0, wallPrefabs.Length);
                prefabToUse = wallPrefabs[idx];
            }
            // Instanciamos la pieza con la misma rotación que la pared
            GameObject piece2 = Instantiate(
                prefabToUse,
                worldPos,
                Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 180, rotation.eulerAngles.z),
                parent.transform
            );

            // Escalamos: ancho=tileWidth, alto=size.y, grosor=size.z
            piece2.transform.localScale = new Vector3(tileWidth, size.y * 1.01f, size.z);


            // Instanciamos la pieza con la misma rotación que la pared
            GameObject collider = Instantiate(
                colliderPrefab,
                worldPos + Vector3.up * 0.5f,
                rotation,
                parent.transform
            );

            // Escalamos: ancho=tileWidth, alto=size.y, grosor=size.z
            collider.transform.localScale = new Vector3(1.05f, 1.05f, tileWidth);
        }
    }

    void PlaceRoom(Vector3Int location, Vector3Int size)
    {
        // PlaceCube(location, size, redMaterial);

        // Place floor
        Vector3 floorSize = new Vector3(size.x / 4f, 1, size.z / 4f);
        Vector3 floorLocation = location + new Vector3(size.x / 2f, 0, size.z / 2f);
        PlaceFloor(floorLocation, floorSize);

        // Place ceiling
        Vector3 ceilingLocation = location + new Vector3(size.x / 2f, size.y, size.z / 2f);
        PlaceCeiling(ceilingLocation, floorSize);

        // Place walls
        Vector3 wallSizeX = new Vector3(size.z / 4f, size.y / 4f, 0.25f);
        Vector3 wallSizeZ = new Vector3(size.x / 4f, size.y / 4f, 0.25f);

        // Front wall
        Vector3Int loc = location + Vector3Int.forward;
        if (grid.InBounds(loc) && (grid[loc] == CellType.None))
        {
            Vector3 backWallLocation = location + new Vector3(size.x / 2f, 0, size.z);
            PlaceWall(backWallLocation, wallSizeZ, Quaternion.identity);
        }
        else if (!grid.InBounds(loc))
        {
            Vector3 backWallLocation = location + new Vector3(size.x / 2f, 0, size.z);
            PlaceWall(backWallLocation, wallSizeZ, Quaternion.identity);
        }

        // Back wall
        loc = location + Vector3Int.back;
        if (grid.InBounds(loc) && (grid[loc] == CellType.None))
        {
            Vector3 frontWallLocation = location + new Vector3(size.x / 2f, 0, 0);
            PlaceWall(frontWallLocation, wallSizeZ, Quaternion.Euler(0, 180, 0));
        }
        else if (!grid.InBounds(loc))
        {
            Vector3 frontWallLocation = location + new Vector3(size.x / 2f, 0, 0);
            PlaceWall(frontWallLocation, wallSizeZ, Quaternion.Euler(0, 180, 0));

        }

        // Left wall
        loc = location + Vector3Int.left;
        if (grid.InBounds(loc) && (grid[loc] == CellType.None))
        {
            Vector3 leftWallLocation = location + new Vector3(0, 0, size.z / 2f);
            PlaceWall(leftWallLocation, wallSizeX, Quaternion.Euler(0, -90, 0));
        }
        else if (!grid.InBounds(loc))
        {
            Vector3 leftWallLocation = location + new Vector3(0, 0, size.z / 2f);
            PlaceWall(leftWallLocation, wallSizeX, Quaternion.Euler(0, -90, 0));
        }

        // Right wall
        loc = location + Vector3Int.right;
        if (grid.InBounds(loc) && (grid[loc] == CellType.None))
        {
            Vector3 rightWallLocation = location + new Vector3(size.x, 0, size.z / 2f);
            PlaceWall(rightWallLocation, wallSizeX, Quaternion.Euler(0, 90, 0));
        }
        else if (!grid.InBounds(loc))
        {
            Vector3 rightWallLocation = location + new Vector3(size.x, 0, size.z / 2f);
            PlaceWall(rightWallLocation, wallSizeX, Quaternion.Euler(0, 90, 0));
        }
    }
    void PlaceHallway(Vector3Int location)
    {
        // PlaceRoom(location, new Vector3Int(1, 1, 1));
        // PlaceCube(location, new Vector3Int(1, 1, 1), blueMaterial);

        bool isInPath(Vector3Int _loc, Vector3Int _location)
        {
            bool isPath = false;
            if (path == null)
            {
                Debug.Log("Path is null");
            }
            if (path != null && path.Contains(_location))
            {

                List<int> indices = new List<int>();
                for (int i = 0; i < path.Count; i++)
                {
                    if (path[i] == _location)
                        indices.Add(i);
                }

                foreach (int idx in indices)
                {
                    if (idx > 0)
                    {
                        Vector3Int prev = path[idx - 1];
                        if (prev == _loc)
                            isPath = true;
                        if (grid[prev] == CellType.Room)
                            isPath = true;
                    }
                    if (!isPath && idx < path.Count - 1)
                    {
                        Vector3Int next = path[idx + 1];
                        if (next == _loc)
                            isPath = true;
                    }
                }
            }
            return isPath;
        }
        bool cond(Vector3Int _loc, Vector3Int _location)
        {
            bool isPath = isInPath(_loc, _location);

            if (grid.InBounds(_loc) && ((grid[_loc] == CellType.None) || (grid[_loc] == CellType.Hallway && !isPath) || (grid[_loc] == CellType.Room && !isPath)))
            {
                return true;
            }
            return false;
        }

        Vector3Int size = new Vector3Int(1, 1, 1);
        // Place floor
        Vector3 floorSize = new Vector3(size.x / 4f, 1, size.z / 4f);
        Vector3 floorLocation = location + new Vector3(size.x / 2f, 0, size.z / 2f);
        PlaceFloor(floorLocation, floorSize);

        // Place ceiling
        Vector3 ceilingLocation = location + new Vector3(size.x / 2f, size.y, size.z / 2f);
        PlaceCeiling(ceilingLocation, floorSize);

        // Place walls
        Vector3 wallSizeX = new Vector3(size.z / 4f, size.y / 4f, 0.25f);
        Vector3 wallSizeZ = new Vector3(size.x / 4f, size.y / 4f, 0.25f);

        // Front wall
        Vector3Int loc = location + Vector3Int.forward;
        if (cond(loc, location))
        {
            Vector3 backWallLocation = location + new Vector3(size.x / 2f, 0, size.z);
            PlaceWall(backWallLocation, wallSizeZ, Quaternion.identity);
        }
        else if (!grid.InBounds(loc))
        {
            Vector3 backWallLocation = location + new Vector3(size.x / 2f, 0, size.z);
            PlaceWall(backWallLocation, wallSizeZ, Quaternion.identity);
        }
        else if (grid[loc] == CellType.Room && isInPath(loc, location))
        {
            Vector3 backWallLocation = location + new Vector3(size.x / 2f, 0, size.z);
            PlaceDoor(backWallLocation, Quaternion.identity, 0, 1);
        }

        // Back wall
        loc = location + Vector3Int.back;
        if (cond(loc, location))
        {
            Vector3 frontWallLocation = location + new Vector3(size.x / 2f, 0, 0);
            PlaceWall(frontWallLocation, wallSizeZ, Quaternion.Euler(0, 180, 0));
        }
        else if (!grid.InBounds(loc))
        {
            Vector3 frontWallLocation = location + new Vector3(size.x / 2f, 0, 0);
            PlaceWall(frontWallLocation, wallSizeZ, Quaternion.Euler(0, 180, 0));
        }
        else if (grid[loc] == CellType.Room && isInPath(loc, location))
        {
            Vector3 frontWallLocation = location + new Vector3(size.x / 2f, 0, 0);
            PlaceDoor(frontWallLocation, Quaternion.Euler(0, 180, 0), 0, -1);
        }

        // Left wall
        loc = location + Vector3Int.left;
        if (cond(loc, location))
        {
            Vector3 leftWallLocation = location + new Vector3(0, 0, size.z / 2f);
            PlaceWall(leftWallLocation, wallSizeX, Quaternion.Euler(0, -90, 0));
        }
        else if (!grid.InBounds(loc))
        {
            Vector3 leftWallLocation = location + new Vector3(0, 0, size.z / 2f);
            PlaceWall(leftWallLocation, wallSizeX, Quaternion.Euler(0, -90, 0));
        }
        else if (grid[loc] == CellType.Room && isInPath(loc, location))
        {
            Vector3 leftWallLocation = location + new Vector3(0, 0, size.z / 2f);
            PlaceDoor(leftWallLocation, Quaternion.Euler(0, -90, 0), 1, 0);
        }

        // Right wall
        loc = location + Vector3Int.right;
        if (cond(loc, location))
        {
            Vector3 rightWallLocation = location + new Vector3(size.x, 0, size.z / 2f);
            PlaceWall(rightWallLocation, wallSizeX, Quaternion.Euler(0, 90, 0));
        }
        else if (!grid.InBounds(loc))
        {
            Vector3 rightWallLocation = location + new Vector3(size.x, 0, size.z / 2f);
            PlaceWall(rightWallLocation, wallSizeX, Quaternion.Euler(0, 90, 0));
        }
        else if (grid[loc] == CellType.Room && isInPath(loc, location))
        {
            Vector3 rightWallLocation = location + new Vector3(size.x, 0, size.z / 2f);
            PlaceDoor(rightWallLocation, Quaternion.Euler(0, 90, 0), -1, 0);
        }
    }
    void PlaceStairs(Vector3 location, Vector3 offset, Quaternion rotation, int xDir, int zDir, Vector3Int delta)
    {
        GameObject parent = new GameObject("Stairs");
        parent.transform.position = location;

        Vector3 pos = new Vector3(0f, 0f, 0f);

        // Instanciamos la baldosa como hija del objeto padre
        GameObject tile = Instantiate(
            stairsPrefab,
            location,
            rotation,
            parent.transform
        );

        // Ajustamos escala: tileSize×size.y×tileSize
        tile.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        // Instanciamos la pieza con la misma rotación que la pared
        GameObject collider = Instantiate(
            colliderPrefab,
            location + Vector3.up * 0.5f,
            rotation,
            parent.transform
        );

        var factor = delta.y < 0 ? -1 : 1;
        collider.transform.localPosition = new Vector3(factor * xDir / 2f, 0.5f, factor * zDir / 2f);
        collider.transform.localScale = new Vector3(1f, 0.1f, 2.2360679775f);
        collider.transform.localRotation *= Quaternion.Euler(-28f, 0, 0);

        var size = new Vector3(0.25f, 0.25f, 0.25f);

        PlaceWall(location + new Vector3(factor * zDir / 2f, 0f, -factor * xDir / 2f), size, Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 90, rotation.eulerAngles.z));
        PlaceWall(location + new Vector3(factor * zDir / 2f, 1f, -factor * xDir / 2f), size, Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 90, rotation.eulerAngles.z));
        PlaceWall(location - new Vector3(factor * zDir / 2f, 0f, -factor * xDir / 2f), size, Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 90, rotation.eulerAngles.z));
        PlaceWall(location - new Vector3(factor * zDir / 2f, -1f, -factor * xDir / 2f), size, Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 90, rotation.eulerAngles.z));

        PlaceWall(location + new Vector3(factor * zDir / 2f + factor * xDir, 0f, -factor * xDir / 2f + factor * zDir), size, Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 90, rotation.eulerAngles.z));
        PlaceWall(location + new Vector3(factor * zDir / 2f + factor * xDir, 1f, -factor * xDir / 2f + factor * zDir), size, Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 90, rotation.eulerAngles.z));
        PlaceWall(location - new Vector3(factor * zDir / 2f - factor * xDir, 0f, -factor * xDir / 2f - factor * zDir), size, Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 90, rotation.eulerAngles.z));
        PlaceWall(location - new Vector3(factor * zDir / 2f - factor * xDir, -1f, -factor * xDir / 2f - factor * zDir), size, Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 90, rotation.eulerAngles.z));

        PlaceWall(location + new Vector3(-factor * xDir / 2f, 1f, -factor * zDir / 2f), size, rotation);
        PlaceWall(location + new Vector3(3 * factor * xDir / 2f, 0f, 3 * factor * zDir / 2f), size, rotation);

        PlaceCorner(location + new Vector3(-factor * zDir / 2f - factor * xDir / 2f, 1f, factor * xDir / 2f - factor * zDir / 2f), Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 90, rotation.eulerAngles.z));
        PlaceCorner(location + new Vector3(-factor * zDir / 2f + 3 * factor * xDir / 2f, 1f, factor * xDir / 2f + 3 * factor * zDir / 2f), Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 90, rotation.eulerAngles.z));

        PlaceCeiling(location + new Vector3(0f, 2f, 0f), size);
        PlaceCeiling(location + new Vector3(factor * xDir, 2f, factor * zDir), size);

        PlaceFloor(location + new Vector3(0f, 0f, 0f), size);
        PlaceFloor(location + new Vector3(factor * xDir, 0f, factor * zDir), size);

    }
    void PlaceColumn(Vector3 location)
    {
        GameObject parent = new GameObject("Column");
        parent.transform.position = location;

        GameObject prefabToUse = null;
        if (columnPrefabs != null && columnPrefabs.Length > 0)
        {
            var idx = random.Next(0, columnPrefabs.Length);
            prefabToUse = columnPrefabs[idx];
        }

        // Instanciamos la baldosa como hija del objeto padre
        GameObject tile = Instantiate(
            prefabToUse,
            location,
            Quaternion.identity,
            parent.transform
        );

        // Ajustamos escala: tileSize×size.y×tileSize
        tile.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
    }

    void PlaceDoor(Vector3 location, Quaternion rotation, int xDir, int zDir)
    {
        GameObject parent = new GameObject("Door");
        parent.transform.position = location;

        GameObject prefabToUse = null;
        if (doorPrefabs != null && doorPrefabs.Length > 0)
        {
            var idx = random.Next(0, doorPrefabs.Length);
            prefabToUse = doorPrefabs[idx];
        }

        // Instanciamos la baldosa como hija del objeto padre
        GameObject tile = Instantiate(
            prefabToUse,
            location,
            rotation,
            parent.transform
        );

        // Ajustamos escala: tileSize×size.y×tileSize
        tile.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        // Instanciamos la pieza con la misma rotación que la pared
        GameObject collider = Instantiate(
            colliderPrefab,
            location + Vector3.up * 0.5f,
            rotation,
            parent.transform
        );

        // Escalamos: ancho=tileWidth, alto=size.y, grosor=size.z
        collider.transform.localScale = new Vector3(0.3f, 1f, 0.2f);
        collider.transform.localPosition = new Vector3(-0.31f * zDir, 0.5f, -0.31f * xDir);

        // Instanciamos la pieza con la misma rotación que la pared
        GameObject collider2 = Instantiate(
            colliderPrefab,
            location + Vector3.up * 0.5f,
            rotation,
            parent.transform
        );

        // Escalamos: ancho=tileWidth, alto=size.y, grosor=size.z
        collider2.transform.localScale = new Vector3(0.3f, 1f, 0.2f);
        collider2.transform.localPosition = new Vector3(0.31f * zDir, 0.5f, 0.31f * xDir);
    }

    void PlaceCorner(Vector3 location, Quaternion rotation)
    {
        GameObject parent = new GameObject("Corner");
        parent.transform.position = location;

        GameObject prefabToUse = null;
        if (cornerPrefabs != null && cornerPrefabs.Length > 0)
        {
            var idx = random.Next(0, cornerPrefabs.Length);
            prefabToUse = cornerPrefabs[idx];
        }

        // Instanciamos la baldosa como hija del objeto padre
        GameObject tile = Instantiate(
            prefabToUse,
            location,
            rotation,
            parent.transform
        );

        // Ajustamos escala: tileSize×size.y×tileSize
        tile.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
    }

    void RenderGrids()
    {
        // iterar por todo grid[] comprobando que no sea CellType.None
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    if (grid[pos] == CellType.Room)
                    {
                        PlaceRoom(pos, new Vector3Int(1, 1, 1));
                    }
                    else if (grid[pos] == CellType.Hallway)
                    {
                        PlaceHallway(pos);
                    }
                    else if (grid[pos] == CellType.None)
                    {
                        // PlaceCube(pos, new Vector3Int(1, 1, 1), redMaterial);
                    }
                }
            }
        }
    }

    void PutPillars()
    {
        // Encuentra todas las instancias de wallPrefab en la escena
        GameObject[] allWalls = GameObject.FindObjectsOfType<GameObject>();
        List<GameObject> wallList = new List<GameObject>();
        foreach (var go in allWalls)
        {
            if (go.name.Contains("Env_Wall"))
            {
                wallList.Add(go);
            }
        }
        allWalls = wallList.ToArray();
        HashSet<Vector3> pillarPositions = new HashSet<Vector3>();

        foreach (GameObject wall in allWalls)
        {
            Vector3 wallPos = wall.transform.position;
            Vector3 wallScale = wall.transform.localScale;
            Quaternion wallRot = wall.transform.rotation;

            List<Vector3> localCorners = new List<Vector3>();
            if (wallRot != Quaternion.identity && wallRot != Quaternion.Euler(0, 180, 0))
            {
                localCorners.Add(new Vector3(wallScale.x, wallScale.y, wallScale.z - 0.5f));
                localCorners.Add(new Vector3(wallScale.x, wallScale.y, wallScale.z + 0.5f));
            }
            else
            {
                localCorners.Add(new Vector3(wallScale.x - 0.5f, wallScale.y, wallScale.z));
                localCorners.Add(new Vector3(wallScale.x + 0.5f, wallScale.y, wallScale.z));
            }

            // Para cada esquina, comprobamos si hay otra pared cerca
            foreach (var localCorner in localCorners)
            {
                Vector3 worldCorner = wall.transform.TransformPoint(localCorner);

                // Comprobamos si hay otra pared cerca de esta esquina (en un radio pequeño)
                bool hasNeighbor = false;
                foreach (GameObject otherWall in allWalls)
                {
                    if (otherWall == wall) continue;
                    float dist = Vector3.Distance(worldCorner, otherWall.transform.position);
                    // Si la distancia es menor que la mitad de la escala de la pared y tiene la misma orientación, consideramos que hay una pared vecina
                    if (dist < 0.4f && Mathf.Abs(Quaternion.Angle(wall.transform.rotation, otherWall.transform.rotation)) < 1f)
                    {
                        hasNeighbor = true;
                        break;
                    }
                }
                if (!hasNeighbor)
                {
                    // Añadimos la posición de la esquina como posible pilar
                    pillarPositions.Add(worldCorner);
                }
            }
        }

        // Instanciamos un cubo en cada posición única de pilar
        foreach (var pos in pillarPositions)
        {
            // Debug.Log("Pillar at: " + pos);
            PlaceColumn(Vector3Int.RoundToInt(pos));
        }
    }
}
