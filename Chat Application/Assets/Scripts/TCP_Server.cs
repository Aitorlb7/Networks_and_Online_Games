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
        BROADCAST,
        LISTENING,
        NONE
    }
    Thread serverThread;

    ServerState state = ServerState.NONE;

    private List<Socket> acceptedList = new List<Socket>();

    private Socket serverSocket;

    private int index = 0;

    private byte[] data = new byte[1024];

    private int recievedData;
    private string recievedMessage;

    public int ownPort;
    public string connectionMessage;

    void Start()
    {
        data = new byte[1024];
        

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(IPAddress.Any, ownPort));
        serverSocket.Listen(10);

        serverThread = new Thread(AcceptClients);
        serverThread.Start();
    }
    private void AcceptClients()
    {
        while(true)
        {
            Socket tempSocket = serverSocket.Accept();

            acceptedList.Add(tempSocket);

            connectionMessage = tempSocket.RemoteEndPoint + " Joined the Chat";

            Debug.Log("Server Connected with " + tempSocket.RemoteEndPoint);

            state = ServerState.BROADCAST;
        }
    }
    
    void Update()
    {
        switch(state)
        {
              case ServerState.BROADCAST:
               
                for (int i = 0; i < acceptedList.Count; i++)
                {
                    if (acceptedList[i] != null)
                        SendMessage(connectionMessage, acceptedList[i]);
                }

                state = ServerState.LISTENING;

                break;

            case ServerState.LISTENING:

                RecieveMessage();

                break;
        }
        

    }

    private void PollAndAccept()
    {

        if (serverSocket.Poll(500, SelectMode.SelectRead))
        {
            acceptedList.Add(serverSocket.Accept());

            connectionMessage = serverSocket.RemoteEndPoint + " Joined the Chat";

            Debug.Log("Server Connected with " + acceptedList[index].RemoteEndPoint);

            state = ServerState.BROADCAST;
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

    private void RecieveMessage()
    {
        for (int i = 0; i < acceptedList.Count; i++)
        {
            recievedData = acceptedList[i].Receive(data);

            if (recievedData == 0)
            {
                Debug.Log("Client " + (acceptedList[i]).RemoteEndPoint + " Disconnected");

                acceptedList[i].Close();

                acceptedList[i] = null;

                connectionMessage = acceptedList[i].RemoteEndPoint + " Disconnected the Chat";

                state = ServerState.BROADCAST;

                continue;
            }

            recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

            recievedMessage.Trim('\0'); //Trim all zeros from the string and save space

            Debug.Log("Recieved: " + recievedMessage);
            
            ProcessMessage(recievedMessage, acceptedList[i]);

        }
    }

    private void ProcessMessage(string message, Socket senderClient)
    {
        string command = string.Empty;
        string outputMessage = string.Empty;
        int h;
        for (int i = 0; i < message.Length; ++i)
        {
            if(message[i] == '/')
            {
                 h = i;
                while(message[h] != ' ' && h < message.Length)
                {
                    command += message[h];
                    message.Remove(h);
                    h++;
                }

                //Handle Commands
                switch (command)
                {
                    case "/list":
                        for (int j = 0; j < acceptedList.Count; j++)
                        {
                            //Substitute remote end point for Name
                            outputMessage += acceptedList[j].RemoteEndPoint + ",";
                        }
                        SendMessage(outputMessage, senderClient);
                        break;
                    case "/help":
                        message = "Available commands:" + "\n"
                            + "/list -> Show all the users connected" + "\n"
                            + "/kick -> Kick an specific user" + "\n"
                            + "/changeName -> Change your name" + "\n"
                            + "/clear -> Clear your chat history";

                        break;

                    case "/kick":
                        //Kick an specific user
                        break;

                    case "/changeName":
                        //Change the sender Client name
                        break;

                    case "/clear":
                        //clear the client chat messages
                        break;

                    default:
                        //Iterate trough all names and send message to the reciever

                        //SendMessage(message, socket)
                        break;
                }
            }
        }

        
    }

    private void OnDestroy()
    {
        Debug.Log("Disconecting Server");

        serverThread.Abort();

        serverSocket.Close();
        serverSocket = null;

        for (int i = 0; i < acceptedList.Count; i++)
        {
            acceptedList[i].Close();
            acceptedList[i] = null;
        }

    }
}
