using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTranslation : Singleton<LaserTranslation>
{
    [Header("激光设置")]
    public GameObject laserPrefab;                // 激光预制体
    public float moveSpeed = 5f;                  // 移动速度
    public float moveRange = 10f;                 // 移动范围（从中心点向两侧）

    [Header("生成位置")]
    public bool creatLaser = false;
    public bool creatBlockade = false;
    public bool createBullet = false;             // 是否创建子弹
    public bool createProjectile = false;                         // 是否发射

    private GameObject currentLaser;              // 当前激光物体


    [Header("障碍物设置")]
    public GameObject blockadePrefab;             // 障碍物预制体
    public GameObject warningPrefab;              // 警告提示预制体
    public float blockadeDestroyTime = 15f;       // 障碍物存在时间
    public float warningTime = 5f;                // 警告时间
    private GameObject currentBlockade;           // 当前障碍物
    private GameObject currentWarning;            // 当前警告

    [Header("子弹设置")]
    public GameObject bulletPrefab;               // 子弹预制体
    public Transform shootPoint;                  // 发射点
    public float bulletSpeed = 8f;                // 子弹速度
    public Transform playerTransform;            // 玩家位置引用
    public float bulletInterval1 = 1f;                             // 发射间隔

    [Header("多点发射设置")]
    public List<Transform> shootPoints = new List<Transform>();    // 发射点列表
    public GameObject projectilePrefab;                            // 发射物预制体
    public float projectileSpeed = 40f;                            // 发射物速度
    public float bulletInterval2 = 0.3f;                             // 发射间隔


    [Header("子弹管理")]
    private List<GameObject> activeBullets = new List<GameObject>();    // 活跃子弹列表


    [Header("攻击管理")]
    private bool isAttacking = false;             // 是否正在攻击
    private float attackCooldown = 3f;            // 攻击间隔时间
    private bool startAttackLoop = false;         // 是否开始攻击循环
    private bool isAttackComplete = false;        // 攻击是否完成

    // 在Start方法中添加
    private void Start()
    {

    }



    // Update方法保持不变
    private void Update()
    {
        if (creatLaser)
        {
            //这个生成一次只能有一条激光，只有之前激光销毁后才能生成下一条，不然之前那条激光无法销毁
            GenerateLaser(Random.Range(0, 2) == 0 ? true : false);
        }

        if (creatBlockade)
        {
            GenerateBlockade();
        }

        if (createBullet)
        {
            StartCoroutine(ShootBullets());
            createBullet = false;
        }
        if (createProjectile)
        {
            StartCoroutine(ShootProjectiles(Random.Range(0, 2) == 0));
            createProjectile = false;
        }
    }


    #region 初始化
    public void InitializeLaserSystem()
    {
        // 重置所有状态
        isAttacking = false;
        startAttackLoop = false;
        isAttackComplete = true;
        creatLaser = false;
        creatBlockade = false;
        createBullet = false;
        createProjectile = false;

        // 清理现有物体
        if (currentLaser != null) Destroy(currentLaser);
        if (currentBlockade != null) Destroy(currentBlockade);
        if (currentWarning != null) Destroy(currentWarning);
        // 清理所有活跃子弹
        foreach (var bullet in activeBullets)
        {
            if (bullet != null)
            {
                Destroy(bullet);
            }
        }
        activeBullets.Clear();

        // 检查必要组件
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null)
            {
                Debug.LogWarning("未找到玩家引用，部分攻击可能无法正常工作");
            }
        }

        // 检查预制体引用
        if (laserPrefab == null || blockadePrefab == null ||
            warningPrefab == null || bulletPrefab == null ||
            projectilePrefab == null)
        {
            Debug.LogError("部分预制体引用缺失，请检查Inspector面板设置");
        }

        // 检查发射点设置
        if (shootPoint == null)
        {
            Debug.LogWarning("未设置子弹发射点，将使用当前物体位置");
        }

        if (shootPoints.Count < 3)
        {
            Debug.LogWarning("多点发射点数量不足3个，该攻击模式可能无法正常工作");
        }
    }
    #endregion

    #region 攻击管理
    // 开始攻击循环
    public void StartAttackLoop()
    {
        if (!startAttackLoop)
        {
            startAttackLoop = true;
            StartCoroutine(AttackManager());
        }
    }

    // 停止攻击循环
    public void StopAttackLoop()
    {
        startAttackLoop = false;
        StopCoroutine(AttackManager());
        // 清理当前攻击
        StopLaser();
        if (currentBlockade != null) Destroy(currentBlockade);
        if (currentWarning != null) Destroy(currentWarning);
    }

    private IEnumerator AttackManager()
    {
        while (startAttackLoop)
        {
            if (!isAttacking)
            {
                isAttacking = true;
                isAttackComplete = false;
                // 随机选择攻击类型
                int attackType = Random.Range(0, 4);

                switch (attackType)
                {
                    case 0:
                        creatLaser = true;
                        break;
                    case 1:
                        creatBlockade = true;
                        break;
                    case 2:
                        createBullet = true;
                        break;
                    case 3:
                        createProjectile = true;
                        break;
                }

                // 等待攻击完成
                yield return new WaitUntil(() => isAttackComplete);

                // 冷却时间
                yield return new WaitForSeconds(attackCooldown);
                isAttacking = false;
            }
            yield return null;
        }
    }

    #endregion

    #region 平移激光
    // 生成激光
    public void GenerateLaser(bool moveToRight = true)
    {
        creatLaser = false;
        // 计算生成位置
        Vector3 spawnPos = new Vector3(
            moveToRight ? -moveRange : moveRange,
            laserPrefab.transform.position.y,
            laserPrefab.transform.position.z
        );

        // 生成激光
        currentLaser = Instantiate(laserPrefab, spawnPos, Quaternion.identity);

        // 设置移动
        float targetX = moveToRight ? moveRange : -moveRange;

        // 使用DOTween执行移动
        currentLaser.transform.DOMoveX(targetX, moveSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                // 移动完成后销毁
                Destroy(currentLaser);
                isAttackComplete = true;
            });

        // 设置自动销毁（作为备用销毁机制）
        //Destroy(currentLaser, destroyDelay);
    }

    // 停止当前激光
    public void StopLaser()
    {
        if (currentLaser != null)
        {
            DOTween.Kill(currentLaser.transform);
            Destroy(currentLaser);
        }
    }
    #endregion

    #region 半区封锁
    // 生成障碍物
    public void GenerateBlockade()
    {
        creatBlockade = false;
        if (currentBlockade != null)
        {
            Destroy(currentBlockade);
        }

        // 随机选择左右半场
        float randomX = Random.Range(0, 2) == 0 ?
            -moveRange / 2 : moveRange / 2;

        Vector3 blockadePos = new Vector3(
            randomX,
            blockadePrefab.transform.position.y,
            blockadePrefab.transform.position.z
        );

        // 先生成警告
        currentWarning = Instantiate(warningPrefab, blockadePos, Quaternion.identity);

        // 5秒后生成障碍物并销毁警告
        StartCoroutine(SpawnBlockadeAfterWarning(blockadePos));
    }

    private IEnumerator SpawnBlockadeAfterWarning(Vector3 position)
    {
        yield return new WaitForSeconds(warningTime);

        // 销毁警告
        if (currentWarning != null)
        {
            Destroy(currentWarning);
        }

        // 生成障碍物
        currentBlockade = Instantiate(blockadePrefab, position, Quaternion.identity);
        currentBlockade.tag = "Blockade";

        // 设置自动销毁
        Destroy(currentBlockade, blockadeDestroyTime);


        yield return new WaitForSeconds(blockadeDestroyTime);
        isAttackComplete = true;
    }
    #endregion

    #region 发射子弹
    private IEnumerator ShootBullets()
    {
        for (int i = 0; i < 3; i++)
        {
            if (playerTransform != null)
            {
                // 从指定位置创建子弹
                Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position;
                Vector3 spawnPoint = new Vector3(spawnPos.x, bulletPrefab.transform.position.y, spawnPos.z);
                GameObject bullet = Instantiate(bulletPrefab, spawnPoint, Quaternion.identity);

                activeBullets.Add(bullet);  // 添加到列表

                Vector3 playerPoint = new Vector3(playerTransform.transform.position.x,
                    bulletPrefab.transform.position.y,
                    playerTransform.transform.position.z);
                // 计算方向
                Vector3 direction = (playerPoint - spawnPoint).normalized;

                // 启动子弹移动协程
                StartCoroutine(MoveBullet(bullet, direction));

                yield return new WaitForSeconds(bulletInterval1);
            }
        }

        isAttackComplete = true;
    }

    private IEnumerator MoveBullet(GameObject bullet, Vector3 direction)
    {
        float elapsedTime = 0f;
        Vector3 startPos = bullet.transform.position;

        while (elapsedTime < 3f && bullet != null)
        {
            bullet.transform.position += direction * bulletSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (bullet != null)
        {
            activeBullets.Remove(bullet);  // 从列表移除
            Destroy(bullet);
        }
    }
    #endregion

    #region 发射子弹2
    private IEnumerator ShootProjectiles(bool leftToRight)
    {
        if (shootPoints.Count < 3)
        {
            isAttackComplete = true;
            yield break;
        }

        var points = new List<Transform>(shootPoints);
        if (!leftToRight)
        {
            points.Reverse();
        }

        foreach (Transform point in points)
        {
            Vector3 apoint = new Vector3(point.position.x, projectilePrefab.transform.position.y,
                point.position.z);
            GameObject projectile = Instantiate(projectilePrefab, apoint, Quaternion.identity);
            StartCoroutine(MoveProjectile(projectile));

            activeBullets.Add(projectile);  // 添加到列表

            yield return new WaitForSeconds(bulletInterval2);
        }
        isAttackComplete = true;
    }

    private IEnumerator MoveProjectile(GameObject projectile)
    {
        float elapsedTime = 0f;
        while (elapsedTime < 3f && projectile != null)
        {
            projectile.transform.Translate(Vector3.back * projectileSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (projectile != null)
        {
            activeBullets.Remove(projectile);  // 从列表移除
            Destroy(projectile);
        }
    }
    #endregion

}
