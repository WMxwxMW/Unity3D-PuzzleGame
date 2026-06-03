using UnityEngine;

public class IDcard : MonoBehaviour, IInteractable
{
    public string itemName = "IDcard";
    public GameObject pickupEffect; // 拾取时的粒子特效 (可选)

    public void Interact()
    {
        Debug.Log($"你拾取了：{itemName}");
        UIManager.Instance?.ShowNotification("按 F 翻找寻找线索");
        // 播放特效
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

    }

    public string GetPromptText()
    {
        return $"拾取 {itemName}";
    }
}