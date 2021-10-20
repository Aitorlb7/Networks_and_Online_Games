using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Base_UDP : MonoBehaviour
{
    protected Thread thread = null;

    protected Socket socket;
    protected IPEndPoint ip;
    protected EndPoint remoteIP;

    protected byte[] data = new byte[64];

    protected int recievedData;
    protected string recievedMessage;

    protected readonly object lockObject = new object();

    public int ownPort;
    public int destPort;
    public string message;
    public int sleepSeconds;


    public Text consoleText;
    private string consoleString;
    bool updateConsole;

    private void Update()
    {
        if(updateConsole)
        {
            lock (lockObject)
            {
                consoleText.text = consoleString;
                updateConsole = false;

                //Debug.Log("Text so far:" + consoleString);
            }
        }
    }

    protected virtual void Listen()
    {
        while (true)
        {
            recievedData = socket.ReceiveFrom(data, ref remoteIP);

            if (recievedData > 0)
            {
                recievedMessage = Encoding.ASCII.GetString(data, 0, recievedData);
                Debug.Log("Recieved:" + recievedMessage);

                AddTextToConsole(recievedMessage);

                Thread.Sleep(sleepSeconds);

                SendMessage();

            }
        }
    }

    protected virtual void SendMessage()
    {
        try
        {
            data = Encoding.ASCII.GetBytes(message);

            socket.SendTo(data, remoteIP);

            data = new byte[1024];

        }
        catch (Exception e)
        {
            Debug.Log("Failed to send message");
            Debug.LogError(e.StackTrace);
        }
    }

    public virtual void DisconnectAndDestroy()
    {
        socket.Close();
        thread.Abort();
        thread = null;
    }

    public virtual void Reconnect()
    {
    }

    void AddTextToConsole(string textToAdd)
    {
        lock(lockObject)
        {
            consoleString += textToAdd + "\r\n";
            updateConsole = true;
        }
          
        
    }
}
