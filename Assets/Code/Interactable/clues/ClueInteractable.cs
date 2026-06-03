using UnityEngine;

public class ClueInteractable : MonoBehaviour
{
    [Header("线索配置")]
    public int clueID = 0; // 对应GameManager.clues数组的索引
    public string promptMessage = "这里似乎有什么东西"; // 提示文本
    public bool isCollected = false; // 是否已被收集

    private void OnGUI()
    {
        // 简单的提示显示 (实际项目中建议使用 TextMeshProUGUI 提示框)
        if (isCollected) return; // 已收集则不显示

        // 计算屏幕位置
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        // 检查物体是否在摄像机视野内
        if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
        {
            // 显示提示文字
            GUI.color = Color.yellow;
            GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y, 100, 30), promptMessage);
        }
    }

    void Update()
    {
        // 防止重复触发
        if (isCollected) return;

        // 检测是否按下了 F 键
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 检测玩家是否在物体附近
            if (IsPlayerNear())
            {
                CollectClue();
            }
        }
    }

    // 检测玩家是否在附近
    private bool IsPlayerNear()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        return distance <= 3f; // 距离阈值
    }

    // 收集线索
    private void CollectClue()
    {
        // 1. 调用 GameManager 添加线索
        GameManager.Instance.AddClue(clueID);
        UIManager.Instance.ShowNotification("收集到了新线索，按 J 查看线索，根据页面提示按 E 或 C 进入下个阶段。");
        // 2. 标记为已收集，防止重复触发
        isCollected = true;

        // 3. 可选：隐藏物体或播放特效
        //gameObject.SetActive(false);

        // 4. 可选：播放获得线索音效
        // AudioSource.PlayClipAtPoint(collectSound, transform.position);

        Debug.Log($"[交互] 玩家获得了线索 ID: {clueID}");
        if (clueID == 2) UIManager.Instance.ShowNotification("好多尖树？好像听隔壁的邻居厨师大叔说过，不然去问一下吧!");
    }
}