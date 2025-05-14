using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SecondSceneObstacleGenerator : Singleton<SecondSceneObstacleGenerator>
{
    [Header("生成设置")]
    public GameObject[] obstaclePrefabs;          // 路障预制体数组
    public float spawnInterval = 2f;              // 生成间隔
    public float obstacleSpeed = 5f;              // 路障移动速度
    public float destroyDistance = -15f;          // 销毁距离

    [Header("生成位置")]
    public float spawnPositionZ = 20f;            // 生成位置Z轴
    public float minX = -5f;                      // 最小X坐标
    public float maxX = 5f;                       // 最大X坐标

    [Header("生成物体")]
    [SerializeField]
    public List<GameObject> ActiveObjectsInScene
        = new List<GameObject>();

    private void Start()
    {
        //StartCoroutine(SpawnObstacles());
    }

    public void InitializationSecondObstacle()
    {
        StopAllCoroutines();
        if (ActiveObjectsInScene != null)
        {
            foreach (var item in ActiveObjectsInScene)
            {
                Destroy(item);
            }
            ActiveObjectsInScene.Clear();
        }
    }

    public void StartSecondObstacleGenerator()
    {
        // 开始生成
        StartCoroutine(SpawnObstacles());
    }

    private IEnumerator SpawnObstacles()
    {
        while (true)
        {
            // 随机选择一个路障预制体
            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            
            // 随机X轴位置
            float randomX = Random.Range(minX, maxX);

            // 生成路障
            Vector3 spawnPosition = new Vector3(randomX,
                obstaclePrefab.transform.position.y,
                transform.position.z) + Vector3.forward * 20f;

            GameObject obstacle = Instantiate(obstaclePrefab, 
                spawnPosition, 
                Quaternion.identity, 
                transform);

            ActiveObjectsInScene.Add(obstacle);

            int secondR = Random.Range(0, 2);
            if (secondR ==0)
            {
                obstacle.GetComponentsInChildren<ObstacleAttribute>()[0].ObstacleType = ObstacleType.Ordinary;
            }
            else
            {
                obstacle.GetComponentsInChildren<ObstacleAttribute>()[0].ObstacleType = ObstacleType.BossRecovery;
            }

            // 添加移动和销毁逻辑
            StartCoroutine(MoveObstacle(obstacle));

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
            Destroy(obstacle);
        }
    }

}