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
    [Header("Timing")]
    public float openDelay = 0f;

    private bool isOpen = false;

    private void Start()
    {
        Close(false);
    }

    public void ResetWall()
    {
        Close(false);
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        if (GridManager.Instance != null && wallCells != null)
        {
            foreach (Vector2Int cell in wallCells)
                GridManager.Instance.SetWalkable(cell, true);
        }

        if (openDelay > 0f && gameObject.activeInHierarchy)
        {
            StartCoroutine(DelayedOpenVisualAndAudio(openDelay));
        }
        else
        {
            ApplyOpenVisualAndAudio();
        }
    }

    private System.Collections.IEnumerator DelayedOpenVisualAndAudio(float delay)
    {
        yield return new WaitForSeconds(delay);
        ApplyOpenVisualAndAudio();
    }

    private void ApplyOpenVisualAndAudio()
    {
        if (!isOpen) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayDoorOpen();

        if (wallObjects != null)
        {
            foreach (GameObject obj in wallObjects)
                if (obj != null) obj.SetActive(false);
        }

        Debug.Log("Wall opened!");
    }

    public void Close(bool playSound = true)
    {
        StopAllCoroutines();
        bool wasOpen = isOpen;
        isOpen = false;

        if (wasOpen && playSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDoorClose();
        }

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
