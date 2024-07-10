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

    //����غ���
    public Conn()
    {
        readBuff = new byte[Buffer_Size];
    }

    //��ʼ��
    public void Init(Socket scoket)
    {
        this.socket = scoket;
        isUse = true;
        buffCount = 0;
    }

    //������ʣ��
    public int BuffRemain()
    {
        return Buffer_Size - buffCount;
    }

    //��ȡ�ͻ��˵�ַ
    public string GetAdress()
    {
        if (!isUse)
            return "�û�Ϊ��";  

        return socket.RemoteEndPoint.ToString();//׷�ٿͻ�ip��˿ں�
    }

    //�ر�
    public void Cloae()
    {
        if (!isUse)
            return;

        Console.WriteLine("����" + GetAdress());
        socket.Close();
        isUse = false;
    }

}


/// <summary>
/// �첽����������������:����BeginAccept/EndAccept/AcceptCb, ��ȡBeginReceive/EndReceive/REceiveCb
/// </summary>
public class Serv
{
    public Socket listenfd;
    public Conn[] conns;
    public int maxConn = 50;

    //��������δ��ʹ�õ��û�
    public int NewIndex()
    {
        if (conns == null)//û��λ�÷���-1, ��ʾ�׳��쳣
            return -1;

        for (int i = 0; i < conns.Length; i++)
        {
            if (conns[i] == null)
            {
                //�п�λ�� ����λ��,��������
                conns[i] = new Conn();
                return i;
            }
            else if (conns[i].isUse == false)
                return i;
        }
        return -1;
    }



    //����������
    public void Start(string host, int port)
    {
        conns = new Conn[maxConn];
        for (int i = 0; i < maxConn; i++)
        {
            conns[i] = new Conn();
        }

        //����Socket
        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //Bind ip�Ͷ˿�
        IPAddress ipAdr = IPAddress.Parse(host);
        IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
        listenfd.Bind(ipEp);

        //����
        listenfd.Listen(maxConn);

        //���տͻ���
        listenfd.BeginAccept(AcceptCb, null);
        Console.WriteLine("������ ����!!!");
    }

    //���µ����ӷ���conn
    //���ܿͻ�������
    //�ٴε���BeginAccept
    private void AcceptCb(IAsyncResult ar)
    {
        try
        {
            Socket socket = listenfd.EndAccept(ar);
            int index = NewIndex();
            if (index < 0)
            {
                socket.Close();
                Console.WriteLine("��������");
            }
            
            else
            {
                Conn conn = conns[index];
                conn.Init(socket); //�����û�

                string adr = conn.GetAdress();//��ȡ�����IP�Ͷ˿ں�
                Console.WriteLine("����" + adr + "�û�ID" + index);
                //�������ݲ��ص�����
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
            }
            //���ӿͻ��� 
            listenfd.BeginAccept(AcceptCb, null);
        }
        catch(Exception e)
        {
            Console.WriteLine("����ʧ��" + e.Message);
        }
    }

    //���ջص�
    public void ReceiveCb(IAsyncResult ar)
    {
        Conn conn = (Conn)ar.AsyncState;
        try
        {
            int count = conn.socket.EndReceive(ar); //��������
            if(count<=0)
            {
                Console.WriteLine("�Ͽ�����:" + conn.GetAdress());
                conn.Cloae();
                return;
            }

            //���ݴ���
            string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, 0, count);
            Console.WriteLine(conn.GetAdress() + "�յ�����:" + str);
            str = conn.GetAdress() + ":" + str;
            byte[] bytes = System.Text.Encoding.Default.GetBytes(str);

            //�㲥
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
                Console.WriteLine("������Ϣ��:" + conns[i].GetAdress());
                conns[i].socket.Send(bytes);
            }

            //��������
            conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
        }
        catch(Exception e)
        {
            Console.WriteLine("�Ͽ�����" + conn.GetAdress());
        }
    }
}



class MainClass
{
    public static void Main(string[] args)
    {
        Serv serv = new Serv();
        Console.WriteLine("������  ����!!!");
        serv.Start("127.0.0.1", 1234);
        Console.WriteLine("��������ַ:127.0.0.1:1234");
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