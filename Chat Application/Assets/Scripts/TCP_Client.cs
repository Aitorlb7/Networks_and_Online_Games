using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TCP_Client : MonoBehaviour
{
    //Client generator                                      --> Done. Revise that the generated parameters are correct.

    //Menu for the client generator                         --> Done. Requires testing.

    //Handle recived messages                               --> Filter all incoming messages and check for server actions, etc.
        //-> Recieve color                                  --> Have a tag for server messages ([COLOR: 0.5f, 0.5f, 0.5f])
        //-> Recieve response for a specific command        --> The only "tricky" thing is to make sure only the requesting client receives the message.

    //Re-structure client information                       --> Is it really needed tho? For a 5% delivery?
    
    public Color            color;
    public string           name;
    public int              destPort;
    public Text             chatText;
    public Socket           socket = null;
    public EndPoint         remoteIp;

    private string          IP                  = "127.0.0.1";                                              // --- Connection Variables
    private bool            breakAndDisconect   = false;                                                    // ------------------------
    
    private byte[]          data                = new byte[1024];                                           // --------------
    private int             receivedData        = 0;                                                        // Data Variables
    private string          receivedMessage     = "";                                                       // --------------
    
    private List<string>    chatMessages        = new List<string>();                                       // --------------
    private bool            updateChat          = false;                                                    // Chat Variables
    private bool            clearChat           = false;                                                    //---------------

    void Start()
    {
        data        = new byte[1024];
        remoteIp    = new IPEndPoint(IPAddress.Parse(IP), destPort);
        socket      = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    void Update()
    {
        //StablishConnectionWithServer();
        //UpdateChat();
        //CheckForNewMessages();

        if (updateChat)
        {
            if (clearChat)
            {
                chatText.text = "";
                clearChat = false;
            }

            foreach (string message in chatMessages)
            {
                chatText.text += message + '\n';
            }

            chatMessages.Clear();
            updateChat = false;
        }

        if (!socket.Connected)
        {
            ConnectToServer();
            SendMessage(name);

            return;
        }

        if(socket.Poll(500, SelectMode.SelectRead))
        {
            receivedData = socket.ReceiveFrom(data, ref remoteIp);

            if (receivedData == 0)
            {
                AddTextToConsole("Client disconnected from server");
                ClearClientConsole();

                socket.Close();
                socket = null;

                return;
            }

            receivedMessage = Encoding.ASCII.GetString(data, 0, receivedData);
            receivedMessage.Trim('\0');                                                                     //Trim all zeros from the string and save space

            //CheckServerActions(receivedMessage);
            
            //Check for server actions
            if (receivedMessage[0] == '[')                                                                  // This is for format [TAG:value]
            {
                /*string tag = string.Empty;
                string value = string.Empty;
                int i = 0;
                while(receivedMessage[i] != ']' && i < receivedMessage.Length)
                {
                    // Get Server Message Tag
                    while(receivedMessage[i] != ':' && i < receivedMessage.Length)
                    {
                        tag += receivedMessage[i];
                        receivedMessage.Remove(i);
                        ++i;
                    }

                    i = 0;
                    while(receivedMessage[i] != ']' && i < receivedMessage.Length)
                    {
                        value += receivedMessage[i];
                        receivedMessage.Remove(i);
                        ++i;
                    }
                }

                switch(tag)
                {
                    //case "COLOR":   { ChangeClientColor(value); }    break;
                    //case "NAME":    { ChangeClientName(value);}      break;
                }*/
            }
            else
            {
                AddTextToConsole("Recieved: " + receivedMessage);
            }

            Debug.Log("Recieved: " + receivedMessage);

        }
    }

    private void SendMessage(string message)
    {
        try
        {
            data = Encoding.ASCII.GetBytes(message);
            socket.SendTo(data, remoteIp);

            data = new byte[1024];

            //AddTextToConsole("Sent: " + message);
        }
        catch (Exception e)
        {
            AddTextToConsole("Failed to send message");
            Debug.LogError(e.StackTrace);
        }
    }

    public void ConnectToServer()
    {
        try
        {
            socket.Connect(remoteIp);
            AddTextToConsole("Connected with Server!");
        }
        catch (SocketException e)
        {
            AddTextToConsole("Unable to connect to server.");
            AddTextToConsole(e.ToString());
        }
    }

    private void AddTextToConsole(string textToAdd)
    {
        chatMessages.Add(textToAdd);
        updateChat = true;
    }

    private void ClearClientConsole()
    {
        chatMessages.Clear();
        clearChat = true;
        updateChat = true;
    }

    public void DisconnectAndDestroy()
    {
        breakAndDisconect = true;
    }

    private void OnDestroy()
    {
        Debug.Log("Disconecting Client Socket");

        if (socket != null)
        {
            socket.Close();
            socket = null;
        }
    }
}

//private void Listen()
//{
//    while (true)
//    {
//        receivedData = socket.ReceiveFrom(data, ref remoteIp);
//        if (breakAndDisconect)
//        {
//            break;
//        }
//
//        receivedMessage = Encoding.ASCII.GetString(data, 0, receivedData);
//        receivedMessage.Trim('\0'); //Trim all zeros from the string and save space
//        AddTextToConsole("Recieved: " + receivedMessage);
//        //SendMessage();
//    }
//
//    AddTextToConsole("Client disconnected from server");
//    ClearClientConsole();
//    socket.Close();
//    socket = null;
//    breakAndDisconect = false;
//}

//public void RecieveInput(string messageInput)
//{
//    SendMessage(messageInput);
//}