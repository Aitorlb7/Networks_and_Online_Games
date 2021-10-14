using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UCP_Client : MonoBehaviour
{
    private Thread waitingThread;
    private Thread sendThread;

    private Socket serverSocket;
    private IPEndPoint ip;
    private EndPoint remoteIP;

    private string IP = "127.0.0.1";
    private int port = 7777;

    private string pingMessage = "Ping";
    private byte[] data = new byte[1024];

    private int recievedData;
    private string recievedMessage;
    private bool recievedPong = false;
    private float sleepSeconds = 5000;
    void Start()
    {
        

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        ip = new IPEndPoint(IPAddress.Any, port);
        serverSocket.Bind(ip);

        remoteIP = new IPEndPoint(IPAddress.Parse(IP), port);

    }

    // Update is called once per frame
    void Update()
    {
        if (recievedPong)
        {
            data = Encoding.ASCII.GetBytes(pingMessage);
            serverSocket.SendTo(Encoding.ASCII.GetBytes(pingMessage), remoteIP);
            recievedPong = false;
            data = new byte[1024];
        }
        else
        {
            waitingThread = new Thread(RecieveData);
            waitingThread.Start();

            
        }
    }

    void RecieveData()
    {
        Debug.Log("Starting Thread");

        data = new byte[1024];
        recievedData = serverSocket.ReceiveFrom(data, ref remoteIP);
        recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);
        if (recievedMessage == "Pong")
        {
            Console.WriteLine(recievedMessage);
            recievedPong = true;
        }

        //Stop thread
        waitingThread.Abort();
    }
}
