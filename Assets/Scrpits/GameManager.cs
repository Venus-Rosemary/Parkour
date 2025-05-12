using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("获取组件")]

    [Tooltip("玩家控制器组件")]
    public PlayerControl playerControl;

    [Tooltip("boss物体")]
    public GameObject bossObject;

    [Header("设置统一的移动速度")]
    public float UniformSpeed = 40f;

    [Header("设置当前场景")]
    public bool isFirstScene = false;
    public bool isSecondScene = false;

    [Header("私有变量")]
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

    #region 初始化

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


    #region 设置统一的移动速度
    public void SetUniformSpeed()
    {
        MapGenerator.Instance.trackSpeed = UniformSpeed;
        ObstacleGenerator.Instance.obstacleSpeed = UniformSpeed;
        PropsGenerator.Instance.propsSpeed = UniformSpeed;
        SubduePropsGenerator.Instance.FragmentSpeed = UniformSpeed;
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
            MapGenerator.Instance.StartMapGenerator();                      //开始轨道生成运动
            ObstacleGenerator.Instance.StartObstacleGenerator();            //开始障碍生成运动
            LaserGenerator.Instance.StartObstacleGenerator();               //开始激光生成
            PropsGenerator.Instance.StartPropsGenerator();                  //开始道具生成
            SubduePropsGenerator.Instance.InitializationSubdue();           //关闭碎片生成
        }

        if (isSecondScene)
        {
            playerControl.gameObject.SetActive(true);
            bossObject.SetActive(true);
            playerControl.FirstScene = false;
            playerControl.SecendScene = true;
            MapGenerator.Instance.StartMapGenerator();
            ObstacleGenerator.Instance.InitializationObstacle();            //关闭障碍生成运动
            LaserGenerator.Instance.InitializationLaser();                  //关闭激光生成
            PropsGenerator.Instance.InitializationProps();                  //关闭道具生成
            SubduePropsGenerator.Instance.StartSubduePropsGenerator();      //开始碎片生成
        }

        if (!isFirstScene && !isSecondScene)
        {
            Initialization();
        }
    }
    #endregion
}
