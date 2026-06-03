using UnityEngine;
using UnityEngine.Rendering; // 添加这个命名空间

/// <summary>
/// 案发现场/梦境场景控制器
/// 根据梦境等级控制场景内容的显示/隐藏
/// 挂载到 CrimeScene 中的空物体上
/// </summary>
public class CrimeSceneController : MonoBehaviour
{
    public static CrimeSceneController Instance { get; private set; }

    [Header("=== 梦境等级显示控制 ===")]
    [Tooltip("梦境等级 1-5 对应的场景物体组")]
    [SerializeField] private GameObject[] dreamLevelObjects; // 5 个元素，对应 5 个等级

    [Header("=== 迷雾/遮掩效果 ===")]
    [Tooltip("迷雾粒子或遮罩，梦境等级越低越浓")]
    [SerializeField] private GameObject fogEffect;
    [SerializeField] private ParticleSystem fogParticles;
    [SerializeField] private float[] fogDensityByLevel = { 1.0f, 0.8f, 0.6f, 0.4f, 0.2f };

    [Header("=== 光照效果 ===")]
    [Tooltip("场景光照，梦境等级越低越暗")]
    [SerializeField] private Light mainLight;
    [SerializeField]
    private Color[] lightColorByLevel = {
        new Color(0.2f, 0.2f, 0.4f),  // 等级 0 - 深蓝
        new Color(0.3f, 0.3f, 0.5f),  // 等级 1
        new Color(0.5f, 0.5f, 0.6f),  // 等级 2
        new Color(0.7f, 0.7f, 0.8f),  // 等级 3
        new Color(0.9f, 0.9f, 1.0f),  // 等级 4
        Color.white                    // 等级 5 - 纯白
    };
    [SerializeField] private float[] lightIntensityByLevel = { 0.3f, 0.5f, 0.7f, 0.9f, 1.0f, 1.2f };

    [Header("=== 迷雾浓度（Unity 内置） ===")]
    [Tooltip("使用 Unity 内置迷雾，无需 Post Processing")]
    [SerializeField] private float[] fogDensityValues = { 0.5f, 0.4f, 0.3f, 0.2f, 0.1f, 0f };

    [Header("=== 线索物体 ===")]
    [Tooltip("场景中可交互的线索物体")]
    [SerializeField] private ClueObject[] clueObjects;

    [Header("=== 音效 ===")]
    [SerializeField] private AudioClip[] dreamAmbienceByLevel;
    [SerializeField] private AudioSource ambientAudio;

