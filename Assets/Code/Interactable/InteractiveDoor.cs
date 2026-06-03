using UnityEngine;

public class InteractiveDoor : MonoBehaviour, IInteractable
{
    public string promptText = "按 F键 开门";
    private bool isOpen = false;

    // 记录当前是否在范围内
    private bool playerInRange = false;

    public string GetPromptText()
    {
        return promptText;
    }

    // 当玩家进入触发范围
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // 找到玩家身上的 InteractionManager 并通知它
            InteractionManager manager = other.GetComponent<InteractionManager>();
            if (manager != null)
            {
                manager.SetCurrentInteractable(this, true);
            }
        }
    }

    // 当玩家离开触发范围
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            InteractionManager manager = other.GetComponent<InteractionManager>();
            if (manager != null)
            {
                manager.SetCurrentInteractable(this, false);
            }
        }
    }

    public void Interact()
    {
        if (!playerInRange) return; // 如果不在范围内，按了也没用

        if (!isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    void OpenDoor()
    {
        isOpen = true;
        // 隐藏
        gameObject.SetActive(false);
        
        Debug.Log("门开了！");
    }

    void CloseDoor()
    {
        isOpen = false;
        // 激活
        gameObject.SetActive(true);

        Debug.Log("门关了！");
    }
}