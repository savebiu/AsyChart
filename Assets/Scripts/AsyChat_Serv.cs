using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Conn
{
    public const int Buffer_Size = 1024;
    public Socket socket;
    public bool isUse = false;
    public byte[] readBuff = new byte[Buffer_Size];
    public int buffCount = 0;
    //public int BuffRemain => Buffer_Size - buffCount;

    //对象池函数
    public Conn()
    {
        readBuff = new byte[Buffer_Size];
    }

    //初始化
    public void Init(Socket scoket)
    {
        this.socket = scoket;
        isUse = true;
        buffCount = 0;
    }

    //缓冲区剩余
    public int BuffRemain()
    {
        return Buffer_Size - buffCount;
    }

    //获取客户端地址
    public string GetAdress()
    {
        if (!isUse)
            return "用户为空";  

        return socket.RemoteEndPoint.ToString();//追踪客户ip与端口号
    }

    //关闭
    public void Cloae()
    {
        if (!isUse)
            return;

        Console.WriteLine("断连" + GetAdress());
        socket.Close();
        isUse = false;
    }

}


/// <summary>
/// 异步服务器的三个作用:连接BeginAccept/EndAccept/AcceptCb, 获取BeginReceive/EndReceive/REceiveCb
/// </summary>
public class Serv
{
    public Socket listenfd;
    public Conn[] conns;
    public int maxConn = 50;

    //索引返回未被使用的用户
    public int NewIndex()
    {
        if (conns == null)//没有位置返回-1, 表示抛出异常
            return -1;

        for (int i = 0; i < conns.Length; i++)
        {
            if (conns[i] == null)
            {
                //有空位置 创建位置,返回索引
                conns[i] = new Conn();
                return i;
            }
            else if (conns[i].isUse == false)
                return i;
        }
        return -1;
    }



    //启动服务器
    public void Start(string host, int port)
    {
        conns = new Conn[maxConn];
        for (int i = 0; i < maxConn; i++)
        {
            conns[i] = new Conn();
        }

        //建立Socket
        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //Bind ip和端口
        IPAddress ipAdr = IPAddress.Parse(host);
        IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
        listenfd.Bind(ipEp);

        //监听
        listenfd.Listen(maxConn);

        //接收客户端
        listenfd.BeginAccept(AcceptCb, null);
        Console.WriteLine("服务器 启动!!!");
    }

    //给新的连接分配conn
    //接受客户端数据
    //再次调用BeginAccept
    private void AcceptCb(IAsyncResult ar)
    {
        try
        {
            Socket socket = listenfd.EndAccept(ar);
            int index = NewIndex();
            if (index < 0)
            {
                socket.Close();
                Console.WriteLine("连接已满");
            }
            
            else
            {
                Conn conn = conns[index];
                conn.Init(socket); //分配用户

                string adr = conn.GetAdress();//获取分配的IP和端口号
                Console.WriteLine("连接" + adr + "用户ID" + index);
                //接收数据并回调接收
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
            }
            //连接客户端 
            listenfd.BeginAccept(AcceptCb, null);
        }
        catch(Exception e)
        {
            Console.WriteLine("连接失败" + e.Message);
        }
    }

    //接收回调
    public void ReceiveCb(IAsyncResult ar)
    {
        Conn conn = (Conn)ar.AsyncState;
        try
        {
            int count = conn.socket.EndReceive(ar); //结束接收
            if(count<=0)
            {
                Console.WriteLine("断开连接:" + conn.GetAdress());
                conn.Cloae();
                return;
            }

            //数据处理
            string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, 0, count);
            Console.WriteLine(conn.GetAdress() + "收到数据:" + str);
            str = conn.GetAdress() + ":" + str;
            byte[] bytes = System.Text.Encoding.Default.GetBytes(str);

            //广播
            for (int i = 0; i<conns.Length; i++)
            {
                if (conns[i] == null)
                {
                    continue ;
                }
                else if (!conns[i].isUse)  
                {
                    continue ;
                }
                Console.WriteLine("发送消息给:" + conns[i].GetAdress());
                conns[i].socket.Send(bytes);
            }

            //继续接收
            conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
        }
        catch(Exception e)
        {
            Console.WriteLine("断开连接" + conn.GetAdress());
        }
    }
}



class MainClass
{
    public static void Main(string[] args)
    {
        Serv serv = new Serv();
        Console.WriteLine("服务器  启动!!!");
        serv.Start("127.0.0.1", 1234);
        Console.WriteLine("服务器地址:127.0.0.1:1234");
        while (true)
        {
            string str = Console.ReadLine();
            switch(str)
            {
                case "quit": return;
            }
        }
    }
}