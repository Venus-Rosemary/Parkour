using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSetting : MonoBehaviour
{
    [SerializeField] private GameObject playerTarget;              //���Ŀ��
    private bool isCanMove= false;
    private float magnetSpeed = 15f;                                    // ����ƶ��ٶ�
    void Start()
    {
        playerTarget = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (isCanMove)
        {

            // ������������
            Vector3 direction = (playerTarget.transform.position - transform.position).normalized;
            transform.position += direction * magnetSpeed * Time.deltaTime;

        }

    }

    public void SetCanMove(bool isCan)
    {
        isCanMove = isCan;
    }
}
