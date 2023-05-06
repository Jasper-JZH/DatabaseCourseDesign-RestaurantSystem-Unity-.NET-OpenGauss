using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using TMPro;

public class CusController : MonoSingleton<CusController>
{
    private Transform transform;    //����_inv����
    private Transform orderTrans;   //order�����trans
    private Transform cusCellParTrans; //ʵ������ĸ�
    private Transform orderCellParTrans; //ʵ������ĸ�

    private TMP_InputField food;      
    private TMP_InputField size;
    private TMP_InputField address;

    private Button addButton;   //���Ӱ�ť
    private Button confirmButton;   //�µ���ť

    private TextMeshProUGUI sumText;    //�ܼ�
    private TextMeshProUGUI outputText; //���

    //���ذ�ť
    private Button returnButton;
    private string foodStr;          //����ı��
    private string sizeStr;
    private string addressStr;
    private float sumPrice;   //����õ����ܼ�
    private static string outPutStr = "";

    private int cusID;  //�ͻ���ID
    private int curOId; //��ǰ������id

    private GameObject cusCellPrefab;
    private GameObject orderCellPrefab;


    //��������ʵ����_cell�Ķ���ͨ��id������
    private List<GameObject> orderCellObjList;
    private Dictionary<int, GameObject> cusCellObjDic;

    //���ػ����
    private Dictionary<string, Dictionary<string,Tsize>> sizeDic; //��������size
    private List<Tsize> curList; //��ŵ�ǰOrder�е�s_id�Ͷ�Ӧprice



    private void Awake()
    {
        //����Prefab
        cusCellPrefab= Resources.Load<GameObject>("Prefabs/_cusCell");
        orderCellPrefab = Resources.Load<GameObject>("Prefabs/_orderCell");

        transform = GetComponent<Transform>();
        orderTrans = transform.GetChild(1);
        cusCellParTrans = GameObject.FindGameObjectWithTag("menuData").transform;
        orderCellParTrans = GameObject.FindGameObjectWithTag("orderData").transform;

        //UI��
        returnButton = transform.parent.Find("return[Button]").GetComponent<Button>();
        outputText = transform.parent.Find("output[T]").GetComponent<TextMeshProUGUI>();
        addButton = orderTrans.Find("_addArea").GetComponentInChildren<Button>();
        TMP_InputField[] addInputFields = orderTrans.Find("_addArea").GetComponentsInChildren<TMP_InputField>();
        food = addInputFields[0];
        size = addInputFields[1];
        //Button[] buttenArray = transform.Find("_button").GetComponentsInChildren<Button>();

        sumText = orderTrans.Find("_sum").GetComponentsInChildren<TextMeshProUGUI>()[1];

        address = orderTrans.Find("_confirmOrder").GetComponentInChildren<TMP_InputField>();
        confirmButton = orderTrans.Find("_confirmOrder").GetComponentInChildren<Button>();


        //��ʼ��
        orderCellObjList = new();
        cusCellObjDic = new();
        sizeDic = new();
        curList = new();

        //��ӵ�Client
        Client.ReceiveCbDic.Add(DataDisposer.Ins.select_cusMenuView, CusController.Instance.ShowCusMenu);
        Client.ReceiveCbDic.Add(DataDisposer.Ins.insert_Order, CusController.Instance.OnOrderAdded);
        Client.ReceiveCbDic.Add(DataDisposer.Ins.insert_List, CusController.Instance.OnListSent);
    }

    private void Start()
    {
        //������
        addButton.onClick.AddListener(() => { AddOne2Order(); });
        confirmButton.onClick.AddListener(() => { ConfirmOrder(); });
        //���ذ�ť��
        returnButton.onClick.AddListener(() => { SceneController.OnReturnButtonClik(); });
        //��ȡ�˿�ID
        cusID = Authentication.Instance.GetCurCusID();
        //��ȡ�˿Ͳ˵���ͼ
        GetCusMenu();
    }

    private void Update()
    {
        //ʵʱͬ������������
        foodStr = food.text;
        sizeStr = size.text;
        addressStr = address.text;
        outputText.text = outPutStr;
        sumText.text = sumPrice.ToString();
    }

    #region����ȡ�ͻ��˵���ͼ�����ݡ�

