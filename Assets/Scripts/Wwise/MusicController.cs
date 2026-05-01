using UnityEngine;
using System.Collections;
using System.IO;

public class MusicController : MonoBehaviour
{
    private const uint MusicStateGroupId = 1021618141;
    private const uint ExplorationStateId = 2582085496;
    private const uint CombatStateId = 2764240573;
    private const uint BossPhase1StateId = 851884604;
    private const uint BossPhase2StateId = 851884607;
    private const uint BossPhase3StateId = 851884606;

    public static MusicController Instance;

    [Header("Wwise Event")]
    [SerializeField] private string musicBankName = "Music";
    [SerializeField] private string musicEventName = "Play_Level01_Music";

    [Header("Wwise State")]
    [SerializeField] private bool disableStateChangesForDebug = false;
    [SerializeField] private bool useStateById = false;
    [SerializeField] private string musicStateGroup = "MusicState";
    [SerializeField] private string explorationState = "Exploration";
    [SerializeField] private string bossPhase1State = "BossPhase1";

    [Header("Wwise Switch")]
    [SerializeField] private bool useSwitchRouting = false;
    [SerializeField] private string musicSwitchGroup = "Level01_Music";
    [SerializeField] private string introExplorationSwitch = "intro_exploration";
    [SerializeField] private string explorationSwitch = "exploration";
    [SerializeField] private string introBossSwitch = "intro_fight_boss";
    [SerializeField] private string bossStartSwitch = "fight_boss_start";
    [SerializeField] private string bossLowHpSwitch = "fight_boss_low-hp";
    [SerializeField] private string bossEndSwitch = "fight_boss_end";

    [Header("Transition Timings (seconds)")]
    [SerializeField] private float introExplorationDuration = 6f;
    [SerializeField] private float introBossDuration = 6f;

    [Header("Boss HP Tracking")]
    [SerializeField] private bool autoFindBossOnBossRoomEnter = true;
    [SerializeField] private string bossTag = "Boss";
    [SerializeField] private Health bossHealth;
    [SerializeField] [Range(0f, 1f)] private float lowHpThresholdNormalized = 0.30f;

