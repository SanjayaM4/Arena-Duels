using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelayTestUI : MonoBehaviour
{
    public Button hostButton;
    public Button joinButton;
    public TMP_InputField joinCodeInputField;
    public TextMeshProUGUI statusText;

    private bool isBusy = false;

    async void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinClicked);

        SetStatus("Signing in...");

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            SetStatus("Signed in. Ready.");
        }
        catch (System.Exception e)
        {
            SetStatus("Auth failed: " + e.Message);
        }
    }

    void OnHostClicked()
    {
        _ = StartHostWithRelay();
    }

    void OnJoinClicked()
    {
        _ = StartClientWithRelay(joinCodeInputField.text);
    }

    private void SetStatus(string message)
    {
        statusText.text = message;
    }

    private void SetInteractable(bool interactable)
    {
        hostButton.interactable = interactable;
        joinButton.interactable = interactable;
    }

    private async Task StartHostWithRelay()
    {
        isBusy = true;
        SetInteractable(false);
        SetStatus("Creating relay allocation...");

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            SetStatus("Hosting! Join Code: " + joinCode);
            joinCodeInputField.text = joinCode; // shown for convenience, so you can copy it
        }
        catch (System.Exception e)
        {
            SetStatus("Host failed: " + e.Message);
            SetInteractable(true);
        }

        isBusy = false;
    }

    private async Task StartClientWithRelay(string joinCode)
    {
        if (string.IsNullOrEmpty(joinCode))
        {
            SetStatus("Enter a join code first.");
            return;
        }

        isBusy = true;
        SetInteractable(false);
        SetStatus("Joining relay...");

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            SetStatus("Joined!");
        }
        catch (System.Exception e)
        {
            SetStatus("Join failed: " + e.Message);
            SetInteractable(true);
        }

        isBusy = false;
    }
}