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
    Thread serverThread = null;
    
    private Socket serverSocket = null;
    //private List<Socket> clientList = new List<Socket>();
    //private List<Socket> acceptedList = new List<Socket>();

    ArrayList clientList = new ArrayList();
    ArrayList acceptedList = new ArrayList();

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

        ////Initialize TCP Server
        //serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //serverSocket.Bind(ip);

        ////Max 10 users at the same time
        //serverSocket.Listen(10);



        for (int i = 0; i < 10; i++)
        {
            Socket tempSocket = null;
            clientList.Add(tempSocket);
            acceptedList.Add(tempSocket);


            clientList[i] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ((Socket)clientList[i]).Bind(new IPEndPoint(IPAddress.Any, 11000 + i));

            ((Socket)clientList[i]).Listen(5);
        }


        serverThread = new Thread(Listen);
        serverThread.Start();
    }

    
    void Update()
    {

        try
        {
            Socket.Select(clientList, null, null, 5000);
        }
        catch (SocketException e)
        {
            Debug.Log("Unable to connect to server.");
            Debug.Log(e.ToString());
        }

    }

    private void Listen()
    {
        while(true)
        {

            if (clientList.Count != 1)
                continue;

            for (int i = 0; i < clientList.Count; i++)
            {
                //if (((Socket)clientList[i]).RemoteEndPoint == null)
                //    continue;

                acceptedList[i] = ((Socket)clientList[i]).Accept();

                Debug.Log("Server Connected with " + ((Socket)acceptedList[i]).RemoteEndPoint);
            }

            for (int i = 0; i < acceptedList.Count; i++)
            {
                if (acceptedList[i] == null)
                    continue;

                SocketError error;
                recievedData = ((Socket)acceptedList[i]).Receive(data, 0, 1024, SocketFlags.None, out error);
                Debug.Log(error.ToString());

                if (recievedData == 0)
                {
                    Debug.Log("Client " + ((Socket)acceptedList[i]).RemoteEndPoint + " Disconnected");

                    ((Socket)acceptedList[i]).Close();
                    acceptedList[i] = null;
                    continue;
                }

                recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

                recievedMessage.Trim('\0'); //Trim all zeros from the string and save space

                Debug.Log("Recieved: " + recievedMessage);
            }
        }
    }

    private void SendMessage()
    {
        //try
        //{
        //    data = Encoding.ASCII.GetBytes(message);

        //    client.Send(data);

        //    data = new byte[1024];

        //    AddTextToConsole("Sent: " + message);
        //}
        //catch (Exception e)
        //{
        //    AddTextToConsole("Failed to send message");
        //    Debug.LogError(e.StackTrace);
        //}
    }


    private void OnDestroy()
    {
        Debug.Log("Disconecting Server");

        for (int i = 0; i < clientList.Count; i++)
        {
            ((Socket)clientList[i]).Close();
            clientList[i] = null;
        }

        for (int i = 0; i < acceptedList.Count; i++)
        {
            if (acceptedList[i] == null)
                continue;
            
            ((Socket)acceptedList[i]).Close();
            acceptedList[i] = null;
        }

    }
}
