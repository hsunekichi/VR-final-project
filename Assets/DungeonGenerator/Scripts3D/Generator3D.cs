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
        public BoundsInt bounds;

        public Room(Vector3Int location, Vector3Int size)
        {
            bounds = new BoundsInt(location, size);
        }

        public static bool Intersect(Room a, Room b)
        {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y)
                || (a.bounds.position.z >= (b.bounds.position.z + b.bounds.size.z)) || ((a.bounds.position.z + a.bounds.size.z) <= b.bounds.position.z));
        }
    }

    [SerializeField]
    Vector3Int size;
    [SerializeField]
    int roomCount;
    [SerializeField]
    Vector3Int roomMaxSize;
    [SerializeField]
    GameObject cubePrefab;
    [SerializeField]
    GameObject floorPrefab;
    [SerializeField]
    GameObject ceilingPrefab;
    [SerializeField]
    GameObject wallPrefab;
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

    void Start()
    {
        random = new Random(0);
        grid = new Grid3D<CellType>(size, Vector3Int.zero);
        rooms = new List<Room>();

        PlaceRooms();
        Triangulate();
        CreateHallways();
        PathfindHallways();
        RenderGrids();
    }

    void PlaceRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            Vector3Int location = new Vector3Int(
                random.Next(0, size.x),
                random.Next(0, size.y),
                random.Next(0, size.z)
            );

            Vector3Int roomSize = new Vector3Int(
                random.Next(1, roomMaxSize.x + 1),
                random.Next(1, roomMaxSize.y + 1),
                random.Next(1, roomMaxSize.z + 1)
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

            if (newRoom.bounds.xMin < 0 || newRoom.bounds.xMax >= size.x
                || newRoom.bounds.yMin < 0 || newRoom.bounds.yMax >= size.y
                || newRoom.bounds.zMin < 0 || newRoom.bounds.zMax >= size.z)
            {
                add = false;
            }

            if (add)
            {
                rooms.Add(newRoom);
                // PlaceRoom(newRoom.bounds.position, newRoom.bounds.size);

                foreach (var pos in newRoom.bounds.allPositionsWithin)
                {
                    grid[pos] = CellType.Room;
                }
            }
        }
    }

    void Triangulate()
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in rooms)
        {
            vertices.Add(new Vertex<Room>((Vector3)room.bounds.position + ((Vector3)room.bounds.size) / 2, room));
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
    }

    void PathfindHallways()
    {
        DungeonPathfinder3D aStar = new DungeonPathfinder3D(size);

        foreach (var edge in selectedEdges)
        {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector3Int((int)startPosf.x, (int)startPosf.y, (int)startPosf.z);
            var endPos = new Vector3Int((int)endPosf.x, (int)endPosf.y, (int)endPosf.z);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder3D.Node a, DungeonPathfinder3D.Node b) =>
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
                    else if (grid[b.Position] == CellType.None)
                    {
                        pathCost.cost += 1;
                    }

                    pathCost.traversable = true;
                }
                else
                {
                    //staircase
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

                            // PlaceStairs(prev + horizontalOffset);
                            // PlaceStairs(prev + horizontalOffset * 2);
                            // PlaceStairs(prev + verticalOffset + horizontalOffset);
                            // PlaceStairs(prev + verticalOffset + horizontalOffset * 2);
                        }

                        Debug.DrawLine(prev + new Vector3(0.5f, 0.5f, 0.5f), current + new Vector3(0.5f, 0.5f, 0.5f), Color.blue, 100, false);
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
    }

    void PlaceCube(Vector3Int location, Vector3Int size, Material material)
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
        Vector3 totalOffset = new Vector3(countX * tileSize, 0f, countZ * tileSize)*2;
        Vector3 startPos = location - totalOffset + new Vector3(tileSize*2, 0f, tileSize*2);

        // Instanciamos cada baldosa en la cuadrícula
        for (int x = 0; x < countX; x++)
        {
            for (int z = 0; z < countZ; z++)
            {
                // Posición de la baldosa actual
                Vector3 tilePos = startPos + new Vector3(x, 0f, z);

                // Instanciamos la baldosa como hija del objeto padre
                GameObject tile = Instantiate(
                    floorPrefab,
                    tilePos,
                    Quaternion.identity,
                    parent.transform
                );

                // Ajustamos escala: tileSize×size.y×tileSize
                tile.transform.localScale = new Vector3(tileSize, size.y, tileSize);
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
        Vector3 totalOffset = new Vector3(countX * tileSize, 0f, countZ * tileSize)*2;
        Vector3 startPos = location - totalOffset + new Vector3(tileSize*2, 0f, tileSize*2);

        // Instanciamos cada baldosa en la cuadrícula
        for (int x = 0; x < countX; x++)
        {
            for (int z = 0; z < countZ; z++)
            {
                // Posición de la baldosa actual
                Vector3 tilePos = startPos + new Vector3(x, 0f, z);

                // Instanciamos la baldosa como hija del objeto padre
                GameObject tile = Instantiate(
                    ceilingPrefab,
                    tilePos,
                    Quaternion.identity,
                    parent.transform
                );

                // Ajustamos escala: tileSize×size.y×tileSize
                tile.transform.localScale = new Vector3(tileSize, size.y, tileSize);
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
        float halfWidth    = size.x*2;
        float startOffset  = -halfWidth + (tileWidth*2);

        // 3) Instanciamos cada columna a lo largo del eje elegido
        for (int i = 0; i < countColumns; i++)
        {
            // Calculamos posición en mundo: centro + axis * (offset + paso)
            float step = startOffset + i * tileWidth*4;
            Vector3 worldPos = location + axis * step;

            // Instanciamos la pieza con la misma rotación que la pared
            GameObject piece = Instantiate(
                wallPrefab,
                worldPos,
                rotation,
                parent.transform
            );

            // Escalamos: ancho=tileWidth, alto=size.y, grosor=size.z
            piece.transform.localScale = new Vector3(tileWidth, size.y, size.z);
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
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Hallway || grid[loc] == CellType.Room))
        {
            Vector3 backWallLocation = location + new Vector3(size.x / 2f, 0, size.z);
            PlaceWall(backWallLocation, wallSizeZ, Quaternion.identity);
        } else if(!grid.InBounds(loc))
        {
            Vector3 backWallLocation = location + new Vector3(size.x / 2f, 0, size.z);
            PlaceWall(backWallLocation, wallSizeZ, Quaternion.identity);
        }

        // Back wall
        loc = location + Vector3Int.back;
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Hallway || grid[loc] == CellType.Room))
        {
            Vector3 frontWallLocation = location + new Vector3(size.x / 2f, 0, 0);
            PlaceWall(frontWallLocation, wallSizeZ, Quaternion.Euler(0, 180, 0));
        } else if(!grid.InBounds(loc))
        {
            Vector3 frontWallLocation = location + new Vector3(size.x / 2f, 0, 0);
            PlaceWall(frontWallLocation, wallSizeZ, Quaternion.Euler(0, 180, 0));

        }

        // Left wall
        loc = location + Vector3Int.left;
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Hallway || grid[loc] == CellType.Room))
        {
            Vector3 leftWallLocation = location + new Vector3(0, 0, size.z / 2f);
            PlaceWall(leftWallLocation, wallSizeX, Quaternion.Euler(0, -90, 0));
        } else if(!grid.InBounds(loc))
        {
            Vector3 leftWallLocation = location + new Vector3(0, 0, size.z / 2f);
            PlaceWall(leftWallLocation, wallSizeX, Quaternion.Euler(0, -90, 0));
        }

        // Right wall
        loc = location + Vector3Int.right;
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Hallway || grid[loc] == CellType.Room))
        {
            Vector3 rightWallLocation = location + new Vector3(size.x, 0, size.z / 2f);
            PlaceWall(rightWallLocation, wallSizeX, Quaternion.Euler(0, 90, 0));
        } else if(!grid.InBounds(loc))
        {
            Vector3 rightWallLocation = location + new Vector3(size.x, 0, size.z / 2f);
            PlaceWall(rightWallLocation, wallSizeX, Quaternion.Euler(0, 90, 0));
        }
    }
    void PlaceHallway(Vector3Int location)
    {
        // PlaceRoom(location, new Vector3Int(1, 1, 1));
        // PlaceCube(location, new Vector3Int(1, 1, 1), blueMaterial);
        
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
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Hallway || grid[loc] == CellType.Stairs || grid[loc] == CellType.Room))
        {
            Vector3 backWallLocation = location + new Vector3(size.x / 2f, 0, size.z);
            PlaceWall(backWallLocation, wallSizeZ, Quaternion.identity);
        } else if(!grid.InBounds(loc))
        {
            Vector3 backWallLocation = location + new Vector3(size.x / 2f, 0, size.z);
            PlaceWall(backWallLocation, wallSizeZ, Quaternion.identity);
        }

        // Back wall
        loc = location + Vector3Int.back;
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Hallway || grid[loc] == CellType.Stairs || grid[loc] == CellType.Room))
        {
            Vector3 frontWallLocation = location + new Vector3(size.x / 2f, 0, 0);
            PlaceWall(frontWallLocation, wallSizeZ, Quaternion.Euler(0, 180, 0));
        } else if(!grid.InBounds(loc))
        {
            Vector3 frontWallLocation = location + new Vector3(size.x / 2f, 0, 0);
            PlaceWall(frontWallLocation, wallSizeZ, Quaternion.Euler(0, 180, 0));
        }

        // Left wall
        loc = location + Vector3Int.left;
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Hallway || grid[loc] == CellType.Stairs || grid[loc] == CellType.Room))
        {
            Vector3 leftWallLocation = location + new Vector3(0, 0, size.z / 2f);
            PlaceWall(leftWallLocation, wallSizeX, Quaternion.Euler(0, -90, 0));
        } else if(!grid.InBounds(loc))
        {
            Vector3 leftWallLocation = location + new Vector3(0, 0, size.z / 2f);
            PlaceWall(leftWallLocation, wallSizeX, Quaternion.Euler(0, -90, 0));
        }

        // Right wall
        loc = location + Vector3Int.right;
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Hallway || grid[loc] == CellType.Stairs || grid[loc] == CellType.Room))
        {
            Vector3 rightWallLocation = location + new Vector3(size.x, 0, size.z / 2f);
            PlaceWall(rightWallLocation, wallSizeX, Quaternion.Euler(0, 90, 0));
        } else if(!grid.InBounds(loc))
        {
            Vector3 rightWallLocation = location + new Vector3(size.x, 0, size.z / 2f);
            PlaceWall(rightWallLocation, wallSizeX, Quaternion.Euler(0, 90, 0));
        }
    }

    void PlaceStairs(Vector3Int location)
    {
        // PlaceCube(location, new Vector3Int(1, 1, 1), greenMaterial);
        
        Vector3Int size = new Vector3Int(1, 1, 1);
        // Place floor
        Vector3 floorSize = new Vector3(size.x / 4f, 1, size.z / 4f);
        Vector3 floorLocation = location + new Vector3(size.x / 2f, 0, size.z / 2f);
        
        Vector3Int loc = location + Vector3Int.down;
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Stairs))
        {
            PlaceFloor(floorLocation, floorSize);
        } else if(!grid.InBounds(loc))
        {
            PlaceFloor(floorLocation, floorSize);
        }

        // Place ceiling
        loc = location + Vector3Int.up;
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Stairs))
        {
            Vector3 ceilingLocation = location + new Vector3(size.x / 2f, size.y, size.z / 2f);
            PlaceCeiling(ceilingLocation, floorSize);
        } else if(!grid.InBounds(loc))
        {
            Vector3 ceilingLocation = location + new Vector3(size.x / 2f, size.y, size.z / 2f);
            PlaceCeiling(ceilingLocation, floorSize);
        }
        
        // Place walls
        Vector3 wallSizeX = new Vector3(size.z / 4f, size.y / 4f, 0.25f);
        Vector3 wallSizeZ = new Vector3(size.x / 4f, size.y / 4f, 0.25f);

        // Front wall
        loc = location + Vector3Int.forward;
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Stairs))
        {
            Vector3 backWallLocation = location + new Vector3(size.x / 2f, 0, size.z);
            PlaceWall(backWallLocation, wallSizeZ, Quaternion.identity);
        } else if(!grid.InBounds(loc))
        {
            Vector3 backWallLocation = location + new Vector3(size.x / 2f, 0, size.z);
            PlaceWall(backWallLocation, wallSizeZ, Quaternion.identity);
        }

        // Back wall
        loc = location + Vector3Int.back;
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Stairs))
        {
            Vector3 frontWallLocation = location + new Vector3(size.x / 2f, 0, 0);
            PlaceWall(frontWallLocation, wallSizeZ, Quaternion.Euler(0, 180, 0));
        } else if(!grid.InBounds(loc))
        {
            Vector3 frontWallLocation = location + new Vector3(size.x / 2f, 0, 0);
            PlaceWall(frontWallLocation, wallSizeZ, Quaternion.Euler(0, 180, 0));
        }

        // Left wall
        loc = location + Vector3Int.left;
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Stairs))
        {
            Vector3 leftWallLocation = location + new Vector3(0, 0, size.z / 2f);
            PlaceWall(leftWallLocation, wallSizeX, Quaternion.Euler(0, -90, 0));
        } else if(!grid.InBounds(loc))
        {
            Vector3 leftWallLocation = location + new Vector3(0, 0, size.z / 2f);
            PlaceWall(leftWallLocation, wallSizeX, Quaternion.Euler(0, -90, 0));
        }

        // Right wall
        loc = location + Vector3Int.right;
        if(grid.InBounds(loc) && !(grid[loc] == CellType.Stairs))
        {
            Vector3 rightWallLocation = location + new Vector3(size.x, 0, size.z / 2f);
            PlaceWall(rightWallLocation, wallSizeX, Quaternion.Euler(0, 90, 0));
        } else if(!grid.InBounds(loc))
        {
            Vector3 rightWallLocation = location + new Vector3(size.x, 0, size.z / 2f);
            PlaceWall(rightWallLocation, wallSizeX, Quaternion.Euler(0, 90, 0));
        }
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
                    else if (grid[pos] == CellType.Stairs)
                    {
                        PlaceStairs(pos);
                    }
                    else if (grid[pos] == CellType.None)
                    {
                        // PlaceCube(pos, new Vector3Int(1, 1, 1), redMaterial);
                    }
                }
            }
        }
    }
}
