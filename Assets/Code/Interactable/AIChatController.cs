using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class AIChatController : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_InputField inputField;
    public Button sendButton;
    public TMP_Text chatLogText;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("AIChatController成功启动！");
        // 给按钮添加点击事件
        sendButton.onClick.AddListener(OnSendButtonClicked);
        // 使用 onSubmit 更符合回车发送的语义
        inputField.onSubmit.AddListener((val) => OnSendButtonClicked());
    }

    void OnSendButtonClicked()
    {
        
        chatLogText.text = "";
        Debug.Log("点击按钮 成功启动！");
        // 获取用户输入
        string question = inputField.text.Trim();
        Debug.Log(question);
        if (string.IsNullOrEmpty(question)) return;
        // UI 更新：显示用户的问题
        AppendToChatLog("我: " + question);
        inputField.text = "";
        sendButton.interactable = false;

        // --- 线索检测逻辑 开始 ---
        bool hasFoundClue = false;
        string aiResponseOverride = "";
        // 遍历线索库，检查关键词
        foreach (var cluePair in GameManager.Instance.clueDict)
        {
            int index = cluePair.Key;
            ClueData data = cluePair.Value;

            // 如果该线索已经收集过了，跳过
            if (GameManager.Instance.HasClue(index)) continue;

            // 检查玩家输入中是否包含任意一个关键词
            foreach (string keyword in data.triggerKeywords)
            {
                if (question.Contains(keyword))
                {
                    // 1. 标记找到了线索
                    hasFoundClue = true;

                    // 2. 调用 GameManager 增加线索
                    GameManager.Instance.AddClue(index);

                    // 3. 准备回复内容（可以是线索自带的，也可以是通用的）
                    aiResponseOverride = data.discoveryDialog;
                    break;
                }
            }
            if (hasFoundClue) break; // 找到一个就跳出
        }
        // --- 线索检测逻辑 结束 ---



        // 检查单例是否存在
        if (AIRequestHandler.Instance == null)
        {
            Debug.LogError("AIRequestHandler.Instance 未初始化！");
            sendButton.interactable = true;
            return;
        }
        // 调用 AIRequestHandler 发送请求
        StartCoroutine(AIRequestHandler.Instance.GetAIResponse(question, (aiResponse) =>
        {
            // 防止回调时对象已被销毁，接收回调：显示 AI 的回答
            if (this == null) return;
            sendButton.interactable = true;
            AppendToChatLog(aiResponse);
            // 自动重新聚焦输入框
            inputField.ActivateInputField();
        }));
    }

    void AppendToChatLog(string msg)
    {
        if (chatLogText != null)
        {
            chatLogText.text += msg + "\n\n";
        }
        Debug.Log(msg);
    }
}