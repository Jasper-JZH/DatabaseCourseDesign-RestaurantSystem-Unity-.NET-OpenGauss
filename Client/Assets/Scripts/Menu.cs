using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ���˵�����UI�������ö�Ӧ�ӿ�
/// </summary>

public class Menu : MonoBehaviour
{
    [SerializeField] private Transform transform;

    [SerializeField] private TMP_InputField id; //id��������
    [SerializeField] private TMP_InputField name; //name��������
    [SerializeField] private TMP_InputField phone;   //������������

    [SerializeField] private TextMeshProUGUI output;    //���

    [SerializeField] private Button customerSign;   //�˿�ע�ᰴť
    [SerializeField] private Button customerLogin;  //�˿͵�¼��ť
    [SerializeField] private Button adminLogin;   //����Ա��¼��ť
    [SerializeField] private Button quit;   //�˳���ť

    private string idStr;   //�˺�    
    private string nameStr; //����
    private string phoneStr; //����
    private string outputStr;  //���



    public void updateOutPut(string _output)
    {
        output.text = _output;
    }

    private void Awake()
    {
        transform = GetComponent<Transform>();
        //UI��
        id = transform.Find("_id").GetComponentInChildren<TMP_InputField>();
        name = transform.Find("_name").GetComponentInChildren<TMP_InputField>();
        phone = transform.Find("_ps").GetComponentInChildren<TMP_InputField>();
        output = transform.Find("_output").GetComponentInChildren<TextMeshProUGUI>();

        //��ȡ���а�ť�������
        Button[] ButtonArray = transform.Find("_button").GetComponentsInChildren<Button>();
        foreach(var button in ButtonArray)
        {
            switch(button.name)
            {
                case "customerSign[Button]":
                    customerSign = button;break;
                case "customerLogin[Button]":
                    customerLogin = button; break;
                case "adminLogin[Button]":
                    adminLogin = button; break;
                case "quit[Button]":
                    quit = button; break;
                default:Debug.Log("get unknown button!");break;
            }
        }
    }

    private void Start()
    {
        //��ʼ��
        idStr = phoneStr = "";
        //��ť��
        customerSign.onClick.AddListener(() => { CustomerSign(); });
        customerLogin.onClick.AddListener(() => { CustomerLogin(); });
        adminLogin.onClick.AddListener(() => { AdminLogin(); });
        quit.onClick.AddListener(() => { Quit(); });

        //�¼�����
        Authentication.AdminLoginEvent += OnAuthenticate;
        Authentication.CusLoginEvent += OnAuthenticate;
        Authentication.CusSignEvent += OnAuthenticate;
    }

    private void Update()
    {
        //ʵʱͬ������������
        idStr = id.text;
        nameStr = name.text;
        phoneStr = phone.text;
    }

    /// <summary>
    /// ��ӡ���
    /// </summary>
    /// <param name="_str"></param>
    private void Print(string _str)
    {
        outputStr = _str;
        output.text = outputStr;
    }

    /// <summary>
    /// ���绰����ĸ�ʽ�Ƿ���ȷ
    /// </summary>
    /// <param name="_phoneNum"></param>
    private bool CheckPhoneType(string _phoneNum)
    {
        if (_phoneNum.Length != 11)
        {
            Print("��������ȷ��ʽ��phone��");
            return false;
        }
        return true;
    }

    /// <summary>
    /// ע��
    /// </summary>
    private void CustomerSign()
    {
        //������
        if (idStr == "" || phoneStr == "" || nameStr == "")
        {
            if (idStr == "") Print("������id");
            else if (phoneStr == "") Print("������phone");
            else Print("������name");
            return;
        }
        else if(CheckPhoneType(phoneStr))
            Authentication.Instance.CusSign(idStr, nameStr, phoneStr);
    }

    /// <summary>
    /// ��¼
    /// </summary>
    private void CustomerLogin()
    {
        //������
        if (idStr == "")
        {
            Print("������id");
            return;
        } 
        else if (phoneStr == "")
        {
            Print("������phone");
            return;
        }
        else if (CheckPhoneType(phoneStr))
            Authentication.Instance.CusLogin(idStr, phoneStr);
    }

    private void AdminLogin()
    {
        //������
        if (idStr == "")
        {
            Print("������id");
            return;
        }
        else if (phoneStr == "")
        {
            Print("������phone");
            return;
        }
        else if (CheckPhoneType(phoneStr))
            Authentication.Instance.AdminLogin(idStr, phoneStr);
    }

    public void OnAuthenticate(Authentication.Result _authenticateResult)
    {
        switch (_authenticateResult)
        {
            case Authentication.Result.idNull:
                {
                    Print("id�����ڣ�");
                }
                break;
            case Authentication.Result.passwordMis:
                {
                    Print("�������");
                }
                break;
            case Authentication.Result.loginAsAdmin:
                {
                    Print("����Ա��¼�ɹ���");
                    //��ת����Ա����
                    SceneController.LoadScene(SceneController.myScene.ADMINE);
                }
                break;
            case Authentication.Result.loginAsCustomer:
                {
                    Print("�˿͵�¼�ɹ���");
                    //��ת�˿ͳ���
                    SceneController.LoadScene(SceneController.myScene.CUS);
                }
                break;
            case Authentication.Result.signAsCustomer:
                {
                    Print("�˿�ע��ɹ���");
                }
                break;
            case Authentication.Result.signFailAsIdUsed:
                {
                    Print("��ID�ѱ�ע�ᣡ");
                }
                break;
        }
        ResetInputField();
    }
    /// <summary>
    /// �����������
    /// </summary>
    private void ResetInputField()
    {
        idStr = phoneStr = nameStr = "";
        id.text = phone.text = name.text = "";
    }


    /// <summary>
    /// �˳�����
    /// </summary>
    public void Quit()
    {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        //Client.Instance.DisConnect();
    }
}
