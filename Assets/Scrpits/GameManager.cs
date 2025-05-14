using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("获取组件")]

    [Tooltip("玩家控制器组件")]
    public PlayerControl playerControl;

    [Tooltip("boss物体")]
    public GameObject bossObject;

    [Header("设置统一的移动速度")]
    public float UniformSpeed = 40f;
    public float LaserTranslationSpeed = 10f;
    public float BulletMoveSpeed = 60f;

    [Header("设置当前场景")]
    public bool isFirstScene = false;
    public bool isSecondScene = false;

    [Header("倒计时设置")]
    [Tooltip("第一场景时间")]
    public float firstSceneTime = 120f;    // 第一场景2分钟
    [Tooltip("第二场景时间")]
    public float secondSceneTime = 180f;   // 第二场景3分钟
    private float currentTime;              // 当前倒计时
    private bool isCountingDown = false;    // 是否正在倒计时
    private bool isTransitioning = false;   // 是否正在场景切换


    [Header("私有变量")]
    public bool isTestbool = false;

    private void Awake()
    {

    }


    void Start()
    {
        StartGame();
    }

    
    void Update()
    {
        if (isTestbool)
        {
            isTestbool = false;
            StartFirstSceneCountdown();
        }
        if (isCountingDown)
        {
            UpdateCountdown();
        }
    }

    #region 初始化

    public void Initialization()
    {
        playerControl.gameObject.SetActive(false);
        bossObject.SetActive(false);
        bossObject.GetComponent<BossSetting>().InitializeBoss();
        UIControl.Instance.ClearImage();
        playerControl.FirstScene = false;
        playerControl.SecendScene = false;
        isCountingDown = false;
        isTransitioning = false;
        playerControl.InitializePlayer();
        SubdueBoss.Instance.SetGameState(false);
        MapGenerator.Instance.InitializationMap();
        ObstacleGenerator.Instance.InitializationObstacle();
        PropsGenerator.Instance.InitializationProps();
        LaserGenerator.Instance.InitializationLaser();
        SubduePropsGenerator.Instance.InitializationSubdue();
        SecondSceneObstacleGenerator.Instance.InitializationSecondObstacle();
        LaserTranslation.Instance.InitializeLaserSystem();
    }

    #endregion


    #region 设置统一的移动速度
    public void SetUniformSpeed()
    {
        MapGenerator.Instance.trackSpeed = UniformSpeed;
        ObstacleGenerator.Instance.obstacleSpeed = UniformSpeed;
        PropsGenerator.Instance.propsSpeed = UniformSpeed;
        SubduePropsGenerator.Instance.FragmentSpeed = UniformSpeed;
        SecondSceneObstacleGenerator.Instance.obstacleSpeed = UniformSpeed;
        LaserTranslation.Instance.moveSpeed = LaserTranslationSpeed;
        LaserTranslation.Instance.bulletSpeed = BulletMoveSpeed;
        LaserTranslation.Instance.projectileSpeed = UniformSpeed;
    }
    #endregion

    #region 设置不同场景所需的不同事件处理
    public void SetDifferentSceneState()
    {

        if (isFirstScene)
        {
            playerControl.gameObject.SetActive(true);
            bossObject.SetActive(false);
            playerControl.FirstScene = true;
            playerControl.SecendScene = false;

            UIControl.Instance.SetHpChangeUI(false);
            UIControl.Instance.SetLeftUI(false);


            SubdueBoss.Instance.SetGameState(false);

            MapGenerator.Instance.StartMapGenerator();                      //开始轨道生成运动
            ObstacleGenerator.Instance.StartObstacleGenerator();            //开始障碍生成运动
            LaserGenerator.Instance.StartObstacleGenerator();               //开始激光生成
            PropsGenerator.Instance.StartPropsGenerator();                  //开始道具生成
            SubduePropsGenerator.Instance.InitializationSubdue();           //关闭碎片生成
            SecondSceneObstacleGenerator.Instance.InitializationSecondObstacle();
            LaserTranslation.Instance.StopAttackLoop();

        }

        if (isSecondScene)
        {
            playerControl.gameObject.SetActive(true);
            bossObject.SetActive(true);
            playerControl.FirstScene = false;
            playerControl.SecendScene = true;

            UIControl.Instance.SetHpChangeUI(true);
            UIControl.Instance.SetLeftUI(true);


            SubdueBoss.Instance.SetGameState(true);

            MapGenerator.Instance.StartMapGenerator();
            ObstacleGenerator.Instance.InitializationObstacle();            //关闭障碍生成运动
            LaserGenerator.Instance.InitializationLaser();                  //关闭激光生成
            PropsGenerator.Instance.InitializationProps();                  //关闭道具生成
            SubduePropsGenerator.Instance.StartSubduePropsGenerator();      //开始碎片生成
            SecondSceneObstacleGenerator.Instance.StartSecondObstacleGenerator();
            LaserTranslation.Instance.StartAttackLoop();
        }

        if (!isFirstScene && !isSecondScene)
        {
            Initialization();
        }
    }

    private void SetTransitionalState()
    {
        playerControl.gameObject.SetActive(true);
        bossObject.SetActive(false);
        playerControl.FirstScene = false;
        playerControl.SecendScene = true;
        MapGenerator.Instance.StartMapGenerator();                      //开始轨道生成运动
        ObstacleGenerator.Instance.InitializationObstacle();            //关闭障碍生成运动
        LaserGenerator.Instance.InitializationLaser();                  //关闭激光生成
        PropsGenerator.Instance.InitializationProps();                  //关闭道具生成
        SubduePropsGenerator.Instance.InitializationSubdue();           //关闭碎片生成
        SecondSceneObstacleGenerator.Instance.InitializationSecondObstacle();
        LaserTranslation.Instance.StopAttackLoop();
    }
    #endregion

    #region 倒计时
    private void StartFirstSceneCountdown()
    {
        currentTime = firstSceneTime;
        isFirstScene = true;
        isSecondScene = false;
        isCountingDown = true;
        SetDifferentSceneState();
    }

    private void StartSecondSceneCountdown()
    {
        currentTime = secondSceneTime;
        isFirstScene = false;
        isSecondScene = true;
        isCountingDown = true;
        SetDifferentSceneState();
    }

    private void UpdateCountdown()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            // 可以在这里更新UI显示倒计时
            UIControl.Instance.UpdateCountdownText(Mathf.CeilToInt(currentTime));
        }
        else if (!isTransitioning)
        {
            if (isFirstScene)
            {
                StartCoroutine(TransitionToSecondScene());
            }
            else if (isSecondScene)
            {
                GameOver();//游戏结束
            }
        }
    }

    private IEnumerator TransitionToSecondScene()
    {
        isTransitioning = true;
        isCountingDown = false;
        SetTransitionalState();
        // 等待10秒
        float transitionTime = 10f;
        while (transitionTime > 0)
        {
            UIControl.Instance.UpdateTransitionText(Mathf.CeilToInt(transitionTime));
            transitionTime -= Time.deltaTime;
            yield return null;
        }

        isTransitioning = false;
        StartSecondSceneCountdown();
    }


    public void AddSecondSceneTime(float time)
    {
        currentTime += time;
    }
    #endregion

    #region 游戏状态管理
    public void StartGame()
    {
        Initialization();
        SetUniformSpeed();
        isTestbool = false;
        UIControl.Instance.SetPanelControl(true, false, false);
        UIControl.Instance.SetHpChangeUI(false);
        UIControl.Instance.SetLeftUI(false);
    }

    public void PauseGame()
    {
        isTestbool = true;
        UIControl.Instance.SetPanelControl(false, true, false);
        UIControl.Instance.SetHpChangeUI(false);
        UIControl.Instance.SetLeftUI(false);
    }

    public void ReturnMenu()//返回主菜单（预留）
    {
        isTestbool = false;
        UIControl.Instance.SetPanelControl(true, false, false);
        UIControl.Instance.SetHpChangeUI(false);
        UIControl.Instance.SetLeftUI(false);
    }

    public void RestarGame()
    {
        StopAllCoroutines();
        Initialization();
        isTestbool = false;
        PauseGame();
    }

    public void GameOver()
    {
        isCountingDown = false;

        isTestbool = false;
        // 清理场景
        isFirstScene = false;
        isSecondScene = false;

        SetDifferentSceneState();//相当于执行初始化


        UIControl.Instance.SetPanelControl(false, false, true);
        UIControl.Instance.SetHpChangeUI(false);
        UIControl.Instance.SetLeftUI(false);

        Debug.Log("游戏结束！");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion
}
