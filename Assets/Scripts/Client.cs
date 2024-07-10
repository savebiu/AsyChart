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
    //服务器的IP和端口
    public InputField hostInput;
    public InputField portInput;

    //显示客户端收到的消息
    public Text recvText;
    public string recvStr;

    //显示客户端ip和端口
    public Text clientText;

    //聊天输入框
    public InputField textInput;

    //socket和接收缓冲区
    Socket socket;
    const int BUFFSIZE = 1024;
    public byte[] readBuff = new byte[BUFFSIZE];

    //实时跟新文本数据
    public void Update()
    {

        recvText.text = recvStr;

    }

    //连接按钮
    public void Connection()
    {
        recvText.text = "";
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//建立连接
        string host = hostInput.text;
        int port = int.Parse(portInput.text);//获取ip和端口号
        socket.Connect(host, port);//连接
        clientText.text = "客户端地址:" +  socket.LocalEndPoint.ToString();

        socket.BeginReceive(readBuff, 0, BUFFSIZE, SocketFlags.None, ReceiveCb, null); //接收数据并回调接受
    }


    //接收并回调,开启下一次接受
    private void ReceiveCb(IAsyncResult ar)
    {
        try
        {
            int count = socket.EndReceive(ar); //将收到数据存到count中
            string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count); //转换数据格式为UTF8
            if (recvStr.Length > 300)
                recvStr = "";
            recvStr += str + "\n";
            socket.BeginReceive(readBuff, 0, BUFFSIZE, SocketFlags.None, ReceiveCb, null);
        }

        catch (Exception e)
        {
            recvText.text += "断开连接";
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


