using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIControl : Singleton<UIControl>
{

    [Header("��Ϸ�еĶ���UI")]
    public TMP_Text scoreText;                      // �÷�UI
    public TMP_Text HpText;                         // Ѫ��UI
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetScoreText(int score)
    {
        scoreText.text = $"�÷֣�{score}";
    }
    public void SetHpText(int hp, int maxHp)
    {
        HpText.text = $"Ѫ����{hp}/{maxHp}";
    }
}
