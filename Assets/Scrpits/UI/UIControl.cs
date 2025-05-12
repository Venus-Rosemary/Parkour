using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIControl : Singleton<UIControl>
{

    [Header("��Ϸ�еĶ���UI")]
    public TMP_Text scoreText;                      // �÷�UI
    public TMP_Text HpText;                         // Ѫ��UI
    public GameObject HpProtect;                    // Ѫ������UI

    [Header("���UI��λϵͳ")]
    public List<RectTransform> rectTransforms 
        =new List<RectTransform>();                 // �洢��λλ����Ϣ
    public List<GameObject> itemsInSlots = 
        new List<GameObject>();                     // ��ǰUI���е���Ʒ

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
        scoreText.text = $"�÷֣�{score}";
    }
    public void SetHpText(int hp, int maxHp)
    {
        HpText.text = $"Ѫ����{hp}/{maxHp}";
    }
    public void SetHpProtect(bool hpProtect)
    {
        HpProtect.SetActive(hpProtect);
    }

    public void AddNewItemToSlot(SubdueBossProps subdueBossProps)
    {
        if (itemsInSlots.Count< rectTransforms.Count)
        {
            //UI���е���ƷС��λ������˵����ûռ����λ
            GameObject newItem = Instantiate(imagePrefab, enterPoint.position, Quaternion.identity);
            newItem.transform.SetParent(enterPoint.parent, false);
            newItem.transform.position = enterPoint.position;

            UpdateImage(newItem, subdueBossProps);

            itemsInSlots.Add(newItem);

            UpdateSlotPositions();
        }
        else
        {
            //��λռ�����ҵ���һ�����壬�����Ƴ�
            //�ƶ����� dotween
            GameObject oldItem = itemsInSlots[0];
            itemsInSlots.RemoveAt(0);
            oldItem.transform.DOMove(exitPoint.position, 0.5f).SetEase(Ease.OutBack).OnComplete(() => {
                Destroy(oldItem);
            });

            // �����µ���
            GameObject newItem = Instantiate(imagePrefab, enterPoint.position, Quaternion.identity);
            newItem.transform.SetParent(enterPoint.parent, false);
            newItem.transform.position = enterPoint.position;

            UpdateImage(newItem, subdueBossProps);

            itemsInSlots.Add(newItem);

            UpdateSlotPositions();
        }
    }

    //�����ƶ�
    private void UpdateSlotPositions()
    {
        for (int i = 0; i < itemsInSlots.Count; i++)
        {
            // ʹ��DOTween�ƶ���Ŀ��λ��
            itemsInSlots[i].transform
                .DOMove(rectTransforms[i].position, 0.5f)
                .SetEase(Ease.OutBack);

        }
    }

    //����ͼƬ
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
