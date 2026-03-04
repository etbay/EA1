using UnityEngine;
using System.Collections.Generic;

public class PaintTracker : MonoBehaviour
{
    private static PaintTracker _instance;
    public static PaintTracker Instance => _instance;

    // Spatial hash: stores painted grid cells per surface
    private Dictionary<Collider, HashSet<Vector3Int>> paintedCells = new Dictionary<Collider, HashSet<Vector3Int>>();
    private const float CELL_SIZE = 0.25f; // 25cm cells for fine granularity

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void RegisterPaint(Collider surface, Vector3 worldPosition, float radius)
    {
        if (surface == null) return;

        if (!paintedCells.ContainsKey(surface))
        {
            paintedCells[surface] = new HashSet<Vector3Int>();
        }

        // Mark all cells within radius as painted
        int cellRadius = Mathf.CeilToInt(radius / CELL_SIZE);
        Vector3Int centerCell = WorldToCell(worldPosition);

        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int z = -cellRadius; z <= cellRadius; z++)
            {
                Vector3Int cell = centerCell + new Vector3Int(x, 0, z);
                if (Vector3.Distance(CellToWorld(cell), worldPosition) <= radius)
                {
                    paintedCells[surface].Add(cell);
                }
            }
        }
    }

    public bool IsPainted(Collider surface, Vector3 worldPosition, float checkRadius = 0.3f)
    {
        if (surface == null || !paintedCells.ContainsKey(surface))
            return false;

        // Check if any cells within checkRadius are painted
        int cellRadius = Mathf.CeilToInt(checkRadius / CELL_SIZE);
        Vector3Int centerCell = WorldToCell(worldPosition);

        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int z = -cellRadius; z <= cellRadius; z++)
            {
                Vector3Int cell = centerCell + new Vector3Int(x, 0, z);
                if (paintedCells[surface].Contains(cell))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private Vector3Int WorldToCell(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x / CELL_SIZE),
            Mathf.FloorToInt(worldPos.y / CELL_SIZE),
            Mathf.FloorToInt(worldPos.z / CELL_SIZE)
        );
    }

    private Vector3 CellToWorld(Vector3Int cell)
    {
        return new Vector3(
            cell.x * CELL_SIZE + CELL_SIZE * 0.5f,
            cell.y * CELL_SIZE,
            cell.z * CELL_SIZE + CELL_SIZE * 0.5f
        );
    }

    public void ClearPaint(Collider surface)
    {
        if (paintedCells.ContainsKey(surface))
        {
            paintedCells[surface].Clear();
        }
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}