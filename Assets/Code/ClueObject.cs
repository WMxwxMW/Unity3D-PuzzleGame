using UnityEngine;
using TMPro;

/// <summary>
/// 场景中的可交互线索物体
/// 挂载到每个线索物体上
/// </summary>
public class ClueObject : MonoBehaviour
{
    [Header("=== 线索配置 ===")]
    [SerializeField] private int clueIndex;
    [SerializeField] private string clueName;
    [SerializeField] private string clueDescription;

    [Header("=== 显示状态 ===")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private GameObject collectedIndicator;
    [SerializeField] private TextMeshProUGUI clueNameText;

    [Header("=== 交互设置 ===")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private CrimeSceneController sceneController;
    private bool isCollected = false;
    private bool isPlayerNear = false;

    public void Initialize(int index, CrimeSceneController controller)
    {
        clueIndex = index;
        sceneController = controller;

        // 检查是否已收集
        if (GameManager.Instance != null)
        {
            isCollected = GameManager.Instance.HasClue(clueIndex);
            UpdateVisuals();
        }
    }

    private void Update()
    {
        if (isCollected) return;

        // 检测玩家距离（简化版，实际可用触发器）
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        isPlayerNear = distance <= interactDistance;

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(isPlayerNear);
        }

        // 交互输入
        if (isPlayerNear && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void Interact()
    {
        if (isCollected) return;

        Debug.Log($"[线索] 发现：{clueName} - {clueDescription}");

        if (sceneController != null)
        {
            sceneController.CollectClue(clueIndex);
        }

        isCollected = true;
        UpdateVisuals();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification($"Found clue: {clueName}!");
        }
    }

    private void UpdateVisuals()
    {
        if (collectedIndicator != null)
        {
            collectedIndicator.SetActive(isCollected);
        }

        if (clueNameText != null)
        {
            clueNameText.text = isCollected ? $"{clueName} " : clueName;
        }
    }

    private void OnDrawGizmos()
    {
        // 在编辑器中显示交互范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}