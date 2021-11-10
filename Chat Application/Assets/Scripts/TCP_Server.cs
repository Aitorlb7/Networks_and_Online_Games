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
        WELCOME,
        LISTENING,
        NONE
    }

    ServerState state = ServerState.SELECTING;

    private List<Socket> clientList = new List<Socket>();
    private List<Socket> acceptedList = new List<Socket>();

    private int index = 0;

    private byte[] data = new byte[1024];

    private int recievedData;
    private string recievedMessage;

    public int ownPort;
    public string welcomeMessage;

    void Start()
    {
        data = new byte[1024];
        Socket tempSocket = null;
        for (int i = 0; i < 10; i++)
        {
            clientList.Add(tempSocket);
            acceptedList.Add(tempSocket);


            clientList[i] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            (clientList[i]).Bind(new IPEndPoint(IPAddress.Any, ownPort + i));

            (clientList[i]).Listen(2);
        }

    }

    
    void Update()
    {
        switch(state)
        {
            case ServerState.SELECTING:
               
                if(index >= acceptedList.Count)
                {
                    Debug.Log("Server Available Connections Full");
                    break;
                }
                
                for (int i = 0; i < clientList.Count; ++i)
                {
                    if (clientList[i].Poll(500, SelectMode.SelectRead))
                    {
                        if(acceptedList[index] == null)
                        {
                            acceptedList[index] = clientList[i].Accept();

                            welcomeMessage = acceptedList[index].RemoteEndPoint + " Joined the Chat";

                            Debug.Log("Server Connected with " + acceptedList[index].RemoteEndPoint);

                            clientList.RemoveAt(i);

                            index++;

                            state = ServerState.WELCOME;
                        }
                    }                   
                }

                if(state != ServerState.WELCOME) 
                    state = ServerState.LISTENING;

                break;

            case ServerState.WELCOME:

                for (int i = 0; i < acceptedList.Count; i++)
                {
                    if (acceptedList[i] != null)
                        SendMessage(welcomeMessage, acceptedList[i]);
                }

                state = ServerState.LISTENING;

                break;

            case ServerState.LISTENING:

                for (int i = 0; i < acceptedList.Count; i++)
                {
                    if (acceptedList[i] == null)
                        continue;

                    recievedData = acceptedList[i].Receive(data);

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


    private void SendMessage(string message, Socket clientSocket)
    {
        try
        {
            data = Encoding.ASCII.GetBytes(message);

           clientSocket.Send(data);

            data = new byte[1024];

            Debug.Log("Sent: " + message + " to " + clientSocket.RemoteEndPoint);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to send message");
            Debug.LogError(e.StackTrace);
        }
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
