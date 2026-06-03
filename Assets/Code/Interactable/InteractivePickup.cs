using UnityEngine;

public class InteractivePickup : MonoBehaviour, IInteractable
{
    public string itemName = "神秘水晶";
    public GameObject pickupEffect; // 拾取时的粒子特效 (可选)

    public void Interact()
    {
        Debug.Log($"你拾取了：{itemName}");

        // 播放特效
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        // 销毁物体 (或者放入背包)
        Destroy(gameObject);
    }

    public string GetPromptText()
    {
        return $"拾取 {itemName}";
    }
}