using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : Singleton<MapGenerator>
{
    [Header("轨道设置")]
    public GameObject trackPrefab;        // 轨道预制体
    public float trackSpeed = 8f;         // 轨道移动速度
    public float trackLength = 10f;       // 单个轨道长度（和模型一样长）
    public int initialTrackCount = 10;     // 初始轨道数量
    public float destroyDistance = 20f;   // 销毁距离（和模型一样长）


    private List<GameObject> activeTracks = new List<GameObject>();
    private float totalDistance;

    void Start()
    {


        // 初始化轨道
        for (int i = 0; i < initialTrackCount; i++)
        {
            SpawnTrack();
        }
    }

    void Update()
    {
        // 移动所有轨道
        MoveAndManageTracks();
    }

    void SpawnTrack()
    {
        Vector3 spawnPosition = Vector3.zero;
        if (activeTracks.Count > 0)
        {
            spawnPosition = activeTracks[activeTracks.Count - 1].transform.position + Vector3.forward * trackLength;
        }

        GameObject newTrack = Instantiate(trackPrefab, spawnPosition, Quaternion.identity,gameObject.transform);
        activeTracks.Add(newTrack);
    }

    void MoveAndManageTracks()
    {
        // 移动所有轨道
        foreach (GameObject track in activeTracks.ToArray())
        {
            track.transform.Translate(Vector3.back * trackSpeed * Time.deltaTime);

            // 检查并销毁超出范围的轨道
            if (track.transform.position.z < -destroyDistance)
            {
                activeTracks.Remove(track);
                Destroy(track);
                SpawnTrack();
            }
        }
    }
}

// 障碍物移动组件
public class ObstacleMovement : MonoBehaviour
{
    public float speed;
    private float destroyDistance = 20f;

    void Update()
    {
        transform.Translate(Vector3.back * speed * Time.deltaTime);

        if (transform.position.z < -destroyDistance)
        {
            Destroy(gameObject);
        }
    }
}