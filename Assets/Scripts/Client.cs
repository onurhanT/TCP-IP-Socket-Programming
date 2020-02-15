// ===============================
// AUTHOR     : Onurhan Turfanda
// CREATE DATE     : 6th Feb, 2020
// PURPOSE     : Understanding server client programming, TCP protochol by creating chat program
//==================================

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Client : MonoBehaviour
{
    public GameObject messageBox;
    public GameObject chatPanel;
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;

    public void ConnectToServer()
    {
        if (socketReady)
        {
            return;
        }
        else
        {
            string host = "127.0.0.1";
            int port = 6321;
            string h;
            int p;
            h = GameObject.Find("HostInput").GetComponent<TMP_InputField>().text;
            if(h != "")
            {
                host = h;
            }
            int.TryParse(GameObject.Find("PortInput").GetComponent<TMP_InputField>().text, out p);
            if(p != 0)
            {
                port = p;
            }
            try
            {
                socket = new TcpClient(host, port);
                stream = socket.GetStream();
                writer = new StreamWriter(stream);
                reader = new StreamReader(stream);
                socketReady = true;
            }
            catch (Exception _e)
            {
                Debug.LogError("Socket Error: " + _e.Message);
            }
        }
    }

    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                {
                    OnIncomingData(data);
                }
            }
        }
    }

    private void OnIncomingData(string data)
    {
        if(data == "%INFO")
        {
            SendChat("%INFO|" + GameObject.Find("NameInput").GetComponent<TMP_InputField>().text);
            return;
        }
        GameObject message = Instantiate(messageBox, chatPanel.transform);
        message.GetComponentInChildren<TextMeshProUGUI>().text = data;
    }

    private void SendChat(String data)
    {
        if (!socketReady)
        {
            return;
        }
        else
        {
            writer.WriteLine(data);
            writer.Flush();
        }
    }

    public void OnSendButton()
    {
        string message = GameObject.Find("MessageInput").GetComponent<TMP_InputField>().text;
        this.SendChat(message);
    }
}
