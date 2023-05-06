using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

/// <summary>
/// ʵ��ע�ᡢ��¼���߼�
/// </summary>
public class Authentication : MonoSingleton<Authentication>
{
    //public Client client;
    public enum Result
    {
        idNull,             //id������
        passwordMis,        //�������
        loginAsAdmin,       //����Ա�����֤�ɹ�
        loginAsCustomer,    //�˿������֤�ɹ�
        signAsCustomer,      //�˿�ע��ɹ�
        signFailAsIdUsed     //�˿�ע��ʧ�ܣ�ID�ѱ�ʹ��
    }

    //�����¼���ί��
    public delegate void LoginHandler(Result _authenticateResult);
    //�����¼����¼�event���Կ���һ�����ƣ�����ί�е�ʵ��ֻ���ڱ����е��ã�invoke�ȣ���
    public static event LoginHandler AdminLoginEvent;
    public static event LoginHandler CusLoginEvent;
    public static event LoginHandler CusSignEvent;

    public Tadmin curAdmin;
    public Tcustomer curCustomer;

    public int GetCurCusID()
    {
        return curCustomer.id;
    }

    private void Awake()
    {
        //ReceiveCbDic�������
        if(!Client.ReceiveCbDic.ContainsKey(DataDisposer.Ins.select_Login_Admin))
            Client.ReceiveCbDic.Add(DataDisposer.Ins.select_Login_Admin, Authentication.Instance.AuthenticateAdmin);
        
        if (!Client.ReceiveCbDic.ContainsKey(DataDisposer.Ins.select_Login_Cus))
            Client.ReceiveCbDic.Add(DataDisposer.Ins.select_Login_Cus, Authentication.Instance.AuthenticateCus);
        
        if (!Client.ReceiveCbDic.ContainsKey(DataDisposer.Ins.insert_Sign_Cus))
            Client.ReceiveCbDic.Add(DataDisposer.Ins.insert_Sign_Cus, Authentication.Instance.AuthenticateCusSign);
    }

    private void Start()
    {
        curAdmin = new();
        curCustomer = new();
       
    }

    /// <summary>
    /// ����Ա��¼
    /// </summary>
    /// <returns></returns>
    public void AdminLogin(string _id, string _phone)
    {
        //����id��ps
        curAdmin.id = Convert.ToInt16(_id);
        curAdmin.phone = _phone;

        //��װid�������Insָ��
        JArray Jproperty = 
        DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("a_id", _id)});

        //JArray Jtable = DataDisposer.CombineTables2Json(new string[] { "admin" });

        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.select_Login_Admin,Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.select_Login_Admin);
    }

    /// <summary>
    /// �˿͵�¼
    /// </summary>
    /// <param name="_id"></param>
    /// <param name="_phone"></param>
    public void CusLogin(string _id, string _phone)
    {
        //����id��ps
        curCustomer.id = Convert.ToInt16(_id);
        curCustomer.phone = _phone;

        //��װid�������Insָ��
        JArray Jproperty =
         DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("cus_id", _id)});

        //JArray Jtable = DataDisposer.CombineTables2Json(new string[] { "customer" });

        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.select_Login_Cus, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.select_Login_Cus);
    }

    /// <summary>
    /// �˿�ע��
    /// </summary>
    /// <param name="_id"></param>
    /// <param name="_name"></param>
    /// <param name="_phone"></param>
    public void CusSign(string _id, string _name, string _phone)
    {
        //����id��ps
        curCustomer.id = Convert.ToInt16(_id);
        curCustomer.name = _name;
        curCustomer.phone = _phone;

        //��װid�������Insָ��
        JArray Jproperty =
         DataDisposer.CombineProperty2Json(new JObject[] {
            DataDisposer.ConvertStrProperty2Json("cus_id", _id),
            DataDisposer.ConvertStrProperty2Json("cus_name", _name),
            DataDisposer.ConvertStrProperty2Json("cus_phone", _phone)
         });

        //JArray Jtable = DataDisposer.CombineTables2Json(new string[] { "customer" });

        string jsonInstruction = DataDisposer.Combine2Instruction(
            DataDisposer.Ins.insert_Sign_Cus, Jproperty);

        //����
        Client.Instance.SendMsg(jsonInstruction);

        //���ս���serv������json
        Client.Instance.ReceiveAndDispose(DataDisposer.Ins.insert_Sign_Cus);
    }

    /// <summary>
    /// ��֤Admin����ݣ����룩
    /// </summary>
    public void AuthenticateAdmin(string _adminStr)
    {
        //����DataDisposer�ķ�������
        Tadmin newAdmin = DataDisposer.DisposeTadminFromJson2Objs(_adminStr);
        
        //�ж��Ƿ�Ϊ��
        if(newAdmin.id == 0)    //a_id������
        {
            AdminLoginEvent?.Invoke(Result.idNull);
        }
        else
        {
            //�Ƚ�newAdmin�ͻ����curAdmin��phone�Ƿ���ͬ,�����ȽϵĽ��֪ͨ��AdminLoginHandlerEvent
            //�Ķ����ߣ�����Menu��ʵ��UI����ʾ��
            Result result = curAdmin.phone.Equals(newAdmin.phone) == true ? Result.loginAsAdmin : Result.passwordMis; 
            AdminLoginEvent?.Invoke(result);
        }
    }

    /// <summary>
    /// ��֤Customer����ݣ����룩
    /// </summary>
    public void AuthenticateCus(string _cusStr)
    {
        //����DataDisposer�ķ�������
        Tcustomer newCus = DataDisposer.DisposeTcustomerFromJson2Objs(_cusStr);

        //�ж��Ƿ�Ϊ��
        if (newCus.id == 0)    //cus_id������
        {
            Debug.Log("cus_id������");
            CusLoginEvent?.Invoke(Result.idNull);
        }
        else
        {
            Result result = curCustomer.phone.Equals(newCus.phone) == true ? Result.loginAsCustomer : Result.passwordMis;
            CusLoginEvent?.Invoke(result);
        }
    }

    /// <summary>
    /// Customerע����֤
    /// </summary>
    /// <param name="_cusStr"></param>
    public void AuthenticateCusSign(string _result)
    {
        Result result = _result == "1" ? Result.signAsCustomer : Result.signFailAsIdUsed;
        CusSignEvent?.Invoke(result);
    }
}
