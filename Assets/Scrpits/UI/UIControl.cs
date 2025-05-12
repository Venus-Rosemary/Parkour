using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIControl : Singleton<UIControl>
{

    [Header("游戏中的顶部UI")]
    public TMP_Text scoreText;                      // 得分UI
    public TMP_Text HpText;                         // 血量UI
    public GameObject HpProtect;                    // 血量保护UI

    [Header("左侧UI槽位系统")]
    public List<RectTransform> rectTransforms 
        =new List<RectTransform>();                 // 存储槽位位置信息
    public List<GameObject> itemsInSlots = 
        new List<GameObject>();                     // 当前UI槽中的物品

    //public GameObject leftPanel;
    public GameObject imagePrefab;
    public RectTransform enterPoint;
    public RectTransform exitPoint;
    public List<Sprite> objectSprites = new List<Sprite>();
    public GameObject successObject;
    public bool leftPanelTextBool = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (leftPanelTextBool)
        {
            leftPanelTextBool = false;
            AddNewItemToSlot(SubdueBossProps.RestoreHealth);
        }
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

    public void AddNewItemToSlot(SubdueBossProps subdueBossProps)
    {
        if (itemsInSlots.Count< rectTransforms.Count)
        {
            //UI槽中的物品小于位置数量说明还没占满槽位
            GameObject newItem = Instantiate(imagePrefab, enterPoint.position, Quaternion.identity);
            newItem.transform.SetParent(enterPoint.parent, false);
            newItem.transform.position = enterPoint.position;

            UpdateImage(newItem, subdueBossProps);

            itemsInSlots.Add(newItem);

            UpdateSlotPositions();
        }
        else
        {
            //槽位占满，找到第一个物体，将他移出
            //移动物体 dotween
            GameObject oldItem = itemsInSlots[0];
            itemsInSlots.RemoveAt(0);
            oldItem.transform.DOMove(exitPoint.position, 0.5f).SetEase(Ease.OutBack).OnComplete(() => {
                Destroy(oldItem);
            });

            // 创建新道具
            GameObject newItem = Instantiate(imagePrefab, enterPoint.position, Quaternion.identity);
            newItem.transform.SetParent(enterPoint.parent, false);
            newItem.transform.position = enterPoint.position;

            UpdateImage(newItem, subdueBossProps);

            itemsInSlots.Add(newItem);

            UpdateSlotPositions();
        }
    }

    //更新移动
    private void UpdateSlotPositions()
    {
        for (int i = 0; i < itemsInSlots.Count; i++)
        {
            // 使用DOTween移动到目标位置
            itemsInSlots[i].transform
                .DOMove(rectTransforms[i].position, 0.5f)
                .SetEase(Ease.OutBack);

        }
    }

    //更新图片
    private void UpdateImage(GameObject gameObject, SubdueBossProps subdueBossProps)
    {
        switch (subdueBossProps)
        {
            case SubdueBossProps.RestoreHealth:
                if (objectSprites[0]!=null)
                {
                    gameObject.GetComponent<Image>().sprite = objectSprites[0];
                }
                break;
            case SubdueBossProps.DoubleSubdue:
                if (objectSprites[1] != null)
                {
                    gameObject.GetComponent<Image>().sprite = objectSprites[1];
                }
                break;
            case SubdueBossProps.ExtensionTime:
                if (objectSprites[2] != null)
                {
                    gameObject.GetComponent<Image>().sprite = objectSprites[2];
                }
                break;
            default:
                break;
        }
    }

    public void ClearImage()
    {
        foreach (var item in itemsInSlots)
        {
            Destroy(item);
        }
        itemsInSlots.Clear();
    }

    public void SetSuccessUI(bool setSuccess)
    {
        successObject.SetActive(setSuccess);
    }
}
