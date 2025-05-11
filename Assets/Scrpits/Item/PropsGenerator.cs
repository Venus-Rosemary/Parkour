using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class PropsGenerator : Singleton<PropsGenerator>
{
    [Header("生成设置")]
    public List<GameObject> propsPrefabs = new List<GameObject>();      // 道具预制体列表
    public float spawnInterval = 21f;                                   // 生成间隔(3的倍数)
    public float propsSpeed = 5f;                                       // 道具移动速度
    public float destroyDistance = -15f;                                // 销毁距离

    [Header("生成位置")]
    public PlayerControl playerControl;                                 // 获取玩家控制器中的点位
    private List<Vector3> spawnPoints = new List<Vector3>();           // 存储生成点

    [Header("生成物体")]
    [SerializeField]
    private List<GameObject> activePropsInScene = new List<GameObject>();  // 存储当前场景中的道具

    private void Start()
    {
        if (playerControl != null)
        {
            // 初始化生成点列表
            foreach (GameObject point in playerControl.movePoint)
            {
                spawnPoints.Add(new Vector3(point.transform.position.x, 1f, 0f));
            }

            // 开始生成
            StartPropsGenerator();
        }
    }

    //道具生成初始化---(可用于结束)
    public void InitializationProps()
    {
        //如果需要自然停止，请给while设置条件
        StopAllCoroutines();
        if (activePropsInScene != null)
        {
            foreach (var item in activePropsInScene)
            {
                Destroy(item);
            }
            activePropsInScene.Clear();
        }
    }

    //开始道具生成
    private void StartPropsGenerator()
    {
        // 开始生成
        StartCoroutine(SpawnProps());
    }

    private IEnumerator SpawnProps()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 随机选择一个生成点
            int randomPointIndex = Random.Range(0, spawnPoints.Count);
            
            // 随机选择一个道具预制体
            GameObject propsPrefab = propsPrefabs[Random.Range(0, propsPrefabs.Count)];


            DOVirtual.DelayedCall(1.5f, () =>
            {
                // 生成道具
                GameObject props = Instantiate(propsPrefab,
                    new Vector3(spawnPoints[randomPointIndex].x,
                    propsPrefab.transform.position.y,
                    transform.position.z) + Vector3.forward * 20f, // 在前方20单位处生成
                    Quaternion.identity,
                    transform);

                activePropsInScene.Add(props);

                // 添加移动和销毁逻辑
                StartCoroutine(MoveProps(props));

            });

        }
    }



    private IEnumerator MoveProps(GameObject props)
    {
        while (props != null && props.transform.position.z > destroyDistance)
        {
            props.transform.Translate(Vector3.back * propsSpeed * Time.deltaTime);
            yield return null;
        }

        if (props != null)
        {
            if (activePropsInScene.Contains(props))
            {
                activePropsInScene.Remove(props);
            }
            Destroy(props);
        }
    }

    private void OnDrawGizmos()
    {
        // 可视化生成点和销毁线
        Gizmos.color = Color.green;
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
}