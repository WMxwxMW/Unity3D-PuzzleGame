using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    // ==================== [1] 单例模式 ====================
    public static GameManager Instance { get; private set; }
    private bool isDataInitialized = false;
    public event Action OnCluesChanged;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] 新实例创建并设为常驻");
        }
        else
        {
            // 2. 如果已经有实例
            // 判断是否是“重新开始游戏”或者“加载初始场景”
            if (Instance != this)
            {
                // 销毁旧的实例，保留新的
                DestroyImmediate(Instance.gameObject);
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[GameManager] 旧实例被销毁，使用新实例");

                // 可选：强制重置数据
                InitGame();
            }
            else
            {
                // 同一个实例，防止重复添加
                Destroy(gameObject);
            }
        }
    }

    // ==================== [2] 游戏阶段 ====================
    public enum GamePhase
    {
        Intro,                      // 开场噩梦
        Phase1_Day1_Story,          // 第一天强制剧情
        Phase2_DreamReconstruction, // 梦境重建 (Day2+)
        Phase3_Hometown             // 家乡
    }

    public GamePhase currentPhase = GamePhase.Intro;
    public bool isInCrimeScene = false;  // 在梦境或家乡
    public bool isGameOver = false;

    // ==================== [3] 剧情进度 ====================
    public bool hasCompletedDay1Morning = false;
    public bool hasCompletedDay1Afternoon = false;
    public bool hasFoundClue1 = false;
    public bool hasCompletedDay1Night = false;
    public string mainScene = "Dayscene";

    // ==================== [4] 线索系统 ====================
    public bool[] clues = new bool[7] { false, false, false, false, false, false , false };
    public int unlockedCluesCount = 0;

    // 梦境线索 (DayScene + CrimeScene 收集 5 个0-4)
    public int dreamCluesCollected = 0;
    public int dreamCluesRequired = 5;

    // 家乡线索 (Hometown 收集 2 个)
    public int hometownCluesCollected = 0;
    public int hometownCluesRequired = 2;

    public bool hasNewClueSinceLastDream = false;
    public int currentDreamLevel = 0;


    // 定义线索数据结构
    [System.Serializable]
    public class ClueData
    {
        public int clueId; // 线索编号
        public string clueName;
        public string[] triggerKeywords; // 触发该线索的关键词数组
        public string discoveryDialog;   // 发现线索时，AI 应该回复的话术（可选）
    }

    // 在 GameManager 中声明线索库
    public ClueData[] clueDatabase;

    // 为了方便调试，定义一个字典来快速通过索引获取数据
    public Dictionary<int, ClueData> clueDict = new Dictionary<int, ClueData>();

    // ==================== [5] 经济与时间 ====================
    public int money = 50;
    public int wage = 100;
    public int dailyExpense = 20;
    public int fare = 120;

    public int day = 1;
    public int maxDays = 15;

    public int timePeriod = 0;
    public bool isWorking = false;

    public bool hasFare = false;

    // ==================== [6] 结局枚举 ====================
    public enum EndingType
    {
        None,
        LuLuWuWei,
        LateTruth,
        JusticeServed,
    }

    // ==================== [7] 结局文本 ====================
    private Dictionary<EndingType, (string title, string desc)> endingData =
        new Dictionary<EndingType, (string, string)>
        {
            { EndingType.LuLuWuWei,
                ("结局: 碌碌无为",
                 "15天过去了，似乎因为错过很多机会，随着日子一点点推移，直至死亡，你也没能找到梦境的真相。") },

            { EndingType.LateTruth,
                ("结局: 回不去的故乡",
                 "一切似乎尽在眼前，但是一文钱难倒英雄汉，故乡再也回不去了。") },

            { EndingType.JusticeServed,
                ("结局: 皆大欢喜！",
                 "善恶终有报！ 你竭尽所能终于发现了一切的真相。") },

        };

    // ==================== [8] 初始化 ====================
    public void InitGame()
    {
        day = 1;
        money = 50;
        timePeriod = 0;
        isWorking = false;
        unlockedCluesCount = 0;
        dreamCluesCollected = 0;
        hometownCluesCollected = 0;
        hasNewClueSinceLastDream = false;
        isGameOver = false;
        currentPhase = GamePhase.Intro;
        isInCrimeScene = false;
        hasFare = false;
        hasCompletedDay1Morning = false;
        hasCompletedDay1Afternoon = false;
        hasFoundClue1 = false;
        hasCompletedDay1Night = false;

        for (int i = 0; i < clues.Length; i++) clues[i] = false;

        Debug.Log("=== 游戏初始化完成 - 开场噩梦 ===");
        UpdateUI();
    }

    private void Start()
    {
        if (!isDataInitialized)
        {
            InitGame();
            isDataInitialized = true;
        }
        clueDict.Clear();
        for (int i = 0; i < clueDatabase.Length; i++)
        {
            clueDict[i] = clueDatabase[i];
        }
    }

    public void ResetGame()
    {
        isDataInitialized = false;
        InitGame();
        isDataInitialized = true;
        Debug.Log("[GameManager] 游戏已重置");
    }

    // ==================== [9] 开场流程 ====================
    public void StartIntro()
    {
        currentPhase = GamePhase.Intro;
        Debug.Log("[GameManager] 开始开场噩梦...");

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene("Beginning");  
        }
    }

    public void FinishIntro()
    {
        currentPhase = GamePhase.Phase1_Day1_Story;
        day = 1;
        timePeriod = 0;

        Debug.Log("[GameManager] 开场结束，进入第一天上午");

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene("DayScene");  
        }
        Invoke("Day1Notification", 2f);
        UpdateUI();
    }
    public void Day1Notification()
    {
        ShowNotification("Day 1 - 父母为你找了份工作，今天就先去工作吧！（按W前往工作场地进行工作）");
    }

    // ==================== [10] 第一天强制剧情 ====================
    public void TryWork()
    {
        if (isGameOver) return;

        if (currentPhase == GamePhase.Phase1_Day1_Story && !hasCompletedDay1Morning)
        {
            if (timePeriod != 0)
            {
                ShowNotification("第一次的工作结束了，熟能生巧，下次就不用这么做了。（后续赚钱按W直接获得一天的工资并跳转时间）");
                return;
            }
            SceneManager.LoadScene(4);
            Debug.Log("[剧情] 第一天上午：老板说没带身份证，先做半天");
            hasCompletedDay1Morning = true;
            money += wage / 2;

            AdvanceTimePeriod();
            UpdateUI();
            return;
        }

        if (isWorking)
        {
            CompleteWork();
            return;
        }

        if (timePeriod == 2)
        {
            ShowNotification("夜深回去休息吧，别工作了，要劳逸结合。");
            return;
        }
        Debug.Log("[打工] 开始工作...");
        isWorking = true;
        AdvanceTimePeriod();
        ShowNotification("努力打工赚钱吧！");
        UpdateUI();
    }

    private void CompleteWork()
    {
        money += wage;
        isWorking = false;
        Debug.Log($"[打工] 工作完成！获得 {wage} 元");
        ShowNotification($"工作终于结束了！赚到了{wage}元");
        AdvanceTimePeriod();
        UpdateUI();
    }

    //在外面探索一圈之后想要进入下一个阶段之后就要探险。
    public void TrySearchClue()
    {
        if (isGameOver) return;

        if (currentPhase == GamePhase.Phase1_Day1_Story && !hasCompletedDay1Afternoon)
        {

            Debug.Log("[剧情] 第一天下午：回家找身份证，发现身份证和书信");
            if (clues[0])
            {
                hasCompletedDay1Afternoon = true;
                hasFoundClue1 = true;
                ShowNotification("你发现了一封信。来自于人贩子给你养父母的？！");
                AdvanceTimePeriod();
                UpdateUI();
                currentPhase = GamePhase.Phase2_DreamReconstruction;
            }
            return;
        }

        if (timePeriod == 2)
        {
            ShowNotification("晚上不宜外出。");
            return;
        }

        if (isWorking)
        {
            ShowNotification("先完成工作吧！");
            return;
        }

        Debug.Log("[探索] 外出寻找线索...");
        ShowNotification("探索结束！(在场景中进行交互或者寻找人物交流寻找线索)");
        AdvanceTimePeriod();
        UpdateUI();
    }

    //阶段一的睡觉。
    public void TrySleep()
    {
        if (isGameOver) return;

        if (currentPhase == GamePhase.Phase1_Day1_Story && !hasCompletedDay1Night)
        {
            if (timePeriod != 2)
            {
                ShowNotification("还没有到睡觉的时候吧，等入夜了再睡吧。");
                return;
            }

            Debug.Log("[剧情] 第一天晚上：自动进入梦境，寻找线索 2 植被");
            hasCompletedDay1Night = true;

            money -= dailyExpense;
            Debug.Log($"扣除生活费 {dailyExpense} 元");


            isInCrimeScene = true;
            ShowNotification("进入梦乡......");

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene("CrimeScene");  // ⭐ 睡觉进入 CrimeScene
            }

            UpdateUI();
            return;
        }

        if (timePeriod != 2)
        {
            ShowNotification("还没有到睡觉的时候吧，等入夜了再睡吧。");
            return;
        }

        money -= dailyExpense;
        if (money < 0) Debug.Log("目前负有债务！");

        if (hasNewClueSinceLastDream)
        {
            Debug.Log($"带着新线索入睡... 梦境层级:{currentDreamLevel}");
            isInCrimeScene = true;
            ShowNotification("进入梦乡......");

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene("CrimeScene");  // ⭐ 睡觉进入 CrimeScene
            }
        }
        else
        {
            Debug.Log("没有新线索，做了个混乱的梦");
            ShowNotification("脑中一片混乱，什么都没有发生（未触发新线索）。但是新的一天开始了，我今天应该做什么呢？（根据底下提示决定今日活动内容）");
            StartNewDay();
        }

        UpdateUI();
    }

    // ==================== [11] 线索系统 ====================
    public void AddClue(int clueIndex)
    {
        if (isGameOver) return;
        if (clueIndex < 0 || clueIndex >= clues.Length) return;

        if (!clues[clueIndex])
        {
            clues[clueIndex] = true;
            unlockedCluesCount++;
            currentDreamLevel = unlockedCluesCount;
            hasNewClueSinceLastDream = true;

            // 统计梦境线索 (线索 2 植被，3 小吃 ，4 地名 ，0 身份证，1 信，5 证词，6 秃额头)
            if (clueIndex < 5)
            {
                dreamCluesCollected++;
            }
            // 统计家乡线索 (线索 5-6 是家乡线索)
            else
            {
                hometownCluesCollected++;
            }
            if(clueIndex == 3)
                clues[4] = true;
            Debug.Log($"[线索] 获得线索 #{clueIndex}! 梦境:{dreamCluesCollected}/5, 家乡:{hometownCluesCollected}/2");


            // 检查是否可以进入家乡
            if (dreamCluesCollected >= dreamCluesRequired && currentPhase == GamePhase.Phase2_DreamReconstruction)
            {
                ShowNotification("家乡的线索已经收集齐全，按 G 查看是否满足全部条件。");
                if (money >= fare)
                {
                    currentPhase = GamePhase.Phase3_Hometown;
                    ShowNotification("现在可以回老家了！");
                }
                else
                {
                    ShowNotification($"梦境完整了！ 但是车费还需要{fare - money}元。");
                }
            }

            UpdateUI();
        }
        OnCluesChanged?.Invoke();
    }

    //是否拥有某线索
    public bool HasClue(int clueIndex)
    {
        if (clueIndex < 0 || clueIndex >= clues.Length) return false;
        return clues[clueIndex];
    }


    // ==================== [12] 退出梦境 ====================
    public void ExitCurrentScene()
    {
        if (isGameOver) return;
        Debug.Log("[GameManager] ExitCurrentScene 被调用！");
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;

        Debug.Log($"[GameManager] 退出场景：{sceneName}");

        if (sceneName == "CrimeScene")
        {
            // 从梦境醒来
            hasNewClueSinceLastDream = false;
            isInCrimeScene = false;

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene("DayScene");
            }

            Debug.Log("从梦境醒来，回到现实...");
            ShowNotification("新的一天开始了，我今天应该做什么呢？（根据底下提示决定今日活动内容）");

            StartNewDay();
        }
        else if (sceneName == "Hometown")
        {
            // 从家乡返回（未完成结局）
            isInCrimeScene = false;

            Debug.Log("从家乡返回...");

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene("DayScene");
            }
        }
    }

    // ==================== [13] 去家乡 ====================
    public void TryGoToHometown()  
    {
        if (isGameOver) return;

        if (currentPhase != GamePhase.Phase3_Hometown)
        {
            if (dreamCluesCollected < dreamCluesRequired)
            {
                ShowNotification($"需要{dreamCluesRequired}条梦境中的线索！ (现在： {dreamCluesCollected})");
                return;
            }

            if (money < fare)
            {
                ShowNotification($"还没有足够的钱用来车费。还需要 {fare - money} 元.");
                return;
            }

            currentPhase = GamePhase.Phase3_Hometown;
            ShowNotification("梦境完整了！现在可以尝试回老家了！");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateUIForCurrentScene();
            }

            return;
        }

        // 前往家乡
        if (money >= fare)
        {
            Debug.Log("[家乡] 前往家乡...");
            money -= fare;
            isInCrimeScene = true;
            hasFare = true;
            UpdateUI();

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene("Hometown");  
            }
        }
        else
        {
            ShowNotification($"钱不够！还需要 {fare - money} 。");
        }
    }

    // ==================== [14] 完成家乡场景（结局） ====================
    public void FinishHometown(bool isPerfectEnding)  
    {
        if (isGameOver) return;
        isInCrimeScene = false;

        if (isPerfectEnding)
        {
            TriggerEnding(EndingType.JusticeServed);
        }
        else
        {
            TriggerEnding(EndingType.LuLuWuWei);
            
        }
    }

    // ==================== [15] 时间与天数 ====================
    private void StartNewDay()
    {
        day++;
        timePeriod = 0;
        isWorking = false;
        Debug.Log($"=== 第 {day} 天 上午 开始 ===");
        if (day > maxDays && hasFare == false && dreamCluesCollected != 5)
        {
            TriggerEnding(EndingType.LuLuWuWei);
            return;
        }
        
            UpdateUI();
    }

    private void AdvanceTimePeriod()
    {
        timePeriod++;
        if (timePeriod > 3)
        {
            StartNewDay();
        }
    }

    // ==================== [16] 经济系统 ====================
    public void EarnMoney(int amount)
    {
        if (isGameOver) return;
        money += amount;
        Debug.Log($"赚了 {amount} 元");
        UpdateUI();
    }

    public bool SpendMoney(int amount)
    {
        if (isGameOver) return false;
        if (money >= amount)
        {
            money -= amount;
            Debug.Log($"花费 {amount} 元");
            UpdateUI();
            return true;
        }
        Debug.Log("金钱不足！");
        return false;
    }

    // ==================== [17] 结局系统 ====================
    public void TriggerEnding(EndingType endingType)
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log($"Game Over! Ending: {endingType}");
        

        if (UIManager.Instance != null)
        {
            var endingInfo = endingData[endingType];
            UIManager.Instance.ShowEnding(endingInfo.title, endingInfo.desc);
        }
    }

    // ==================== [18] UI 更新 ====================
    private void UpdateUI()
    {
        if (isGameOver) return;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHUD(
                day, money, unlockedCluesCount, clues.Length, timePeriod, GetTimePeriodName()
            );

            string objective = GetObjectiveText();
            UIManager.Instance.UpdateObjective(objective);
        }

        Debug.Log($"[UI] Day:{day} Money:{money} Clues:{unlockedCluesCount} Dream:{dreamCluesCollected}/3 Hometown:{hometownCluesCollected}/3");
    }

    private string GetObjectiveText()
    {
        switch (currentPhase)
        {
            case GamePhase.Intro:
                return "从噩梦中醒来......";

            case GamePhase.Phase1_Day1_Story:
                if (!hasCompletedDay1Morning) return "必须工作！";
                if (!hasCompletedDay1Afternoon) return "在家寻找身份证。";
                if (!hasCompletedDay1Night) return "该睡觉了。";
                return "第一天结束了。";

            case GamePhase.Phase2_DreamReconstruction:
                if (dreamCluesCollected < dreamCluesRequired)
                    return $"收集{dreamCluesRequired}线索({dreamCluesCollected}/5)";
                else if (money < fare)
                    return $"回老家的车费需要{fare}（现在: {money}）";
                else
                    return "回老家去吧！";

            case GamePhase.Phase3_Hometown:
                return $"收集老家的{hometownCluesRequired}条线索({hometownCluesCollected}/2)";

            default:
                return "错误！";
        }
    }

    public string GetTimePeriodName()
    {
        return timePeriod switch
        {
            0 => "白天",
            1 => "白天",
            2 => "夜晚",
            _ => "未知"
        };
    }

    // ==================== [19] 通知系统 ====================
    public void ShowNotification(string msg)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification(msg);
        }
        Debug.Log("[通知] " + msg);
    }
    // ==================== 对话系统 ====================
    //对话状态 (防止对话时时间流逝)
    private bool isDialogueActive = false;
    //属性包装器，供UIManager调用
    public bool IsDialogueActive
    {
        get { return isDialogueActive; }
        set
        {
            isDialogueActive = value;
            // 1. 时间暂停
            Time.timeScale = value ? 0 : 1;

            // 2. 查找 Player 并锁定输入
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                player.SetInputLock(value); // value 是 true(锁定) 或 false(解锁)
            }

        }
    }
    // ==================== [20] 调试快捷键 ====================
    void Update()
    {
        //新增：如果正在对话，直接返回，不处理任何游戏逻辑
        if (isDialogueActive)
        {
            return;// 跳过所有逻辑，只保留对话 UI 运行
        }

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha1)) AddClue(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) AddClue(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) AddClue(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) AddClue(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) AddClue(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) AddClue(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) AddClue(6);
        if (Input.GetKeyDown(KeyCode.W)) TryWork();
        if (Input.GetKeyDown(KeyCode.E)) TrySearchClue();
        if (Input.GetKeyDown(KeyCode.Space)) TrySleep();
        if (Input.GetKeyDown(KeyCode.G)) TryGoToHometown();
        if (Input.GetKeyDown(KeyCode.Alpha0)) EarnMoney(100);
        if (Input.GetKeyDown(KeyCode.C)) ExitCurrentScene();
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("尝试打开线索系统。");
        }

        if (clues[6]) FinishHometown(true);
#endif
    }
}