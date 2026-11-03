using UnityEngine;

public enum ItemType
{
    Weapon,
    PoisonBottle,   // Blue health potion: restores 1 heart (+6 HP) after enemy attacks
    AntidotePotion  // Green antidote potion (tilemap_packed_114): neutralizes deadly poison
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
        if (isCollected || unit == null) return false;

        // Check if unit is on same tile or adjacent (for manual interact key E)
        int dist = Mathf.Abs(unit.GridPosition.x - gridPos.x) + Mathf.Abs(unit.GridPosition.y - gridPos.y);
        if (dist > 1) return false;

        return ExecuteCollect(unit);
    }

    private bool ExecuteCollect(Unit unit)
    {
        isCollected = true;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayItemCollect();

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
            // Blue Health Potion: restores 1 full heart (+6 units)
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.Heal(1);
            }
            Debug.Log("<color=cyan>BLUE HEALTH POTION DRUNK! +1 Heart Restored.</color>");
        }
        else if (itemType == ItemType.AntidotePotion)
        {
            // Green Antidote Potion: cures poison and restores health
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.CurePoison();
            }
            Debug.Log("<color=green>GREEN ANTIDOTE DRUNK! Poison neutralized and full health restored.</color>");
        }

        gameObject.SetActive(false);
        return true;
    }
}
