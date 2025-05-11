using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerControl : MonoBehaviour
{
    public GameObject playerObject;                                     //玩家物体

    [Header("不同场景的不同移动设置")]
    public bool FirstScene = false;                                     //第一场景
    public bool SecendScene = false;                                    //第二场景

    [Header("玩家状态")]
    public int maxHealth = 3;                                           // 最大生命值
    private int currentHealth;                                          // 当前生命值
    private int score;                                                  // 得分

    private int TemporaryHealth = 0;                                    // 临时血量

    public Transform coinPoint;
    private float addScoreTimer = 0f;                                   // 计时器变量

    [Header("受伤状态设置")]
    public float invincibleTime = 3f;                                   // 受伤后的无敌时间
    private bool isInvincible = false;                                  // 是否处于无敌状态


    [Header("混乱状态设置")]
    private bool isControlReversed = false;                             // 控制是否反转
    private Coroutine reverseControlCoroutine;                          // 存储协程引用
    public GameObject ChaosVFX;                                         // 混乱特效

    [Header("被锁定状态设置")]
    public GameObject unlockVFX;                                        // 被锁定特效
    public int lockFrequency = 3;                                       // 锁定攻击次数
    [SerializeField] private int currentLockCount = 0;                  // 当前锁定次数
    private Coroutine lockAttackCoroutine;

    [Header("吸引金币状态设置")]
    private bool isMagnetActive = false;                                // 磁铁是否激活
    private float magnetRange = 10f;                                    // 磁铁吸引范围
    private Coroutine magnetCoroutine;                                 // 磁铁效果协程

    [Header("移动设置")]
    [SerializeField] 
    private float moveSpeed = 15f;                                       //左右移动速度
    [SerializeField] 
    private Vector2 moveBoundaryMin = new Vector2(-10f, -10f);          //移动范围限制
    [SerializeField] 
    private Vector2 moveBoundaryMax = new Vector2(10f, 10f);

    [Header("跳跃参数")]
    [SerializeField] private float jumpHeight = 6f;                     //跳跃最大高度
    [SerializeField] private float jumpDuration = 0.6f;                 //跳跃持续时间
    private bool isJumping = false;                                     //是否正在跳跃
    private float originalY;

    [Header("翻滚参数")]
    [SerializeField] private float rollDuration = 0.6f;                 //翻滚持续时间
    private bool canRollOver = true;
    private bool isRolling = false;                                     // 是否正在翻滚

    [Header("第一场景移动设置")]
    public List<GameObject> movePoint = new List<GameObject>();         //存放3个移动点位置
    private bool isMoving = false;                                      //移动限制

    private InputSystemActions inputActions;
    private void Awake()
    {
        inputActions=new InputSystemActions();
    }
    void Start()
    {
        currentHealth = maxHealth;
        score = 0;

        UIControl.Instance.SetHpText(currentHealth, maxHealth);
    }

    void Update()
    {
        if (!isRolling)  // 只有不在翻滚状态才能移动和跳跃
        {
            if (FirstScene)
            {
                MovementFirst();
            }
            if (SecendScene)
            {
                MovementSecond();
            }
            CheckJump();
        }
        RollOver();
    }

    private void OnEnable()
    {
        inputActions.PC.Enable();
    }

    private void OnDisable()
    {
        inputActions.PC.Disable();
    }


    #region 初始化
    public void InitializationLaser()
    {

    }
    #endregion

    #region 触发器内容处理
    private void OnTriggerEnter(Collider other)
    {
        if (!isInvincible)
        {
            if (other.CompareTag("Obstacle"))
            {

                ObstacleAttribute obstacle = other.GetComponentsInChildren<ObstacleAttribute>()[0];
                if (obstacle != null)
                {
                    switch (obstacle.ObstacleType)
                    {
                        case ObstacleType.Ordinary:
                            TakeDamage();
                            break;

                        case ObstacleType.Chaos:
                            // 如果已经有正在运行的协程，先停止它
                            if (reverseControlCoroutine != null)
                            {
                                StopCoroutine(reverseControlCoroutine);
                            }
                            SetControlReverse(true);
                            ChaosVFX.SetActive(false);
                            ChaosVFX.SetActive(true);
                            // 启动新的协程并保存引用
                            reverseControlCoroutine = StartCoroutine(ResetControlAfterDelay(10f));
                            break;

                        case ObstacleType.Lock:
                            // 重置锁定次数
                            currentLockCount = 0;

                            //if (lockAttackCoroutine!=null)
                            //{
                            //    StopCoroutine(lockAttackCoroutine);
                            //}
                            if (lockAttackCoroutine==null)
                            {
                                lockAttackCoroutine = StartCoroutine(LockAttackRoutine());
                            }
                            break;
                    }
                }
            }
        }
        

        if (other.CompareTag("Coin"))
        {
            CoinPickUp(other.gameObject, 1);
        }

        if (other.CompareTag("GigCoin"))
        {
            CoinPickUp(other.gameObject, 5);
        }


        if (other.CompareTag("Item"))
        {
            PropsSetting obstacle = other.GetComponentsInChildren<PropsSetting>()[0];
            if (obstacle != null)
            {
                switch (obstacle.propsType)
                {
                    case PropsType.PointGold:

                        ObstacleGenerator.Instance.ActiveObstacleChange();
                        Debug.Log("点金");
                        break;

                    case PropsType.Recovery:
                        RecoveryHp();
                        Debug.Log("回血");
                        break;
                    case PropsType.ClearScreen:
                        TemporaryHealth = 1;

                        UIControl.Instance.SetHpProtect(true);
                        ObstacleGenerator.Instance.ActiveObstacleDisable();
                        Debug.Log("清屏");
                        break;
                    case PropsType.Magnet:
                        if (magnetCoroutine != null)
                        {
                            StopCoroutine(magnetCoroutine);
                        }
                        magnetCoroutine = StartCoroutine(MagnetEffect());
                        Debug.Log("磁铁");
                        break;
                }

                Destroy(other.gameObject);
            }
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("AddL"))
        {
            addScoreTimer += Time.deltaTime;
            if (addScoreTimer >= 0.1f)
            {
                score++;
                UIControl.Instance.SetScoreText(score);
                addScoreTimer = 0f;
            }
        }

        if (other.CompareTag("Blockade"))
        {
            if (!isInvincible)
            {
                //不在无敌状态下受伤
                TakeDamage();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
    }

    //受伤
    private void TakeDamage()
    {
        if (TemporaryHealth==1)
        {
            TemporaryHealth = 0;
            UIControl.Instance.SetHpProtect(false);
            ObstacleGenerator.Instance.ActiveObstacleDisable();
        }
        else
        {
            currentHealth--;
        }
        Debug.Log($"受到伤害！当前生命值：{currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            GameOver();
        }
        else
        {
            StartCoroutine(InvincibleRoutine());
        }

        UIControl.Instance.SetHpText(currentHealth, maxHealth);
    }

    //拾取金币
    private void CoinPickUp(GameObject other, int score)
    {
        //取出金币，不再跟随移动
        other.transform.SetParent(null);
        other.GetComponent<CoinSetting>().SetCanMove(true);
        other.GetComponent<BoxCollider>().enabled = false;
        CollectCoin(score);

        //拾取动画，向上移动，加快旋转
        other.GetComponent<RotateSelf>().rotationSpeed = new Vector3(0, 300, 0);
        other.transform.DOMoveY(coinPoint.position.y + 2, 1f);

        DOVirtual.DelayedCall(1.1f, () => Destroy(other.gameObject));
    }

    //回血
    private void RecoveryHp()
    {
        currentHealth++;
        if (currentHealth >= maxHealth)
        {
            currentHealth= maxHealth;
        }
        UIControl.Instance.SetHpText(currentHealth, maxHealth);
    }


    //加分
    private void CollectCoin(int a)
    {
        score+=a;
        UIControl.Instance.SetScoreText(score);
    }

    //无敌
    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        // 可以在这里添加无敌状态的视觉效果，比如闪烁等
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    //反转操作
    private IEnumerator ResetControlAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        ChaosVFX.SetActive(false);
        SetControlReverse(false);

    }

    //锁定攻击
    private IEnumerator LockAttackRoutine()
    {
        while (currentLockCount < lockFrequency)
        {
            GameObject effect = Instantiate(unlockVFX, playerObject.transform.position, Quaternion.identity);
            currentLockCount++;
            yield return new WaitForSeconds(4f); // 每次攻击间隔1秒
        }
        lockAttackCoroutine = null;
    }

    //吸引金币
    private IEnumerator MagnetEffect()
    {
        isMagnetActive = true;
        float duration = 15f; // 磁铁持续时间
        float timer = 0f;

        while (timer < duration)
        {
            // 查找场景中所有金币
            GameObject[] normalCoins = GameObject.FindGameObjectsWithTag("Coin");
            GameObject[] gigCoins = GameObject.FindGameObjectsWithTag("GigCoin");

            // 处理普通金币
            foreach (GameObject coin in normalCoins)
            {
                ProcessCoin(coin);
            }

            // 处理大金币
            foreach (GameObject coin in gigCoins)
            {
                ProcessCoin(coin);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isMagnetActive = false;
        magnetCoroutine = null;
    }
    private void ProcessCoin(GameObject coin)
    {
        if (coin != null)
        {
            CoinSetting coinSetting = coin.GetComponent<CoinSetting>();
            float zDistance = Mathf.Abs(coin.transform.position.z - transform.position.z);
            if (zDistance <= magnetRange)
            {
                coin.transform.SetParent(null);
                coinSetting.SetCanMove(true);
            }
        }
    }

    #endregion


    #region 玩家跳跃
    //玩家跳跃
    private void CheckJump()
    {
        Vector3 moveVector2 = transform.position;
        bool isMovingTo = false;
        foreach (var item in movePoint)
        {
            if (SecendScene)
            {
                isMovingTo = false;
                break;
            }
            if (Mathf.Abs(item.transform.position.x - moveVector2.x)<=0.5f)
            {
                isMovingTo = false;
                break;
            }
            else
            {
                isMovingTo = true;
            }
        }

        if (isControlReversed)
        {

            Vector2 moveInput3 = inputActions.PC.Move.ReadValue<Vector2>();
            float vert = moveInput3.y;
            if (vert < 0 && !isJumping && !isMovingTo)
            {
                PerformJump();
            }
        }
        else
        {
            // 只有在没有水平移动输入且没有在跳跃时才允许跳跃
            if (inputActions.PC.Jump.triggered && !isJumping && !isMovingTo)
            {
                PerformJump();
            }
        }



    }
    private void PerformJump()
    {
        isJumping = true;
        originalY = transform.position.y;

        // 创建只影响Y轴的贝塞尔曲线动画
        DOTween.To(() => transform.position.y,
            (y) => transform.position = new Vector3(transform.position.x, y, transform.position.z),
            originalY + jumpHeight,
            jumpDuration /2)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // 下落动画
                DOTween.To(() => transform.position.y,
                    (y) => transform.position = new Vector3(transform.position.x, y, transform.position.z),
                    originalY,
                    jumpDuration /2)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => {
                        isJumping = false;
                    });
            });
    }
    #endregion

    #region 玩家翻滚
    private void RollOver()
    {
        // 在跳跃、移动或已经在翻滚中时，禁止翻滚
        if (isJumping || isMoving || isRolling) return;

        Vector2 moveInput = inputActions.PC.Move.ReadValue<Vector2>();
        float vert = moveInput.y;
        if (isControlReversed)
        { 
            if (inputActions.PC.Jump.triggered && canRollOver)
            {
                StartRoll();
            }
        }
        else
        {
            if (vert < 0 && canRollOver)  // 修改为只有按下S键才触发翻滚
            {
                StartRoll();
            }
        }

    }

    private void StartRoll()
    {
        isRolling = true;
        canRollOver = false;

        // 保存原始缩放值
        Vector3 originalScale = playerObject.transform.localScale;


        // 执行翻滚动画
        playerObject.transform.DOScale(new Vector3(
                    originalScale.x,
                    originalScale.y / 2,
                    originalScale.z), rollDuration * 0.3f).SetEase(Ease.OutQuad);

        playerObject.transform.DOMoveY(0.5f, 0.1f);

        // 延迟恢复
        DOVirtual.DelayedCall(rollDuration, () => {
            playerObject.transform.DOMoveY(1.25f, 0.1f);
            playerObject.transform.DOScale(
                originalScale, 
                rollDuration * 0.3f).SetEase(Ease.InQuad).OnComplete(() =>
            SetRollOverEnd());
        });
    }

    private void SetRollOverEnd()
    {
        isRolling = false;
        canRollOver = true;
    }
    #endregion

    #region 第一场景左右移动控制
    //第一场景左右移动
    private void MovementFirst()
    {
        //获取inputSystem的输出
        Vector2 moveVector2 = inputActions.PC.Move.ReadValue<Vector2>();
        //获取左右移动
        float htal = moveVector2.x;

        // 如果控制反转，则反转输入
        if (isControlReversed)
        {
            htal = -htal;
        }

        if (movePoint.Count == 0) return;

        // 如果正在移动中，不执行后续操作
        if (isMoving) return;

        // 获取当前位置
        Vector3 currentPosition = transform.position;

        // 找到最近的左侧和右侧点
        GameObject leftPoint = null;
        GameObject rightPoint = null;
        float minLeftDistance = float.MaxValue;
        float minRightDistance = float.MaxValue;

        foreach (GameObject point in movePoint)
        {
            if (point == null) continue;

            float xDiff = point.transform.position.x - currentPosition.x;

            // 检查左侧点
            if (xDiff < 0 && Mathf.Abs(xDiff) < minLeftDistance)
            {
                leftPoint = point;
                minLeftDistance = Mathf.Abs(xDiff);
            }
            // 检查右侧点
            else if (xDiff > 0 && Mathf.Abs(xDiff) < minRightDistance)
            {
                rightPoint = point;
                minRightDistance = Mathf.Abs(xDiff);
            }
        }

        // 根据输入移动到最近的点
        if (htal < 0 && leftPoint != null)  // 向左移动
        {
            isMoving = true;
            transform.DOMoveX(leftPoint.transform.position.x, 0.5f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => isMoving = false);
        }
        else if (htal > 0 && rightPoint != null)  // 向右移动
        {
            isMoving = true;
            transform.DOMoveX(rightPoint.transform.position.x, 0.5f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => isMoving = false);
        }

    }

    // 公共方法供其他脚本调用
    public void SetControlReverse(bool reverse)
    {
        isControlReversed = reverse;
    }

    #endregion

    #region 第二场景左右移动控制
    //第二场景左右移动
    private void MovementSecond()
    {
        //获取inputSystem的输出
        Vector2 moveVector2=inputActions.PC.Move.ReadValue<Vector2>();  
        //获取左右移动
        float htal = moveVector2.x;

        //获取W和S的输出（S翻滚、空格跳跃）
        float vert = moveVector2.y;

        Vector3 movement = new Vector3(htal, 0.0f, 0f).normalized;
        transform.position += movement * Time.deltaTime * moveSpeed;

        // 限制移动范围
        float clampedX = Mathf.Clamp(transform.position.x, moveBoundaryMin.x, moveBoundaryMax.x);
        float clampedZ = Mathf.Clamp(transform.position.z, moveBoundaryMin.y, moveBoundaryMax.y);
        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);

    }
    #endregion


    private void GameOver()
    {
        Debug.Log("游戏结束！");
        // 在这里添加游戏结束的逻辑
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 绘制移动边界
        Gizmos.color = Color.blue;
        Vector3 center = new Vector3(
            (moveBoundaryMin.x + moveBoundaryMax.x) / 2,
            1f,
            (moveBoundaryMin.y + moveBoundaryMax.y) / 2
        );
        Vector3 size = new Vector3(
            moveBoundaryMax.x - moveBoundaryMin.x,
            0.1f,
            moveBoundaryMax.y - moveBoundaryMin.y
        );
        Gizmos.DrawWireCube(center, size);

    }
#endif
}