    public void GetCusMenu()
    {
        //��װid�������Insָ��
        JArray Jproperty =
         DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("s_id", "0")
         });

        //���ָ��
        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.select_cusMenuView, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.select_cusMenuView);
    }

    public void ShowCusMenu(string _str)
    {
        //1. ���ղ�����
        List<VcusMenu> cusMenuList = DataDisposer.DisposeVcusMenuListFromJson2Objs(_str);

        //2. ת��Ϊobj��ʾ����һ��sizeDic
        //����Ƿ��Ѽ�����Դ
        if (cusCellPrefab == null)
        {
            //Print("Ԥ�������ʧ�ܣ�");
            return;
        }

        //����cusMenuList,��ͨ��ȥ��s_id��ĩβ�õ�mu_id
        //�����Ѵ��ڵ�mu_id�����޸�obj; ��������µ�obj
        foreach (var item in cusMenuList)
        {
            int muId = item.s_id / 10;  //����10ȥ����λ����mu_id

            if (cusCellObjDic.TryGetValue(muId, out GameObject obj))   //����mu_id
            {
              
                //����sizeDic�����ֵ�
                sizeDic[item.mu_food].Add(item.s_size, new(item.s_id, muId, item.s_size, item.s_price));

                //ֻ���޸�size��price���ֵ���ʾ
                Transform sizeTrans = obj.transform.GetChild(1);
                Transform priceTrans = obj.transform.GetChild(2);

                sizeTrans.GetComponentInChildren<TextMeshProUGUI>().text += item.s_size + "/";
                priceTrans.GetComponentInChildren<TextMeshProUGUI>().text += item.s_price.ToString() + "/";
            }
            else  //�µ�mu_id
            {
                //sizeDic�½�һ��
                sizeDic.Add(item.mu_food, new Dictionary<string, Tsize>());
                //����sizeDic�����ֵ�
                sizeDic[item.mu_food].Add(item.s_size, new(item.s_id, muId, item.s_size, item.s_price));

                GameObject newCellObj = Instantiate(cusCellPrefab);
                newCellObj.transform.parent = cusCellParTrans;
                Transform foodTrans = newCellObj.transform.GetChild(0);
                Transform sizeTrans = newCellObj.transform.GetChild(1);
                Transform priceTrans = newCellObj.transform.GetChild(2);
                Transform descTrans = newCellObj.transform.GetChild(3);

                foodTrans.GetComponentInChildren<TextMeshProUGUI>().text = item.mu_food;
                foodTrans.gameObject.name = $"[food]";
                sizeTrans.GetComponentInChildren<TextMeshProUGUI>().text = item.s_size + "/";
                sizeTrans.gameObject.name = $"[size]";
                priceTrans.GetComponentInChildren<TextMeshProUGUI>().text = item.s_price.ToString() + "/";
                priceTrans.gameObject.name = $"[price]";
                descTrans.GetComponentInChildren<TextMeshProUGUI>().text = item.mu_desc;
                descTrans.gameObject.name = $"[desc]";

                cusCellObjDic.Add(muId, newCellObj);//�����ֵ�
            }
        }
    }

    #endregion

    #region�����ʳ�ﵽ�����С�
    private void AddOne2Order()
    {
        if (!CheckInputField(true, true)) return;
        //�ȼ�������food�Ƿ����
        if(sizeDic.ContainsKey(foodStr))
        {
            //��ӵ�curList��
            Tsize tarSize = sizeDic[foodStr][sizeStr];
            curList.Add(tarSize);

            //ʵ����obj����ʾ
            GameObject newCell = Instantiate(orderCellPrefab);
            newCell.transform.parent = orderCellParTrans;

            Transform foodTrans = newCell.transform.GetChild(0);
            Transform sizeTrans = newCell.transform.GetChild(1);
            Button deleteButton = newCell.transform.GetComponentInChildren<Button>();

            foodTrans.GetComponentInChildren<TextMeshProUGUI>().text = foodStr;
            foodTrans.gameObject.name = $"[food]";
            sizeTrans.GetComponentInChildren<TextMeshProUGUI>().text = sizeStr;
            sizeTrans.gameObject.name = tarSize.id.ToString();
            //��̬�󶨰�ť
            deleteButton.onClick.AddListener(() => OnDeleteButtonClick());
            orderCellObjList.Add(newCell);
            sumPrice += tarSize.price; //�����ܼ�
            ResetInputField();
        }
        else
        {
            Print($"{foodStr}�����ڣ�");
        }
    }

    /// <summary>
    /// �����е�ɾ����ť���º���ã�����ɾ����������cell
    /// </summary>
    private void OnDeleteButtonClick()
    {
        GameObject cellObj = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        int sId = Convert.ToInt32(cellObj.transform.GetChild(1).gameObject.name);
        
        foreach(var item in curList)//�ӵ�ǰList���Ƴ�
        {
            if (item.id == sId)
            {
                curList.Remove(item);
                sumPrice -= item.price; //�����ܼ�
                break;
            }  
        }
        orderCellObjList.Remove(cellObj);   //���ֵ����Ƴ�
        //ɾ�������壨���ǵ�ǰcell��
        Destroy(cellObj);
    }

    #endregion

    # region ��ȷ�϶�����
    
    public void ConfirmOrder()
    {
        //���order�͵�ַ�Ƿ�Ϊ��
        if (orderCellObjList.Count <= 0)
        {
            Print("�������ʳƷ��"); return;
        }
        if (!CheckInputField(false, false, true)) return;

        //��ȡϵͳʱ����Ϊ������ID
        DateTime nowTime = DateTime.Now;
        string o_id = nowTime.Month.ToString() + nowTime.Day.ToString() + nowTime.Hour.ToString() + nowTime.Minute.ToString() + nowTime.Second.ToString();
        curOId = Convert.ToInt32(o_id);
        //��װid�������Insָ��
        JArray Jproperty =
         DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("o_id", o_id),
            DataDisposer.ConvertStrProperty2Json("o_cus_id", cusID.ToString()),
            DataDisposer.ConvertStrProperty2Json("o_price", sumPrice.ToString()),
            DataDisposer.ConvertStrProperty2Json("o_address", addressStr)
         });

        //���ָ��
        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.insert_Order, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.insert_Order);
    }

    public void OnOrderAdded(string _str)
    {
        if (_str == "1") //�ɹ�
        {
            
            Print($"�µ��ɹ���");
            ResetInputField();
            //����ϸ��List�������ݿ�
            SendAllList();
        }
        else //ʧ��
        {
            Print($"�µ�ʧ�ܣ�");
        }
    }
    #endregion

    #region ������ϸ��List�������ݿ⡿
    
    public void SendAllList()
    {
        //����List����,��ÿһ�����ַ�/�ֿ������ϵ������ܳ����ַ����У��ڷ��������и�
        string long_o_id = ""; //ÿһ���ɡ�id+/�����
        string long_s_id = "";
        foreach (var item in curList)
        {
            long_o_id += $"{curOId}/";
            long_s_id += $"{item.id}/";
        }
        Debug.Log($"[long_o_id]{long_o_id};[long_s_id]{long_s_id}");
        //��װ�������Insָ��
        JArray Jproperty =
         DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("l_o_id", long_o_id),
            DataDisposer.ConvertStrProperty2Json("l_s_id", long_s_id)
         });

        //���ָ��
        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.insert_List, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.insert_List);
    }

    public void OnListSent(string _str)
    {
        if (_str != "0") //�ɹ�
        {
            Debug.Log($"ȫ��List��Ϣ���ͳɹ���");
        }
        else //ʧ��
        {
            Debug.Log($"ȫ��List��Ϣ����ʧ�ܣ�");
        }
    }

    #endregion

    private bool CheckInputField(bool _checkFood = false, bool _checkSize = false, bool _checkAddress = false)
    {
        //������
        if (_checkFood && foodStr == "")
        {
            Print("������ʳ�");
            return false;
        }
        if (_checkSize && sizeStr == "")
        {
            Print("��������");
            return false;
        }
        if (_checkAddress && addressStr == "")
        {
            Print("�������ջ���ַ��");
            return false;
        }
        return true;
    }

    private void ResetInputField()
    {
        foodStr = addressStr = sizeStr = "";
        food.text = size.text = address.text = "";
    }

    public static void Print(string _str)
    {
        outPutStr = _str;
    }
}

