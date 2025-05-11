using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSetting : MonoBehaviour
{
    [SerializeField] private GameObject playerTarget;              //玩家目标
    private bool isCanMove= false;
    private float magnetSpeed = 15f;                                    // 金币移动速度
    void Start()
    {
        playerTarget = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (isCanMove)
        {

            // 将金币移向玩家
            Vector3 direction = (playerTarget.transform.position - transform.position).normalized;
            transform.position += direction * magnetSpeed * Time.deltaTime;

        }

    }

    public void SetCanMove(bool isCan)
    {
        isCanMove = isCan;
    }
}
