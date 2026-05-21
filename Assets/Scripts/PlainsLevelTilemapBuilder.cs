using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlainsLevelTilemapBuilder : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private TextAsset layoutCsv;
    [SerializeField] private bool buildOnStart = true;
    [SerializeField] private bool clearBeforeBuild = true;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap decorationTilemap;
    [SerializeField] private Tilemap blockedObjectTilemap;
    [SerializeField] private Tilemap endPointTilemap;

    [Header("Ground Tiles")]
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase flowerGrassTile;
    [SerializeField] private TileBase dirtPathTile;
    [SerializeField] private TileBase encounterGroundTile;
    [SerializeField] private TileBase startGroundTile;

    [Header("Blocked Object Placeholder Tiles")]
    [SerializeField] private TileBase treeTile;
    [SerializeField] private TileBase cottageWreckageTile;
    [SerializeField] private TileBase ruinTile;
    [SerializeField] private TileBase debrisTile;
    [SerializeField] private TileBase stoneWallTile;
    [SerializeField] private TileBase rockTile;
    [SerializeField] private TileBase fenceTile;

    [Header("Endpoint")]
    [SerializeField] private TileBase endPointTile;

    private readonly HashSet<char> blockedCodes = new HashSet<char> { 'T', 'C', 'R', 'B', 'W', 'K', 'F' };

    private void Start()
    {
        if (buildOnStart) BuildLevel();
    }

    [ContextMenu("Build Level From CSV")]
    public void BuildLevel()
    {
        if (layoutCsv == null)
        {
            Debug.LogError("No layout CSV assigned.");
            return;
        }

        if (clearBeforeBuild)
        {
            groundTilemap?.ClearAllTiles();
            decorationTilemap?.ClearAllTiles();
            blockedObjectTilemap?.ClearAllTiles();
            endPointTilemap?.ClearAllTiles();
        }

        string[] rows = layoutCsv.text.Trim().Split('\n');
        int rowCount = rows.Length;

        for (int csvY = 0; csvY < rowCount; csvY++)
        {
            string[] cells = rows[csvY].Trim().Split(',');
            for (int x = 0; x < cells.Length; x++)
            {
                if (string.IsNullOrWhiteSpace(cells[x])) continue;

                char code = cells[x].Trim()[0];
                int y = rowCount - 1 - csvY; // invert CSV rows so row 0 appears at top in the scene
                Vector3Int pos = new Vector3Int(x, y, 0);

                PaintCell(pos, code);
            }
        }

        Debug.Log("Plains level built from CSV.");
    }

    private void PaintCell(Vector3Int pos, char code)
    {
        // Always paint a walkable ground underneath, even under blocked objects.
        TileBase ground = GetGroundTile(code);
        if (groundTilemap != null && ground != null)
            groundTilemap.SetTile(pos, ground);

        if (endPointTilemap != null && code == 'X' && endPointTile != null)
            endPointTilemap.SetTile(pos, endPointTile);

        if (blockedObjectTilemap != null && blockedCodes.Contains(code))
        {
            TileBase blockedTile = GetBlockedTile(code);
            if (blockedTile != null)
                blockedObjectTilemap.SetTile(pos, blockedTile);
        }
    }

    private TileBase GetGroundTile(char code)
    {
        switch (code)
        {
            case 'P': return dirtPathTile != null ? dirtPathTile : grassTile;
            case 'E': return encounterGroundTile != null ? encounterGroundTile : dirtPathTile != null ? dirtPathTile : grassTile;
            case 'S': return startGroundTile != null ? startGroundTile : grassTile;
            case 'L': return flowerGrassTile != null ? flowerGrassTile : grassTile;
            default: return grassTile;
        }
    }

    private TileBase GetBlockedTile(char code)
    {
        switch (code)
        {
            case 'T': return treeTile;
            case 'C': return cottageWreckageTile;
            case 'R': return ruinTile;
            case 'B': return debrisTile;
            case 'W': return stoneWallTile;
            case 'K': return rockTile;
            case 'F': return fenceTile;
            default: return null;
        }
    }
}
