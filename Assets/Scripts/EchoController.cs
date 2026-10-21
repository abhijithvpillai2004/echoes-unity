using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives one echo unit through its recorded action list, one command per turn.
/// Put this alongside a Unit component on your echo prefab.
/// LoopManager calls StepTurn() once per turn during EchoReplay state.
/// </summary>
[RequireComponent(typeof(Unit))]
public class EchoController : MonoBehaviour
{
    private List<ICommand> actions;
    private int currentTurnIndex = 0;
    private Unit unit;

    public void Initialize(List<ICommand> recordedActions, Vector2Int startPosition)
    {
        actions = recordedActions;
        currentTurnIndex = 0;
        unit = GetComponent<Unit>();
        if (unit != null)
        {
            unit.isEcho = true;
            unit.SetStartGridPosition(startPosition);
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEchoReplayStart();
        }
    }

    public void Initialize(List<ICommand> recordedActions)
    {
        actions = recordedActions;
        currentTurnIndex = 0;
        unit = GetComponent<Unit>();
        if (unit != null)
        {
            unit.isEcho = true;
            unit.ResetUnit();
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEchoReplayStart();
        }
    }

    /// <summary>Executes this echo's next recorded command. Call once per turn.</summary>
    public void StepTurn()
    {
        if (actions == null || currentTurnIndex >= actions.Count) return;

        if (unit != null)
        {
            actions[currentTurnIndex].Execute(unit);
        }
        currentTurnIndex++;
    }

    public void StepTurn(int turnIndex)
    {
        if (actions == null || turnIndex < 0 || turnIndex >= actions.Count) return;
        if (unit != null)
        {
            actions[turnIndex].Execute(unit);
        }
        currentTurnIndex = turnIndex + 1;
    }

    public bool IsFinished()
    {
        return actions == null || currentTurnIndex >= actions.Count;
    }
}
