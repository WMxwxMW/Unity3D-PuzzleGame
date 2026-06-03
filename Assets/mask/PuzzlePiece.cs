using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    // 正确位置（用于胜利判定）
    public int correctRow;
    public int correctCol;

    // 当前位置（用于移动逻辑）
    public int currentRow;
    public int currentCol;

    private PuzzleManager puzzleManager;

    void Start()
    {
        // 优先尝试从父物体获取（假设拼图块是管理器的子物体），这比 FindObjectOfType 更快更稳
        if (transform.parent != null)
        {
            puzzleManager = transform.parent.GetComponent<PuzzleManager>();
        }

        // 如果父物体没有，再回退到全局查找（作为保底）
        if (puzzleManager == null)
        {
            puzzleManager = FindObjectOfType<PuzzleManager>();
        }
    }

    void OnMouseDown()
    {
        // 确保管理器存在才调用，防止报错
        if (puzzleManager != null)
        {
            puzzleManager.OnPieceClicked(this);
        }
    }
}