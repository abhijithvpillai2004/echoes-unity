using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoopManager : MonoBehaviour
{
    public static LoopManager Instance { get; private set; }

    [Header("Settings")]
    public int turnsPerLoop = 12;
    public int maxLoops = 3;
    public Vector2Int chestGridPos = new Vector2Int(0, 3);
    public Vector2Int playerStartPosition = new Vector2Int(-4, -4);

    [Header("References")]
    public Unit livePlayer;
    public Transform echoContainer;

    [Header("UI References (Optional)")]
    public string levelTitle = "";
    public TMP_Text turnText;
    public TMP_Text loopText;
    public TMP_Text levelText;
    public GameObject winPanel;
    public GameObject gameOverPanel;

    [Header("Prefabs & Visuals")]
    public GameObject playerPrefab;
    public GameObject echoPrefab;
    public Sprite playerSprite;
    public Sprite echoSprite;

    [Header("Runtime State")]
    public int currentLoop = 1;
    public int currentTurn = 0;
    public bool isGameOver = false;
    public bool isGameWon = false;

    private ActionRecorder recorder;
    private List<List<ICommand>> pastLoopRecordings = new List<List<ICommand>>();
    private List<EchoController> spawnedEchoes = new List<EchoController>();
    private EnemyPatrol[] enemies;
    private CombatEnemy[] combatEnemies;
    private CollectibleItem[] collectibleItems;
    private PressurePlate[] pressurePlates;
    private ToggleWall[] toggleWalls;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

        recorder = GetComponent<ActionRecorder>();
        if (recorder == null) recorder = gameObject.AddComponent<ActionRecorder>();

        if (echoContainer == null)
        {
            GameObject containerObj = GameObject.Find("EchoContainer");
            if (containerObj == null) containerObj = new GameObject("EchoContainer");
            echoContainer = containerObj.transform;
        }

        if (livePlayer == null)
        {
            GameObject pObj = GameObject.Find("Player");
            if (pObj != null)
            {
                livePlayer = pObj.GetComponent<Unit>();
            }
        }

        if (livePlayer == null)
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
            foreach (var u in units)
            {
                if (u != null && !u.isEcho)
                {
                    livePlayer = u;
                    break;
                }
            }
        }

        // Auto-find UI texts if not set
        if (turnText == null)
        {
            GameObject tt = GameObject.Find("TurnText");
            if (tt != null) turnText = tt.GetComponent<TMP_Text>();
        }
        if (loopText == null)
        {
            GameObject lt = GameObject.Find("LoopText");
            if (lt != null) loopText = lt.GetComponent<TMP_Text>();
        }
        if (levelText == null)
        {
            GameObject lvlt = GameObject.Find("LevelText");
            if (lvlt != null) levelText = lvlt.GetComponent<TMP_Text>();
        }
        if (winPanel == null)
        {
            GameObject wp = GameObject.Find("WinText");
            if (wp == null) wp = GameObject.Find("WinPanel");
            if (wp != null)
            {
                winPanel = wp;
                winPanel.SetActive(false);
            }
        }
        if (gameOverPanel == null)
        {
            GameObject gop = GameObject.Find("GameOverText");
            if (gop == null) gop = GameObject.Find("GameOverPanel");
            if (gop != null)
            {
                gameOverPanel = gop;
                gameOverPanel.SetActive(false);
            }
        }
    }

    private void Start()
    {
        enemies = Object.FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        combatEnemies = Object.FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
        collectibleItems = Object.FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None);
        pressurePlates = Object.FindObjectsByType<PressurePlate>(FindObjectsSortMode.None);
        toggleWalls = Object.FindObjectsByType<ToggleWall>(FindObjectsSortMode.None);

        if (livePlayer != null)
        {
            livePlayer.SetStartGridPosition(playerStartPosition);
            EnsurePlayerVisibility(livePlayer.gameObject);
        }
        else
        {
            SpawnLivePlayer();
        }

        if (levelText != null && !string.IsNullOrEmpty(levelTitle))
        {
            levelText.text = levelTitle;
        }

        StartLoop(1);
    }

    private void EnsurePlayerVisibility(GameObject playerObj)
    {
        if (playerObj == null) return;
        SpriteRenderer sr = playerObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 10;
            if (sr.sprite == null && playerSprite != null)
            {
                sr.sprite = playerSprite;
            }
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }

    private void SpawnLivePlayer()
    {
        if (playerPrefab != null)
        {
            GameObject pObj = Instantiate(playerPrefab);
            pObj.name = "Player";
            livePlayer = pObj.GetComponent<Unit>();
            livePlayer.SetStartGridPosition(playerStartPosition);
            EnsurePlayerVisibility(pObj);
            return;
        }

        GameObject playerObj = new GameObject("Player");
        playerObj.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
        SpriteRenderer sr = playerObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        if (playerSprite != null) sr.sprite = playerSprite;
        livePlayer = playerObj.AddComponent<Unit>();
        livePlayer.SetStartGridPosition(playerStartPosition);
    }

    public void StartLoop(int loopIndex)
    {
        currentLoop = loopIndex;
        currentTurn = 0;
        isGameOver = false;
        isGameWon = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        else
        {
            GameObject gop = GameObject.Find("GameOverText") ?? GameObject.Find("GameOverPanel");
            if (gop != null)
            {
                gameOverPanel = gop;
                gameOverPanel.SetActive(false);
            }
        }

        if (winPanel != null) winPanel.SetActive(false);
        else
        {
            GameObject wp = GameObject.Find("WinText") ?? GameObject.Find("WinPanel");
            if (wp != null)
            {
                winPanel = wp;
                winPanel.SetActive(false);
            }
        }

        if (loopIndex == 1)
        {
            pastLoopRecordings.Clear();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayBGM();
        }

        if (livePlayer != null)
        {
            livePlayer.SetStartGridPosition(playerStartPosition);
        }

        foreach (var echo in spawnedEchoes)
        {
            if (echo != null)
            {
                if (Application.isPlaying) Destroy(echo.gameObject);
                else DestroyImmediate(echo.gameObject);
            }
        }
        spawnedEchoes.Clear();

        for (int i = 0; i < pastLoopRecordings.Count; i++)
        {
            GameObject echoObj = CreateEchoInstance();
            if (echoObj != null)
            {
                EchoController ctrl = echoObj.GetComponent<EchoController>();
                if (ctrl != null)
                {
                    ctrl.Initialize(pastLoopRecordings[i], playerStartPosition);
                    spawnedEchoes.Add(ctrl);
                }
            }
        }

        if (enemies == null) enemies = Object.FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (e != null) e.ResetToStart();
        }

        if (combatEnemies == null) combatEnemies = Object.FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
        foreach (var ce in combatEnemies)
        {
            if (ce != null) ce.ResetToStart();
        }

        if (collectibleItems == null) collectibleItems = Object.FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None);
        foreach (var item in collectibleItems)
        {
            if (item != null) item.ResetItem();
        }

        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.ResetHealth();
        }

        if (PlayerCombat.Instance != null)
        {
            PlayerCombat.Instance.ResetCombat();
        }

        if (toggleWalls == null) toggleWalls = Object.FindObjectsByType<ToggleWall>(FindObjectsSortMode.None);
        foreach (var tw in toggleWalls)
        {
            if (tw != null) tw.ResetWall();
        }

        if (pressurePlates == null) pressurePlates = Object.FindObjectsByType<PressurePlate>(FindObjectsSortMode.None);
        foreach (var p in pressurePlates)
        {
            if (p != null) p.ResetPlate();
        }

        TrapTile[] traps = Object.FindObjectsByType<TrapTile>(FindObjectsSortMode.None);
        foreach (var t in traps)
        {
            if (t != null) t.ResetTrap();
        }

        if (recorder == null)
        {
            recorder = GetComponent<ActionRecorder>();
            if (recorder == null) recorder = gameObject.AddComponent<ActionRecorder>();
        }
        if (recorder != null)
        {
            recorder.Clear();
        }

        UpdateUI();
    }

    private GameObject CreateEchoInstance()
    {
        if (echoPrefab != null)
        {
            GameObject echoObj = Instantiate(echoPrefab, echoContainer);
            SpriteRenderer sr = echoObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 9;
                Color c = sr.color;
                c.a = 0.5f;
                sr.color = c;
            }
            return echoObj;
        }

        if (livePlayer != null)
        {
            GameObject echoObj = Instantiate(livePlayer.gameObject, echoContainer);
            echoObj.name = "Echo";
            
            PlayerInputHandler pih = echoObj.GetComponent<PlayerInputHandler>();
            if (pih != null) Destroy(pih);

            PlayerHealth pHealth = echoObj.GetComponent<PlayerHealth>();
            if (pHealth != null) Destroy(pHealth);

            PlayerCombat pCombat = echoObj.GetComponent<PlayerCombat>();
            if (pCombat != null) Destroy(pCombat);

            Transform wVis = echoObj.transform.Find("WeaponVisual");
            if (wVis != null) Destroy(wVis.gameObject);

            Unit echoUnit = echoObj.GetComponent<Unit>();
            if (echoUnit != null)
            {
                echoUnit.isEcho = true;
                echoUnit.SetStartGridPosition(playerStartPosition);
            }

            SpriteRenderer sr = echoObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (sr.sprite == null && (echoSprite != null || playerSprite != null))
                {
                    sr.sprite = echoSprite != null ? echoSprite : playerSprite;
                }
                Color c = sr.color;
                c.a = 0.5f;
                sr.color = c;
                sr.sortingOrder = 9;
            }

            EchoController echoCtrl = echoObj.GetComponent<EchoController>();
            if (echoCtrl == null)
            {
                echoCtrl = echoObj.AddComponent<EchoController>();
            }
            return echoObj;
        }

        return null;
    }

    public void SubmitPlayerCommand(ICommand cmd)
    {
        if (isGameOver || isGameWon) return;
        if (currentTurn >= turnsPerLoop) return;

        // 1. Step all Echoes for the current turn
        foreach (var echo in spawnedEchoes)
        {
            if (echo != null) echo.StepTurn(currentTurn);
        }

        // 2. Evaluate pressure plates so doors opened by Echoes open immediately
        if (pressurePlates != null)
        {
            foreach (var p in pressurePlates)
            {
                if (p != null) p.EvaluatePlate();
            }
        }

        // 3. Execute live player command
        cmd.Execute(livePlayer);
        if (recorder == null) recorder = GetComponent<ActionRecorder>() ?? gameObject.AddComponent<ActionRecorder>();
        if (recorder != null) recorder.Record(cmd);

        // 4. Evaluate pressure plates and boss events
        if (Level5BossController.Instance != null)
        {
            Level5BossController.Instance.EvaluateBossDoorState();
        }
        if (pressurePlates != null)
        {
            foreach (var p in pressurePlates)
            {
                if (p != null) p.EvaluatePlate();
            }
        }

        // Check spike trap tiles for live player
        TrapTile[] activeTraps = Object.FindObjectsByType<TrapTile>(FindObjectsSortMode.None);
        foreach (var trap in activeTraps)
        {
            if (trap != null && livePlayer != null) trap.CheckUnitTrigger(livePlayer);
        }

        // 5. Step enemies (which react to echoes and player)
        if (enemies == null) enemies = Object.FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        if (enemies != null)
        {
            foreach (var e in enemies)
            {
                if (e != null) e.StepTurn();
            }
        }

        if (combatEnemies == null) combatEnemies = Object.FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
        if (combatEnemies != null)
        {
            foreach (var ce in combatEnemies)
            {
                if (ce != null) ce.StepTurn();
            }
        }

        if (isGameOver) return;

        currentTurn++;
        UpdateUI();

        if (livePlayer != null && livePlayer.GridPosition == chestGridPos)
        {
            HandleWin();
            return;
        }

        if (currentTurn >= turnsPerLoop)
        {
            EndCurrentLoop();
        }
    }

    private void HandleWin()
    {
        isGameWon = true;
        Debug.Log("<color=green>WINNER! Chest reached!</color>");
        
        LevelManager lm = LevelManager.Instance ?? Object.FindFirstObjectByType<LevelManager>();
        bool isFinal = (lm != null && lm.IsFinalLevel()) || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals("Level5", System.StringComparison.OrdinalIgnoreCase);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            TMP_Text winTmp = winPanel.GetComponent<TMP_Text>() ?? winPanel.GetComponentInChildren<TMP_Text>();
            if (winTmp != null)
            {
                if (isFinal)
                {
                    winTmp.text = "<color=#FFD700><size=36>🏆 CONGRATULATIONS! 🏆</size></color>\n\n<size=24><color=#00FFAA>YOU CONQUERED ALL TIME LOOPS!</color></size>\n\n<size=20><color=#FFFFFF>YOU HAVE WON THE GAME!</color></size>";
                    winTmp.alignment = TextAlignmentOptions.Center;
                }
                else
                {
                    winTmp.text = "<color=#00FFAA><size=36>LEVEL CLEARED!</size></color>";
                    winTmp.alignment = TextAlignmentOptions.Center;
                }
            }
        }
        UpdateUI();

        if (lm != null)
        {
            float delay = isFinal ? lm.finalWinTransitionDelay : lm.winTransitionDelay;
            Invoke(nameof(TransitionToNextLevel), delay);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayChestOpen();
            AudioManager.Instance.PlayVictory(isFinal);
        }
    }

    public void TransitionToNextLevel()
    {
        LevelManager lm = LevelManager.Instance ?? Object.FindFirstObjectByType<LevelManager>();
        if (lm != null)
        {
            lm.LoadNextLevel();
        }
    }

    public void EndCurrentLoopEarly()
    {
        if (recorder == null) recorder = GetComponent<ActionRecorder>() ?? gameObject.AddComponent<ActionRecorder>();
        if (recorder != null)
        {
            var commands = recorder.GetCommands();
            while (commands.Count < turnsPerLoop)
            {
                commands.Add(new WaitCommand());
            }
            pastLoopRecordings.Add(commands);
        }

        if (currentLoop < maxLoops)
        {
            StartLoop(currentLoop + 1);
        }
        else
        {
            isGameOver = true;
            Debug.Log("<color=red>Loop limit reached. Game Over.</color>");
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();
        }
    }

    private void EndCurrentLoop()
    {
        if (recorder == null) recorder = GetComponent<ActionRecorder>() ?? gameObject.AddComponent<ActionRecorder>();
        if (recorder != null)
        {
            var commands = recorder.GetCommands();
            while (commands.Count < turnsPerLoop)
            {
                commands.Add(new WaitCommand());
            }
            pastLoopRecordings.Add(commands);
        }

        if (currentLoop < maxLoops)
        {
            StartLoop(currentLoop + 1);
        }
        else
        {
            isGameOver = true;
            Debug.Log("<color=red>Loop limit reached. Game Over.</color>");
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();
        }
    }

    public void TriggerLoopFailure()
    {
        StartLoop(currentLoop);
    }

    private void UpdateUI()
    {
        if (turnText != null)
            turnText.text = $"Turn: {currentTurn}/{turnsPerLoop}";
        if (loopText != null)
            loopText.text = $"Loop: {currentLoop}/{maxLoops}";
    }
}
