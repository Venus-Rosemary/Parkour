using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSetting : MonoBehaviour
{
    public GameObject playerObject;

    [Header("Boss状态")]
    public int maxHealth = 10;                  // Boss最大生命值
    private int currentHealth;                  // Boss当前生命值
    public bool isDead = false;                // Boss是否死亡

    void Start()
    {
        InitializeBoss();
    }

    void Update()
    {
        //ansform.position = new Vector3(playerObject.transform.position.x,
            //transform.position.y, transform.position.z);
    }

    // 初始化Boss状态
    public void InitializeBoss()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    // Boss受到伤害
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // 检查是否死亡
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            OnBossDeath();
        }

        UIControl.Instance.slider.value = (float)currentHealth / maxHealth;
        Debug.Log($"slider.value={(float)currentHealth / maxHealth}");
        Debug.Log($"Boss受到{damage}点伤害！当前生命值：{currentHealth}");
    }

    // Boss恢复
    public void BossRecoveryHp()
    {
        currentHealth++;
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        UIControl.Instance.slider.value = (float)currentHealth / maxHealth;
    }

    // Boss死亡时调用
    private void OnBossDeath()
    {
        GameManager.Instance.GameOver();
        Debug.Log("Boss已死亡！");
        // 可以在这里添加死亡动画、掉落物品等
        gameObject.SetActive(false);
    }
}