    private int currentDreamLevel = 0;
    private bool isSceneInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("[CrimeScene] 场景已加载");
        InitializeScene();
    }

    /// <summary>
    /// 初始化场景状态
    /// </summary>
    public void InitializeScene()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[CrimeScene] GameManager 未找到！");
            return;
        }

        currentDreamLevel = GameManager.Instance.unlockedCluesCount;

        Debug.Log($"[CrimeScene] 当前线索等级：{currentDreamLevel}/5");

        // 应用梦境等级效果
        ApplyDreamLevel(currentDreamLevel);

        // 初始化线索物体
        InitializeClueObjects();

        isSceneInitialized = true;
    }

    /// <summary>
    /// 根据梦境等级应用所有效果
    /// </summary>
    public void ApplyDreamLevel(int level)
    {
        level = Mathf.Clamp(level, 0, 5);
        currentDreamLevel = level;

        Debug.Log($"[CrimeScene] 应用梦境等级：{level}");

        // 1. 控制场景物体显示
        UpdateSceneObjects(level);

        // 2. 控制迷雾效果
        UpdateFogEffect(level);

        // 3. 控制光照
        UpdateLighting(level);

        // 4. 控制音效
        UpdateAmbience(level);

        // 5. 更新 UI 通知
        if (UIManager.Instance != null)
        {
            if (level >= 5)
            {
                UIManager.Instance.ShowNotification("梦境终于清楚了！ 你可以看清这里了，准备车费来这里寻找最后的真相吧！");
            }
            else
            {
                UIManager.Instance.ShowNotification($"线索等级: {level}/5");
            }
        }
    }

    /// <summary>
    /// 更新场景物体显示
    /// </summary>
    private void UpdateSceneObjects(int level)
    {
        if (dreamLevelObjects == null) return;

        for (int i = 0; i < dreamLevelObjects.Length; i++)
        {
            if (dreamLevelObjects[i] != null)
            {
                // 等级达到 i+1 时显示该物体
                dreamLevelObjects[i].SetActive(i < level);
            }
        }

        // 如果等级为 0，显示"完全模糊"的占位物体
        if (level == 0)
        {
            Debug.Log("[CrimeScene] 梦境完全模糊，无法辨认任何内容");
        }
    }

    /// <summary>
    /// 更新迷雾效果 
    /// </summary>
    private void UpdateFogEffect(int level)
    {
        // 控制迷雾游戏对象
        if (fogEffect != null)
        {
            fogEffect.SetActive(level < 5); // 等级 5 时完全清除迷雾
        }

        // 控制粒子系统 
        if (fogParticles != null && level < fogDensityByLevel.Length)
        {
            ParticleSystem.EmissionModule emission = fogParticles.emission;
            // 使用 rateOverTime 
            emission.rateOverTime = 50f * fogDensityByLevel[level];

            // 可选：控制粒子数量
            ParticleSystem.MainModule main = fogParticles.main;
            main.maxParticles = (int)(100 * fogDensityByLevel[level]);
        }

        // 使用 Unity 内置迷雾 
        RenderSettings.fog = level < 5;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = fogDensityValues[Mathf.Min(level, fogDensityValues.Length - 1)];
    }

    /// <summary>
    /// 更新光照
    /// </summary>
    private void UpdateLighting(int level)
    {
        if (mainLight != null)
        {
            int clampedLevel = Mathf.Clamp(level, 0, lightIntensityByLevel.Length - 1);
            mainLight.intensity = lightIntensityByLevel[clampedLevel];
            mainLight.color = lightColorByLevel[clampedLevel];
        }
    }

    /// <summary>
    /// 更新环境音效
    /// </summary>
    private void UpdateAmbience(int level)
    {
        if (ambientAudio != null && level < dreamAmbienceByLevel.Length)
        {
            if (dreamAmbienceByLevel[level] != null)
            {
                ambientAudio.clip = dreamAmbienceByLevel[level];
                if (!ambientAudio.isPlaying)
                {
                    ambientAudio.Play();
                }
            }
        }
    }

    /// <summary>
    /// 初始化线索物体
    /// </summary>
    private void InitializeClueObjects()
    {
        if (clueObjects == null) return;

        for (int i = 0; i < clueObjects.Length; i++)
        {
            if (clueObjects[i] != null)
            {
                clueObjects[i].Initialize(i, this);
            }
        }
    }

    /// <summary>
    /// 收集线索（从场景内调用）
    /// </summary>
    public void CollectClue(int clueIndex)
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.AddClue(clueIndex);

        // 收集后更新梦境等级
        int newLevel = GameManager.Instance.unlockedCluesCount;
        ApplyDreamLevel(newLevel);

        // 如果达到等级 5，显示完整场景
        if (newLevel >= 4)
        {
            Debug.Log("[CrimeScene] 梦境完全清晰！原来这就是家乡！");
        }
    }

    /// <summary>
    /// 从梦境醒来（返回主场景）
    /// </summary>
    public void WakeFromDream()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene("DayScene");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DayScene");
        }
    }

    /// <summary>
    /// 直接触发结局（等级 4 时可用）
    /// </summary>
    public void TriggerEnding()
    {
        if (currentDreamLevel >= 4)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerEnding(GameManager.EndingType.JusticeServed);
            }
        }
        else
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNotification($"需要线索数量达到 5 级! 当前等级: {currentDreamLevel}");
            }
        }
    }

    // 获取当前梦境等级（供其他脚本使用）
    public int GetCurrentDreamLevel() => currentDreamLevel;
    public bool IsSceneInitialized() => isSceneInitialized;

    // 在场景禁用时重置迷雾
    private void OnDisable()
    {
        RenderSettings.fog = false;
    }
}