using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.F;

    private IInteractable currentInteractable;

    void Update()
    {
        // 检测当前是否有物体在范围内
        // 我们通过查找场景中所有标记为 "InRange" 的对象，或者让门自己通知管理器

        // 更简单的做法：直接在 Update 里检测按键，然后让最近的门响应
        // 但为了配合上面的 Door 脚本，我们可以这样写：

        if (Input.GetKeyDown(interactKey))
        {
            // 尝试寻找范围内所有的 Interactable
            // 这里需要一个简单的方法来知道谁在范围内
            // 最简单的方法：让 Door 脚本在 Enter/Exit 时注册自己到管理器

            if (currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }
    }

    // 供 Door 脚本调用的方法，告诉管理器“我现在可以交互了”
    public void SetCurrentInteractable(IInteractable interactable, bool inRange)
    {
        if (inRange)
        {
            currentInteractable = interactable;
            UIManager.Instance?.ShowNotification(interactable.GetPromptText()); 
        }
        else
        {
            if (currentInteractable == interactable)
            {
                currentInteractable = null;
                //UIManager.Instance?.HidePrompt();
            }
        }
    }
}