    private Coroutine transitionCoroutine;
    private bool musicStarted;
    private bool inBossFight;
    private bool lowHpTriggered;
    private bool bossEndTriggered;
    private float nextBossLookupTime;
    private bool switchCallsDisabled;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        ConfigureWwiseBasePath();
        LoadMusicBankWithFallback();
        // Safe fallback: if room trigger is missing, music still starts correctly.
        EnterExplorationRoom();
    }

    void Update()
    {
        if (!inBossFight || bossEndTriggered)
        {
            return;
        }

        if (bossHealth == null && autoFindBossOnBossRoomEnter && Time.time >= nextBossLookupTime)
        {
            TryResolveBossHealth();
            nextBossLookupTime = Time.time + 0.5f;
        }

        if (bossHealth == null)
        {
            return;
        }

        if (!lowHpTriggered && bossHealth.maxHealth > 0f)
        {
            float hpNormalized = bossHealth.currentHealth / bossHealth.maxHealth;
            if (hpNormalized <= lowHpThresholdNormalized && bossHealth.currentHealth > 0f)
            {
                SetBossLowHp();
                lowHpTriggered = true;
            }
        }

        if (bossHealth.currentHealth <= 0f)
        {
            SetBossEnd();
            bossEndTriggered = true;
        }
    }

    public void SetState(string stateName)
    {
        if (disableStateChangesForDebug)
        {
            Debug.Log("[MusicController] SetState skipped (debug): " + stateName);
            return;
        }

        AKRESULT result = AkSoundEngine.SetState(musicStateGroup, stateName);
        if (result == AKRESULT.AK_Success)
        {
            Debug.Log("[MusicController] SetState by name success: " + musicStateGroup + "/" + stateName);
            return;
        }

        Debug.LogWarning("[MusicController] SetState by name failed: " + musicStateGroup + "/" + stateName + " -> " + result);

        if (useStateById && TrySetStateById(stateName))
        {
            Debug.Log("[MusicController] SetState fallback by ID success: " + stateName);
            return;
        }
    }

    private bool TrySetStateById(string stateName)
    {
        uint stateId;
        switch (stateName)
        {
            case "Exploration":
                stateId = ExplorationStateId;
                break;
            case "Combat":
                stateId = CombatStateId;
                break;
            case "BossPhase1":
                stateId = BossPhase1StateId;
                break;
            case "BossPhase2":
                stateId = BossPhase2StateId;
                break;
            case "BossPhase3":
                stateId = BossPhase3StateId;
                break;
            default:
                return false;
        }

        AKRESULT result = AkSoundEngine.SetState(MusicStateGroupId, stateId);
        if (result == AKRESULT.AK_Success)
        {
            Debug.Log("[MusicController] SetState by ID success: " + stateName);
            return true;
        }

        Debug.LogWarning("[MusicController] SetState by ID failed: " + stateName + " -> " + result);
        return false;
    }

    public void EnterExplorationRoom()
    {
        inBossFight = false;
        StartMusicIfNeeded();
        StartTransition(introExplorationSwitch, introExplorationDuration, explorationSwitch, explorationState);
    }

    public void EnterBossRoom()
    {
        inBossFight = true;
        lowHpTriggered = false;
        bossEndTriggered = false;
        nextBossLookupTime = 0f;

        if (autoFindBossOnBossRoomEnter)
        {
            TryResolveBossHealth();
        }

        StartMusicIfNeeded();
        StartTransition(introBossSwitch, introBossDuration, bossStartSwitch, bossPhase1State);
    }

    public void SetBossHealthTarget(Health healthTarget)
    {
        bossHealth = healthTarget;
    }

    private void TryResolveBossHealth()
    {
        if (bossHealth != null)
        {
            return;
        }

        GameObject bossObject = GameObject.FindGameObjectWithTag(bossTag);
        if (bossObject != null)
        {
            bossHealth = bossObject.GetComponent<Health>();
            if (bossHealth != null)
            {
                return;
            }
        }

        // Fallback for dynamically spawned bosses without tag assignment.
        Health[] allHealth = FindObjectsOfType<Health>();
        for (int i = 0; i < allHealth.Length; i++)
        {
            Health candidate = allHealth[i];
            if (candidate == null)
            {
                continue;
            }

            if (candidate.GetComponent<Boss_SpiderQueen_Complete>() != null ||
                candidate.GetComponent<Boss_Eel_Complete>() != null ||
                candidate.GetComponent<Boss_Golem_Complete>() != null)
            {
                bossHealth = candidate;
                return;
            }

            string lowerName = candidate.gameObject.name.ToLowerInvariant();
            if (lowerName.Contains("boss"))
            {
                bossHealth = candidate;
                return;
            }
        }
    }

    public void SetBossLowHp()
    {
        SetSwitch(bossLowHpSwitch);
    }

    public void SetBossEnd()
    {
        SetSwitch(bossEndSwitch);
    }

    private void StartMusicIfNeeded()
    {
        if (musicStarted)
        {
            return;
        }

        AkSoundEngine.PostEvent(musicEventName, gameObject);
        musicStarted = true;
    }

    private void StartTransition(string introSwitch, float introDuration, string loopSwitch, string stateName)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(PlayIntroThenLoop(introSwitch, introDuration, loopSwitch, stateName));
    }

    private IEnumerator PlayIntroThenLoop(string introSwitch, float introDuration, string loopSwitch, string stateName)
    {
        SetSwitch(introSwitch);
        SetState(stateName);

        if (introDuration > 0f)
        {
            yield return new WaitForSeconds(introDuration);
        }

        SetSwitch(loopSwitch);
        transitionCoroutine = null;
    }

    private void SetSwitch(string switchName)
    {
        if (!useSwitchRouting)
        {
            return;
        }

        if (switchCallsDisabled)
        {
            return;
        }

        AKRESULT result = AkSoundEngine.SetSwitch(musicSwitchGroup, switchName, gameObject);
        if (result == AKRESULT.AK_Success)
        {
            return;
        }

        Debug.LogWarning("[MusicController] SetSwitch failed: " + musicSwitchGroup + "/" + switchName + " -> " + result);

        // If switch group is not configured in this bank, avoid log spam.
        if (result == AKRESULT.AK_IDNotFound || result == AKRESULT.AK_InvalidParameter)
        {
            switchCallsDisabled = true;
            Debug.LogWarning("[MusicController] Switch calls disabled for this session.");
        }
    }

    private void LoadMusicBankWithFallback()
    {
        if (TryLoadBank(musicBankName))
        {
            return;
        }

        if (TryLoadBank(musicBankName + ".bnk"))
        {
            return;
        }
    }

    private bool TryLoadBank(string bankNameOrPath)
    {
        AKRESULT result = AkSoundEngine.LoadBank(bankNameOrPath, out _);
        if (result == AKRESULT.AK_Success)
        {
            Debug.Log("[MusicController] Loaded bank: " + bankNameOrPath);
            return true;
        }

        Debug.LogWarning("[MusicController] Bank load failed: " + bankNameOrPath + " -> " + result);
        return false;
    }

    private void ConfigureWwiseBasePath()
    {
        string windowsBankFolder = Path.Combine(
            Application.streamingAssetsPath,
            "Audio",
            "GeneratedSoundBanks",
            "Windows");

        string normalizedPath = windowsBankFolder.Replace("\\", "/");
        AKRESULT basePathResult = AkSoundEngine.SetBasePath(normalizedPath);
        Debug.Log("[MusicController] SetBasePath: " + normalizedPath + " -> " + basePathResult);

        AKRESULT languageResult = AkSoundEngine.SetCurrentLanguage("English(US)");
        Debug.Log("[MusicController] SetCurrentLanguage: English(US) -> " + languageResult);
    }
}