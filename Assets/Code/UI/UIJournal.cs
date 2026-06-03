using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIJournal : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI dreamLevelText;
    [SerializeField] private GameObject Panel;

    [System.Serializable]
    public class ClueItem
    {
        public TextMeshProUGUI clueName;
    }
    [SerializeField] private List<ClueItem> clueItems = new List<ClueItem>();

    private bool isOpen = false;

    // 1. 当对象被激活时（进入场景/显示）
    private void OnEnable()
    {
        // 订阅事件
        if (GameManager.Instance != null)
        {
            // 1.1 先取消订阅（防止重复绑定）
            GameManager.Instance.OnCluesChanged -= UpdateClueDisplay;
            // 1.2 再订阅
            GameManager.Instance.OnCluesChanged += UpdateClueDisplay;

            // 1.3 立即刷新一次，显示当前数据
            UpdateClueDisplay();
        }
    }

    // 2. 当对象被禁用时（离开场景/隐藏）
    private void OnDisable()
    {
        // 取消订阅，防止内存泄漏和对已销毁对象的操作
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCluesChanged -= UpdateClueDisplay;
        }
    }

    // 3. Start 中只做初始化，不要 DontDestroyOnLoad
    private void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        // 日记面板由新场景的 UIManager 或按钮重新打开
        Debug.Log(" 线索系统 初始化完成");

        // 初始隐藏
        if (Panel != null)
            canvasGroup.alpha = 0;
    }

    void Update()
    {
        // 防止在对话或游戏结束时操作
        //if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // 监听 J 键
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("UI尝试打开线索系统。");
            Toggle();
        }
    }

    public void Toggle()
    {
        if (Panel == null) return;

        // 确保 Panel 是激活的
        if (!Panel.activeSelf)
        {
            canvasGroup.alpha = 1;
            Open(); // 直接打开
        }
        else
        {
            // 如果已经激活，则切换开关状态
            if (isOpen) Close();
            else Open();
        }
    }

    public void Open()
    {
        isOpen = true;
        // 安全检查
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        Time.timeScale = 0f; // 暂停游戏
        UpdateClueDisplay(); // 再次刷新，确保打开瞬间数据最新
    }

    public void Close()
    {
        isOpen = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        Time.timeScale = 1f; // 恢复游戏
    }

    // 更新线索显示
    public void UpdateClueDisplay()
    {
        if (GameManager.Instance == null) return;

        // 更新线索列表
        for (int i = 0; i < clueItems.Count; i++)
        {
            // 检查当前槽位和文本组件是否有效
            if (clueItems[i] == null || clueItems[i].clueName == null) continue;

            bool hasClue = GameManager.Instance.HasClue(i);
            clueItems[i].clueName.text = hasClue ? GetClueName(i) : "???";
        }

        // 更新进度文本
        if (dreamLevelText != null)
        {
            dreamLevelText.text = $"线索进度: {GameManager.Instance.unlockedCluesCount}/7";
        }
    }

    // 辅助方法：根据ID返回线索名称
    private string GetClueName(int clueID)
    {
        switch (clueID)
        {
            case 0: return "身份证";
            case 1: return "来自X地的信";
            case 2: return "家乡的尖树";
            case 3: return "熟悉的菜肴";
            case 4: return "X地地名";
            case 5: return "久住人的证词";
            case 6: return "真凶";
            default: return "未知线索";
        }
    }
}