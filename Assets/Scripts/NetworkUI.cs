using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : NetworkBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TextMeshProUGUI playersCountText;
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private int port = 7777;

    private NetworkVariable<int> playersNum = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            HostGame();
        });

        clientButton.onClick.AddListener(() =>
        {
            JoinGame();
        });
    }

    private void Update()
    {
        playersCountText.text = "Players: " + playersNum.Value.ToString();

        if (!IsServer) return;
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.ConnectedClients != null)
        {
            playersNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
        }
    }

    public void HostGame()
    {
        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.ConnectionData.Address = "0.0.0.0";
        port = int.Parse(portInput.text);
        unityTransport.ConnectionData.Port = (ushort)port;
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.BitConverter.GetBytes(port);
        Debug.Log("Hosting on port: " + unityTransport.ConnectionData.Port);
        NetworkManager.Singleton.StartHost();
    }

    public void JoinGame()
    {
        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.ConnectionData.Address = ipInput.text;
        port = int.Parse(portInput.text);
        unityTransport.ConnectionData.Port = (ushort)port;
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.BitConverter.GetBytes(port);
        NetworkManager.Singleton.StartClient();
    }
}
