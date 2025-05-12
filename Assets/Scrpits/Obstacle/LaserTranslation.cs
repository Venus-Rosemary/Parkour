using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTranslation : MonoBehaviour
{
    [Header("激光设置")]
    public GameObject laserPrefab;                // 激光预制体
    public float moveSpeed = 5f;                  // 移动速度
    public float moveRange = 10f;                 // 移动范围（从中心点向两侧）
    public float destroyDelay = 3f;              // 销毁延迟

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

    private void Start()
    {
    }

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
            Destroy(bullet);
        }
    }
    #endregion

    #region 发射子弹2
    private IEnumerator ShootProjectiles(bool leftToRight)
    {
        if (shootPoints.Count < 3) yield break;

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
            yield return new WaitForSeconds(bulletInterval2);
        }
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
            Destroy(projectile);
        }
    }
    #endregion

}
