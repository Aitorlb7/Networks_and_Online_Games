using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDP_Client : MonoBehaviour
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
    private bool recievedPong = true;
    private float sleepSeconds = 5000;

    static readonly object lockObject = new object();

    void Start()
    {
        waitingThread = new Thread(RecieveData);
        waitingThread.Start();

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        ip = new IPEndPoint(IPAddress.Any, port);
        serverSocket.Bind(ip);


    }

    // Update is called once per frame
    void Update()
    {
        if (recievedPong)
        {
            lock (lockObject)
            {
                data = Encoding.ASCII.GetBytes(pingMessage);
                serverSocket.SendTo(Encoding.ASCII.GetBytes(pingMessage), remoteIP);
                recievedPong = false;
                data = new byte[1024];
            }
        }
    }

    void RecieveData()
    {
        Debug.Log("Starting Client Thread");
        while (true)
        {
            remoteIP = new IPEndPoint(IPAddress.Parse(IP), port);
            recievedData = serverSocket.ReceiveFrom(data, ref remoteIP);
            lock (lockObject)
            {
                recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

                Debug.Log(recievedMessage);

                if (recievedMessage == "Pong")
                {
                    Debug.Log(recievedMessage);
                    recievedPong = true;
                }
            }
        }

            

        

        //Stop thread
        //waitingThread.Abort();
    }
}
