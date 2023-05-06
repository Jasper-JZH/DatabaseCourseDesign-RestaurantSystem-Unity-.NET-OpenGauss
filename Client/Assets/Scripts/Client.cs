using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoSingleton<Client>
{
    [SerializeField] private string host = "127.0.0.1";
    [SerializeField] private int port = 1234;
    [SerializeField] private Socket socket;

    const int BUFFER_SIZE = 1024 * 6;
    private byte[] readBuff = new byte[BUFFER_SIZE];

    [SerializeField] private string recvStr = "";

    //private bool isConnect = false;

    private DataDisposer.Ins curIns;    //��ʾ��ǰ���ڴ����Ins

    /// <summary>
    /// ��Ų�ͬ���͵�ָ���Ӧ�Ļص�����,�ڸ���ģ���ʼ�����Ҫ�õķ�����ӽ���
    /// </summary>
    public static Dictionary<DataDisposer.Ins, Action<string>> ReceiveCbDic = new();


    private void Awake()
    {
        //StartCoroutine(TryConnect(3f));       //TODO :�����ƣ�ʹ��BeginConnectѭ����������
        Connect();
    }


    public void DisConnect()
    {
        socket?.Disconnect(true);
    }

  /*  IEnumerator TryConnect(float _reTryTimeLag)
    {
        //��ʼ��socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        do
        {
            //Connect();
            //���ӷ�����
            socket.BeginConnect()
            socket.Connect(host, port);
            Debug.Log($"�ͻ��˵�ַ��{socket.LocalEndPoint}");
            yield return new WaitForSeconds(_reTryTimeLag);
            Debug.Log("�������ӣ�");
        } while (!socket.Connected);

        //��ʼ����
        socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
    }*/

    public void Connect()
    {
        //��ʼ��socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //���ӷ�����
        socket.Connect(host, port);
        Debug.Log($"�ͻ��˵�ַ��{socket.LocalEndPoint.ToString()}");
    }

    private string GetReceiveStr(int _count)
    {
        string recvStr = System.Text.Encoding.UTF8.GetString(readBuff, 0, _count);
        Debug.Log($"�����յ�������{_count}���ֽڵ����ݣ���{recvStr}");
        return recvStr;
    }
    /*
        private void ReceiveCb(IAsyncResult ar)
        {
            try
            {
                GetReceiveStr(socket.EndReceive(ar));
                //ѭ�����ü�������
                socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
            }
            catch(Exception e)
            {
                Debug.Log($"ReceiveCb�쳣:{e.Message}");
                socket.Close();
            }
        }
    */

    public void SendMsg(string _msg)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(_msg);
        try
        {
            socket.Send(bytes);
        }
        catch(Exception e)
        {
            Debug.Log($"SendMsg�쳣:{e.Message}");
        }
    }

    public void ReceiveAndDispose(DataDisposer.Ins _ins)
    {
        curIns = _ins;
        //����_ins��dic��ѡ��ʹ�ò�ͬ��RecieveCb
        socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
    }

    /// <summary>
    /// ͨ�õĽ��ջص�������ͨ��Insȷ��Ҫ�������߳���ִ�еķ���
    /// </summary>
    /// <param name="ar"></param>
    private void ReceiveCb(IAsyncResult ar)
    {
        //RCBֻ�������str�����ö�Ӧ����ģ��Ĵ�����
        try
        {
            string str = GetReceiveStr(socket.EndReceive(ar));
            //���ֵ���ѡȡ��Ӧ�ķ������������߳��е���
            Loom.QueueOnMainThread((param) => { ReceiveCbDic[curIns]?.Invoke(str); }, null);
        }
        catch (Exception e)
        {
            Debug.Log($"ReceiveCb�쳣:{e.Message}");
            socket.Close();
        }
    }
}
