using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubduePropsGenerator : Singleton<SubduePropsGenerator>
{
    [Header("碎片生成设置")]
    public List<GameObject> SubdueProps=new List<GameObject>();
    public float spawnInterval = 6f;
    public float FragmentSpeed = 40f;
    public float destoryDistance = -15f;
    private List<GameObject> activeSubdueProps = new List<GameObject>();

    [Header("生成位置范围")]
    public float minX = -10f;
    public float maxX = 10f;

    private bool shouldExecute = true;

    private void Start()
    {
        //StartSubduePropsGenerator();
    }

    //道具生成初始化---(可用于结束)
    public void InitializationSubdue()
    {
        shouldExecute = false;
        //如果需要自然停止，请给while设置条件
        StopAllCoroutines();
        if (activeSubdueProps != null)
        {
            foreach (var item in activeSubdueProps)
            {
                Destroy(item);
            }
            activeSubdueProps.Clear();
        }
    }

    //开始道具生成
    public void StartSubduePropsGenerator()
    {

        shouldExecute = true;
        // 开始生成
        StartCoroutine(SpawnProps());
    }

    private IEnumerator SpawnProps()
    {
        while (true)
        {

            if (SubdueProps.Count > 0)
            {
                // 随机选择一个道具预制体
                GameObject propsPrefab = SubdueProps[Random.Range(0, SubdueProps.Count)];

                // 随机X坐标
                float randomX = Random.Range(minX, maxX);

                DOVirtual.DelayedCall(1.5f, () =>
                {
                    if (shouldExecute)
                    {
                        // 生成道具
                        GameObject props = Instantiate(propsPrefab,
                            new Vector3(randomX, propsPrefab.transform.position.y, transform.position.z) + Vector3.forward * 20f,
                            Quaternion.identity,
                            transform);


                        activeSubdueProps.Add(props);

                        // 添加移动和销毁逻辑
                        StartCoroutine(MoveProps(props));
                    }

                });


            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator MoveProps(GameObject props)
    {
        while (props != null && props.transform.position.z > destoryDistance)
        {
            props.transform.Translate(Vector3.back * FragmentSpeed * Time.deltaTime);
            yield return null;
        }

        if (props != null)
        {
            Destroy(props);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 可视化生成范围和销毁线
        Gizmos.color = Color.green;
        Vector3 center = new Vector3((minX + maxX) / 2, transform.position.y, transform.position.z) + Vector3.forward * 20f;
        Vector3 size = new Vector3(maxX - minX, 1f, 0.1f);
        Gizmos.DrawWireCube(center, size);

        // 销毁线
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(minX, transform.position.y, destoryDistance),
            new Vector3(maxX, transform.position.y, destoryDistance));
    }
#endif
}
