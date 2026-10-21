using UnityEngine;

public class Level5BossController : MonoBehaviour
{
    public static Level5BossController Instance { get; private set; }

    [Header("References")]
    public CombatEnemy westSentry;
    public CombatEnemy eastSentry;
    public CombatEnemy boss;
    public ToggleWall bossDoor;

    [Header("State")]
    public bool bossHasExited = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    private void Start()
    {
        AutoFindReferences();
        ResetBossState();
    }

    public void AutoFindReferences()
    {
        if (westSentry == null)
        {
            GameObject go = GameObject.Find("CombatEnemy_West");
            if (go != null) westSentry = go.GetComponent<CombatEnemy>();
        }
        if (eastSentry == null)
        {
            GameObject go = GameObject.Find("CombatEnemy_East");
            if (go != null) eastSentry = go.GetComponent<CombatEnemy>();
        }
        if (boss == null)
        {
            GameObject go = GameObject.Find("CombatEnemy_Boss");
            if (go != null) boss = go.GetComponent<CombatEnemy>();
        }
        if (bossDoor == null)
        {
            GameObject go = GameObject.Find("D5_L");
            if (go != null) bossDoor = go.GetComponent<ToggleWall>();
        }
    }

    public void ResetBossState()
    {
        AutoFindReferences();
        bossHasExited = false;
        if (bossDoor != null)
        {
            bossDoor.Close();
        }
    }

    public void EvaluateBossDoorState()
    {
        if (bossDoor == null) AutoFindReferences();
        if (bossDoor == null) return;

        // 1. If final boss is slain, the door permanently opens to allow reaching the chest
        if (boss != null && boss.IsDead)
        {
            bossDoor.Open();
            return;
        }

        // 2. Check if boss has exited into the courtyard (y <= 2)
        if (boss != null && !boss.IsDead && boss.GetGridPosition().y <= 2)
        {
            bossHasExited = true;
        }

        // 3. If boss exited, door closes behind it to seal the chest room
        if (bossHasExited)
        {
            bossDoor.Close();
            return;
        }

        // 4. Check if both other bosses (West Demon + East Spider) are killed
        bool bothSentriesDead = (westSentry != null && westSentry.IsDead) &&
                               (eastSentry != null && eastSentry.IsDead);

        if (bothSentriesDead)
        {
            // Both sentries defeated -> release the final boss!
            bossDoor.Open();
        }
        else
        {
            // Sentries still alive -> keep boss trapped
            bossDoor.Close();
        }
    }

    private void Update()
    {
        EvaluateBossDoorState();
    }
}
