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
    //Client generator
    //Menu for the client generator
    //Handle recived messages
        //-> Recieve color
        //-> Recieve response for a specific command

    //Reestructure client informartion
    
    
    public Color color;
    public string name;
    public Socket socket = null;
    public EndPoint remoteIp;

    private string          IP = "127.0.0.1";

    private byte[]          data = new byte[1024];

    private int             recievedData = 0;
    private string          recievedMessage;

    private bool            breakAndDisconect;

    public int              destPort;
    //public Text             inputText;
    public Text             chatText;

    private List<string>    chatMessages = new List<string>();
    private bool            updateChat;
    private bool            clearChat;

    void Start()
    {
        data = new byte[1024];

        remoteIp = new IPEndPoint(IPAddress.Parse(IP), destPort);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                chatText.text += message + '\n';

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
            recievedData = socket.ReceiveFrom(data, ref remoteIp);


            if (recievedData == 0)
            {
                AddTextToConsole("Client disconnected from server");

                ClearClientConsole();

                socket.Close();

                socket = null;

                return;
            }

            recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

            recievedMessage.Trim('\0'); //Trim all zeros from the string and save space

            AddTextToConsole("Recieved: " + recievedMessage);

            Debug.Log("Recieved: " + recievedMessage);
        }

        
 

    }
    //private void Listen()
    //{
    //    while (true)
    //    {
    //        recievedData = socket.ReceiveFrom(data, ref remoteIp);

    //        if (breakAndDisconect)
    //        {
    //            break;
    //        }

    //        recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

    //        recievedMessage.Trim('\0'); //Trim all zeros from the string and save space

    //        AddTextToConsole("Recieved: " + recievedMessage);

    //        //SendMessage();
    //    }

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

    //public void CreateNewClient()
    //{
    //    GameObject.Instantiate(GameObject.Find("TCP_Client"));
    //}

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
