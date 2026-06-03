using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject piecePrefab; // 拼图块的预制体
    public Sprite[] sprites;       // 从 Sprite Editor 切割后得到的6个Sprite
    public int rows = 2;           // 行数
    public int cols = 3;           // 列数
    public float pieceSize = 100f; // 每个拼图块的大小

    private GameObject[,] pieces;  // 二维数组存储所有拼图块对象
    private int emptyRow;          // 空白格的行
    private int emptyCol;          // 空白格的列
    void Start()
    {
        InitializePuzzle();
        ShufflePuzzle();
    }

    // 初始化拼图，按正确顺序生成所有块
    void InitializePuzzle()
    {
        pieces = new GameObject[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // 右下角最后一个位置留空
                if (r == rows - 1 && c == cols - 1)
                {
                    emptyRow = r;
                    emptyCol = c;
                    continue;
                }

                // 计算当前块对应的sprite索引
                int spriteIndex = r * cols + c;

                // 实例化拼图块
                GameObject piece = Instantiate(piecePrefab, transform);
                piece.GetComponent<SpriteRenderer>().sprite = sprites[spriteIndex];

                // 设置位置
                Vector3 pos = new Vector3(c * pieceSize, -r * pieceSize, 0);
                piece.transform.localPosition = pos;

                // 设置 PuzzlePiece 组件的属性
                PuzzlePiece pieceScript = piece.GetComponent<PuzzlePiece>();
                pieceScript.correctRow = r;
                pieceScript.correctCol = c;
                pieceScript.currentRow = r;
                pieceScript.currentCol = c;

                // 存入数组
                pieces[r, c] = piece;
            }
        }
    }

    // 通过模拟随机移动来打乱拼图，确保拼图一定可解
    void ShufflePuzzle()
    {
        int shuffleMoves = 100; // 随机移动的步数
        for (int i = 0; i < shuffleMoves; i++)
        {
            // 找到所有可以和空白格交换的邻居
            System.Collections.Generic.List<PuzzlePiece> possibleMoves = new System.Collections.Generic.List<PuzzlePiece>();

            if (emptyRow > 0) possibleMoves.Add(pieces[emptyRow - 1, emptyCol].GetComponent<PuzzlePiece>()); // 上方
            if (emptyRow < rows - 1) possibleMoves.Add(pieces[emptyRow + 1, emptyCol].GetComponent<PuzzlePiece>()); // 下方
            if (emptyCol > 0) possibleMoves.Add(pieces[emptyRow, emptyCol - 1].GetComponent<PuzzlePiece>()); // 左方
            if (emptyCol < cols - 1) possibleMoves.Add(pieces[emptyRow, emptyCol + 1].GetComponent<PuzzlePiece>()); // 右方

            // 随机选择一个邻居进行移动
            PuzzlePiece randomPiece = possibleMoves[Random.Range(0, possibleMoves.Count)];
            MovePiece(randomPiece);
        }
    }

    // 响应拼图块的点击事件
    public void OnPieceClicked(PuzzlePiece clickedPiece)
    {
        // 检查点击的拼图块是否与空白格相邻
        if (IsAdjacent(clickedPiece.currentRow, clickedPiece.currentCol, emptyRow, emptyCol))
        {
            MovePiece(clickedPiece);
            CheckWin();
        }
    }

    // 判断两个位置是否相邻 (曼哈顿距离为1)
    bool IsAdjacent(int r1, int c1, int r2, int c2)
    {
        return Mathf.Abs(r1 - r2) + Mathf.Abs(c1 - c2) == 1;
    }

    // 执行移动操作
    void MovePiece(PuzzlePiece pieceToMove)
    {
        // 1. 获取要移动到的目标位置（即当前空白格的位置）
        int targetRow = emptyRow;
        int targetCol = emptyCol;

        // 2. 更新数据模型
        // 交换二维数组中的引用
        pieces[pieceToMove.currentRow, pieceToMove.currentCol] = pieces[emptyRow, emptyCol];
        pieces[emptyRow, emptyCol] = pieceToMove.gameObject;

        // 更新各自的当前位置
        int tempRow = pieceToMove.currentRow;
        int tempCol = pieceToMove.currentCol;
        pieceToMove.currentRow = targetRow;
        pieceToMove.currentCol = targetCol;
        emptyRow = tempRow;
        emptyCol = tempCol;

        // 3. 更新视觉表现（移动GameObject的位置）
        Vector3 targetPosition = pieceToMove.transform.localPosition;
        pieceToMove.transform.localPosition = new Vector3(targetCol * pieceSize, -targetRow * pieceSize, 0);
    }

    // 检查是否获胜
    void CheckWin()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // 跳过空白格
                if (r == rows - 1 && c == cols - 1) continue;

                // 只要有一个块不在正确位置，就返回
                if (pieces[r, c].GetComponent<PuzzlePiece>().correctRow != r ||
                    pieces[r, c].GetComponent<PuzzlePiece>().correctCol != c)
                {
                    return;
                }
            }
        }
        // 如果循环结束都没有返回，说明所有块都在正确位置
        Debug.Log("恭喜你，拼图完成！");
        NPC_mask.hasGame = true;
        SceneManager.LoadScene(4);
    }
}