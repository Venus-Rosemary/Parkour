using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ObstacleType
{
    Ordinary,           //正常
    Chaos,              //混乱
    Lock                //锁定
}

public enum PropsType
{
    GigCoin,
    PointGold,
    Recovery,
    ClearScreen,
    Magnet
}

public class ObstacleGenerator : Singleton<ObstacleGenerator>
{
    [Header("生成设置")]
    public List<GameObject> obstacles = new List<GameObject>();         // 路障预制体列表
    public GameObject specialObstacle;                                  // 特殊障碍物预制体
    [Range(0, 100)]
    public float specialObstacleChance = 10f;                          // 特殊障碍物生成概率(0-100)
    private bool hasSpecialObstacleThisWave = false;                   // 本波是否已生成特殊障碍物


    public float spawnInterval = 3f;                                    // 生成间隔
    public float obstacleSpeed = 5f;                                   // 路障移动速度(和地板一样)
    public float destroyDistance = -15f;                               // 销毁距离

    [Header("生成位置")]
    public PlayerControl playerControl;                                // 获取玩家控制器中的点位
    private List<Vector3> spawnPoints = new List<Vector3>();          // 存储生成点
    private List<bool> spawnPointOccupied = new List<bool>();         // 记录每个点是否被占用

    [Header("生成物体")]
    [SerializeField] 
    public List<GameObject> ActiveObjectsInScene 
        = new List<GameObject>();                                       // 存储障碍

    [Header("物体变化设置")]
    public bool isChangeState = false;                                  //将路障变为大金币

    private void Start()
    {
        if (playerControl != null)
        {
            // 初始化生成点列表
            foreach (GameObject point in playerControl.movePoint)
            {
                spawnPoints.Add(new Vector3(point.transform.position.x, 1f, 0f));
                spawnPointOccupied.Add(false);
            }

            // 开始生成
            StartObstacleGenerator();
        }
    }

    //路障生成初始化---(可用于结束)
    public void InitializationObstacle()
    {
        //如果需要自然停止，请给while设置条件
        StopAllCoroutines();
        if (ActiveObjectsInScene != null)
        {
            foreach (var item in ActiveObjectsInScene)
            {
                Destroy(item);
            }
            ActiveObjectsInScene.Clear();
        }
        hasSpecialObstacleThisWave = false;
        isChangeState = false;
    }

    //开始路障生成
    private void StartObstacleGenerator()
    {
        // 开始生成
        StartCoroutine(SpawnObstacles());
    }

    private IEnumerator SpawnObstacles()
    {
        while (true)
        {
            // 重置所有点位状态
            for (int i = 0; i < spawnPointOccupied.Count; i++)
            {
                spawnPointOccupied[i] = false;
            }

            hasSpecialObstacleThisWave = false;  // 重置特殊障碍物状态

            // 随机决定这一波生成多少个障碍物(1-3个)
            int obstacleCount = Random.Range(1, 4);
            
            // 生成障碍物
            for (int i = 0; i < obstacleCount; i++)
            {
                // 随机选择一个未被占用的点位
                List<int> availablePoints = new List<int>();
                for (int j = 0; j < spawnPointOccupied.Count; j++)
                {
                    if (!spawnPointOccupied[j])
                    {
                        availablePoints.Add(j);
                    }
                }

                if (availablePoints.Count > 0)
                {
                    int randomPointIndex = availablePoints[Random.Range(0, availablePoints.Count)];
                    spawnPointOccupied[randomPointIndex] = true;

                    GameObject obstaclePrefab;

                    // 检查是否生成特殊障碍物
                    if (!hasSpecialObstacleThisWave && specialObstacle != null &&
                        Random.Range(0f, 100f) < specialObstacleChance)
                    {
                        obstaclePrefab = specialObstacle;
                        hasSpecialObstacleThisWave = true;
                    }
                    else
                    {
                        obstaclePrefab = obstacles[Random.Range(0, obstacles.Count)];
                    }

                    // 生成障碍物
                    GameObject obstacle = Instantiate(obstaclePrefab, 
                        new Vector3(spawnPoints[randomPointIndex].x, 
                        obstaclePrefab.transform.position.y, 
                        transform.position.z) + Vector3.forward * 20f, // 在前方20单位处生成
                        Quaternion.identity,transform);

                    ActiveObjectsInScene.Add(obstacle);



                    int b = Random.Range(0, 4);
                    if (b <= 1)
                    {
                        //50%正常扣血
                        obstacle.GetComponentsInChildren<ObstacleAttribute>()[0].ObstacleType = ObstacleType.Ordinary; 
                    }
                    else
                    {
                        //25%混乱------25%被锁定
                        obstacle.GetComponentsInChildren<ObstacleAttribute>()[0].ObstacleType = b == 2 ?  ObstacleType.Lock : ObstacleType.Chaos;
                    }
                    if (isChangeState)
                    {
                        ObstacleChange obstacleChange = obstacle.GetComponent<ObstacleChange>();
                        if (obstacleChange != null)
                        {
                            obstacleChange.SetObstacleChange();
                        }
                    }
                    // 添加移动和销毁逻辑
                    StartCoroutine(MoveObstacle(obstacle));
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator MoveObstacle(GameObject obstacle)
    {
        while (obstacle != null && obstacle.transform.position.z > destroyDistance)
        {
            obstacle.transform.Translate(Vector3.back * obstacleSpeed * Time.deltaTime);
            yield return null;
        }

        if (obstacle != null)
        {
            if (ActiveObjectsInScene.Contains(obstacle))
            {
                ActiveObjectsInScene.Remove(obstacle);
            }
            Destroy(obstacle);
        }
    }

    //执行场景中障碍改变
    public void ActiveObstacleChange()
    {
        isChangeState = true;
        // 对现有物体执行变化
        foreach (GameObject obj in ActiveObjectsInScene)
        {
            if (obj != null)
            {
                ObstacleChange obstacleChange = obj.GetComponent<ObstacleChange>();
                if (obstacleChange != null)
                {
                    obstacleChange.SetObstacleChange();
                }
            }
        }


        // 15秒后重置状态
        StartCoroutine(ResetChangeState());
    }

    private IEnumerator ResetChangeState()
    {
        yield return new WaitForSeconds(15f);
        isChangeState = false;
    }

    //关闭障碍物物体
    public void ActiveObstacleDisable()
    {
        // 对现有物体执行变化
        foreach (GameObject obj in ActiveObjectsInScene)
        {
            if (obj != null)
            {
                ObstacleChange obstacleChange = obj.GetComponent<ObstacleChange>();
                if (obstacleChange != null)
                {
                    obstacleChange.SetObstacleActive(false);
                }
            }
        }
    }

    //磁石吸引金币

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 可视化生成点和销毁线
        Gizmos.color = Color.red;
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            foreach (Vector3 point in spawnPoints)
            {
                Gizmos.DrawWireSphere(point, 0.5f);
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(-10f, 0f, destroyDistance), new Vector3(10f, 0f, destroyDistance));
    }
#endif
}
