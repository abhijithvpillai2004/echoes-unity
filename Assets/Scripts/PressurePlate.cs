using UnityEngine;

/// <summary>
/// Detects when any unit stands on this grid cell.
/// Evaluated at the end of each turn cycle by LoopManager.
/// </summary>
public class PressurePlate : MonoBehaviour
{
    [Header("Config")]
    public Vector2Int platePosition;
    public Vector2Int[] extraTriggerPositions;
    public ToggleWall linkedWall;
    public TrapTile[] linkedTraps;

    private bool isPressed = false;
    public bool IsPressed => isPressed;

    public void ResetPlate()
    {
        isPressed = false;
        if (linkedTraps != null)
        {
            foreach (var trap in linkedTraps)
                if (trap != null) trap.ResetTrap();
        }
        EvaluatePlate();
    }

    public void EvaluatePlate()
    {
        Unit[] allUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        bool anyUnitOnPlate = false;

        foreach (Unit unit in allUnits)
        {
            if (unit != null && unit.gameObject.activeInHierarchy)
            {
                if (unit.GridPosition == platePosition)
                {
                    anyUnitOnPlate = true;
                    break;
                }
                if (extraTriggerPositions != null)
                {
                    foreach (var extra in extraTriggerPositions)
                    {
                        if (unit.GridPosition == extra)
                        {
                            anyUnitOnPlate = true;
                            break;
                        }
                    }
                    if (anyUnitOnPlate) break;
                }
            }
        }

        if (anyUnitOnPlate && !isPressed)
        {
            isPressed = true;
            if (linkedWall != null) linkedWall.Open();
            if (linkedTraps != null)
            {
                foreach (var trap in linkedTraps)
                    if (trap != null) trap.SetRetracted(true);
            }
        }
        else if (!anyUnitOnPlate && isPressed)
        {
            isPressed = false;
            if (linkedWall != null) linkedWall.Close();
            if (linkedTraps != null)
            {
                foreach (var trap in linkedTraps)
                    if (trap != null) trap.SetRetracted(false);
            }
        }
    }
}
