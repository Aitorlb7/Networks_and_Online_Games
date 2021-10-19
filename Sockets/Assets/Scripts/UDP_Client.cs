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
        ip = new IPEndPoint(IPAddress.Any, ownPort);
        remoteIP = new IPEndPoint(IPAddress.Parse(IP), destPort);

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
        Debug.Log("Starting Client Thread");
        data = new byte[1024];

        data = Encoding.ASCII.GetBytes(message);

        UDPSocket.SendTo(data, remoteIP);

        while (true)
        {
            recievedData = UDPSocket.ReceiveFrom(data, ref remoteIP);

            if (recievedData > 0)
            {
                recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);
                Debug.Log("Client Recieved:" + recievedMessage);

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

            Debug.Log("Client sent message to: " + remoteIP.ToString());

            //readyToSend = false;
        }
        catch
        {
            Debug.Log("Client: Failed to send message");

            Debug.Log("Client End Point: " + UDPSocket.RemoteEndPoint.ToString());

            Debug.Log("Client End Point: " + UDPSocket.LocalEndPoint.ToString());

            //SendMessage();
        }
    }

    public void DisconnectAndDestroy()
    {
        //Socket.CancelConnectAsync();
        //thread.Abort();
        UDPSocket.Close();
        thread.Interrupt();
        thread = null;
    }
}
