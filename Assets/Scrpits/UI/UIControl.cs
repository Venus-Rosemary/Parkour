using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

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

    [Header("����ʱUI")]
    public TMP_Text countdownText;      // ����ʱ�ı�
    public TMP_Text transitionText;     // �����л�����ʱ�ı�

    [Header("BossѪ��UI")]
    public Slider slider;
    public Image imageToResize;
    public float originalWidth;
    public float targetWidth;
    public bool bossHpUITest = false;

    public TMP_Text hintText;


    [Header("UI������")]
    public GameObject StartUI;
    public GameObject GameUI;
    public GameObject BossHpUI;
    public GameObject LeftUI;
    public GameObject EndUI;

    void Start()
    {
        if (imageToResize != null)
        {
            // �����ʼ���
            originalWidth = imageToResize.rectTransform.sizeDelta.x;

            // ȷ����ʼ״̬��ȷ
            OnSliderValueChanged(slider.value);
        }
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    // Update is called once per frame
    void Update()
    {
        if (leftPanelTextBool)
        {
            leftPanelTextBool = false;
            AddNewItemToSlot(SubdueBossProps.RestoreHealth);
        }
        if (bossHpUITest)
        {
            bossHpUITest = false;
            //OnSliderValueChanged();
            slider.value = 0.5f;
        }
    }

    #region UI������
    public void SetPanelControl(bool start, bool game, bool end)
    {
        StartUI.SetActive(start);
        GameUI.SetActive(game);
        EndUI.SetActive(end);
    }

    public void SetHpChangeUI(bool hp)
    {
        BossHpUI.SetActive(hp);
    }

    public void SetLeftUI(bool left)
    {
        LeftUI.SetActive(left);
    }

    #endregion

    #region ������Ѫ����Ѫ������UI
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
    #endregion

    #region ����λϵͳUI
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
        SetSuccessUI(false);
    }

    public void SetSuccessUI(bool setSuccess)
    {
        successObject.SetActive(setSuccess);
    }

    #endregion

    #region ����ʱ
    public void UpdateCountdownText(int seconds)
    {
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;
        countdownText.text = $"����ʱ��{minutes:00}:{remainingSeconds:00}";
    }

    public void UpdateTransitionText(int seconds)
    {
        transitionText.text = $"�����л��У�{seconds}��";
    }
    #endregion

    #region Ѫ���仯����
    public void OnSliderValueChanged(float newValue)
    {
        Debug.Log("�ı�ֵ?");
        // ��������ͼƬ��ȣ���ʹ��Э��
        if (imageToResize != null)
        {
            Debug.Log("�ı�ֵ������");
            // ȡ��֮ǰ������ DOTween ����
            imageToResize.rectTransform.DOKill();

            targetWidth = originalWidth * newValue;

            // ʹ�� DOTween ƽ��������ȣ���ȷ��ê��������ȷ
            RectTransform rect = imageToResize.rectTransform;
            Vector2 currentSize = rect.sizeDelta;

            rect.DOSizeDelta(new Vector2(targetWidth, currentSize.y), 1f)
                .SetEase(Ease.OutQuad);


        }
    }

    //����ָʾ
    public void SetHintText(int oneKey, int twoKey)
    {
        hintText.gameObject.SetActive(true);
        hintText.text = $"�����ָʾ������{oneKey}��{twoKey}����";
        DOVirtual.DelayedCall(3f, () => hintText.gameObject.SetActive(false));
    }
    #endregion
}
