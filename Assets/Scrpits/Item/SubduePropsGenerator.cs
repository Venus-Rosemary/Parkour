using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubduePropsGenerator : Singleton<SubduePropsGenerator>
{
    [Header("��Ƭ��������")]
    public List<GameObject> SubdueProps=new List<GameObject>();
    public float spawnInterval = 6f;
    public float FragmentSpeed = 40f;
    public float destoryDistance = -15f;
    private List<GameObject> activeSubdueProps = new List<GameObject>();

    [Header("����λ�÷�Χ")]
    public float minX = -10f;
    public float maxX = 10f;

    private bool shouldExecute = true;

    private void Start()
    {
        //StartSubduePropsGenerator();
    }

    //�������ɳ�ʼ��---(�����ڽ���)
    public void InitializationSubdue()
    {
        shouldExecute = false;
        //�����Ҫ��Ȼֹͣ�����while��������
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

    //��ʼ��������
    public void StartSubduePropsGenerator()
    {

        shouldExecute = true;
        // ��ʼ����
        StartCoroutine(SpawnProps());
    }

    private IEnumerator SpawnProps()
    {
        while (true)
        {

            if (SubdueProps.Count > 0)
            {
                // ���ѡ��һ������Ԥ����
                GameObject propsPrefab = SubdueProps[Random.Range(0, SubdueProps.Count)];

                // ���X����
                float randomX = Random.Range(minX, maxX);

                DOVirtual.DelayedCall(1.5f, () =>
                {
                    if (shouldExecute)
                    {
                        // ���ɵ���
                        GameObject props = Instantiate(propsPrefab,
                            new Vector3(randomX, propsPrefab.transform.position.y, transform.position.z) + Vector3.forward * 20f,
                            Quaternion.identity,
                            transform);


                        activeSubdueProps.Add(props);

                        // ����ƶ��������߼�
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
        // ���ӻ����ɷ�Χ��������
        Gizmos.color = Color.green;
        Vector3 center = new Vector3((minX + maxX) / 2, transform.position.y, transform.position.z) + Vector3.forward * 20f;
        Vector3 size = new Vector3(maxX - minX, 1f, 0.1f);
        Gizmos.DrawWireCube(center, size);

        // ������
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(minX, transform.position.y, destoryDistance),
            new Vector3(maxX, transform.position.y, destoryDistance));
    }
#endif
}
