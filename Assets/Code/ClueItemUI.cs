using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClueItemUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI clueNameText;
    public TextMeshProUGUI clueStatusText;
    public Image backgroundImage;

    /// <summary>
    /// 设置线索显示内容
    /// </summary>
    /// <param name="name">线索名称</param>
    /// <param name="status">状态文本</param>
    /// <param name="color">背景颜色</param>
    public void SetClue(string name, string status, Color color)
    {
        if (clueNameText != null)
            clueNameText.text = name;

        if (clueStatusText != null)
            clueStatusText.text = status;

        if (backgroundImage != null)
            backgroundImage.color = color;
    }

    /// <summary>
    /// 设置线索（重载版本，使用解锁状态）
    /// </summary>
    public void SetClue(string name, bool isUnlocked)
    {
        if (isUnlocked)
        {
            SetClue(name, "已解锁", Color.green);
        }
        else
        {
            SetClue(name, "？？？", Color.gray);
        }
    }
}