using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
//using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#region restaurant���ݿ��и����Ӧ��obj
[Serializable]public class Tadmin    //admin
{
    public int id;
    public string name;
    public string phone;

    public Tadmin() { }
    public Tadmin(int _id, string _name, string _phone) { id = _id; name = _name; phone = _phone; }
}
[Serializable]public class Tcustomer  //customer
{
    public int id;
    public string name;
    public string phone;
    public Tcustomer() { }
    public Tcustomer(int _id, string _name, string _phone) 
    { id = _id; name = _name; phone = _phone;}
}

[Serializable]public class Tinventory  //inventory
{
    public int id;
    public int remain; 
    public Tinventory() { }
    public Tinventory(int _id, int _remain)
    { id = _id; remain = _remain; }
}

[Serializable]public class Tmenu  //menu
{
    public int id;
    public string food;
    public string description;
    public Tmenu() { }
    public Tmenu(int _id, string _food, string _description) { id = _id; food = _food; description = _description; }

}

[Serializable]
public class Tsize  //size
{
    public int id;
    public int mu_id;
    public float price;
    public string size;
    public Tsize() { }
    public Tsize(int _id, int _mu_id,string _size,float _price)
    { id = _id; mu_id = _mu_id;size = _size;price = _price ; }
}

//���ڹ˿Ͳ˵���ͼ�����ݽṹ
[Serializable]
public class VcusMenu
{
    public int s_id;
    public string mu_food;
    public string s_size;
    public float s_price;
    public string mu_desc;

    public VcusMenu() { }
    public VcusMenu(int _id, string _food, string _size, float _price, string _desc)
    { s_id = _id; mu_food = _food; s_size = _size; s_price = _price; mu_desc = _desc; }
}

[Serializable]
public class Torder  //order
{
    public int id;
    public int cus_id;
    public float price;
    public string address;
    public Torder() { }
    public Torder(int _id, int _cus_id, float _price, string _address)
    { id = _id; cus_id = _cus_id; price = _price; address = _address; }
}

[Serializable]
public class Tlist
{
    public int o_id;
    public int s_id;
    public Tlist() { }
    public Tlist(int _o_id,int _s_id)
    { o_id = _o_id; s_id = _s_id; }
}

#endregion

public class DataDisposer
{
    //��Server�е�Ins���Ӧ�����ڱ�ʶָ��
    public enum Ins
    {
        select_Login_Admin,
        select_Login_Cus,
        insert_Sign_Cus,
        select_All_Inventory,
        update_Inventory,
        select_All_Menu,
        insert_Menu,
        delete_Menu,
        update_Menu,
        update_Size,
        select_All_size,
        insert_Size,
        select_cusMenuView,
        insert_Order,
        insert_List

    }


    /// <summary>
    /// ����jsonģ�����Ϊ���շ��͵�json���ݰ�
    /// </summary>
    /// <param name="_action">��������</param>
    /// <param name="_table">Ŀ���</param>
    /// <param name="_property">Ŀ���������</param>
    /// <returns></returns>
    public static string Combine2Instruction(Ins _ins, JArray _property)
    {
        JObject jobj = new();
        jobj.Add("I", (int)_ins);
        //jobj.Add("T", _table);
        jobj.Add("P", _property);
        return jobj.ToString();
    }

    /// <summary>
    /// ��������ΪJarray
    /// </summary>
    /// <returns></returns>
    public static JArray CombineProperty2Json(JObject[] jobjs)
    {
        JArray jary = new();
        foreach(var item in jobjs)
        {
            jary.Add(item);
        }
        return jary;
    }

    /// <summary>
    /// ��string���͵���������ֵתΪjobject
    /// </summary>
    /// <param name="_key">������</param>
    /// <param name="_value">ֵ</param>
    /// <returns></returns>
    public static JObject ConvertStrProperty2Json(string _key,string _value)
    {
        if (_key == null || _value == null) return null;
        JObject jobj = new();
        jobj.Add(_key, _value);
        //Debug.Log($"ConvertStrProperty2Json��{jobj.ToString()}");
        return jobj;
    }

    /// <summary>
    /// ��json��ʽ��TadminתΪobj
    /// </summary>
    /// <param name="_jsonStr"></param>
    public static Tadmin DisposeTadminFromJson2Objs(string _jsonStr)
    {
        JObject jobj = JObject.Parse(_jsonStr);
        return jobj.ToObject<Tadmin>();
    }

    /// <summary>
    /// ��json��ʽ��TcustomerתΪobj
    /// </summary>
    /// <param name="_jsonStr"></param>
    public static Tcustomer DisposeTcustomerFromJson2Objs(string _jsonStr)
    {
        JObject jobj = JObject.Parse(_jsonStr);
        return jobj.ToObject<Tcustomer>();
    }

    /// <summary>
    /// ��json��ʽ��TinventoryתΪobj
    /// </summary>
    /// <param name="_jsonStr"></param>
    public static List<Tinventory> DisposeTinventoryListFromJson2Objs(string _jsonStr)
    {
        //��ֹһ�� Tinventory�����������
        JArray jary = JArray.Parse(_jsonStr);
        int length = jary.Count;

        List<Tinventory> invList = new();
        for(int i = 0; i < length; i++)
        {
            invList.Add(jary[i].ToObject<Tinventory>());
        }
        return invList;
    }

    public static List<Tmenu> DisposeTmenuListFromJson2Objs(string _jsonStr)
    {
        //���������
        JArray jary = JArray.Parse(_jsonStr);
        int length = jary.Count;

        List<Tmenu> List = new();
        for (int i = 0; i < length; i++)
        {
            List.Add(jary[i].ToObject<Tmenu>());
        }
        return List;
    }

    public static Tmenu DisposeTmenuFromJson2Objs(string _jsonStr)
    {
        JObject jobj = JObject.Parse(_jsonStr);
        return jobj.ToObject<Tmenu>();
    }

    public static List<Tsize> DisposeTsizeListFromJson2Objs(string _jsonStr)
    {
        //���������
        JArray jary = JArray.Parse(_jsonStr);
        int length = jary.Count;

        List<Tsize> List = new();
        for (int i = 0; i < length; i++)
        {
            List.Add(jary[i].ToObject<Tsize>());
        }
        return List;
    }

    public static List<VcusMenu> DisposeVcusMenuListFromJson2Objs(string _jsonStr)
    {
        //���������
        JArray jary = JArray.Parse(_jsonStr);
        int length = jary.Count;

        List<VcusMenu> List = new();
        for (int i = 0; i < length; i++)
        {
            List.Add(jary[i].ToObject<VcusMenu>());
        }
        return List;
    }
}
