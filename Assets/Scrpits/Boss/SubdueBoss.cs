using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;
using DG.Tweening;

//�̶���С����
public class FixedSizeQueue<T>
{
    private Queue<T> queue = new Queue<T>();
    private int maxSize;

    //���ö��д�С
    public FixedSizeQueue(int maxSize)
    {
        this.maxSize = maxSize;
    }

    //�����������
    public void Enqueue(T item)
    {
        if (queue.Count >= maxSize)
        {
            queue.Dequeue(); // �Ƴ������Ԫ��
        }
        queue.Enqueue(item); // �����Ԫ��
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

    [Header("��ǰ״̬")]
    public bool isSubdueState = false;
    public string subdueBoss;

    [Header("�ֵ�")]
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
        // ���1-9�İ�������
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                if (!pressedKeys.Contains(i))
                {
                    pressedKeys.Add(i);
                    Debug.Log($"�����˰���: {i}");

                    if (pressedKeys.Count == 2)
                    {
                        ExecuteEffect();
                        pressedKeys.Clear();
                    }
                }
            }
        }
    }

    #region ��Ƭ������ӡ����

    //��ӵ���
    public void AddObjectToQueue(string obj)
    {
        objectQueue.Enqueue(obj);
    }

    //������
    public void CheckIsTriplet()
    {
        if (objectQueue.Items.Count()!=3)
        {
            subdueBoss = null;
            Debug.Log("��Ƭ����3��");
            return;
        }

        SubduePropsList.Clear();

        // �Ѷ���ת��������б����Ƚ�
        string[] array = objectQueue.Items.ToArray();

        foreach (var obj in array)
        {
            SubduePropsList.Add(obj);
        }

        if (SubduePropsList[0] == SubduePropsList[1] 
            && SubduePropsList[1] == SubduePropsList[2])
        {
            Debug.Log("3����ͬ����ʼѱ��");
            //ִ��ѱ������
            //��յ���ǰ���к��б�

            //0���ȴ���ѱ��״̬---1����ճ����������ɵ�---2������UI�ȵ�---3���������
            subdueBoss = SubduePropsList[0];
            isExecuteCompleted = true;
            StartSubdueState();
        }
        else
        {
            subdueBoss = null;
            Debug.Log("3����Ƭ����ͬ");
        }
    }
    #endregion

    #region �����ж�
    //ִ�е�Ч��
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
        // ���ݽ���ָ�2��Ѫ
        int healthToRestore =2;
        //PlayerControl.Instance.RecoveryHp(healthToRestore);
        Debug.Log($"�ָ�Ѫ����{healthToRestore}");
    }

    private void ExecuteDoubleSubdue(int key1, int key2)
    {
        isExecuteCompleted = false;
        Debug.Log($"˫���˺�ѱ��ֵ");
    }

    private void ExecuteExtensionTime(int key1, int key2)
    {
        isExecuteCompleted = false;
        // ��������ӳ�ʱ����߼�
        Debug.Log($"�ӳ���Ϸʱ�䣺{20}��");
    }
    #endregion

    public void StartSubdueState()
    {

        pressedKeys.Clear();  // ���֮ǰ�İ�����¼

        SubduePropsGenerator.Instance.InitializationSubdue();                   //1����ճ����������ɵ�

        UIControl.Instance.SetSuccessUI(true);
        DOVirtual.DelayedCall(3f, () =>
        {
            isSubdueState = true;                                               //����ѱ��״̬
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
