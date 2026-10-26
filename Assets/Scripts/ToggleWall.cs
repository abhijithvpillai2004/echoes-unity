using UnityEngine;

/// <summary>
/// A wall that can be opened/closed by a PressurePlate.
/// Add all wall GameObjects to wallObjects list — all hide/show together.
/// </summary>
public class ToggleWall : MonoBehaviour
{
    [Header("Config")]
    public Vector2Int[] wallCells;
    public GameObject[] wallObjects; // drag Wall + Wall2 here

    private bool isOpen = false;

    private void Start()
    {
        Close();
    }

    public void ResetWall()
    {
        Close();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        if (wallObjects != null)
        {
            foreach (GameObject obj in wallObjects)
                if (obj != null) obj.SetActive(false);
        }

        if (GridManager.Instance != null && wallCells != null)
        {
            foreach (Vector2Int cell in wallCells)
                GridManager.Instance.SetWalkable(cell, true);
        }

        Debug.Log("Wall opened!");
    }

    public void Close()
    {
        isOpen = false;

        if (wallObjects != null)
        {
            foreach (GameObject obj in wallObjects)
                if (obj != null) obj.SetActive(true);
        }

        if (GridManager.Instance != null && wallCells != null)
        {
            foreach (Vector2Int cell in wallCells)
                GridManager.Instance.SetWalkable(cell, false);
        }

        Debug.Log("Wall closed!");
    }

    public bool IsOpen() => isOpen;
}
