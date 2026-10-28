using UnityEngine;

public class Level4CombatController : MonoBehaviour
{
    public static Level4CombatController Instance { get; private set; }

    [Header("Guards")]
    public CombatEnemy guard1;
    public CombatEnemy guard2;

    [Header("Citadel Plate 4")]
    public PressurePlate plate4;

    [Header("Door to Goal")]
    public ToggleWall goalDoor;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    private void Start()
    {
        AutoFindReferences();
        ResetState();
    }

    public void AutoFindReferences()
    {
        if (guard1 == null)
        {
            GameObject go = GameObject.Find("CombatEnemy_1");
            if (go != null) guard1 = go.GetComponent<CombatEnemy>();
        }
        if (guard2 == null)
        {
            GameObject go = GameObject.Find("CombatEnemy_2");
            if (go != null) guard2 = go.GetComponent<CombatEnemy>();
        }
        if (goalDoor == null)
        {
            GameObject go = GameObject.Find("D4_L");
            if (go != null) goalDoor = go.GetComponent<ToggleWall>();
        }
        if (plate4 == null)
        {
            GameObject go = GameObject.Find("PressurePlate4");
            if (go != null) plate4 = go.GetComponent<PressurePlate>();
        }
    }

    public void ResetState()
    {
        AutoFindReferences();
        if (goalDoor != null)
        {
            goalDoor.Close(false);
        }
        EvaluateDoorState();
    }

    public void EvaluateDoorState()
    {
        if (goalDoor == null || plate4 == null) AutoFindReferences();

        // 1. Update Sentry Shields: Plate 4 acts as the Shield Disruptor
        bool shieldDisrupted = plate4 != null && plate4.IsPressed;
        if (guard1 != null && !guard1.IsDead) guard1.SetShielded(!shieldDisrupted);
        if (guard2 != null && !guard2.IsDead) guard2.SetShielded(!shieldDisrupted);

        // 2. Citadel Gate opens SOLELY when both guards are defeated!
        if (goalDoor == null) return;
        bool allGuardsDefeated = (guard1 == null || guard1.IsDead) && (guard2 == null || guard2.IsDead);

        if (allGuardsDefeated)
        {
            if (!goalDoor.IsOpen())
            {
                Debug.Log("<color=green>[Level4CombatController] Both guards defeated! Opening Citadel Gate (Door 4).</color>");
                goalDoor.Open();
            }
        }
        else
        {
            if (goalDoor.IsOpen())
            {
                goalDoor.Close();
            }
        }
    }
}
