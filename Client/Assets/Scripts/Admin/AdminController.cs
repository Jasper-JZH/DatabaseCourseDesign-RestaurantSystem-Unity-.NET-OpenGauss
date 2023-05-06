using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ����Admin,��������ģ��ĵ���
/// </summary>
public class AdminController : MonoBehaviour
{
    //���ذ�ť
    private Button returnButton;
    private TextMeshProUGUI outputText; //���

    private static string outputStr;

    private void Awake()
    {
        returnButton = transform.Find("return[Button]").GetComponent<Button>();
        outputText = transform.Find("output[T]").GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        //��ȡinventory�б�
        InvManager.Instance.GetAllInvData();

        //���ذ�ť��
        returnButton.onClick.AddListener(() => { SceneController.OnReturnButtonClik(); });
    }

    /// <summary>
    /// ���ID�Ƿ���ϸ�ʽҪ��
    /// </summary>
    public static bool CheckIDType(string _id)
    {
        return _id.Length == 4 ? true : false;
    }

    private void Update()
    {
        outputText.text = outputStr;
    }

    public static void Print(string _str)
    {
        outputStr = _str;
    }


}
