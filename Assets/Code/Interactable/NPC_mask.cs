using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC_mask : MonoBehaviour
{


    [Header("UI & Game Ref")]
    public UIManager uiManager;
    public GameManager gameManager;

    // 标记玩家是否在身边
    private bool isPlayerNearby = false;
    public static bool hasGame = false;

    // 当有物体进入触发器范围时自动调用
    void OnTriggerEnter(Collider other)
    {
        // 检查进入的物体是否带有 "Player" 标签
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家进入了对话范围！");
            isPlayerNearby = true;
            if (uiManager != null)
                uiManager.ShowNotification("按 [F] 与前台交谈");
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
            if (hasGame)
            {
                uiManager.ShowNotification("老板很满意你的工作，让你回去找你身份证进行签约，并支付了你上午半天的酬劳。（请尽情探索，按E键结束探索）");
                Invoke("DelayedMethod", 2f);
            }
            else
            {
                uiManager.ShowNotification("这里有一份简单工作需要你去完成，完成之后我们再继续谈。");
                SceneLoader.Instance.LoadScene("Mask");
            }
                
            
        }
    }
    public void DelayedMetho2()
    {
        SceneLoader.Instance.LoadScene("Mask");
    }

    public void DelayedMethod()
    {
        SceneLoader.Instance.LoadScene("DayScene");
    }

}