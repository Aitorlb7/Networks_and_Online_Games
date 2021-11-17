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
    public Text             textName;
    public int              destPort;
    public Text             chatText;
    public Socket           socket = null;
    public EndPoint         remoteIp;

    private string          IP                  = "127.0.0.1";                                              // --- Connection Variables
    
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
        name = textName.text;
    }

    void Update()
    {

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
                AddTextToConsole("Disconnecting");
                ClearClientConsole();

                socket.Close();
                socket = null;

                return;
            }

            receivedMessage = Encoding.ASCII.GetString(data, 0, receivedData);

            ProcessMessage(receivedMessage);

        }
    }

    public void SendMessage(string message)
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

    private void ProcessMessage(string message)
    {
        //Check for server actions
        if (message[0] == '[')                                                                  // This is for format [TAG:value]
        {
            string tag = string.Empty;
            string value = string.Empty;
            int i = 0;
            while (message[i] != ']' && i < message.Length)
            {
                // Get Server Message Tag
                while (message[i] != ':' && i < message.Length)
                {
                    tag += message[i];
                    message.Remove(i);
                    ++i;
                }

                i = 0;
                while (message[i] != ']' && i < message.Length)
                {
                    value += message[i];
                    message.Remove(i);
                    ++i;
                }
            }

            switch (tag)
            {
                //case "COLOR":   { ChangeClientColor(value); }    break;
                case "NAME":    { name = value; textName.text = value; AddTextToConsole("Name changed to: " + value); }      break;
            }
        }
        else
        {
            AddTextToConsole( message);
        }

        Debug.Log( message);
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

