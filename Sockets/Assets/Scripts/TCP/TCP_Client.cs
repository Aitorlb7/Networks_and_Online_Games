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

    private Thread thread = null;

    private Socket socket = null;
    private EndPoint remoteIP;

    private string IP = "127.0.0.1";

    private byte[] data = new byte[1024];

    private int recievedData;
    private string recievedMessage;

    private readonly object lockConsole = new object();
    private bool breakAndDisconect;


    public int destPort;
    public string message;
    public int sleepSeconds;
    public Text consoleText;

    private List<string> consoleStrings = new List<string>();
    private bool updateConsole;
    private bool clearConsole;
    private int pongNum = 0;
    void Start()
    {
        data = new byte[1024];

        remoteIP = new IPEndPoint(IPAddress.Parse(IP), destPort);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        thread = new Thread(Listen);

        thread.Start();
        //ConnectToServer();

    }
    void Update()
    {
        if (updateConsole)
        {
            lock (lockConsole)
            {
                if (clearConsole)
                {
                    consoleText.text = "";
                    clearConsole = false;
                }

                foreach (string message in consoleStrings)
                {
                    consoleText.text += message + '\n';

                    consoleStrings.Remove(message);

                }


                updateConsole = false;
            }
        }
    }
    private void Listen()
    {
        if(socket.Connected) 
            SendMessage();
        else
        {
            ConnectToServer();
            SendMessage();
        }

        while (true)
        {
            recievedData = socket.ReceiveFrom(data, ref remoteIP);

            if ( breakAndDisconect || pongNum >= 5)
                break;


            recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);

            recievedMessage.Trim('\0'); //Trim all zeros from the string and save space

            AddTextToConsole("Recieved: " + recievedMessage);

            pongNum++;

            Thread.Sleep(sleepSeconds);

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
            data = Encoding.ASCII.GetBytes(message);

            socket.SendTo(data, remoteIP);

            data = new byte[1024];

            AddTextToConsole("Sent: " + message);

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
            return;
        }
    }

    public void CreateNewClient()
    {
        GameObject.Instantiate(GameObject.Find("TCP_Client"));
      
    }
 

    private void AddTextToConsole(string textToAdd)
    {
        lock (lockConsole)
        {
            consoleStrings.Add(textToAdd);

            updateConsole = true;
        }

    }

    private void ClearClientConsole()
    {
        consoleStrings.Clear();
        clearConsole = true;
        updateConsole = true;
    }
    public void DisconnectAndDestroy()
    {
        breakAndDisconect = true;
    }

    private void OnDestroy()
    {
        Debug.Log("Disconecting Client and aborting Thread");

        if (socket != null)
        {
            socket.Close();
            socket = null;
        }

        if (thread.IsAlive)
        {
            thread.Abort();
            thread = null;
        }
        
    }
}
