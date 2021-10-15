using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDP_Server : MonoBehaviour
{
    private Thread waitingThread;

    private Socket newSocket;
    private IPEndPoint ip;
    private EndPoint remoteIP;

    private string IP = "127.0.0.1";
    private int port = 7777;

    private string pongMessage = "Pong";
    private byte[] data = new byte[1024];

    private int recievedData;
    private string recievedMessage;
    private bool recievedPing = false;
    private float sleepSeconds = 5000;
    // Start is called before the first frame update
    void Start()
    {
        newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        ip = new IPEndPoint(IPAddress.Parse(IP), port);
        newSocket.Bind(ip);

        remoteIP = new IPEndPoint(IPAddress.Any, port);
    }

    // Update is called once per frame
    void Update()
    {

        if (recievedPing)
        {
            data = Encoding.ASCII.GetBytes(pongMessage);
            newSocket.SendTo(Encoding.ASCII.GetBytes(pongMessage), remoteIP);
            recievedPing = false;
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
        Debug.Log("Starting Server Thread");

        data = new byte[1024];
        recievedData = newSocket.ReceiveFrom(data, ref remoteIP);
        recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

        Debug.Log(recievedMessage);

        if (recievedMessage == "Pong")
        {
            Debug.Log(recievedMessage);
            recievedPing = true;
        }

        //Stop thread
        waitingThread.Abort();
    }

}
