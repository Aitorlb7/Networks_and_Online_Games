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
    enum ServerState
    {
        SELECTING,
        ACCEPTING,
        LISTENING,
        NONE
    }

    ServerState state = ServerState.SELECTING;

    Thread serverThread = null;
    
    private Socket serverSocket = null;
    private List<Socket> clientList = new List<Socket>();
    private List<Socket> acceptedList = new List<Socket>();

    //ArrayList clientList = new ArrayList();
    //ArrayList acceptedList = new ArrayList();

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
            (clientList[i]).Bind(new IPEndPoint(IPAddress.Any, 11000 + i));

            (clientList[i]).Listen(5);
        }


        //serverThread = new Thread(Listen);
        //serverThread.Start();
    }

    
    void Update()
    {
        switch(state)
        {
            case ServerState.SELECTING:
                try
                {
                    Socket.Select(clientList, null, null, 5000);

                    //Future Poll implmentation

                    //for (int i = 0; i < clientList.Count; i++)
                    //{
                    //    if(clientList(i).Poll(500, SelectMode.SelectRead))
                    //    {
                    //        clientList(i).
                    //    }

                    //}


                    state = ServerState.ACCEPTING;
                }
                catch (SocketException e)
                {
                    Debug.Log("Unable to connect to server.");
                    Debug.Log(e.ToString());
                }

                break;

            case ServerState.ACCEPTING:

                if (clientList.Count == 0)
                    break;

                for (int i = 0; i < clientList.Count; i++)
                {
                    acceptedList[i] = (clientList[i]).Accept();

                    Debug.Log("Server Connected with " + (acceptedList[i]).RemoteEndPoint);
                }

                state = ServerState.LISTENING;

               break;

            case ServerState.LISTENING:

                for (int i = 0; i < acceptedList.Count; i++)
                {
                    if (acceptedList[i] == null)
                        continue;

                    recievedData = (acceptedList[i]).Receive(data);

                    if (recievedData == 0)
                    {
                        Debug.Log("Client " + (acceptedList[i]).RemoteEndPoint + " Disconnected");

                        (acceptedList[i]).Close();

                        acceptedList[i] = null;

                        continue;
                    }

                    recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

                    recievedMessage.Trim('\0'); //Trim all zeros from the string and save space

                    Debug.Log("Recieved: " + recievedMessage); 
                }

                state = ServerState.SELECTING;

                break;
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
            (clientList[i]).Close();
            clientList[i] = null;
        }

        for (int i = 0; i < acceptedList.Count; i++)
        {
            if (acceptedList[i] == null)
                continue;
            
            (acceptedList[i]).Close();
            acceptedList[i] = null;
        }

    }
}
