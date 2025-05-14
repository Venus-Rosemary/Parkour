using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("��ȡ���")]

    [Tooltip("��ҿ��������")]
    public PlayerControl playerControl;

    [Tooltip("boss����")]
    public GameObject bossObject;

    [Header("����ͳһ���ƶ��ٶ�")]
    public float UniformSpeed = 40f;
    public float LaserTranslationSpeed = 10f;
    public float BulletMoveSpeed = 60f;

    [Header("���õ�ǰ����")]
    public bool isFirstScene = false;
    public bool isSecondScene = false;

    [Header("����ʱ����")]
    [Tooltip("��һ����ʱ��")]
    public float firstSceneTime = 120f;    // ��һ����2����
    [Tooltip("�ڶ�����ʱ��")]
    public float secondSceneTime = 180f;   // �ڶ�����3����
    private float currentTime;              // ��ǰ����ʱ
    private bool isCountingDown = false;    // �Ƿ����ڵ���ʱ
    private bool isTransitioning = false;   // �Ƿ����ڳ����л�


    [Header("˽�б���")]
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

    #region ��ʼ��

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


    #region ����ͳһ���ƶ��ٶ�
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

    #region ���ò�ͬ��������Ĳ�ͬ�¼�����
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

            MapGenerator.Instance.StartMapGenerator();                      //��ʼ��������˶�
            ObstacleGenerator.Instance.StartObstacleGenerator();            //��ʼ�ϰ������˶�
            LaserGenerator.Instance.StartObstacleGenerator();               //��ʼ��������
            PropsGenerator.Instance.StartPropsGenerator();                  //��ʼ��������
            SubduePropsGenerator.Instance.InitializationSubdue();           //�ر���Ƭ����
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
            ObstacleGenerator.Instance.InitializationObstacle();            //�ر��ϰ������˶�
            LaserGenerator.Instance.InitializationLaser();                  //�رռ�������
            PropsGenerator.Instance.InitializationProps();                  //�رյ�������
            SubduePropsGenerator.Instance.StartSubduePropsGenerator();      //��ʼ��Ƭ����
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
        MapGenerator.Instance.StartMapGenerator();                      //��ʼ��������˶�
        ObstacleGenerator.Instance.InitializationObstacle();            //�ر��ϰ������˶�
        LaserGenerator.Instance.InitializationLaser();                  //�رռ�������
        PropsGenerator.Instance.InitializationProps();                  //�رյ�������
        SubduePropsGenerator.Instance.InitializationSubdue();           //�ر���Ƭ����
        SecondSceneObstacleGenerator.Instance.InitializationSecondObstacle();
        LaserTranslation.Instance.StopAttackLoop();
    }
    #endregion

    #region ����ʱ
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
            // �������������UI��ʾ����ʱ
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
                GameOver();//��Ϸ����
            }
        }
    }

    private IEnumerator TransitionToSecondScene()
    {
        isTransitioning = true;
        isCountingDown = false;
        SetTransitionalState();
        // �ȴ�10��
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

    #region ��Ϸ״̬����
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

    public void ReturnMenu()//�������˵���Ԥ����
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
        // ������
        isFirstScene = false;
        isSecondScene = false;

        SetDifferentSceneState();//�൱��ִ�г�ʼ��


        UIControl.Instance.SetPanelControl(false, false, true);
        UIControl.Instance.SetHpChangeUI(false);
        UIControl.Instance.SetLeftUI(false);

        Debug.Log("��Ϸ������");
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
