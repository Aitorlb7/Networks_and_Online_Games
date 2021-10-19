using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UDP_Server : MonoBehaviour
{
    private Thread newThread = null;


    private Socket serverSocket;
    private IPEndPoint ip;
    private EndPoint remoteIP;

    private byte[] data = new byte[1024];

    private int recievedData;
    private string recievedMessage;

    public int ownPort;
    public int destPort;
    public string message;
    public int sleepSeconds;

    void Start()
    {
        ip = new IPEndPoint(IPAddress.Any, ownPort);
        remoteIP = new IPEndPoint(IPAddress.Any, destPort);

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverSocket.Bind(ip);

        newThread = new Thread(Listen);
        newThread.Start();
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void Listen()
    {
        Debug.Log("Starting Server Thread");
        data = new byte[1024];

        //recievedData = serverSocket.ReceiveFrom(data, ref remoteIP);


        while (true)
        {
            recievedData = serverSocket.ReceiveFrom(data, ref remoteIP);
            Debug.Log("Server Recieved:" + recievedData);

            if (recievedData > 0)
            {
                recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);
                Debug.Log("Server Recieved:" + recievedMessage);

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

            serverSocket.SendTo(data, remoteIP);

            data = new byte[1024];

            //readyToSend = false;
        }
        catch
        {
            Debug.Log("Server: Failed to send message");
        }
    }

    public void DisconnectAndDestroy()
    {
        //Socket.CancelConnectAsync();
        newThread.Abort();
    }
}
