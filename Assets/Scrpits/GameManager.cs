using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("��ȡ���")]

    [Tooltip("��ҿ��������")]
    public PlayerControl playerControl;

    [Tooltip("boss����")]
    public GameObject bossObject;

    [Header("����ͳһ���ƶ��ٶ�")]
    public float UniformSpeed = 40f;

    [Header("���õ�ǰ����")]
    public bool isFirstScene = false;
    public bool isSecondScene = false;

    [Header("˽�б���")]
    public bool isTestbool = false;

    private void Awake()
    {

    }


    void Start()
    {
        Initialization();
        SetUniformSpeed();
    }

    
    void Update()
    {
        if (isTestbool)
        {
            isTestbool = false;
            SetDifferentSceneState();
        }
    }

    #region ��ʼ��

    public void Initialization()
    {
        playerControl.gameObject.SetActive(false);
        bossObject.SetActive(false);
        playerControl.FirstScene = false;
        playerControl.SecendScene = false;
        MapGenerator.Instance.InitializationMap();
        ObstacleGenerator.Instance.InitializationObstacle();
        PropsGenerator.Instance.InitializationProps();
        LaserGenerator.Instance.InitializationLaser();
        SubduePropsGenerator.Instance.InitializationSubdue();
    }

    #endregion


    #region ����ͳһ���ƶ��ٶ�
    public void SetUniformSpeed()
    {
        MapGenerator.Instance.trackSpeed = UniformSpeed;
        ObstacleGenerator.Instance.obstacleSpeed = UniformSpeed;
        PropsGenerator.Instance.propsSpeed = UniformSpeed;
        SubduePropsGenerator.Instance.FragmentSpeed = UniformSpeed;
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
            MapGenerator.Instance.StartMapGenerator();                      //��ʼ��������˶�
            ObstacleGenerator.Instance.StartObstacleGenerator();            //��ʼ�ϰ������˶�
            LaserGenerator.Instance.StartObstacleGenerator();               //��ʼ��������
            PropsGenerator.Instance.StartPropsGenerator();                  //��ʼ��������
            SubduePropsGenerator.Instance.InitializationSubdue();           //�ر���Ƭ����
        }

        if (isSecondScene)
        {
            playerControl.gameObject.SetActive(true);
            bossObject.SetActive(true);
            playerControl.FirstScene = false;
            playerControl.SecendScene = true;
            MapGenerator.Instance.StartMapGenerator();
            ObstacleGenerator.Instance.InitializationObstacle();            //�ر��ϰ������˶�
            LaserGenerator.Instance.InitializationLaser();                  //�رռ�������
            PropsGenerator.Instance.InitializationProps();                  //�رյ�������
            SubduePropsGenerator.Instance.StartSubduePropsGenerator();      //��ʼ��Ƭ����
        }

        if (!isFirstScene && !isSecondScene)
        {
            Initialization();
        }
    }
    #endregion
}
