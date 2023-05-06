using UnityEngine;
using UnityEngine.SceneManagement;

public class MonoSingleton<T> : MonoBehaviour where T:MonoBehaviour //Ҫ��ֻ�м̳���Mono������Լ̳и�MonoSingleton
{
    //ʵ�ʵ���
    private static T instance;

    //����ӿ�
    public static T Instance
    {
        get
        {
            if(instance==null)
            {
                instance = GameObject.FindObjectOfType<T>();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    private void Awake()
    {
        instance = Instance;    //ȷ���ⲿ��ȡInstanceʱ��Ϊ��
    }


    //�����л�ʱ���ո������class��
    //todo:
    //��ʱ��  �е���������л���ʱ����յ�
    public static bool destroyOnLoad = false;
    //��ӳ����л�ʱ����¼�
    public void AddSceneChangedEvent()
    {
        //SceneManager�Դ�����activeSceneChanged����һ��ί�У�������Ӱ󶨷���
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnSceneChanged(Scene arg0, Scene arg1)
    {
        if (destroyOnLoad == true)
        {
            if (instance != null)
            {
                DestroyImmediate(instance);//��������
                Debug.Log(instance == null);
            }
        }
    }
}
