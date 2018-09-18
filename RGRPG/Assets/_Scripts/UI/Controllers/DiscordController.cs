#pragma warning disable 0219 // Variable assigned but not used


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RGRPG.Core;

[System.Serializable]
public class DiscordJoinEvent : UnityEngine.Events.UnityEvent<string> { }

[System.Serializable]
public class DiscordSpectateEvent : UnityEngine.Events.UnityEvent<string> { }

[System.Serializable]
public class DiscordJoinRequestEvent : UnityEngine.Events.UnityEvent<DiscordRpc.DiscordUser> { }

public class DiscordController : MonoBehaviour
{

    private static DiscordController instance;
    public static DiscordController Instance { get { return instance; } }

    public DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence(); //This is fucking bullshit <---why?
    public string applicationId = "491434833151787008"; //TODO: secure app ID
    public string optionalSteamId;
    public int callbackCalls;
    public int clickCounter;
    public DiscordRpc.DiscordUser joinRequest;
    public UnityEngine.Events.UnityEvent onConnect;
    public UnityEngine.Events.UnityEvent onDisconnect;
    public UnityEngine.Events.UnityEvent hasResponded;
    public DiscordJoinEvent onJoin;
    public DiscordJoinEvent onSpectate;
    public DiscordJoinRequestEvent onJoinRequest;

    DiscordRpc.EventHandlers handlers;

    public void OnClick()
    {
        Debug.Log("Discord: on click!");
        clickCounter++;

        presence.details = string.Format("Button clicked {0} times", clickCounter);

        DiscordRpc.UpdatePresence(presence);
    }

    public void InMainMenu()
    {
        Debug.Log("Discord: on main menu!");

        presence.details = "Main Menu";

        DiscordRpc.UpdatePresence(presence);
    }

    public void InOverworld()
    {
        //Debug.Log("Discord: in the lobby!");

        presence.state = "Playing Solo";
        presence.details = "Overworld";
        presence.largeImageKey = "die";
        presence.largeImageText = "Lobby";
        presence.smallImageKey = "sword-medium";
        presence.smallImageText = "Attacker";
        presence.partyId = "ae488379-351d-4a4f-ad32-2b9b01c91657";
        presence.partySize = 1;
        presence.partyMax = 4;
        //presence.spectateSecret = "MTIzNDV8MTIzNDV8MTMyNDU0";
        //presence.joinSecret = "MTI4NzM0OjFpMmhuZToxMjMxMjM= ";

        DiscordRpc.UpdatePresence(presence);
    }

    public void InBattle()
    {
        //Debug.Log("Discord: in a battle!");

        presence.state = "Playing Solo";
        presence.details = "Battle";
        presence.largeImageKey = "fight";
        presence.largeImageText = "Battle";
        presence.smallImageKey = "sword-medium";
        presence.smallImageText = "Attacker";
        presence.partyId = "ae488379-351d-4a4f-ad32-2b9b01c91657";
        presence.partySize = 1;
        presence.partyMax = 4;
        //presence.spectateSecret = "MTIzNDV8MTIzNDV8MTMyNDU0";
        //presence.joinSecret = "MTI4NzM0OjFpMmhuZToxMjMxMjM= ";

        DiscordRpc.UpdatePresence(presence);
    }

    public void RequestRespondYes()
    {
        Debug.Log("Discord: responding yes to Ask to Join request");
        DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.Yes);
        hasResponded.Invoke();
    }

    public void RequestRespondNo()
    {
        Debug.Log("Discord: responding no to Ask to Join request");
        DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.No);
        hasResponded.Invoke();
    }

    public void ReadyCallback(ref DiscordRpc.DiscordUser connectedUser)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: connected to {0}#{1}: {2}", connectedUser.username, connectedUser.discriminator, connectedUser.userId));
        onConnect.Invoke();
    }

    public void DisconnectedCallback(int errorCode, string message)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: disconnect {0}: {1}", errorCode, message));
        onDisconnect.Invoke();
    }

    public void ErrorCallback(int errorCode, string message)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: error {0}: {1}", errorCode, message));
    }

    public void JoinCallback(string secret)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: join ({0})", secret));
        onJoin.Invoke(secret);
    }

    public void SpectateCallback(string secret)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: spectate ({0})", secret));
        onSpectate.Invoke(secret);
    }

    public void RequestCallback(ref DiscordRpc.DiscordUser request)
    {
        ++callbackCalls;
        Debug.Log(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId));
        joinRequest = request;
        onJoinRequest.Invoke(request);
    }

    void Start()
    {

    }

    void Update()
    {
        DiscordRpc.RunCallbacks();
    }

    public void OnEnable()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogWarning("[DiscordController] Warning: Multiple instances created!");

        Debug.Log("Discord: init");
        callbackCalls = 0;

        handlers = new DiscordRpc.EventHandlers
        {
            readyCallback = ReadyCallback
        };
        handlers.disconnectedCallback += DisconnectedCallback;
        handlers.errorCallback += ErrorCallback;
        handlers.joinCallback += JoinCallback;
        handlers.spectateCallback += SpectateCallback;
        handlers.requestCallback += RequestCallback;
        DiscordRpc.Initialize(applicationId, ref handlers, true, optionalSteamId);
    }

    public void OnApplicationQuit()
    {
        Debug.Log("Discord: shutdown");
        DiscordRpc.Shutdown();
    }

    //DEBUG METHOD
    /*void OnApplicationPause()
    {
        Debug.Log("Discord: shutdown");
        DiscordRpc.Shutdown();
    }
     void OnApplicationFocus()
    {
        OnEnable();
    }*/

    void OnDestroy()
    {

    }
}