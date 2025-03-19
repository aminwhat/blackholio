using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpacetimeDB;
using SpacetimeDB.Types;

public class GameManager : MonoBehaviour
{
    const string SERVER_URL = "http://127.0.0.1:3000";

    const string MODULE_NAME = "blackholio";

    const string AUTH_TOKEN_KEY = "spacetimedb_auth_token";

    public static event Action OnConnected;

    public static event Action OnSubscriptionApplied;

    public float borderThickness = 2;

    public Material borderMaterial;


    public static GameManager Instance { get; private set; }

    public static Identity LocalIdentity { get; private set; }

    public static DbConnection Conn { get; private set; }


    private void Start()
    {
        Instance = this;

        Application.targetFrameRate = 60;


        var builder = DbConnection.Builder()
            .OnConnect(HandleConnect)
            .OnConnectError(HandleConnectError)
            .OnDisconnect(HandleDisconnect)
            .WithUrl(SERVER_URL)
            .WithModule(MODULE_NAME);

        // If the user has a SpacetimeDB auth token stored in the Unity player prefs, use it.
        if (PlayerPrefs.HasKey(AUTH_TOKEN_KEY))
        {
            builder = builder.WithToken(PlayerPrefs.GetString(AUTH_TOKEN_KEY));
        }

        // Connect to the server.
        Conn = builder.Build();
    }

    void HandleConnect(DbConnection _conn, Identity identity, string token)
    {
        Debug.Log("Connected to the server.");
        PlayerPrefs.SetString(AUTH_TOKEN_KEY, token);
        LocalIdentity = identity;

        OnConnected?.Invoke();

        Conn.SubscribeBuilder()
            .OnApplied(HandleSubscriptionApplied)
            .SubscribeToAllTables();
    }

    void HandleConnectError(DbConnection _conn, string error)
    {
        Debug.LogError("Failed to connect to the server: " + error);
    }

    void HandleDisconnect(DbConnection _conn, Exception ex)
    {
        Debug.LogError("Disconnected from the server");
        if (ex != null)
        {
            Debug.LogException(ex);
        }
    }

    void HandleSubscriptionApplied(ReducerEventContext ctx)
    {
        Debug.Log("Subscription applied");
        OnSubscriptionApplied?.Invoke();
    }


    public static bool IsConnected => Conn != null && Conn.IsActive;


    public static void Disconnect()
    {
        if (Conn != null)
        {
            Conn.Disconnect();
            Conn = null;
        }
    }

}


// Was in the tutorial, but not in my code, because I did not have the error.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}