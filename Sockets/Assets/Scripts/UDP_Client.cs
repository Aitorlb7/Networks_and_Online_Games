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


    private Socket clientSocket;
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

        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientSocket.Bind(ip);

        thread = new Thread(Listen);
        thread.Start();

        data = new byte[1024];
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Listen()
    {
        Debug.Log("Starting Client Thread");
        

        SendMessage();

        while (true)
        {
            recievedData = clientSocket.ReceiveFrom(data, ref remoteIP);

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

            clientSocket.SendTo(data, remoteIP);

            data = new byte[1024];

            //Debug.Log("Client sent message to: " + remoteIP.ToString());

            //readyToSend = false;
        }
        catch
        {
            Debug.Log("Client: Failed to send message");

            Debug.Log("Client End Point: " + clientSocket.RemoteEndPoint.ToString());

            Debug.Log("Client End Point: " + clientSocket.LocalEndPoint.ToString());

            //SendMessage();
        }
    }

    public void DisconnectAndDestroy()
    {

        clientSocket.Close();
        thread.Abort();
        thread = null;
    }

    void Reconnect()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientSocket.Bind(ip);

        thread = new Thread(Listen);
        thread.Start();
    }
   
}
