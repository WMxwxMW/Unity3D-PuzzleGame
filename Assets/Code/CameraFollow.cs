using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target;
    public Player playerScript;

    [Header("基础偏移 (现实模式)")]
    public Vector3 realOffset = new Vector3(0, 2, -4);
    public float realSmoothSpeed = 10f;

    [Header("基础偏移 (梦境模式)")]
    public Vector3 dreamOffset = new Vector3(0, 6, -10);
    public float dreamSmoothSpeed = 4f;

    [Header("旋转控制设置")]
    public float mouseSensitivity = 2.0f; // 鼠标灵敏度
    public float minVerticalAngle = -10f; // 最低能看到哪里 (防止看脚底)
    public float maxVerticalAngle = 60f;  // 最高能看到哪里 (防止翻过头)

    [Header("自动回正设置")]
    public bool autoResetAngle = true; // 松开鼠标后是否自动转回背后
    public float resetSpeed = 2.0f;    // 回正速度

    // 内部变量
    private float currentHorizontalAngle = 0f; // 绕角色的水平角度
    private float currentVerticalAngle = 20f;  // 初始垂直角度 (稍微俯视)
    private Vector3 targetOffset;
    private float currentSmoothSpeed;
    private bool isRotating = false;

    void Start()
    {
        if (playerScript == null && target != null)
            playerScript = target.GetComponent<Player>();

        // 初始化角度，避免第一帧跳变
        currentHorizontalAngle = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 确定当前模式参数
        bool isDream = (playerScript != null) ? playerScript.isInDream : false;
        targetOffset = isDream ? dreamOffset : realOffset;
        currentSmoothSpeed = isDream ? dreamSmoothSpeed : realSmoothSpeed;

        // 2. 处理鼠标输入 (右键旋转)
        HandleMouseRotation();

        // 3. 计算摄像机理想位置
        // 基于目标点 + 旋转后的偏移量
        Vector3 desiredPosition = CalculateDesiredPosition();

        // 4. 平滑移动摄像机位置
        transform.position = Vector3.Lerp(transform.position, desiredPosition, currentSmoothSpeed * Time.deltaTime);

        // 5. 强制看向目标 (确保镜头始终对准玩家)
        transform.LookAt(target.position + Vector3.up * 1.5f); // 看向玩家头部附近
    }

    void HandleMouseRotation()
    {
        // 检测是否按住鼠标右键
        if (Input.GetMouseButton(1))
        {
            isRotating = true;

            // 获取鼠标移动量
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // 更新水平角度 (绕着玩家转)
            currentHorizontalAngle += mouseX * mouseSensitivity;

            // 更新垂直角度 (抬头低头)
            currentVerticalAngle -= mouseY * mouseSensitivity;

            // 限制垂直角度，防止翻转
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        }
        else
        {
            isRotating = false;

            // 如果开启了自动回正，且当前不在旋转，则慢慢转回默认角度
            if (autoResetAngle)
            {
                // 简单的回正逻辑：慢慢趋向于 0 (背后) 和 初始垂直角 (比如 20)
                // 注意：这里假设"背后"是相对玩家的局部坐标系，实际实现可能需要更复杂的向量计算
                // 为了简化，我们暂时只回正垂直角度，水平角度保持最后的位置（这样更自然）

                currentVerticalAngle = Mathf.Lerp(currentVerticalAngle, 20f, resetSpeed * Time.deltaTime);
            }
        }
    }

    Vector3 CalculateDesiredPosition()
    {
        // 核心数学：极坐标转换

        // 1. 获取目标位置
        Vector3 targetPos = target.position;

        // 2. 将水平角度转换为弧度
        float radian = currentHorizontalAngle * Mathf.Deg2Rad;

        // 3. 计算水平面上的偏移 (X 和 Z)
        // 注意：这里我们使用 targetOffset 的 magnitude (距离) 作为半径
        // 但为了保留梦境/现实的距离差异，我们动态计算半径
        float distance = targetOffset.magnitude;

        // 实际上，更好的做法是分解 offset
        // 让我们用更直观的方法：基于当前角度构建旋转矩阵

        // 方法 B (推荐): 先创建一个标准背后的向量，然后旋转它
        Vector3 baseOffset = new Vector3(0, targetOffset.y, -Mathf.Abs(targetOffset.z));

        // 创建水平旋转 (绕 Y 轴)
        Quaternion horizontalRot = Quaternion.Euler(0, currentHorizontalAngle, 0);

        // 创建垂直旋转 (绕 X 轴，相对于水平旋转后的坐标系有点复杂，所以用球坐标公式更稳)

        // --- 使用球坐标公式 (最稳健) ---
        float radius = new Vector3(targetOffset.x, 0, targetOffset.z).magnitude; // 水平距离
        float height = targetOffset.y;

        // 垂直角度会影响实际的高度和水平距离
        float verticalRad = currentVerticalAngle * Mathf.Deg2Rad;

        float actualHeight = height + radius * Mathf.Sin(verticalRad);
        float actualRadius = radius * Mathf.Cos(verticalRad);

        // 计算最终的 X 和 Z
        float posX = targetPos.x + actualRadius * Mathf.Sin(radian);
        float posZ = targetPos.z - actualRadius * Mathf.Cos(radian); // 注意 Z 的方向

        return new Vector3(posX, targetPos.y + actualHeight, posZ);
    }

    // 在编辑器中画出摄像机的活动范围，方便调试
    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target.position, 0.5f);

        Vector3 pos = CalculateDesiredPosition();
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(target.position, pos);
        Gizmos.DrawWireSphere(pos, 0.3f);
    }
}