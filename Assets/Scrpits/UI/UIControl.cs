using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIControl : Singleton<UIControl>
{

    [Header("游戏中的顶部UI")]
    public TMP_Text scoreText;                      // 得分UI
    public TMP_Text HpText;                         // 血量UI
    public GameObject HpProtect;                    // 血量保护UI
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetScoreText(int score)
    {
        scoreText.text = $"得分：{score}";
    }
    public void SetHpText(int hp, int maxHp)
    {
        HpText.text = $"血量：{hp}/{maxHp}";
    }
    public void SetHpProtect(bool hpProtect)
    {
        HpProtect.SetActive(hpProtect);
    }
}
