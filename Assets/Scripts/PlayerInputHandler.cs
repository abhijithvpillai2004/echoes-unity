using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public LoopManager loopManager;

    private void Awake()
    {
        if (loopManager == null)
        {
            loopManager = GetComponent<LoopManager>();
            if (loopManager == null) loopManager = Object.FindFirstObjectByType<LoopManager>();
        }
    }

    private void Update()
    {
        if (loopManager == null)
        {
            loopManager = LoopManager.Instance ?? Object.FindFirstObjectByType<LoopManager>();
        }

        // Restart level (from loop 1)
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (loopManager != null)
            {
                loopManager.StartLoop(1);
                return;
            }
        }

        // Advance to next level on win
        if (loopManager != null && loopManager.isGameWon)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                loopManager.TransitionToNextLevel();
                return;
            }
        }

        if (loopManager == null || !loopManager.enabled || loopManager.isGameOver || loopManager.isGameWon) return;

        // Fast-forward / End Loop early (fills remaining turns with Wait so Echoes stay on plates)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("<color=cyan>Fast-forward: Ending loop early, filling remaining turns with Wait.</color>");
            loopManager.EndCurrentLoopEarly();
            return;
        }

        ICommand command = null;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) command = new MoveCommand(Vector2Int.up);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) command = new MoveCommand(Vector2Int.down);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) command = new MoveCommand(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) command = new MoveCommand(Vector2Int.right);
        else if (Input.GetKeyDown(KeyCode.Space)) command = new WaitCommand();
        else if (Input.GetKeyDown(KeyCode.E)) command = new InteractCommand();
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.F)) command = new AttackCommand();

        if (command != null)
        {
            loopManager.SubmitPlayerCommand(command);
        }
    }
}
