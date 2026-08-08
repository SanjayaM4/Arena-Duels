using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayTestUI : MonoBehaviour
{
    private string joinCodeInput = "";
    private string statusMessage = "";
    private bool isBusy = false;

    async void Start()
    {
        // Sign in anonymously as soon as the game starts
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            statusMessage = "Signed in. Ready.";
        }
        catch (System.Exception e)
        {
            statusMessage = "Auth failed: " + e.Message;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 250));

        GUILayout.Label("Status: " + statusMessage);

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (!isBusy && GUILayout.Button("Host (Create Relay)"))
            {
                _ = StartHostWithRelay();
            }

            GUILayout.Space(10);
            GUILayout.Label("Join Code:");
            joinCodeInput = GUILayout.TextField(joinCodeInput);

            if (!isBusy && GUILayout.Button("Join as Client"))
            {
                _ = StartClientWithRelay(joinCodeInput);
            }
        }
        else
        {
            GUILayout.Label("Mode: " + (NetworkManager.Singleton.IsHost ? "Host" : "Client"));
        }

        GUILayout.EndArea();
    }

    private async Task StartHostWithRelay()
    {
        isBusy = true;
        statusMessage = "Creating relay allocation...";

        try
        {
            // "1" here means 1 other player besides the host - adjust if you ever support more
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            statusMessage = "Hosting! Join Code: " + joinCode;
            joinCodeInput = joinCode; // so you can see/copy it from the field too
        }
        catch (System.Exception e)
        {
            statusMessage = "Host failed: " + e.Message;
        }

        isBusy = false;
    }

    private async Task StartClientWithRelay(string joinCode)
    {
        if (string.IsNullOrEmpty(joinCode))
        {
            statusMessage = "Enter a join code first.";
            return;
        }

        isBusy = true;
        statusMessage = "Joining relay...";

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            statusMessage = "Joined!";
        }
        catch (System.Exception e)
        {
            statusMessage = "Join failed: " + e.Message;
        }

        isBusy = false;
    }
}