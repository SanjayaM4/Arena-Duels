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

        NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;

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
        Debug.Log("Host button clicked");
        _ = StartHostWithRelay();
    }

    void OnJoinClicked()
    {
        Debug.Log("Join button clicked");
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
    
    public void ResetButtons()
    {
        SetInteractable(true);
        SetStatus("Ready.");
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
            Task<JoinAllocation> joinTask = RelayService.Instance.JoinAllocationAsync(joinCode);
            Task relayTimeout = Task.Delay(10000);

            Task completed = await Task.WhenAny(joinTask, relayTimeout);
            if (completed == relayTimeout)
            {
                SetStatus("Join timed out. Check the code and try again.");
                SetInteractable(true);
                isBusy = false;
                return;
            }

            JoinAllocation joinAllocation = await joinTask;

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            SetStatus("Connecting...");

            var connectionResult = new TaskCompletionSource<bool>();
            void OnConnected(ulong id) => connectionResult.TrySetResult(true);
            void OnFailed() => connectionResult.TrySetResult(false);

            NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;
            NetworkManager.Singleton.OnTransportFailure += OnFailed;

            NetworkManager.Singleton.StartClient();

            Task connectTimeout = Task.Delay(10000);
            Task connectFinished = await Task.WhenAny(connectionResult.Task, connectTimeout);

            NetworkManager.Singleton.OnClientConnectedCallback -= OnConnected;
            NetworkManager.Singleton.OnTransportFailure -= OnFailed;

            bool succeeded = connectFinished == connectionResult.Task && connectionResult.Task.Result;

            if (!succeeded)
            {
                SetStatus("Connection failed or timed out.");
                NetworkManager.Singleton.Shutdown(); // manually abort the stuck attempt
                SetInteractable(true);
                isBusy = false;
                return;
            }

            SetStatus("Joined!");
        }
        catch (System.Exception e)
        {
            SetStatus("Join failed: " + e.Message);
            SetInteractable(true);
            NetworkManager.Singleton.Shutdown(); // in case it got partway connected before erroring
        }

        isBusy = false;
    }

    void OnTransportFailure()
    {
        Debug.LogError("Transport failure - resetting UI");
        SetStatus("Connection failed. Try again.");
        SetInteractable(true);
        isBusy = false;
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
    }
}