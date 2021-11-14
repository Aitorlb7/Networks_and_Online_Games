using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TCP_Server : MonoBehaviour
{
    // Use GetRandomColor() to generate a random color. Who would have thought it did that right?
    // client.color = GetRandomColor() or something like that.
    // Changed the switch for the commands. Check it out.
    
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

    private int index = 0;                                                                                          // Never used?

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
        //Returned message should be [NAME: newName];
    }

    private void ClearCommand()
    {
        //Clear the client's chat messages
        //Is it really necessary to have it go to the server to clear local messages?
    }

    private void DefaultCommand()
    {
        //Iterate trough all names and send message to the reciever
        //Or just do nothing and send a Log to check why it reached this place.

        //SendMessage(message, socket)
    }

    private Color GetRandomColor()
    {
        return new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
    }

    private Color GetRandomBaseColor()
    {
        int i = Random.Range(0, 8);
        Color newColor = new Color();

        switch (i)
        {
            case 0: { newColor = Color.black; }     break;
            case 1: { newColor = Color.blue; }      break;
            case 2: { newColor = Color.cyan; }      break;
            case 3: { newColor = Color.green; }     break;
            case 4: { newColor = Color.grey; }      break;
            case 5: { newColor = Color.magenta; }   break;
            case 6: { newColor = Color.red; }       break;
            case 7: { newColor = Color.white; }     break;
            case 8: { newColor = Color.yellow; }    break;
        }

        return newColor;
    }

    private string GetRandomBaseColorAsString()
    {
        int i = Random.Range(0, 8);
        string colorName = "black";

        switch (i)
        {
            case 0: { colorName = "black"; }     break;
            case 1: { colorName = "blue"; }      break;
            case 2: { colorName = "cyan"; }      break;
            case 3: { colorName = "green"; }     break;
            case 4: { colorName = "grey"; }      break;
            case 5: { colorName = "magenta"; }   break;
            case 6: { colorName = "red";}        break;
            case 7: { colorName = "yellow"; }    break;
            case 8: { colorName = "white"; }     break;
        }

        return colorName;
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