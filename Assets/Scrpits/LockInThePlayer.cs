using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LockInThePlayer : MonoBehaviour
{
    [SerializeField] private GameObject playerTarget;              //玩家目标

    public float smoothSpeed = 5f;

    public bool lockSuccess = false;

    public GameObject boomObject;

    public Sequence sequence;

    void Start()
    {
        playerTarget = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        SlowFollowPlayer();
    }

    private void OnEnable()
    {
        SetLockTime();
    }

    private void OnDestroy()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
    }

    void SetLockTime()
    {

        sequence = DOTween.Sequence();
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() => {
            lockSuccess = true;
        } );

        sequence.AppendInterval(1.5f);

        sequence.AppendCallback(() => {
            boomObject.SetActive(true);
        });
        sequence.AppendInterval(0.25f);

        sequence.AppendCallback(() => {
            boomObject.transform.DOMoveY(this.gameObject.transform.position.y, 0.25F).OnComplete(() =>
            {
                boomObject.SetActive(false);
                DestoryOtherSelf();
            });
        } );
            //DOVirtual.DelayedCall(2f,)
        }

    void DestoryOtherSelf()
    {
        Destroy(this.gameObject);
    }

    void SlowFollowPlayer()
    {
        if (lockSuccess) return;
        if (playerTarget != null)
        {
            Vector3 targetPosition = new Vector3(playerTarget.transform.position.x,
                transform.position.y,
                playerTarget.transform.position.z);
            
            // 对于小距离移动使用普通插值
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                smoothSpeed * Time.deltaTime
            );

        }
    }
}
