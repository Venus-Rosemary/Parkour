using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LaserGenerator : MonoBehaviour
{
    [Header("激光设置")]
    public List<GameObject> laserPrefabs;        // 激光预制体列表
    public List<GameObject> warningPrefab;       // 提示预制体
    public GameObject warningPrefabRange;        // 提示预制体最终范围
    public PlayerControl playerControl;          // 获取生成点引用

    [Header("时间设置")]
    public float spawnInterval = 15f;            // 生成间隔
    public float warningDuration = 3f;           // 提示持续时间
    public float laserDuration = 0.3f;          // 激光持续时间

    [Header("私有变量")]
    private GameObject warning;
    private GameObject warningRange;
    private GameObject laser;

    private void Start()
    {
        if (playerControl != null)
        {
            StartObstacleGenerator();
        }
    }

    //激光生成初始化---(可用于结束)
    public void InitializationLaser()
    {
        //如果需要自然停止，请给while设置条件
        StopAllCoroutines();
        SetObjectDestory(warning);
        SetObjectDestory(warningRange);
        SetObjectDestory(laser);
    }

    private void SetObjectDestory(GameObject gameObject)
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    //开始激光生成
    private void StartObstacleGenerator()
    {
        // 开始生成
        StartCoroutine(LaserSpawnRoutine());
    }

    private IEnumerator LaserSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            // 随机选择一个生成点
            if (playerControl.movePoint.Count > 0)
            {
                int randomPointIndex = Random.Range(0, playerControl.movePoint.Count);
                GameObject spawnPoint = playerControl.movePoint[randomPointIndex];

                int a = Random.Range(0, laserPrefabs.Count);

                // 随机选择一个激光预制体
                GameObject selectedLaser = laserPrefabs[a];

                // 生成激光和提示物体
                StartCoroutine(SpawnLaserSequence(a, spawnPoint.transform.position, selectedLaser));
            }
        }
    }

    private IEnumerator SpawnLaserSequence(int a, Vector3 spawnPosition, GameObject laserPrefab)
    {
        // 生成提示物体
        warning = Instantiate(warningPrefab[a], new Vector3(spawnPosition.x,
            warningPrefab[a].transform.position.y, warningPrefab[a].transform.position.z), Quaternion.identity);
        warningRange = Instantiate(warningPrefabRange, new Vector3(spawnPosition.x,
            warningPrefabRange.transform.position.y, warningPrefabRange.transform.position.z), Quaternion.identity);

        // 设置初始缩放
        warning.transform.localScale = new Vector3(0, warning.transform.localScale.y, warning.transform.localScale.z);
        
        // 执行缩放动画
        warning.transform.DOScaleX(4f, warningDuration).SetEase(Ease.Linear);
        
        // 等待提示时间
        yield return new WaitForSeconds(warningDuration);
        
        // 关闭提示物体
        Destroy(warning);
        Destroy(warningRange);

        // 生成激光
        laser = Instantiate(laserPrefab, new Vector3(spawnPosition.x,
            laserPrefab.transform.position.y, laserPrefab.transform.position.z), Quaternion.identity);
        laser.SetActive(true);
        
        // 等待激光持续时间
        yield return new WaitForSeconds(laserDuration);
        
        // 关闭并销毁激光
        laser.SetActive(false);
        Destroy(laser);
    }
}