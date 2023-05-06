using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using TMPro;

public class MenuManager : MonoSingleton<MenuManager>
{
    private Transform transform;    //����_inv����
    private Transform cellParentTransform; //ʵ������ĸ�

    private TMP_InputField id;      //id��������
    private TMP_InputField size;    
    private TMP_InputField price;    
    private TMP_InputField food;    //food��������
    private TMP_InputField desc;    //desc��������

    private Button addButton;   //���Ӱ�ť
    private Button deleteButton;
    private Button updateButton;
    //[SerializeField] private Button quit;   //�˳���ť
    private string idStr;          //����ı��
    private string sizeStr;        
    private string priceStr;       
    private string foodStr;        //�����ʳ��
    private string descStr;        //����ļ��


    private GameObject cellPrefab;

    //���ش��һ�ݱ������
    private List<Tmenu> menuList;
    private List<Tsize> sizeList;
    private Dictionary<int, MenuCell> menuDic;

    public class MenuCell
    {
        public string food;
        public Dictionary<string, float> size_price_dic;   //���Ͷ�Ӧ�ļ۸�
        public string desc;

        public MenuCell()
        {
            food = desc = "";
            size_price_dic = new();
        }

        public MenuCell(string _food, string _desc, Dictionary<string,float> _size_price_dic)
        {
            food = _food;
            desc = _desc;
            size_price_dic = new(_size_price_dic);
        }

        public MenuCell(MenuCell _menuCell)
        {
            food = _menuCell.food;
            desc = _menuCell.food;
            size_price_dic = new(_menuCell.size_price_dic); //ֵ���������ǵ�ַ����
        }
    }

    //��������ʵ����_cell�Ķ���ͨ��id������
    private Dictionary<int, GameObject> menuCellObjDic;

    //��ǰ�����
    private Tmenu curMenu;
    private Tsize curSize;

    //��ǰ������������MenuCell,��δ������ʹ��ԭ����ֵ
    private MenuCell curMenuCell;


    private void Awake()
    {
        transform = GetComponent<Transform>();
        cellParentTransform = GameObject.FindGameObjectWithTag("menuData").transform;
        //UI��
        TMP_InputField[] inputFields = transform.Find("_inputField").GetComponentsInChildren<TMP_InputField>();
        id = inputFields[0];
        food = inputFields[1];
        size = inputFields[2];
        price = inputFields[3];
        desc = inputFields[4];

        Button[] buttenArray = transform.Find("_button").GetComponentsInChildren<Button>();
        addButton = buttenArray[0];
        deleteButton = buttenArray[1];
        updateButton = buttenArray[2];

        //����Prefab
        cellPrefab = Resources.Load<GameObject>("Prefabs/_cell");

        //��ʼ��
        menuList = new();
        sizeList = new();

        menuCellObjDic = new();
        menuDic = new();

        //�ֵ��������
        Client.ReceiveCbDic.Add(DataDisposer.Ins.select_All_Menu, MenuManager.Instance.SetMenuData);
        Client.ReceiveCbDic.Add(DataDisposer.Ins.insert_Menu, MenuManager.Instance.SetAndShowNewFoodData);
        Client.ReceiveCbDic.Add(DataDisposer.Ins.delete_Menu, MenuManager.Instance.UnShowOneFoodData);
        Client.ReceiveCbDic.Add(DataDisposer.Ins.update_Menu, MenuManager.Instance.UpdateOneFoodDataShow);
        Client.ReceiveCbDic.Add(DataDisposer.Ins.update_Size, MenuManager.Instance.UpdateOneFoodSizePriceShow);
        Client.ReceiveCbDic.Add(DataDisposer.Ins.select_All_size, MenuManager.Instance.SetSizeData);
        Client.ReceiveCbDic.Add(DataDisposer.Ins.insert_Size, MenuManager.Instance.SetAndShowNewPriceSize);

    }

