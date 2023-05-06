using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �������ƣ����ṩ�л�����������ӿڣ�����ɳ����л�
/// </summary>
public class SceneController : MonoSingleton<SceneController>
{
    public enum myScene
    {
        MENU,
        ADMINE,
        CUS
    }

    public static void OnReturnButtonClik()
    {
        LoadScene(myScene.MENU);
    }

    public static void LoadScene(myScene _tarScene)
    {
        Debug.Log($"���س���{_tarScene.ToString()}");
        SceneManager.LoadScene((int)_tarScene);
    }
}
