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
    
    Thread acceptThread;
    Thread listenThread;

    private readonly object stateLock = new object();
    ServerState state = ServerState.LISTENING;

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

        acceptThread = new Thread(AcceptClients);
        listenThread = new Thread(Listen);
        acceptThread.Start();
    }
    private void AcceptClients()
    {
        while(true)
        {
            Socket tempSocket = serverSocket.Accept();

            acceptedList.Add(tempSocket);

            connectionMessage = tempSocket.RemoteEndPoint + " Joined the Chat";

            Debug.Log("Server Connected with " + tempSocket.RemoteEndPoint);

            lock (stateLock) 
                state = ServerState.BROADCAST;
        }
    }
    private void Listen()
    {
        while(true)
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

                    lock (stateLock)
                        state = ServerState.BROADCAST;

                    continue;
                }

                recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

                recievedMessage.Trim('\0'); //Trim all zeros from the string and save space

                Debug.Log("Recieved: " + recievedMessage);

                ProcessMessage(recievedMessage, acceptedList[i]);

            }
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

                listenThread.Start();

                state = ServerState.NONE;

                break;

            case ServerState.NONE:

                return;
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
                    case "/list":       { ListCommand(senderClient); }  break;
                    case "/help":       { HelpCommand(senderClient); }  break;
                    case "/kick":       { KickCommand(); }              break;
                    case "/changename": { ChangeNameCommand(); }        break;
                    case "/clear":      { ClearCommand(); }             break;
                    default:            { DefaultCommand(); }           break;
                }
            }
        }

        // Maybe commands should only be allowed at the beginning of the string.
        /*string command = string.Empty;
        if (message[0] == '/')
        {
            int h = 0;
            while((message[h] != ' ' || message[h] != '\0') && h < message.Length)
            {
                command += message[h];
                message.Remove(h); // ???
                h++;
            }

            switch (command)
            {
                case "/list":       { ListCommand(); }          break;
                case "/help":       { HelpCommand(); }          break;
                case "/kick":       { KickCommand(); }          break;
                case "/changename": { ChangeNameCommand(); }    break;
                case "/clear":      { ClearCommand(); }         break;
                default:            { DefaultCommand(); }       break;
            }
        }*/
    }

    private void ListCommand(Socket senderClient)
    {
        string listMessage = string.Empty;
        
        for (int j = 0; j < acceptedList.Count; j++)
        {
            //Substitute remote end point for Name
            listMessage += acceptedList[j].RemoteEndPoint + ", ";
        }

        SendMessage(listMessage, senderClient);
    }

    private void HelpCommand(Socket senderClient)
    {
        string helpMessage = "Available commands:\n"
                            + "/list -> Show all the users connected\n"
                            + "/kick -> Kick an specific user\n"
                            + "/changeName -> Change your name\n"
                            + "/clear -> Clear your chat history\n";

        SendMessage(helpMessage, senderClient);
    }

    private void KickCommand()
    {
        //Kick an specific user
        //Check all connected clients and look for a name match.
        //Terminate the connection with that client.

        //string kickMessage = "You have been kicked from the server!";
        //SendMessage(kickMessage, kickedClient);
        //kickedClient.Close();
    }

    private void ChangeNameCommand()
    {
        //Change the client's name.
    }

    private void ClearCommand()
    {
        //Clear the client's chat messages
    }

    private void DefaultCommand()
    {
        //Iterate trough all names and send message to the reciever

        //SendMessage(message, socket)
    }

    private void OnDestroy()
    {
        Debug.Log("Disconecting Server");

        acceptThread.Abort();

        serverSocket.Close();
        serverSocket = null;

        for (int i = 0; i < acceptedList.Count; i++)
        {
            acceptedList[i].Close();
            acceptedList[i] = null;
        }

    }
}

//private void PollAndAccept()
//{
//    if (serverSocket.Poll(500, SelectMode.SelectRead))
//    {
//        acceptedList.Add(serverSocket.Accept());
//        connectionMessage = serverSocket.RemoteEndPoint + " Joined the Chat";
//        state = ServerState.BROADCAST;
//        
//        Debug.Log("Server Connected with " + acceptedList[index].RemoteEndPoint);
//    }
//}

//private void RecieveMessage()
//{
//    for (int i = 0; i < acceptedList.Count; i++)
//    {
//        recievedData = acceptedList[i].Receive(data);
//        if (recievedData == 0)
//        {
//            Debug.Log("Client " + (acceptedList[i]).RemoteEndPoint + " Disconnected");
//            
//            acceptedList[i].Close();
//            acceptedList[i] = null;
//            connectionMessage = acceptedList[i].RemoteEndPoint + " Disconnected the Chat";
//            state = ServerState.BROADCAST;
//
//            continue;
//        }
//
//        recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);
//        recievedMessage.Trim('\0'); //Trim all zeros from the string and save space
//        Debug.Log("Recieved: " + recievedMessage);
//        
//        ProcessMessage(recievedMessage, acceptedList[i]);
//    }
//}