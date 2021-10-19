using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UDP_Client : MonoBehaviour
{
    private Thread thread = null;


    private Socket UDPSocket;
    private IPEndPoint ip;
    private EndPoint remoteIP;

    private string IP = "127.0.0.1";
    private byte[] data = new byte[1024];

    private int recievedData;
    private string recievedMessage;

    public int ownPort;
    public int destPort;
    public string message;
    public int sleepSeconds;
    void Start()
    {
        ip = new IPEndPoint(IPAddress.Parse(IP), ownPort);
        remoteIP = new IPEndPoint(IPAddress.Any, destPort);

        UDPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        UDPSocket.Bind(ip);

        thread = new Thread(Listen);
        thread.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Listen()
    {
        Debug.Log("Starting Server Thread");
        data = new byte[1024];

        SendMessage();


        while (true)
        {
            recievedData = UDPSocket.ReceiveFrom(data, ref remoteIP);
            Debug.Log("Recieved:" + recievedData);

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

            SendMessage();
        }
    }

    public void DisconnectAndDestroy()
    {
        //Socket.CancelConnectAsync();
        thread.Abort();
    }
}
