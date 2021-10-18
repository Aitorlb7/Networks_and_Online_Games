using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDP : MonoBehaviour
{
    private Thread thread = null;


    private Socket UDPSocket;
    private IPEndPoint ip;
    private EndPoint remoteIP;

    private string IP = "127.0.0.1";  
    private byte[] data = new byte[1024];

    private int recievedData;
    private string recievedMessage;
    private bool readyToSend;

    static readonly object lockObject = new object();

    public int ownPort = 7777;
    public int destPort = 7777;
    public string message = "Ping";
    public int sleepSeconds;
    public bool isClient;


    void Start()
    {
        ip = new IPEndPoint(IPAddress.Any, ownPort);
        remoteIP = new IPEndPoint(IPAddress.Parse(IP), destPort);

        UDPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        

        readyToSend = isClient;

        thread = new Thread(Listen);
        thread.Start();

    }


    void Update()
    {
        //lock (lockObject)
        //{
        //    if (readyToSend)
        //    {
        //        resting.Start();

        //        SendMessage();

        //    }
        //    else
        //    {
        //        active.Start();
        //    }
        //}
        
    }


    void Listen()
    {
        Debug.Log("Starting Listening Thread");
            data = new byte[1024];

        if(isClient)
            SendMessage();

        while (true)
        {
            recievedData = UDPSocket.ReceiveFrom(data, ref remoteIP);

            if (recievedData > 0)
            {
                recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);
                Debug.Log("Recieved:" + recievedMessage);

                Thread.Sleep(sleepSeconds);

                SendMessage();

            }
            
            
        }

        //thread.Abort();
    }

    void SendMessage()
    {
        try
        {
            data = Encoding.ASCII.GetBytes(message);

            UDPSocket.SendTo(data, remoteIP);

            data = new byte[1024];

            //readyToSend = false;
        }
        catch
        {
            Debug.Log("Failed to send message");
        }
    }
}
