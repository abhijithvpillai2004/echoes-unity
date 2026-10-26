using UnityEngine;

public class Commands : MonoBehaviour
{
}

public class MoveCommand : ICommand
{
    private Vector2Int direction;

    public MoveCommand(Vector2Int dir)
    {
        direction = dir;
    }

    public void Execute(Unit unit)
    {
        Vector2Int target = unit.GridPosition + direction;

        if (GridManager.Instance != null && !GridManager.Instance.IsWalkable(target))
        {
            return;
        }

        EnemyPatrol[] enemies = Object.FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        foreach (EnemyPatrol e in enemies)
        {
            if (e != null && e.GetGridPosition() == target)
            {
                // Enemy physically blocks movement
                return;
            }
        }

        CombatEnemy[] combatEnemies = Object.FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
        foreach (CombatEnemy ce in combatEnemies)
        {
            if (ce != null && !ce.IsDead && ce.GetGridPosition() == target)
            {
                // Combat enemy blocks movement
                return;
            }
        }

        unit.SetGridPosition(target);
    }

    public string GetLabel()
    {
        if (direction == Vector2Int.up) return "Move Up";
        if (direction == Vector2Int.down) return "Move Down";
        if (direction == Vector2Int.left) return "Move Left";
        if (direction == Vector2Int.right) return "Move Right";
        return "Move";
    }
}

public class WaitCommand : ICommand
{
    public void Execute(Unit unit)
    {
    }

    public string GetLabel()
    {
        return "Wait";
    }
}

public class AttackCommand : ICommand
{
    public AttackCommand()
    {
    }

    public AttackCommand(Vector2Int dir)
    {
    }

    public void Execute(Unit unit)
    {
        if (unit == null) return;
        PlayerCombat combat = unit.GetComponent<PlayerCombat>() ?? Object.FindFirstObjectByType<PlayerCombat>();
        if (combat != null)
        {
            combat.PerformAttack();
        }
    }

    public string GetLabel()
    {
        return "Attack";
    }
}

public class InteractCommand : ICommand
{
    public void Execute(Unit unit)
    {
        if (unit == null) return;
        CollectibleItem[] items = Object.FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None);
        foreach (var item in items)
        {
            if (item != null && !item.IsCollected)
            {
                if (item.TryCollect(unit))
                {
                    break;
                }
            }
        }
    }

    public string GetLabel()
    {
        return "Interact";
    }
}
