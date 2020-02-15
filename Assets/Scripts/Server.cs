// ===============================
// AUTHOR     : Onurhan Turfanda
// CREATE DATE     : 6th Feb, 2020
// PURPOSE     : Understanding server client programming, TCP protochol by creating chat program
//==================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class Server : MonoBehaviour
{
    public int port;

    private List<ServerClient> clients;
    private List<ServerClient> disconnected;
    private TcpListener server;
    private bool serverStarted;

    private void Start()
    {
        clients = new List<ServerClient>();
        disconnected = new List<ServerClient>(); 
    }

    private void Update()
    {
        if (!serverStarted)
        {
            return;
        }
        else
        {
            foreach(ServerClient _client in this.clients)
            {
                if (!IsConnected(_client.tcp))
                {
                    _client.tcp.Close();
                    disconnected.Add(_client);
                    continue;
                }
                else
                {
                    NetworkStream stream = _client.tcp.GetStream();
                    if (stream.DataAvailable)
                    {
                        System.IO.StreamReader reader = new System.IO.StreamReader(stream, true);
                        string data = reader.ReadLine();
                        if(data != null)
                        {
                            OnComingData(_client, data);
                        }
                    }
                }
            }
            foreach(ServerClient _clients in disconnected)
            {
                Broadcast(_clients.clientName + " disconnected", this.clients);
                clients.Remove(_clients);
                disconnected.Remove(_clients);
            }
        }
    }
    
    public void PublishServer()
    {
        try
        {
            int p;
            int.TryParse(GameObject.Find("PortInput").GetComponent<TMP_InputField>().text, out p);
            if (p != 0)
            {
                port = p;
            }
            this.server = new TcpListener(System.Net.IPAddress.Any, this.port);
            this.server.Start();
            StartListening();
            this.serverStarted = true;
            Debug.Log("Server has been started on port " + this.port);
        }
        catch (Exception _e)
        {
            Debug.Log("Socket error: " + _e.Message);
        }
    }

    private void Broadcast(string _data, List<ServerClient> _clients)
    {
        foreach(ServerClient client in _clients)
        {
            try
            {
                StreamWriter writer = new StreamWriter(client.tcp.GetStream());
                writer.WriteLine(_data);
                writer.Flush();

            }
            catch (Exception _e)
            {
                Debug.Log("Write Error: " + _e.Message + " to Client " + client.clientName);
            }
        }
    }

    private void OnComingData(ServerClient _client, string _data)
    {

        if(_data.Contains("%INFO"))
        {
            _client.clientName = _data.Split('|')[1];
            Broadcast(_client.clientName + " has connected", this.clients);
            return;
        }
        Broadcast(_client.clientName + ": " + _data, this.clients);
    }

    private bool IsConnected(TcpClient _tcp)
    {
        try
        {
            if(_tcp != null && _tcp.Client != null && _tcp.Client.Connected)
            {
                if(_tcp.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(_tcp.Client.Receive(new byte[1], SocketFlags.Peek) == 0);                
                }
                return true;
            }
            return false;
        }
        catch (Exception _e)
        {
            Debug.LogError("Exception occured " + _e.Message);
            return false;
        }
    }

    private void StartListening()
    {
        this.server.BeginAcceptTcpClient(AcceptTcpClient, this.server);
    }

    private void AcceptTcpClient(IAsyncResult _ar)
    {
        TcpListener listener = (TcpListener)_ar.AsyncState;
        this.clients.Add(new ServerClient(listener.EndAcceptTcpClient(_ar)));
        StartListening();
        Broadcast("%INFO", new List<ServerClient>(){ this.clients[this.clients.Count - 1] });
    }
}

public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient _clientSocket)
    {
        this.clientName = "Guest";
        this.tcp = _clientSocket;
    }
}