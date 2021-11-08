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
    private Socket          socket = null;
    private EndPoint        remoteIP;

    private string          IP = "127.0.0.1";

    private byte[]          data = new byte[1024];

    private int             recievedData;
    private string          recievedMessage;

    private bool            breakAndDisconect;

    public int              destPort;
    public Text             inputText;
    public Text             chatText;

    private List<string>    chatMessages = new List<string>();
    private bool            updateChat;
    private bool            clearChat;

    void Start()
    {
        data = new byte[1024];

        remoteIP = new IPEndPoint(IPAddress.Parse(IP), destPort);

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

        if (socket.Connected)
        {
            SendMessage();
        }
        else
        {
            ConnectToServer();
            //SendMessage();
        }

    }
    private void Listen()
    {
       

        while (true)
        {
            recievedData = socket.ReceiveFrom(data, ref remoteIP);

            if (breakAndDisconect)
            {
                break;
            }

            recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

            recievedMessage.Trim('\0'); //Trim all zeros from the string and save space

            AddTextToConsole("Recieved: " + recievedMessage);

            SendMessage();
        }

        AddTextToConsole("Client disconnected from server");

        ClearClientConsole();

        socket.Close();

        socket = null;

        breakAndDisconect = false;

    }

    private void SendMessage()
    {
        try
        {
            data = Encoding.ASCII.GetBytes(/*message*/ inputText.text);

            socket.SendTo(data, remoteIP);

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
            socket.Connect(remoteIP);
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
