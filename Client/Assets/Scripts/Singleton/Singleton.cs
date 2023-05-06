public class Singleton<T> where T:new() //Լ��ֻ����һ�㣨��Mono����class
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
                instance = new T();
            }
            return instance;
        }
    }
}
