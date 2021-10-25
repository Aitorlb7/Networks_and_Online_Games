using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TCP_Server : MonoBehaviour
{
    private Socket serverSocket = null;
    private List<Socket> clientList = new List<Socket>();
    private List<Socket> acceptedList = new List<Socket>();
    private IPEndPoint ip;

    private byte[] data = new byte[1024];

    private int recievedData;
    private string recievedMessage;

    public int ownPort;
    public string welcomeMessage;

    void Start()
    {
        data = new byte[1024];
        ip = new IPEndPoint(IPAddress.Any, ownPort);

        //Initialize TCP Server
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(ip);

        //Max 10 users at the same time
        serverSocket.Listen(10);

        for (int i = 0; i < 10; i++)
        {
            clientList[i] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientList[i].Bind(new IPEndPoint(IPAddress.Any, 11000 + i));
        }

    }

    
    void Update(
    {
        if (!Socket.Select(clientList, null, null))
            return;
        
        for(int i = 0; i < clientList.Count; i++)
        {
            serverSocket = clientList[i].Accept();

            Debug.Log("Server Connected with " + acceptedList[i].RemoteEndPoint);
        }



        recievedData = client.Receive(data);

        if (recievedData == 0)
        {
            AddTextToConsole("Client " + client.RemoteEndPoint + " Disconnected");

            client.Close();
            client = null;
            continue;
        }

        recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

        recievedMessage.Trim('\0'); //Trim all zeros from the string and save space

        AddTextToConsole("Recieved: " + recievedMessage);

        Thread.Sleep(sleepSeconds);

        SendMessage();
    }


    private void SendMessage()
    {
        try
        {
            data = Encoding.ASCII.GetBytes(message);

            client.Send(data);

            data = new byte[1024];

            AddTextToConsole("Sent: " + message);
        }
        catch (Exception e)
        {
            AddTextToConsole("Failed to send message");
            Debug.LogError(e.StackTrace);
        }
    }


    private void OnDestroy()
    {
        Debug.Log("Disconecting Server and aborting Thread");

        if (socket != null)
        {
            socket.Close();
            socket = null;
        }

        if (thread.IsAlive)
        {
            thread.Abort();
            thread = null;
        }
    }
}
