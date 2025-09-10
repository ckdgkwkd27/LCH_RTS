using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class UI_Login : MonoBehaviour
{
    private Button connectButton = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        connectButton = GameObject.Find("ConnectButton").GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnConnectButtonClick()
    {
        if (connectButton != null)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            //IPEndPoint endPoint = new IPEndPoint(ipAddr, 8888); (Game Server)
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 8001);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => new ServerSession());
        }
    }
}
