using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using TMPro;

public class InvManager : MonoSingleton<InvManager>
{
    private Transform transform;    //����_inv����
    private Transform dataParentTransform; //ʵ������id��remain�ĸ�

    private TMP_InputField id;      //id��������
    private TMP_InputField remain;  //remain��������

    private TextMeshProUGUI output;    //���
    private Button updateButton;   //���¿�水ť
    //[SerializeField] private Button quit;   //�˳���ť
    private string idStr;       //����ı��
    private string remainStr;   //����Ŀ��

    //������ʾ��id��remain��prefab
    private GameObject invDataCellPrefab;

    //���ش��һ������inventory�ĸ���invArray�������޸����ݺ�Ͳ���ȫ�����»�ȡ��
    [SerializeField] private Dictionary<int,int> invDic;

    //��������ʵ����remain�Ķ���ͨ��id�������Ӷ������޸�remain����ʾ
    private Dictionary<int, GameObject> remainCellDic;

    //��ǰ������������Inv
    private Tinventory curInv;


    private void Awake()
    {
        transform = GetComponent<Transform>();
        dataParentTransform = GameObject.FindGameObjectWithTag("invData").transform;
        //UI��
        TMP_InputField[] inputFields = transform.Find("_inputField").GetComponentsInChildren<TMP_InputField>();
        id = inputFields[0];
        remain = inputFields[1];

        updateButton = transform.Find("_button").GetComponentInChildren<Button>();

        //����Prefab
        invDataCellPrefab = Resources.Load<GameObject>("Prefabs/invDataCell");

        //��ʼ��
        invDic = new();
        remainCellDic = new();
    }

    private void Start()
    {

        //��ʼ��
        idStr = remainStr = "";
        curInv = new();
        //�ֵ��������
        Client.ReceiveCbDic.Add(DataDisposer.Ins.select_All_Inventory, InvManager.Instance.ShowAllInvData);
        Client.ReceiveCbDic.Add(DataDisposer.Ins.update_Inventory, InvManager.Instance.ShowOneInvData);

        //��ť��
        updateButton.onClick.AddListener(() => { UpdateOneInvData(); });
    }

    private void Update()
    {
        //ʵʱͬ������������
        idStr = id.text;
        remainStr = remain.text;
    }

