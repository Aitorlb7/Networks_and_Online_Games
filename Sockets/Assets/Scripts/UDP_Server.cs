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

    static readonly object lockObject = new object();
    void Start()
    {
        waitingThread = new Thread(RecieveData);
        waitingThread.Start();


        newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        ip = new IPEndPoint(IPAddress.Parse(IP), port);
        newSocket.Bind(ip);

       
    }

    // Update is called once per frame
    void Update()
    {

        if (recievedPing)
        {
            lock (lockObject)
            {
                data = Encoding.ASCII.GetBytes(pongMessage);

                //RemoteIP is Wrong
                newSocket.SendTo(Encoding.ASCII.GetBytes(pongMessage), remoteIP);
                
                recievedPing = false;
                data = new byte[1024];
            }
            
            
        }

    }

    void RecieveData()
    {
        Debug.Log("Starting Server Thread");

        while(true)
        {
             remoteIP = new IPEndPoint(IPAddress.Any, port);
             recievedData = newSocket.ReceiveFrom(data, ref remoteIP);

            lock (lockObject)
            {
                
                recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

                Debug.Log(recievedMessage);

                if (recievedMessage == "Ping")
                {
                    Debug.Log(recievedMessage);
                    recievedPing = true;
                }
            }

        }

        

        //Stop thread
        //waitingThread.Abort();
    }

}
