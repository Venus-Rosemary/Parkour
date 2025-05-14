using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

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

    [Header("倒计时UI")]
    public TMP_Text countdownText;      // 倒计时文本
    public TMP_Text transitionText;     // 场景切换倒计时文本

    [Header("Boss血量UI")]
    public Slider slider;
    public Image imageToResize;
    public float originalWidth;
    public float targetWidth;
    public bool bossHpUITest = false;

    public TMP_Text hintText;


    [Header("UI面板管理")]
    public GameObject StartUI;
    public GameObject GameUI;
    public GameObject BossHpUI;
    public GameObject LeftUI;
    public GameObject EndUI;

    void Start()
    {
        if (imageToResize != null)
        {
            // 保存初始宽度
            originalWidth = imageToResize.rectTransform.sizeDelta.x;

            // 确保初始状态正确
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

    #region UI面板管理
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

    #region 分数、血量、血量保护UI
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
    #endregion

    #region 左侧槽位系统UI
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
        SetSuccessUI(false);
    }

    public void SetSuccessUI(bool setSuccess)
    {
        successObject.SetActive(setSuccess);
    }

    #endregion

    #region 倒计时
    public void UpdateCountdownText(int seconds)
    {
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;
        countdownText.text = $"倒计时：{minutes:00}:{remainingSeconds:00}";
    }

    public void UpdateTransitionText(int seconds)
    {
        transitionText.text = $"场景切换中：{seconds}秒";
    }
    #endregion

    #region 血量变化管理
    public void OnSliderValueChanged(float newValue)
    {
        Debug.Log("改变值?");
        // 立即更新图片宽度，不使用协程
        if (imageToResize != null)
        {
            Debug.Log("改变值触发？");
            // 取消之前的所有 DOTween 动画
            imageToResize.rectTransform.DOKill();

            targetWidth = originalWidth * newValue;

            // 使用 DOTween 平滑调整宽度，并确保锚点设置正确
            RectTransform rect = imageToResize.rectTransform;
            Vector2 currentSize = rect.sizeDelta;

            rect.DOSizeDelta(new Vector2(targetWidth, currentSize.y), 1f)
                .SetEase(Ease.OutQuad);


        }
    }

    //动作指示
    public void SetHintText(int oneKey, int twoKey)
    {
        hintText.gameObject.SetActive(true);
        hintText.text = $"请根据指示做出：{oneKey}、{twoKey}动作";
        DOVirtual.DelayedCall(3f, () => hintText.gameObject.SetActive(false));
    }
    #endregion
}
