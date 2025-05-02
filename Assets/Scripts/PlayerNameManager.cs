using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;
using System.Collections.Generic;
using Unity.Services.Multiplayer;
using Unity.Entities;


public class PlayerNameManager : MonoBehaviour
{
    public ISession Session { get; set; }

    public TMP_InputField nameChangeInputField;
    public static PlayerNameManager Instance { get; private set; }

    public event Action<string> OnPlayerNameChanged;

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            await InitializeAndSignInAsync();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async Task InitializeAndSignInAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Signed in as: {AuthenticationService.Instance.PlayerId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize or sign in: {e.Message}");
        }
    }

    public string GetPlayerName()
    {
        return AuthenticationService.Instance.PlayerName;
    }

    public async Task<bool> SetPlayerNameAsync(string newName)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            OnPlayerNameChanged?.Invoke(newName);
            Debug.Log($"Player name updated to: {newName}");
            GetPlayerName();            
            return true;
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"Authentication error: {e}");
        }
        catch (RequestFailedException e)
        {
            Debug.LogError($"Request failed: {e}");
        }

        return false;
    }
    public async void SetPlayerNameButtonHandler()
    {
        await SetPlayerNameAsync(nameChangeInputField.text);
        Debug.Log("Changing Player name to: "+ nameChangeInputField.text);
    }
}