    private void Start()
    {
        //��ʼ��
        idStr = foodStr = descStr = "";

        //��ť��
        addButton.onClick.AddListener(() => { CheckAddType(); });
        deleteButton.onClick.AddListener(() => { DeleteOneFoodData(); });
        updateButton.onClick.AddListener(() => { CheckUpdateType(); });
    }

    private void Update()
    {
        //ʵʱͬ������������
        idStr = id.text;
        foodStr = food.text;
        descStr = desc.text;
        sizeStr = size.text;
        priceStr = price.text;
    }

    #region ����ȡ����ʳƷ�����ݡ�

    /// <summary>
    /// ��ȡ����Menu��������Ϣ
    /// </summary>
    public void GetMenuData()
    {
        //��װid�������Insָ��
        JArray Jproperty =
         DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("mu_id", "0")
         });

        //���ָ��
        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.select_All_Menu, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.select_All_Menu);
    }

    public void SetMenuData(string _dataStr)
    {
        //ֻ���汾��menu
        menuList = DataDisposer.DisposeTmenuListFromJson2Objs(_dataStr);

        //ȷ��menu������Ϻ�ŷ���size������
        GetSizeData();
    }

    /// <summary>
    /// ��ȡ����Size��������Ϣ
    /// </summary>
    public void GetSizeData()
    {
        //��װid�������Insָ��
        JArray Jproperty =
         DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("s_id", "0")
         });

        //���ָ��
        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.select_All_size, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.select_All_size);
    }

    public void SetSizeData(string _dataStr)
    {
        //ֻ���汾��size
        sizeList = DataDisposer.DisposeTsizeListFromJson2Objs(_dataStr);

        //ȷ��size������Ϻ��
        SetMenuCell();
        ShowMenuCell();
    }

    /// <summary>
    /// ����װ���menu��size��listװ��menucell
    /// </summary>
    public void SetMenuCell()
    {
        //����sizelist��װ��
        foreach(var item in sizeList)
        {
            //�ȳ��Դ�Dic���ң�����ҵĵ���ֱ���޸ģ��Ҳ����ͼ�һ���µĵ�Dic��
            int curMuid = item.mu_id;
            if (menuDic.TryGetValue(curMuid, out MenuCell cell)) //�Ѿ����ˣ�ֱ�Ӹ�
            {
                cell.size_price_dic.Add(item.size,item.price);
            }
            else 
            {
                MenuCell newCell = new();
                //��menuList���ҵ���Ӧ��food��desc��Ϣ
                foreach(var menu in menuList)
                {
                    if(menu.id == item. mu_id)
                    {
                        newCell.food = menu.food;
                        newCell.desc = menu.description;
                        break;
                    }
                }
                //װ��������Ϣ
                newCell.size_price_dic.Add(item.size, item.price);
                menuDic.Add(item.mu_id, newCell);  //װ��menu�ֵ��б��ش洢
            }
        }
    }

    /// <summary>
    /// ��ʾ����menucell
    /// </summary>
    public void ShowMenuCell()
    {
        //����Ƿ��Ѽ�����Դ
        if (cellPrefab == null) return;
        //����MenuDic��װ��MenuObjectDic
        foreach (var menuCell in menuDic)
        {
            GameObject newCellObj = Instantiate(cellPrefab);
            newCellObj.transform.parent = cellParentTransform;
            Transform idTrans = newCellObj.transform.GetChild(0);
            Transform foodTrans = newCellObj.transform.GetChild(1);
            Transform sizeTrans = newCellObj.transform.GetChild(2);
            Transform priceTrans = newCellObj.transform.GetChild(3);
            Transform descTrans = newCellObj.transform.GetChild(4);
            idTrans.GetComponentInChildren<TextMeshProUGUI>().text = menuCell.Key.ToString();
            idTrans.gameObject.name = $"[id]";
            foodTrans.GetComponentInChildren<TextMeshProUGUI>().text = menuCell.Value.food;
            foodTrans.gameObject.name = $"[food]";

            //����size����ʾ
            string sizes = "", prices = "";
            foreach(var item in menuCell.Value.size_price_dic)
            {
                sizes += $"{item.Key}/";
                prices += $"{item.Value}/";
            }
            sizeTrans.GetComponentInChildren<TextMeshProUGUI>().text = sizes;
            sizeTrans.gameObject.name = $"[size]";
            priceTrans.GetComponentInChildren<TextMeshProUGUI>().text = prices;
            priceTrans.gameObject.name = $"[price]";
            descTrans.GetComponentInChildren<TextMeshProUGUI>().text = menuCell.Value.desc;
            descTrans.gameObject.name = $"[desc]";

            //�ŵ�MenuCellObjDic�й���
            menuCellObjDic.Add(menuCell.Key, newCellObj);
        }
    }

    #endregion

    #region �����һ��ʳƷ�����ݡ�

    /// <summary>
    /// ������������������ж���ӵ�����
    /// </summary>
    public void CheckAddType()
    {
        //���ֿ���

        //����InputField���������������
        if(foodStr == "" && descStr == "")  //����ID������µ�size��price����Ҫid,size,price��
        {
            AddOneFoodSizePrice();
        }
        else  //ȫ�µ�food(�µ�id)��������size��price��
        {
            AddOneFoodData();
        }
    }

    /// <summary>
    /// ���һ��ʳ��ĵ�Menu��
    /// </summary>
    public void AddOneFoodData()
    {
        if (!CheckInputField(true, false, false, true, true)) return;
        //���浱ǰ�����menuCell
        Dictionary<string, float> new_size_price_Dic = new();   //newһ���յ�dic
        curMenuCell = new(foodStr, descStr, new_size_price_Dic);
        //��װ�������Insָ��
        JArray Jproperty =
         DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("mu_id", idStr),
            DataDisposer.ConvertStrProperty2Json("mu_food", foodStr),
            DataDisposer.ConvertStrProperty2Json("mu_description", descStr)
         });
        //���ָ��
        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.insert_Menu, Jproperty);
        //����
        Client.Instance.SendMsg(jsonInstruction);
        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.insert_Menu);
    }

    /// <summary>
    /// ���һ������ʳ����µ�size��price
    /// </summary>
    public void AddOneFoodSizePrice()
    {
        if (!CheckInputField(true, true, true)) return;
        
        //�ҵ���Ӧ��Menu������Ҳ���˵��û���ȴ���foodData����Ȼ��������sizeprice,����ʧ��
        if(!menuDic.TryGetValue(Convert.ToInt16(idStr),out MenuCell cell))
        {
            //û�ж�ӦID��menu
            AdminController.Print($"�Ҳ���IDΪ{idStr}��ʳ�");
            return;
        }

        //���ҵ���Ӧmenu,����·�װ��menucell
        cell.size_price_dic.Add(sizeStr, (float)Convert.ToDouble(priceStr));
        curMenuCell = new(cell);
        int s_id = GetSizeID(idStr, sizeStr);

        //��װid�������Insָ��
        JArray Jproperty =
         DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("s_id", s_id.ToString()),
            DataDisposer.ConvertStrProperty2Json("s_mu_id", idStr),
            DataDisposer.ConvertStrProperty2Json("s_size", sizeStr),
            DataDisposer.ConvertStrProperty2Json("s_price", priceStr)
         });

        //���ָ��
        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.insert_Size, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.insert_Size);
    }

    /// <summary>
    /// ����µ�Food����ʾ
    /// </summary>
    /// <param name="_str"></param>
    public void SetAndShowNewFoodData(string _str)
    {
        //strΪ2��ʾ���ĳɹ�(��Ϊ������insert��Ӱ��2��)��Ϊ0��ʾʧ��
        if (_str == "2") //�ɹ�
        {
            int newId = Convert.ToInt16(idStr);
            //��Ҫ��curMenuCell����menuCellList
            menuDic.Add(newId, curMenuCell);

            GameObject newCellObj = Instantiate(cellPrefab);
            newCellObj.transform.parent = cellParentTransform;

            Transform idTrans = newCellObj.transform.GetChild(0);
            Transform foodTrans = newCellObj.transform.GetChild(1);
            Transform sizeTrans = newCellObj.transform.GetChild(2);
            Transform priceTrans = newCellObj.transform.GetChild(3);
            Transform descTrans = newCellObj.transform.GetChild(4);

            idTrans.GetComponentInChildren<TextMeshProUGUI>().text = idStr;
            idTrans.gameObject.name = $"[id]";
            foodTrans.GetComponentInChildren<TextMeshProUGUI>().text = curMenuCell.food;
            foodTrans.gameObject.name = $"[food]";

            //size��price��ʱΪ��
            sizeTrans.GetComponentInChildren<TextMeshProUGUI>().text = "";
            sizeTrans.gameObject.name = $"[size]";
            priceTrans.GetComponentInChildren<TextMeshProUGUI>().text = "";
            priceTrans.gameObject.name = $"[price]";
            descTrans.GetComponentInChildren<TextMeshProUGUI>().text = curMenuCell.desc;
            descTrans.gameObject.name = $"[desc]";

            //obj�ŵ�MenuCellObjDic�й���
            menuCellObjDic.Add(newId, newCellObj);

            AdminController.Print($"���Ϊ{idStr}��ʳƷ��ӳɹ���");
            ResetInputField();
        }
        else //ʧ��
        {
            //���ʧ��
            AdminController.Print($"idΪ{idStr}��ʳƷ�Ѵ��ڣ�");
            ResetInputField();
        }
    }

    public void SetAndShowNewPriceSize(string _str)
    {
        //strΪ1��ʾ���ĳɹ���Ϊ0��ʾʧ��
        if (_str == "1") //�ɹ�
        {
            int muId = Convert.ToInt16(idStr);
            //������ʾ
            GameObject tarCellObj = menuCellObjDic[muId];
            Transform sizeTrans = tarCellObj.transform.GetChild(2);
            Transform priceTrans = tarCellObj.transform.GetChild(3);

            string sizes = "", prices = "";
            foreach (var item in curMenuCell.size_price_dic)
            {
                sizes += $"{item.Key}/";
                prices += $"{item.Value}/";
            }
            sizeTrans.GetComponentInChildren<TextMeshProUGUI>().text = sizes;
            priceTrans.GetComponentInChildren<TextMeshProUGUI>().text = prices;

            //����menuCellDic�е�����
            menuDic[muId] = curMenuCell;

            AdminController.Print($"����µĹ��ɹ���");
            ResetInputField();
        }
        else //ʧ��
        {
            //���ʧ��
            AdminController.Print($"����µĹ��ʧ�ܣ�");
            ResetInputField();
        }
    }


    #endregion

    #region ���޸�һ��ʳƷ�����ݡ�

    /// <summary>
    /// �������������ж���Ҫ�����ĸ����menu/size/menu+size��
    /// </summary>
    public void CheckUpdateType()
    {
        if (!CheckInputField(true)) return;        //������id
        //�Ƚ���id��check
        if (menuDic.TryGetValue(Convert.ToInt16(idStr), out MenuCell tarCell))
        {
            //������������������޸ı�������
            if (foodStr != "" || descStr != "")//�ж��Ƿ���Ҫ����menu��
            {
                Tmenu tarMenu = new(Convert.ToInt16(idStr), tarCell.food, tarCell.desc);
                if (foodStr != "") tarMenu.food = foodStr;
                if (descStr != "") tarMenu.description = descStr;
                UpdateOneFoodData(tarMenu);
            }

            //�ж��Ƿ���Ҫ����size��
            if (sizeStr != "")
            {
                if (!CheckInputField(true, true, true)) return; //ȷ��ͬʱ��size��price
                int sizeId = GetSizeID(idStr, sizeStr);
                Tsize tarSize = new(sizeId, Convert.ToInt16(idStr), sizeStr, (float)Convert.ToDouble(priceStr));
                UpdateOneFoodSizePrice(tarSize);
            }
        }
    }

    /// <summary>
    /// �޸�һ��Menu�е�ʳ����Ϣ
    /// </summary>
    public void UpdateOneFoodData(Tmenu tarMenu)
    {
        curMenu = tarMenu;    //���浱ǰ�����menu
        //��װid�������Insָ��
        JArray Jproperty =
        DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("mu_id", idStr),
            DataDisposer.ConvertStrProperty2Json("mu_food", curMenu.food),
            DataDisposer.ConvertStrProperty2Json("mu_description", curMenu.description),
        });

        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.update_Menu, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.update_Menu);
    }

    /// <summary>
    /// �޸�һ��ʳ��Ĺ����Ϣ
    /// </summary>
    public void UpdateOneFoodSizePrice(Tsize _tarSize)
    {
        curSize = _tarSize;
        JArray Jproperty =
        DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("s_id", curSize.id.ToString()),
            DataDisposer.ConvertStrProperty2Json("s_price", curSize.price.ToString()),
            DataDisposer.ConvertStrProperty2Json("s_size", curSize.size),
        });

        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.update_Size, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.update_Size);
    }

    /// <summary>
    /// ����һ������ʳ������ݵ���ʾ
    /// </summary>
    /// <param name="_str"></param>
    public void UpdateOneFoodDataShow(string _str)
    {
        //strΪ1��ʾ���ĳɹ���Ϊ0��ʾʧ��
        if (_str == "1") //�ɹ�
        {
            //�޸ı�������menulist��menucelldic
            foreach (var item in menuList)
            {
                if (item.id == curMenu.id)
                {
                    item.food = curMenu.food;
                    item.description = curMenu.description;
                    break;
                }
            }
            menuDic[curMenu.id].food = curMenu.food;
            menuDic[curMenu.id].desc = curMenu.description;

            //�޸���ʾ��menucellObjDic��
            GameObject tarMenuCell = menuCellObjDic[curMenu.id];
            Transform foodTrans = tarMenuCell.transform.GetChild(1);
            Transform descTrans = tarMenuCell.transform.GetChild(4);
            foodTrans.GetComponentInChildren<TextMeshProUGUI>().text = curMenu.food;
            descTrans.GetComponentInChildren<TextMeshProUGUI>().text = curMenu.description;

            AdminController.Print($"���Ϊ{curMenu.id}��ʳƷ��Ϣ���³ɹ���");
            ResetInputField();

        }
        else //ʧ��
        {
            AdminController.Print($"δ�ҵ�idΪ{curMenu.id}��ʳƷ��");
            ResetInputField();
        }
    }

    public void UpdateOneFoodSizePriceShow(string _str)
    {       
        //strΪ1��ʾ���ĳɹ���Ϊ0��ʾʧ��
        if (_str == "1") //�ɹ�
        {
            //�޸ı���sizelist��menucelldic
            foreach (var item in sizeList)
            {
                if (item.id == curSize.id)
                {
                    item.size = curSize.size;
                    item.price = curSize.price;
                    break;
                }
            }
            menuDic[curSize.mu_id].size_price_dic[curSize.size] = curSize.price;

            //�޸���ʾ��menucellObjDic��
            GameObject tarMenuCell = menuCellObjDic[curSize.mu_id];
            Transform sizeTrans = tarMenuCell.transform.GetChild(2);
            Transform priceTrans = tarMenuCell.transform.GetChild(3);
            //����size����ʾ
            string sizes = "", prices = "";
            foreach (var item in menuDic[curSize.mu_id].size_price_dic)
            {
                sizes += $"{item.Key}/";
                prices += $"{item.Value}/";
            }
            sizeTrans.GetComponentInChildren<TextMeshProUGUI>().text = sizes;
            priceTrans.GetComponentInChildren<TextMeshProUGUI>().text = prices;

            AdminController.Print($"���Ϊ{curSize.id}�Ĺ����Ϣ���³ɹ���");
            ResetInputField();

        }
        else //ʧ��
        {
            //���ʧ��
            AdminController.Print($"δ�ҵ�idΪ{curSize.id}��ʳƷ��");
            ResetInputField();
        }
    }
    #endregion

    #region ��ɾ��һ��ʳƷ�����ݡ�
    /// <summary>
    /// ɾ��һ��Menu�е�ʳ��
    /// </summary>
    public void DeleteOneFoodData()
    {
        //ֻ��Ҫ����ID
        if (!CheckInputField(true)) return;


        //��װid�������Insָ��
        JArray Jproperty =
         DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("mu_id", idStr)
         });

        //���ָ��
        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.delete_Menu, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.delete_Menu);

    }

    /// <summary>
    /// ɾ��һ������ʳ������ݵ���ʾ
    /// </summary>
    /// <param name="_str"></param>
    public void UnShowOneFoodData(string _str)
    {
        //strΪ1��ʾ�ɹ���Ϊ0��ʾʧ��
        if (_str == "1") //�ɹ�
        {
            //���ݿ��е�size��inventory�ἶ��ɾ�����ùܣ���ߴ����ֵ��к���ʾ�����ݵ�ɾ��
            //menu��ߵ�ɾ��
            //��ɾ�ֵ䣬sizeList��menuList��menuDic
            int tarMuId = Convert.ToInt16(idStr);
            foreach(var item in sizeList)
            {
                if(item.mu_id == tarMuId)
                {
                    sizeList.Remove(item);
                    break;
                }
            }
            foreach (var item in menuList)
            {
                if (item.id == tarMuId)
                {
                    menuList.Remove(item);
                    break;
                }
            }
            menuDic.Remove(tarMuId);

            //ɾ����ʾ
            GameObject tarObj = menuCellObjDic[tarMuId];
            menuCellObjDic.Remove(tarMuId);
            GameObject.Destroy(tarObj);

            //ɾ��Inventory�е���ʾ������Inv�ķ�����
            InvManager.Instance.UnShowOneInvData(tarMuId);

            AdminController.Print($"���Ϊ{idStr}��ʳƷ��Ϣɾ���ɹ���");
            ResetInputField();

        }
        else //ʧ��
        {
            //���ʧ��
            AdminController.Print($"δ�ҵ�idΪ{idStr}��ʳƷ��");
        }
    }

    #endregion

    private static int GetSizeID(string _muId, string _size)
    {
        //����s_id��s_id = s_mu_id+s_size(С0����1����2)
        string sizeNumStr = "";
        switch (_size)
        {
            case "��": sizeNumStr = "2"; break;
            case "��": sizeNumStr = "1"; break;
            case "С": sizeNumStr = "0"; break;
        }
        return Convert.ToInt32(_muId + sizeNumStr);
    }


    /// <summary>
    /// ����Ƿ�Ҫ����������
    /// </summary>
    /// <param name="_checkId"></param>
    /// <param name="_checkFood"></param>
    /// <param name="_checkDesc"></param>
    /// <returns></returns>
    private bool CheckInputField(bool _checkId = true, bool _checkSize = false, bool _checkPrice = false, bool _checkFood = false, bool _checkDesc = false)
    {
        //������
        if (_checkId)
        {
            if(idStr == "")
            {
                AdminController.Print("�������ţ�");
                return false;
            }
            else if(!AdminController.CheckIDType(idStr))
            {
                AdminController.Print("������4λ��ID��");
                ResetInputField();
                return false;
            }
        }
        if(_checkSize && sizeStr == "")
        {
            AdminController.Print("��������");
            return false;
        }
        if (_checkPrice && priceStr == "")
        {
            AdminController.Print("�����뵥�ۣ�");
            return false;
        }
        if (_checkFood && foodStr == "")
        {
            AdminController.Print("������ʳƷ��");
            return false;
        }
        if (_checkDesc && descStr == "")
        {
            AdminController.Print("�������飡");
            return false;
        }
        return true;
    }


    private void ResetInputField()
    {
        idStr = foodStr = descStr = sizeStr = priceStr = "";
        id.text = food.text = desc.text = size.text = price.text = "";
    }


}
