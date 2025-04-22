using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapseSetup : MonoBehaviour
{
    private WaveFunctionCollapse waveFunctionCollapse;

    void Awake()
    {
        // Obtén el componente WaveFunctionCollapse del mismo objeto
        waveFunctionCollapse = GetComponent<WaveFunctionCollapse>();

        // Asegúrate de que el componente esté asignado
        if (waveFunctionCollapse == null)
        {
            Debug.LogError("WaveFunctionCollapse no está asignado al objeto.");
        }
    }

    void Start()
    {
        if (waveFunctionCollapse == null)
        {
            return;
        }

        // Asegúrate de que la lista de tiles esté inicializada
        if (waveFunctionCollapse.tiles == null)
        {
            waveFunctionCollapse.tiles = new List<WaveFunctionCollapse.Tile>();
        }

        // Configura los tiles
        waveFunctionCollapse.tiles = new List<WaveFunctionCollapse.Tile>
        {
            new WaveFunctionCollapse.Tile
            {
            name = "chest_simple",
            sprite = Resources.Load<Sprite>("ChestFree/Textures/chest_simple") ?? throw new System.Exception("Sprite 'chest_simple' not found"),
            compatibleNeighbors = new List<string> { "chest_simple", "Path" }
            },
            new WaveFunctionCollapse.Tile
            {
            name = "coins",
            sprite = Resources.Load<Sprite>("ChestFree/Textures/coins") ?? throw new System.Exception("Sprite 'coins' not found"),
            compatibleNeighbors = new List<string> { "coins", "Path" }
            }
        };

        // Llama a la generación del mapa
        try
        {
            waveFunctionCollapse.Start();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during WaveFunctionCollapse.Start(): {ex.Message}");
        }
    }
}
