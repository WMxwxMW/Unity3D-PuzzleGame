using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_DialogueTrigger : MonoBehaviour
{
    [Header("对话设置")]
    public string npcName = "休息的居民";
    public string IdentifySet = "你好，我是第一次见到你。";
    public int clue = 0;
    public int clue2 = 0;
    public bool hasMet = false;

    [Header("UI & Game Ref")]
    public UIManager uiManager;
    public GameManager gameManager;

    // 标记玩家是否在身边
    private bool isPlayerNearby = false;

    // 当有物体进入触发器范围时自动调用
    void OnTriggerEnter(Collider other)
    {
        // 检查进入的物体是否带有 "Player" 标签
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家进入了对话范围！");
            isPlayerNearby = true;
            if (uiManager != null)
                uiManager.ShowNotification("按 [F] 与 " + npcName + " 交谈"); 
        }
    }

    // 当物体离开创发器范围时自动调用
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家离开了对话范围。");
            isPlayerNearby = false;
            if (uiManager != null)
                uiManager.HideDialogue();
        }
    }

    private void Start()
    {
        if (uiManager == null) uiManager = UIManager.Instance;
        if (gameManager == null) gameManager = GameManager.Instance;
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        if (uiManager != null)
        {
            uiManager.StartAIGCDialgoue(npcName, IdentifySet);
            gameManager.AddClue(clue);
            gameManager.AddClue(clue2);
            //if (clue == 4 || clue2 == 4) uiManager.ShowNotification("家乡的线索已经收集齐全，按 G 查看是否满足全部条件。");
        }
    }
}