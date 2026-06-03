using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // 标记：是否已初始化（防止重复初始化）
    private bool isUIInitialized = false;


    //新增字段：对话面板 (需要在 Inspector 中挂载你的对话框 GameObject)
    [Header("=== Dialogue System ===")]
    [SerializeField] private GameObject dialoguePanel; // 对话框面板
    [SerializeField] private TextMeshProUGUI npcNameText; // NPC 名字
    [SerializeField] public TextMeshProUGUI dialogueText; // 对话文本
    [SerializeField] private Button continueButton; // 继续按钮
    //[SerializeField] private TextMeshProUGUI trustText; // 信任度/好感度显示

    //新增字段：记录进入对话前的状态 (用于恢复)
    private string previousSceneState = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[UIManager] 已创建并标记为 DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("[UIManager] 已存在，销毁重复实例");
            Destroy(gameObject);
            return;
        }
    }

    [Header("=== TopBar ===")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI clueText;

    [Header("=== Objective ===")]
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Header("=== Notification ===")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;

    [Header("=== Ending ===")]
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TextMeshProUGUI endingTitle;
    [SerializeField] private TextMeshProUGUI endingDescription;
    [SerializeField] private Button restartButton;

    // ==================== 按钮面板分组 ====================

    [Header("=== Action Buttons (白天) ===")]
    [SerializeField] private GameObject actionButtonsPanel;
    [SerializeField] private Button workButton;
    [SerializeField] private Button exploreButton;
    [SerializeField] private Button sleepButton;
    [SerializeField] private Button goToHometownButton;

    [Header("=== Crime Button (梦境/案发现场) ===")]
    [SerializeField] private GameObject crimeButtonPanel;
    [SerializeField] private Button exitCrimeSceneButton;  // 退出梦境/案发现场


    [Header("=== 开始游戏 ===")]
    [SerializeField] private Button startButton;

    // ==================== 初始化 ====================

    private void Start()
    {
        // 只初始化一次
        if (!isUIInitialized)
        {
            InitializeUI();
            isUIInitialized = true;
        }

        // 设置所有按钮监听
        SetupButtonEvents();

        //根据当前场景更新 UI 显示
        UpdateUIForCurrentScene();
        //初始化对话按钮事件
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(HideDialogue); // 点击继续游戏隐藏对话
        }
    }

    /// <summary>
    /// 初始化 UI（仅第一次）
    /// </summary>
    private void InitializeUI()
    {
        Debug.Log("[UIManager] UI 初始化完成");

        // 隐藏通知和结局面板
        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        if (endingPanel != null)
            endingPanel.SetActive(false);

        //初始隐藏所有场景专属按钮面板
        if (actionButtonsPanel != null) actionButtonsPanel.SetActive(false);
        if (crimeButtonPanel != null) crimeButtonPanel.SetActive(false);
    }

    // ==================== 场景 UI 切换 ====================

    /// <summary>
    /// ⭐ 根据当前场景显示对应的按钮组
    /// </summary>
    public void UpdateUIForCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;

        Debug.Log($"[UIManager] 当前场景：{sceneName}");

        // 隐藏所有按钮组
        HideAllButtonPanels();

        // ⭐ 根据场景名称显示对应按钮
        switch (sceneName)
        {
            case "Beginning":
                // 开场场景：隐藏所有 UI，只显示剧情
                SetTopBarVisible(false);
                SetObjectiveVisible(false);
                break;

            case "DayScene":
                // 白天场景：显示行动按钮
                if (actionButtonsPanel != null) actionButtonsPanel.SetActive(true);
                SetTopBarVisible(true);
                SetObjectiveVisible(true);
                UpdateHometownButton();  // ⭐ 检查是否显示去家乡按钮
                break;

            case "CrimeScene":
                // 梦境场景：显示退出按钮
                if (crimeButtonPanel != null) crimeButtonPanel.SetActive(true);
                SetTopBarVisible(true);   // TopBar 保持显示，线索数可见
                SetObjectiveVisible(true);
                break;

            case "Hometown":
                // 家乡现场：不显示退出按钮
                if (crimeButtonPanel != null) actionButtonsPanel.SetActive(true);
                SetTopBarVisible(true);   // TopBar 保持显示
                SetObjectiveVisible(true);
                UpdateHometownButton();  // ⭐ 检查是否显示去家乡按钮
                break;

            default:
                SetTopBarVisible(false);
                SetObjectiveVisible(false);
                break;
        }
    }

    /// <summary>
    /// 隐藏所有按钮面板
    /// </summary>
    private void HideAllButtonPanels()
    {
        if (actionButtonsPanel != null) actionButtonsPanel.SetActive(false);
        if (crimeButtonPanel != null) crimeButtonPanel.SetActive(false);
    }

    /// <summary>
    /// 控制 TopBar 显示/隐藏
    /// </summary>
    private void SetTopBarVisible(bool visible)
    {
        if (dayText != null) dayText.gameObject.SetActive(visible);
        if (timeText != null) timeText.gameObject.SetActive(visible);
        if (moneyText != null) moneyText.gameObject.SetActive(visible);
        if (clueText != null) clueText.gameObject.SetActive(visible);
    }

    /// <summary>
    /// 控制 Objective 显示/隐藏
    /// </summary>
    private void SetObjectiveVisible(bool visible)
    {
        if (objectiveText != null) objectiveText.gameObject.SetActive(visible);
    }

    /// <summary>
    /// ⭐ 更新去家乡按钮显示（只有条件满足才显示）
    /// </summary>
    private void UpdateHometownButton()
    {
        if (GameManager.Instance == null || goToHometownButton == null) return;

        // 条件：梦境线索≥3 且 钱≥车费
        bool canGoToHometown =
            GameManager.Instance.dreamCluesCollected >= GameManager.Instance.dreamCluesRequired &&
            GameManager.Instance.money >= GameManager.Instance.fare;

        goToHometownButton.gameObject.SetActive(canGoToHometown);

        Debug.Log($"[UI] 家乡按钮：{canGoToHometown} (线索:{GameManager.Instance.dreamCluesCollected}/3, 钱:{GameManager.Instance.money}/{GameManager.Instance.fare})");
    }

    // ==================== 按钮事件 ====================

    /// <summary>
    /// 设置按钮事件监听
    /// </summary>
    void SetupButtonEvents()
    {
        // 确保每次场景加载后都重新设置按钮监听
        if (workButton != null)
            workButton.onClick.AddListener(() => GameManager.Instance?.TryWork());

        if (exploreButton != null)
            exploreButton.onClick.AddListener(() => GameManager.Instance?.TrySearchClue());

        if (sleepButton != null)
            sleepButton.onClick.AddListener(() => GameManager.Instance?.TrySleep());

        // 去家乡按钮
        if (goToHometownButton != null)
            goToHometownButton.onClick.AddListener(() => GameManager.Instance?.TryGoToHometown());

        // 退出梦境按钮
        if (exitCrimeSceneButton != null)
            exitCrimeSceneButton.onClick.AddListener(() => GameManager.Instance?.ExitCurrentScene());

        //重新开始游戏按钮
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        //开始游戏按钮
        if (startButton != null)
            startButton.onClick.AddListener(() => SceneLoader.Instance.LoadScene("DayScene"));

        Debug.Log("[UIManager] 按钮事件已设置");
    }

    // ==================== 场景加载回调 ====================

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[UIManager] 场景加载完成：{scene.name}");

        // 重新设置按钮监听（防止引用丢失）
        SetupButtonEvents();

        //根据新场景更新 UI 显示
        UpdateUIForCurrentScene();

        // 确保 UI 面板可见
        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (endingPanel != null) endingPanel.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ==================== UI 更新 ====================

    public void UpdateHUD(int day, int money, int clues, int maxClues, int timePeriod, string timeName)
    {
        if (dayText == null) Debug.LogWarning("[UI] dayText is null!");
        if (timeText == null) Debug.LogWarning("[UI] timeText is null!");
        if (moneyText == null) Debug.LogWarning("[UI] moneyText is null!");
        if (clueText == null) Debug.LogWarning("[UI] clueText is null!");

        if (dayText != null) dayText.text = $"天数：{day} / 15";
        if (timeText != null) timeText.text = timeName;
        if (moneyText != null) moneyText.text = $"金钱：{money}";
        if (clueText != null) clueText.text = $"线索：{clues}/{maxClues}";

        Debug.Log($"[UI 更新] 天数:{day} 金钱:{money} 线索:{clues}/{maxClues}");
    }

    public void UpdateObjective(string objective)
    {
        if (objectiveText != null) objectiveText.text = $"[目标] {objective}";
    }

    // ==================== 通知系统 ====================

    public void ShowNotification(string message, float duration = 3f)
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("[UI] Notification panel or text is null!");
            return;
        }

        StopAllCoroutines();
        notificationText.text = message;
        notificationPanel.SetActive(true);

        StartCoroutine(HideNotificationAfterDelay(duration));
    }

    IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (notificationPanel != null) notificationPanel.SetActive(false);
    }

    // ==================== 结局系统 ====================

    public void ShowEnding(string title, string description)
    {
        if (endingPanel == null)
        {
            Debug.LogWarning("[UI] Ending panel is null!");
            return;
        }

        if (endingTitle != null) endingTitle.text = title;
        if (endingDescription != null) endingDescription.text = description;

        endingPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // ==================== 重启游戏 ====================

    void OnRestartClicked()
    {
        Debug.Log("[UI] 重启游戏...");
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
        }

        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Beginning");
    }

    //1. 进入 AIGC 对话模式
    // =================================================================================
    /// <summary>
    /// 外部调用：开始与 NPC 对话 (修改版)
    /// </summary>
    public void StartAIGCDialgoue(string npcName, string playerPrompt = "好人")
    {
        // 1. 通知 GameManager 开启“对话模式”
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IsDialogueActive = true;
        }

        // 2. UI 设置
        previousSceneState = SceneManager.GetActiveScene().name;
        HideAllButtonPanels();
        SetTopBarVisible(false);

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (npcNameText != null) npcNameText.text = npcName;
        if (dialogueText != null) dialogueText.text = $"{npcName}思考中...";

        // 3. 发起 AI 请求
        // 检查 AI 处理器是否存在，然后发起协程请求
        if (AIRequestHandler.Instance != null)
        {
            dialogueText.text = "";
            // 构建包含 NPC 名字和玩家输入的 Prompt
            string fullPrompt = $"{npcName}你是一个{playerPrompt}。请以 {npcName} 的身份对接下来的问题进行简短回答，现在开始打招呼介绍你的身份。";

            // 启动协程获取 AI 回复
            StartCoroutine(AIRequestHandler.Instance.GetAIResponse(fullPrompt, OnAIResponseGenerated));
        }
        else
        {
            Debug.LogError("未找到 AIRequestHandler 实例！");
            if (dialogueText != null) dialogueText.text = "错误：AI 系统未启动";
        }

        // 4. 鼠标设置
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// AIGC 回调：AI 生成完文本后的处理
    /// </summary>
    private void OnAIResponseGenerated(string response)
    {
        if (dialogueText != null)
        {
            // 清理返回的文本（去除可能的多余空格或换行）
            dialogueText.text = response.Trim();
        }
        // 这里可以添加语音合成或打字机效果
    }

    // =================================================================================
    //2. 退出对话模式
    // =================================================================================
    /// <summary>
    /// 隐藏对话框，并恢复场景 UI
    /// </summary>
    public void HideDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // ⭐ 1. 通知 GameManager 关闭“对话模式”
        // 这会触发 GameManager 中的 Time.timeScale = 1，恢复游戏世界
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IsDialogueActive = false;
        }

        // ⭐ 2. 恢复 UI
        UpdateUIForCurrentScene_Restore();

        // ⭐ 3. 恢复鼠标 (如果需要)
        // Cursor.lockState = CursorLockMode.Locked; 
        // Cursor.visible = false;
    }

    /// <summary>
    /// 专门用于恢复场景 UI 的方法 (避免状态混乱)
    /// </summary>
    private void UpdateUIForCurrentScene_Restore()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;

        // 隐藏所有按钮组
        HideAllButtonPanels();

        // 根据场景恢复
        switch (sceneName)
        {
            case "DayScene":
                if (actionButtonsPanel != null) actionButtonsPanel.SetActive(true);
                SetTopBarVisible(true);
                SetObjectiveVisible(true);
                break;

            case "CrimeScene":
                if (crimeButtonPanel != null) crimeButtonPanel.SetActive(true);
                SetTopBarVisible(true);
                SetObjectiveVisible(true);
                break;
            case "Hometown":
                if (actionButtonsPanel != null) actionButtonsPanel.SetActive(true);
                SetTopBarVisible(true);
                SetObjectiveVisible(true);
                break;

            default:
                SetTopBarVisible(false);
                SetObjectiveVisible(false);
                break;
        }
    }

}