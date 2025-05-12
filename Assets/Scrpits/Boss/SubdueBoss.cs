using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;
using DG.Tweening;

//固定大小队列
public class FixedSizeQueue<T>
{
    private Queue<T> queue = new Queue<T>();
    private int maxSize;

    //设置队列大小
    public FixedSizeQueue(int maxSize)
    {
        this.maxSize = maxSize;
    }

    //队列添加物体
    public void Enqueue(T item)
    {
        if (queue.Count >= maxSize)
        {
            queue.Dequeue(); // 移除最早的元素
        }
        queue.Enqueue(item); // 添加新元素
    }

    public void Clear()
    {
        queue.Clear();
    }

    public IEnumerable<T> Items => queue;
}


public enum SubdueBossProps
{
    RestoreHealth,
    DoubleSubdue,
    ExtensionTime
}


public class SubdueBoss : Singleton<SubdueBoss>
{
    private FixedSizeQueue<string> objectQueue = 
        new FixedSizeQueue<string>(3);

    public List<string> SubduePropsList = new List<string>();

    [Header("当前状态")]
    public bool isSubdueState = false;
    public string subdueBoss;

    [Header("字典")]
    private List<int> pressedKeys = new List<int>();
    private readonly Dictionary<string, System.Action<int, int>> bossEffects;
    private bool isExecuteCompleted = false;
    public SubdueBoss()
    {
        bossEffects = new Dictionary<string, System.Action<int, int>>
        {
            { "RestoreHealth", ExecuteRestoreHealth },
            { "DoubleSubdue", ExecuteDoubleSubdue },
            { "ExtensionTime", ExecuteExtensionTime }
        };
    }

    void Start()
    {
        
    }


    void Update()
    {
        if (!isSubdueState) return;
        if (!isExecuteCompleted) return;
        // 检测1-9的按键输入
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                if (!pressedKeys.Contains(i))
                {
                    pressedKeys.Add(i);
                    Debug.Log($"按下了按键: {i}");

                    if (pressedKeys.Count == 2)
                    {
                        ExecuteEffect();
                        pressedKeys.Clear();
                    }
                }
            }
        }
    }

    #region 碎片道具添加、检测

    //添加道具
    public void AddObjectToQueue(string obj)
    {
        objectQueue.Enqueue(obj);
    }

    //检测道具
    public void CheckIsTriplet()
    {
        if (objectQueue.Items.Count()!=3)
        {
            subdueBoss = null;
            Debug.Log("碎片不足3个");
            return;
        }

        SubduePropsList.Clear();

        // 把队列转成数组或列表来比较
        string[] array = objectQueue.Items.ToArray();

        foreach (var obj in array)
        {
            SubduePropsList.Add(obj);
        }

        if (SubduePropsList[0] == SubduePropsList[1] 
            && SubduePropsList[1] == SubduePropsList[2])
        {
            Debug.Log("3个相同，开始驯服");
            //执行驯服操作
            //清空掉当前队列和列表

            //0、先处于驯服状态---1、清空场景中已生成的---2、处理UI等的---3、处理完后
            subdueBoss = SubduePropsList[0];
            isExecuteCompleted = true;
            StartSubdueState();
        }
        else
        {
            subdueBoss = null;
            Debug.Log("3个碎片不相同");
        }
    }
    #endregion

    #region 道具判断
    //执行的效果
    private void ExecuteEffect()
    {
        if (bossEffects.ContainsKey(subdueBoss))
        {
            bossEffects[subdueBoss].Invoke(pressedKeys[0], pressedKeys[1]);
        }
    }
    private void ExecuteRestoreHealth(int key1, int key2)
    {
        isExecuteCompleted = false;
        // 根据结果恢复2点血
        int healthToRestore =2;
        //PlayerControl.Instance.RecoveryHp(healthToRestore);
        Debug.Log($"恢复血量：{healthToRestore}");
    }

    private void ExecuteDoubleSubdue(int key1, int key2)
    {
        isExecuteCompleted = false;
        Debug.Log($"双倍伤害驯服值");
    }

    private void ExecuteExtensionTime(int key1, int key2)
    {
        isExecuteCompleted = false;
        // 这里添加延长时间的逻辑
        Debug.Log($"延长游戏时间：{20}秒");
    }
    #endregion

    public void StartSubdueState()
    {

        pressedKeys.Clear();  // 清空之前的按键记录

        SubduePropsGenerator.Instance.InitializationSubdue();                   //1、清空场景中已生成的

        UIControl.Instance.SetSuccessUI(true);
        DOVirtual.DelayedCall(3f, () =>
        {
            isSubdueState = true;                                               //处于驯服状态
            SubduePropsList.Clear();
            objectQueue.Clear();
            UIControl.Instance.SetSuccessUI(false);
            UIControl.Instance.ClearImage();
        });

        DOVirtual.DelayedCall(10f, () => { 
            isSubdueState = false;
            SubduePropsGenerator.Instance.StartSubduePropsGenerator();
        });

    }
}
