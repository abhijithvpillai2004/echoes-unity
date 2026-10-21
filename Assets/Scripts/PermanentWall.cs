using UnityEngine;

/// <summary>
/// Registers this wall's grid cell as permanently unwalkable in GridManager.
/// Attach to any static wall object.
/// </summary>
public class PermanentWall : MonoBehaviour
{
    public Vector2Int[] wallCells;

    private void Start()
    {
        foreach (Vector2Int cell in wallCells)
            GridManager.Instance.SetWalkable(cell, false);
    }
}
