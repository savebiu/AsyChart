using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using UnityEditor.MemoryProfiler;
using System;
using Unity.VisualScripting;

public class Client : MonoBehaviour
{
    //��������IP�Ͷ˿�
    public InputField hostInput;
    public InputField portInput;

    //��ʾ�ͻ����յ�����Ϣ
    public Text recvText;
    public string recvStr;

    //��ʾ�ͻ���ip�Ͷ˿�
    public Text clientText;

    //���������
    public InputField textInput;

    //socket�ͽ��ջ�����
    Socket socket;
    const int BUFFSIZE = 1024;
    public byte[] readBuff = new byte[BUFFSIZE];

    //ʵʱ�����ı�����
    public void Update()
    {

        recvText.text = recvStr;

    }

    //���Ӱ�ť
    public void Connection()
    {
        recvText.text = "";
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//��������
        string host = hostInput.text;
        int port = int.Parse(portInput.text);//��ȡip�Ͷ˿ں�
        socket.Connect(host, port);//����
        clientText.text = "�ͻ��˵�ַ:" +  socket.LocalEndPoint.ToString();

        socket.BeginReceive(readBuff, 0, BUFFSIZE, SocketFlags.None, ReceiveCb, null); //�������ݲ��ص�����
    }


    //���ղ��ص�,������һ�ν���
    private void ReceiveCb(IAsyncResult ar)
    {
        try
        {
            int count = socket.EndReceive(ar); //���յ����ݴ浽count��
            string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count); //ת�����ݸ�ʽΪUTF8
            if (recvStr.Length > 300)
                recvStr = "";
            recvStr += str + "\n";
            socket.BeginReceive(readBuff, 0, BUFFSIZE, SocketFlags.None, ReceiveCb, null);
        }

        catch (Exception e)
        {
            recvText.text += "�Ͽ�����";
            socket.Close();
        }
    }

    public void Send()
    {
        string str = textInput.text;
        byte[] bytes = new byte[BUFFSIZE];
        try
        {
            socket.Send(bytes);
        }
        catch { }
    }
}


