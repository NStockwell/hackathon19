using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { private set; get; }
    public GameObject mainMenu;

    public GameObject serverMenu;
    public Text ipText;

    public GameObject connectMenu;
    public GameObject hostAddressGameObject;

    public GameObject serverPrefab;
    public GameObject clientPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        ClearNetworkingStuff();

        serverMenu.SetActive(false);
        connectMenu.SetActive(false);
    }

    public void ConnectButton()
    {
        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
    }

    public void HostButton()
    {
        try
        {
            ipText.text = LocalIPAddress();

            Server server = Instantiate(serverPrefab).GetComponent<Server>();
            server.Init();

            Client client = Instantiate(clientPrefab).GetComponent<Client>();
            client.ConnectToServerAsHost(server.port);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        mainMenu.SetActive(false);
        serverMenu.SetActive(true);
    }

    public void ConnectToServer()
    {
        string hostAddress = hostAddressGameObject.GetComponent<InputField>().text;
        if (hostAddress == "")
        {
            hostAddress = "127.0.0.1";
        }

        try
        {
            Client client = Instantiate(clientPrefab).GetComponent<Client>();
            client.ConnectToServer(hostAddress, 6321);
            connectMenu.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void BackButton()
    {
        mainMenu.SetActive(true);
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);
        ClearNetworkingStuff();
    }

    public void ClearNetworkingStuff()
    {
        Server[] servers = FindObjectsOfType<Server>();
        foreach (Server server in servers)
        {
            Destroy(server.gameObject);
        }

        Client[] clients = FindObjectsOfType<Client>();
        foreach (Client client in clients)
        {
            Destroy(client.gameObject);
        }
    }


    public string LocalIPAddress()
     {
         IPHostEntry host;
         string localIP = "";
         host = Dns.GetHostEntry(Dns.GetHostName());
         foreach (IPAddress ip in host.AddressList)
         {
             if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
             {

                 localIP = ip.ToString();
                 break;
             }
         }
         return localIP;
     }
}