    /// <summary>
    /// �޸�ĳ��ʳƷ�Ŀ��
    /// </summary>
    public void UpdateOneInvData()
    {
        if (!CheckInputField(true, true)) return;
        //����id��ps
        curInv.id = Convert.ToInt16(idStr);
        curInv.remain = Convert.ToInt16(remainStr);

        //��װid�������Insָ��
        JArray Jproperty =
        DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("i_mu_id", idStr),
            DataDisposer.ConvertStrProperty2Json("i_remain", remainStr)});

        //JArray Jtable = DataDisposer.CombineTables2Json(new string[] { "inventory" });

        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.update_Inventory, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.update_Inventory);

    }

    /// <summary>
    /// ��ȡ����Inv��Ϣ����ͨ��UI��ʾ��������
    /// </summary>
    public void GetAllInvData()
    {
        //JArray Jtable = DataDisposer.CombineTables2Json(new string[] { "inventory" });

        //��װid�������Insָ��
        JArray Jproperty =
         DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("i_mu_id", "0")
         });

        //���ָ��
        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.select_All_Inventory, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.select_All_Inventory);
    }

    public void ShowAllInvData(string _dataStr)
    {
//ȷ��Inv�Ѿ����º���
MenuManager.Instance.GetMenuData();

        //AdminController.Print("����������DataDisposer�ķ�������.....");
        //����DataDisposer�ķ�������
        List<Tinventory>invList = DataDisposer.DisposeTinventoryListFromJson2Objs(_dataStr);
        //ת��Dic���ش洢
        foreach(var item in invList)
        {
            invDic.Add(item.id, item.remain);
        }

        //����Ƿ��Ѽ�����Դ
        if(invDataCellPrefab == null)
        {
            //AdminController.Print("Ԥ�������ʧ�ܣ�");
            return;
        }

        //ʵ����Ԥ������װ��Inventory����
        foreach (var item in invList)
        {
            string id, remain;
            //ʵ����cell����ʾid
            GameObject newId = Instantiate(invDataCellPrefab);
            newId.transform.parent = dataParentTransform;
            id = item.id.ToString();
            newId.name = $"[id]{id}";
            newId.GetComponentInChildren<TextMeshProUGUI>().text = id;

            GameObject newRemain = Instantiate(invDataCellPrefab);
            newRemain.transform.parent = dataParentTransform;
            remain = item.remain.ToString();
            newRemain.name = $"[remain]{remain}";
            newRemain.GetComponentInChildren<TextMeshProUGUI>().text = remain;
            remainCellDic.Add(item.id, newRemain);  //remainҪ��ӵ�Dic�з�������޸�
        }


    }

    public void ShowOneInvData(string _str)
    {
        //strΪ1��ʾ���ĳɹ���Ϊ0��ʾʧ��
        if(_str == "1") //�ɹ�
        {
            //������������ж��ֵ����Ƿ��������������
            if(remainCellDic.ContainsKey(curInv.id))
            {
                //1. �ڳ�ʼ����ȡʱ���е�id�����ҵ��ɵ�ֱ���޸�
                GameObject tarCell = remainCellDic[curInv.id];
                string newRemainStr = curInv.remain.ToString();
                tarCell.GetComponentInChildren<TextMeshProUGUI>().text = newRemainStr;
                tarCell.name = $"[remain]{newRemainStr}";
                //�޸ı���invDic�е�����
                invDic[curInv.id] = curInv.remain;
            }
            else
            {
                //2. ͨ����������ӵ�id��������

                GameObject newId = Instantiate(invDataCellPrefab);
                newId.transform.parent = dataParentTransform;
                newId.name = $"[id]{curInv.id}";
                newId.GetComponentInChildren<TextMeshProUGUI>().text = curInv.id.ToString();

                GameObject newRemain = Instantiate(invDataCellPrefab);
                newRemain.transform.parent = dataParentTransform;
                newRemain.name = $"[remain]{curInv.remain}";
                newRemain.GetComponentInChildren<TextMeshProUGUI>().text = curInv.remain.ToString();
                remainCellDic.Add(curInv.id, newRemain);  //remainҪ��ӵ�Dic�з�������޸�
                //�ڱ���invDic������������
                invDic.Add(curInv.id, curInv.remain);
            }

            AdminController.Print($"{curInv.id}�Ŀ����Ϣ���³ɹ���");
            ResetInputField();
        }
        else //ʧ��
        {
            //���ʧ��
            AdminController.Print($"δ�ҵ�idΪ{curInv.id}ʳƷ��");
            ResetInputField();
        }
    }

    private bool CheckInputField(bool _checkId, bool _checkRemain)
    {
        //������
        if (_checkId)
        {
            if (idStr == "")
            {
                AdminController.Print("�������ţ�");
                ResetInputField();
                return false;
            }
            else if (!AdminController.CheckIDType(idStr))
            {
                AdminController.Print("������4λ��ID��");
                ResetInputField();
                return false;
            }
        }
        else if (_checkRemain && remainStr == "")
        {
            AdminController.Print("�������棡");
            ResetInputField();
            return false;
        }
        return true;
    }

    /// <summary>
    /// ��MenuManager���õķ�����������ɾ��menu��ͬʱ����inv����ʾ
    /// </summary>
    public void UnShowOneInvData(int _tarMuId)
    {
         //�ӱ���invDic��ɾ��
        if (invDic.ContainsKey(_tarMuId))
        {
            invDic.Remove(_tarMuId);
            //ɾ����Ӧ����ʾremaincell��id
            GameObject tarObj = remainCellDic[_tarMuId];
            remainCellDic.Remove(_tarMuId);
            GameObject.Destroy(tarObj);
            GameObject.Destroy(dataParentTransform.Find($"[id]{_tarMuId}").gameObject);
            //AdminController.Print($"����ɾ��{_tarMuId}��inv���ݳɹ���");
        }
    }

    private void ResetInputField()
    {
        idStr = remainStr= "";
        id.text = remain.text = "";
    }
}
