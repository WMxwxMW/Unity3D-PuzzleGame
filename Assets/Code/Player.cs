using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    [Header("组件引用")]
    private CharacterController playerController;
    private Animator anim;
    private NavMeshAgent agent; // 新增：用于自动寻路

    [Header("移动设置")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float dreamSpeed = 8f; // 梦境中更快/更飘
    [SerializeField] private float rotationSpeed = 10f;

    [Header("状态")]
    public bool isInDream = false; // 由 GameManager 控制此状态
    private Vector3 targetPosition;
    private bool isMovingToTarget = false;

    [Header("控制状态")]
    private bool isInputLocked = false; // 是否锁定输入 (由 UI Manager 控制)

    void Start()
    {
        // 获取组件
        playerController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        //添加并配置 NavMeshAgent
        // 如果物体上没有，自动添加一个
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        // 配置 Agent 基础属性
        agent.speed = walkSpeed;
        agent.angularSpeed = rotationSpeed * 60; // 转换角度速度
        agent.acceleration = 50f;
        agent.stoppingDistance = 0.1f; // 离目标多近停止


        if (playerController == null) Debug.LogError("缺少 CharacterController 组件！");
        if (anim == null) Debug.LogError("缺少 Animator 组件！");
    }

    void Update()
    {
        // 如果输入被锁定（例如正在对话），不处理任何移动逻辑
        // 防止玩家在对话时误触鼠标导致角色乱跑
        if (isInputLocked)
        {
            // 强制停止 Agent，防止它在后台继续消耗算力计算路径
            agent.isStopped = true;
            agent.velocity = Vector3.zero; // 瞬间停止
            return; // 跳出 Update
        }
        else
        {
            // 如果之前被停止了，现在恢复
            agent.isStopped = false;
        }

        
        // 1. 根据当前状态更新速度 (梦境 vs 现实)
        UpdateMovementMode();

        // 2. 处理鼠标点击输入
        HandleMouseInput();

        // 3. 更新动画状态
        UpdateAnimation();

    }

    /// <summary>
    /// 切换梦境/现实的速度和移动风格
    /// </summary>
    void UpdateMovementMode()
    {
        if (isInDream)
        {
            // 梦境模式：速度更快，可能无惯性（由 Agent 参数控制）
            agent.speed = dreamSpeed;
            agent.acceleration = 100f; // 瞬间加速，像飘过去
        }
        else
        {
            // 现实模式：正常行走速度
            agent.speed = walkSpeed;
            agent.acceleration = 30f; // 有起步和刹车感
        }
    }

    /// <summary>
    /// 处理鼠标点击地面移动
    /// </summary>
    void HandleMouseInput()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 从摄像机发射射线到鼠标位置
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 检测是否击中了地面 (需要地面有 "Ground" 层或特定标签，或者只要击中任何可导航物体)
            // 这里假设地面有 NavMesh，或者直接检测碰撞体
            if (Physics.Raycast(ray, out hit, 100f))
            {
                // 可选：只允许点击特定的地面层，防止点击到敌人或道具触发移动
                // if (hit.transform.CompareTag("Ground")) 
                {
                    SetDestination(hit.point);
                }
            }
        }
    }

    /// <summary>
    /// 设置寻路目标
    /// </summary>
    void SetDestination(Vector3 position)
    {
        // 将目标点稍微抬高一点，防止因为高度误差导致寻路失败
        Vector3 target = new Vector3(position.x, transform.position.y, position.z);

        agent.SetDestination(target);
        isMovingToTarget = true;

        Debug.Log($"前往目标: {target}");
    }

    /// <summary>
    /// 更新动画器
    /// </summary>
    void UpdateAnimation()
    {
        // NavMeshAgent 有一个属性叫 remainingDistance
        // 如果距离大于 0.5 且 路径有效，说明正在移动
        bool isActuallyMoving = agent.remainingDistance > 0.5f && agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathComplete;

        // 如果到达终点，停止移动状态
        if (!isActuallyMoving && isMovingToTarget)
        {
            isMovingToTarget = false;
            // 可选：到达后强制重置速度为0，防止滑步
            agent.velocity = Vector3.zero;
        }

        // 设置 Animator 参数
        anim.SetBool("isRun", isActuallyMoving);

        // 调试用
        // if(isActuallyMoving) Debug.Log("Walking...");
    }

    // 供外部脚本调用（例如 GameManager 切换场景状态时）
    public void SetDreamState(bool isDream)
    {
        isInDream = isDream;
    }

    /// <summary>
    /// 锁定或解锁玩家输入
    /// </summary>
    public void SetInputLock(bool locked)
    {
        isInputLocked = locked;
        // 同步更新 Agent 状态
        agent.isStopped = locked;
        //如果锁定，直接把速度设为0，彻底停止物理模拟
        if (locked) agent.velocity = Vector3.zero;
    }
}