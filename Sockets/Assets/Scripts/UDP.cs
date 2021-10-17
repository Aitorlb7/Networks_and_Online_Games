using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDP : MonoBehaviour
{
    private Thread active = null;
    private Thread resting = null;

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
    public float sleepSeconds;
    public bool isClient;


    void Start()
    {
        ip = new IPEndPoint(IPAddress.Any, ownPort);
        remoteIP = new IPEndPoint(IPAddress.Parse(IP), destPort);

        UDPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        UDPSocket.Bind(ip);

        if (isClient)
            readyToSend = true;
        else
            readyToSend = false;

    }


    void Update()
    {
        lock (lockObject)
        {
            if (readyToSend)
            {
                resting = new Thread(Rest);
                resting.Start();

                SendMessage();

            }
            else
            {
                active = new Thread(Listen);
                active.Start();
            }
        }


        
    }


    void Listen()
    {
        Debug.Log("Starting Listening Thread");

        data = new byte[1024];
        
        try
        {
            recievedData = UDPSocket.ReceiveFrom(data, ref remoteIP);
            
            recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

            Debug.Log("Recieved:" + recievedMessage);

            readyToSend = true;

        }
        catch
        {
            Debug.Log("Failed to recieve message");

        }

        active = null;
    }

    void Rest()
    {
        Debug.Log("Starting Resting Thread");

        Thread.Sleep((int)sleepSeconds);
        resting = null;
    }

    void SendMessage()
    {
        try
        {
            data = Encoding.ASCII.GetBytes(message);

            UDPSocket.SendTo(data, remoteIP);

            data = new byte[1024];

            readyToSend = false;
        }
        catch
        {
            Debug.Log("Failed to send message");
        }
    }
}
