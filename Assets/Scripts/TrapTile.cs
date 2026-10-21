using System.Collections;
using UnityEngine;

/// <summary>
/// A floor spike trap tile that damages units when stepped on while active.
/// Can be retracted/deactivated by a linked ToggleWall or PressurePlate.
/// </summary>
public class TrapTile : MonoBehaviour
{
    [Header("Grid Position")]
    public Vector2Int gridPos;
    public int damage = 1;

    [Header("Sprites")]
    public Sprite activeSpikesSprite;   // tilemap_packed_69 (extended)
    public Sprite retractedSpikesSprite; // tilemap_packed_68 (flat/safe)

    [Header("State")]
    public bool isRetracted = false;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SnapToGrid();
    }

    private void Start()
    {
        ResetTrap();
    }

    public void SnapToGrid()
    {
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.GridToWorld(gridPos);
        }
    }

    public void ResetTrap()
    {
        isRetracted = false;
        UpdateVisual();
    }

    public void SetRetracted(bool retracted)
    {
        isRetracted = retracted;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (isRetracted)
            {
                if (retractedSpikesSprite != null) spriteRenderer.sprite = retractedSpikesSprite;
                spriteRenderer.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
            }
            else
            {
                if (activeSpikesSprite != null) spriteRenderer.sprite = activeSpikesSprite;
                spriteRenderer.color = Color.white;
            }
        }
    }

    public void CheckUnitTrigger(Unit unit)
    {
        if (unit == null || !unit.gameObject.activeInHierarchy) return;
        if (isRetracted) return;

        if (unit.GridPosition == gridPos)
        {
            if (!unit.isEcho)
            {
                Debug.Log("<color=red>Live player stepped on active spikes! Taking damage...</color>");
                if (PlayerHealth.Instance != null)
                {
                    PlayerHealth.Instance.TakeDamage(damage);
                }
            }
            else
            {
                Debug.Log("<color=yellow>Echo triggered spikes.</color>");
            }
        }
    }
}
