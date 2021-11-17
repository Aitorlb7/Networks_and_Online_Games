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
    
    public class User
    {
        public User(string _name,Socket _socket,Color _color)
        {
            name = _name;
            socket = _socket;
            color = _color;
        }

        public string name = string.Empty;
        public Socket socket = null;
        public Color color = Color.black;
    }

    enum ServerState
    {
        BROADCAST,
        LISTENING,
        SEND,
        NONE
    }
    
    Thread acceptThread;
    Thread listenThread;

    private readonly object stateLock = new object();
    ServerState state = ServerState.LISTENING;

    private List<User> acceptedList = new List<User>();
    private Socket serverSocket;

    private byte[] data = new byte[1024];
    private int recievedData;
    private string recievedMessage;

    public int ownPort;
    public string broadcastMessage;

    //Variables to send message from the main thread
    private User destinationUser;
    private string messageToSend;

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
            User tempUser = new User(null, null, Color.blue);

            tempUser.socket = serverSocket.Accept();

            //recievedData = tempUser.socket.Receive(data);

            //recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

            tempUser.name = "A";

            acceptedList.Add(tempUser);

            broadcastMessage = recievedMessage + " Joined the Chat";

            Debug.Log("Server Connected with " + recievedMessage);

            data = new byte[1024];

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
                if (acceptedList[i] != null && acceptedList[i].socket.Poll(500, SelectMode.SelectRead))
                    recievedData = acceptedList[i].socket.Receive(data);
                else
                    continue;

                if (recievedData == 0)
                {
                    Debug.Log("Client " + acceptedList[i].name + " Disconnected");

                    acceptedList[i].socket.Close();

                    acceptedList[i] = null;

                    broadcastMessage = acceptedList[i].name + " Disconnected the Chat";

                    lock (stateLock)
                        state = ServerState.BROADCAST;

                    continue;
                }

                recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

                Debug.Log("Recieved: " + recievedMessage);

                data = new byte[1024];

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
                        SendMessage(broadcastMessage, acceptedList[i]);
                }

                state = ServerState.NONE;

                break;

            case ServerState.LISTENING:

                listenThread.Start();

                state = ServerState.NONE;

                break;

            case ServerState.SEND:

                SendMessage(messageToSend, destinationUser);

                state = ServerState.NONE;

                break;

            case ServerState.NONE:

                return;
        }
        

    }

    private void SendMessage(string message, User user)
    {
        try
        {
            data = Encoding.ASCII.GetBytes(message);

            user.socket.Send(data);

            data = new byte[1024];

            Debug.Log("Sent: " + message + " to " + user.name);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to send message");
            Debug.LogError(e.StackTrace);
        }
    }

    private void ProcessMessage(string message, User senderUser)
    {
        string command = string.Empty;
        string withoutCommand = string.Empty;
        if (message[0] == '/')
        {
            int h = 0;
            while (message[h] != ' ')
            {
                command += message[h];
                
                if (h >= message.Length - 1)
                    break;
                else
                    h++;
            }

            if (message[h] == ' ')
            {
                h++;
                while(h < message.Length)
                {
                    withoutCommand += message[h];
                    h++;
                }
            }

            switch (command)
            {
                case "/list": { ListCommand(senderUser); } break;
                case "/help": { HelpCommand(senderUser); } break;
                case "/kick": { KickCommand(senderUser, withoutCommand); } break;
                case "/changename": { ChangeNameCommand(senderUser, withoutCommand); } break;
                case "/whisper": { WhisperCommand(senderUser, withoutCommand); } break;
                case "/clear": { ClearCommand(); } break;
                default: { DefaultCommand(); } break;
            }
        }
        else
        {
            broadcastMessage = senderUser.name + ": " + message;
            state = ServerState.BROADCAST;
        }
    }

    private void ListCommand(User senderUser)
    {
        string listMessage = string.Empty;
        
        for (int j = 0; j < acceptedList.Count; j++)
        {
            //Substitute remote end point for Name
            listMessage += acceptedList[j].name + ", ";
        }

        SendMessage(listMessage, senderUser);

        Debug.Log(listMessage);
    }

    private void HelpCommand(User senderUser)
    {
        string helpMessage = "Available commands:\n"
                            + "/list -> Show all the users connected\n"
                            + "/kick -> Kick an specific user\n"
                            + "/changeName -> Change your name\n"
                            + "/clear -> Clear your chat history\n";

        SendMessage(helpMessage, senderUser);

        Debug.Log(helpMessage);
    }

    private void KickCommand(User user, string name)
    {
        //Kick an specific user
        //Check all connected clients and look for a name match.
        //Terminate the connection with that client.

        for(int i = 0; i < acceptedList.Count; ++i)
        {
            if(acceptedList[i].name == name)
            {
                acceptedList[i].socket.Close();
                acceptedList[i].socket = null;
                acceptedList.RemoveAt(i);
            }
        }
    }

    private void ChangeNameCommand(User user, string newName)
    {
        //Change the client's name.
        //Returned message should be [NAME: newName];

        user.name = newName;
        string changeNameMessage = "[NAME: " + user.name + "]";

        destinationUser = user;
        messageToSend = changeNameMessage;

        state = ServerState.SEND;

    }

    private void WhisperCommand(User user, string nameAndMessage)
    {
        string name = string.Empty;
        string message = string.Empty;
        int h = 0;

        //Get name of the destination user
        while (message[h] != ' ')
        {
            name += nameAndMessage[h];

            if (h >= nameAndMessage.Length - 1)
                break;
            else
                h++;
        }
        //Get the message to the user
        if (nameAndMessage[h] == ' ')
        {
            h++;
            while (h < nameAndMessage.Length)
            {
                message += nameAndMessage[h];
                h++;
            }
        }
        //Get the destination user comparing his name
        for (int i = 0; i < acceptedList.Count; ++i)
        {
            if (acceptedList[i].name == name)
            {
                destinationUser = acceptedList[i];
                messageToSend = message;
                state = ServerState.SEND;
            }
        }


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
        listenThread.Abort();

        serverSocket.Close();
        serverSocket = null;

        for (int i = 0; i < acceptedList.Count; i++)
        {
            acceptedList[i].socket.Close();
            acceptedList[i].socket = null;
            acceptedList.RemoveAt(i);
        }

       

    }
}