using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
  // Grid dimensions
  public int width = 2;
  public int height = 2;

  // Tile data
  [System.Serializable]
  public class Tile
  {
    public string name;
    public Sprite sprite;
    public List<string> compatibleNeighbors;
  }

  public List<Tile> tiles;

  // Internal grid representation
  private Tile[,] grid;

  public void Start()
  {
    InitializeGrid();
    GenerateMap();
  }

  // Initialize the grid with default values
  private void InitializeGrid()
  {
    grid = new Tile[width, height];
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        grid[x, y] = null; // Start with all cells unresolved
      }
    }
  }

  // Main function to generate the map
  private void GenerateMap()
  {
    int maxIterations = width * height * 10; // Set a reasonable limit for iterations
    int iterationCount = 0;

    while (!IsGenerationComplete())
    {
      if (iterationCount >= maxIterations)
      {
        Debug.LogWarning("Map generation stopped due to reaching the iteration limit.");
        break;
      }

      CollapseNextCell();
      iterationCount++;
    }
  }

  // Check if the map generation is complete
  private bool IsGenerationComplete()
  {
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        if (grid[x, y] == null)
        {
          return false; // If any cell is unresolved, generation is not complete
        }
      }
    }
    return true;
  }

  // Collapse the next cell in the grid
  private void CollapseNextCell()
  {
    // Find the first unresolved cell
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        if (grid[x, y] == null)
        {
          List<Tile> compatibleTiles = GetCompatibleTiles(x, y);
          if (compatibleTiles.Count > 0)
          {
            // Randomly select a compatible tile
            grid[x, y] = compatibleTiles[Random.Range(0, compatibleTiles.Count)];
          }
          return;
        }
      }
    }
  }

  // Get compatible tiles for a given cell
  private List<Tile> GetCompatibleTiles(int x, int y)
  {
    List<Tile> compatibleTiles = new List<Tile>();

    foreach (Tile tile in tiles)
    {
      bool isCompatible = true;

      // Check compatibility with neighbors
      if (x > 0 && grid[x - 1, y] != null) // Left neighbor
      {
        if (!grid[x - 1, y].compatibleNeighbors.Contains(tile.name))
        {
          isCompatible = false;
        }
      }
      if (x < width - 1 && grid[x + 1, y] != null) // Right neighbor
      {
        if (!grid[x + 1, y].compatibleNeighbors.Contains(tile.name))
        {
          isCompatible = false;
        }
      }
      if (y > 0 && grid[x, y - 1] != null) // Bottom neighbor
      {
        if (!grid[x, y - 1].compatibleNeighbors.Contains(tile.name))
        {
          isCompatible = false;
        }
      }
      if (y < height - 1 && grid[x, y + 1] != null) // Top neighbor
      {
        if (!grid[x, y + 1].compatibleNeighbors.Contains(tile.name))
        {
          isCompatible = false;
        }
      }

      if (isCompatible)
      {
        compatibleTiles.Add(tile);
      }
    }

    return compatibleTiles;
  }

  // Debug function to visualize the grid
  private void DebugDrawGrid()
  {
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        if (grid[x, y] != null)
        {
          Debug.Log($"Cell ({x}, {y}): {grid[x, y].name}");
        }
        else
        {
          Debug.Log($"Cell ({x}, {y}): unresolved");
        }
      }
    }
  }
}