using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RGRPG.Core;

public class RPCInitialize
{
    //TODO: Change variable name for the Discord Presence Button

    public GameObject BackButtonObject;
    public GameObject PresenceButtonObject;
    public Game game;

    private Button BackButton;
    private Button PresenceButton;

    public DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence();
    public string applicationId = "491261225381003274";
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

    //public static DiscordController instance;

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
        presence.largeImageKey = "die";
        presence.largeImageText = "Lobby";
        presence.smallImageKey = "sword-medium";
        presence.smallImageText = "Attacker";
        presence.partyId = "ae488379-351d-4a4f-ad32-2b9b01c91657";
        presence.partySize = 1;
        presence.partyMax = 4;
        //presence.spectateSecret = "MTIzNDV8MTIzNDV8MTMyNDU0";
        presence.joinSecret = "yeet";

        DiscordRpc.UpdatePresence(presence);
    }

    public void InLobby()
    {
        //Debug.Log("Discord: in the lobby!");

        presence.state = "Playing Solo";
        presence.details = "Lobby";
        presence.largeImageKey = "die";
        presence.largeImageText = "Lobby";
        presence.smallImageKey = "sword-medium";
        presence.smallImageText = "Attacker";
        presence.partyId = "ae488379-351d-4a4f-ad32-2b9b01c91657";
        presence.partySize = 1;
        presence.partyMax = 4;
        //presence.spectateSecret = "MTIzNDV8MTIzNDV8MTMyNDU0";
        presence.joinSecret = "MTI4NzM0OjFpMmhuZToxMjMxMjM= ";

        DiscordRpc.UpdatePresence(presence);
    }

    public void SelectingCharacter()
    {
        //Debug.Log("Discord: in the lobby!");

        presence.state = "Playing Solo";
        presence.details = "Selecting Character";
        presence.largeImageKey = "die";
        presence.largeImageText = "Lobby";
        presence.smallImageKey = "sword-medium";
        presence.smallImageText = "Attacker";
        presence.partyId = "ae488379-351d-4a4f-ad32-2b9b01c91657";
        presence.partySize = 1;
        presence.partyMax = 4;
        //presence.spectateSecret = "MTIzNDV8MTIzNDV8MTMyNDU0";
        presence.joinSecret = "MTI4NzM0OjFpMmhuZToxMjMxMjM= ";

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
        BackButton = BackButtonObject.GetComponent<Button>();

        BackButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenuScene");
        });

        PresenceButton = PresenceButtonObject.GetComponent<Button>();
        PresenceButton.onClick.AddListener(() =>
        {
            OnClick();
        });



        OnEnable();
    }

    void Update()
    {
        DiscordRpc.RunCallbacks();

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("GameScene") && !game.IsInCombat)
            InLobby();
    }

    void OnEnable()
    {
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
    void OnApplicationPause()
    {
        Debug.Log("Discord: shutdown");
        DiscordRpc.Shutdown();
    }
    void OnApplicationFocus()
    {
        OnEnable();
    }

    void OnDestroy()
    {

    }
}