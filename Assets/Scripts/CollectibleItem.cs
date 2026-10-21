using UnityEngine;

public enum ItemType
{
    Weapon,
    PoisonBottle
}

public class CollectibleItem : MonoBehaviour
{
    public ItemType itemType = ItemType.Weapon;
    public Vector2Int gridPos;

    private bool isCollected = false;
    public bool IsCollected => isCollected;

    private void Start()
    {
        SnapToGrid();
    }

    public void SnapToGrid()
    {
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.GridToWorld(gridPos);
        }
    }

    public void ResetItem()
    {
        isCollected = false;
        gameObject.SetActive(true);
        SnapToGrid();
    }

    public bool TryCollect(Unit unit)
    {
        if (isCollected) return false;

        // Check if unit is on same tile or adjacent
        int dist = Mathf.Abs(unit.GridPosition.x - gridPos.x) + Mathf.Abs(unit.GridPosition.y - gridPos.y);
        if (dist > 1) return false;

        isCollected = true;

        if (itemType == ItemType.Weapon)
        {
            PlayerCombat combat = unit.GetComponent<PlayerCombat>() ?? Object.FindFirstObjectByType<PlayerCombat>();
            if (combat != null)
            {
                combat.EquipWeapon();
            }
            Debug.Log("<color=yellow>WEAPON COLLECTED! Press X to attack enemies.</color>");
        }
        else if (itemType == ItemType.PoisonBottle)
        {
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.Heal(1);
            }
            Debug.Log("<color=green>POTION DRUNK! +1 HP Restored.</color>");
        }

        gameObject.SetActive(false);
        return true;
    }
}
