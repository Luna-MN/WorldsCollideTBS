// The SteamManager is designed to work with Steamworks.NET on Godot 4.x
// Ported from the public-domain Unity SteamManager (v1.0.12).
// Released into the public domain.

#if !(GODOT_WINDOWS || GODOT_LINUX || GODOT_LINUXBSD || GODOT_OSX || GODOT_MACOS || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using Godot;
#if !DISABLESTEAMWORKS
using System.Text;
using Steamworks;
#endif

/// <summary>
/// Provides a base implementation of Steamworks.NET for Godot 4.
/// Register this script as an Autoload (Project Settings → Autoload) so it
/// persists for the lifetime of the application — that's the Godot equivalent
/// of Unity's <c>DontDestroyOnLoad</c>.
/// </summary>
public partial class SteamManager : Node
{
#if !DISABLESTEAMWORKS
    private static bool _everInitialized;
    private static SteamManager _instance;

    public static SteamManager Instance => _instance;

    private bool _initialized;
    public static bool Initialized => _instance != null && _instance._initialized;

    private SteamAPIWarningMessageHook_t _steamApiWarningMessageHook;
    
    private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
    {
        GD.PushWarning(pchDebugText.ToString());
    }

    public override void _EnterTree()
    {
        // Only one instance allowed.
        if (_instance != null)
        {
            QueueFree();
            return;
        }
        _instance = this;

        if (_everInitialized)
        {
            // Almost always an error: the SteamAPI was already initialized once this session.
            throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
        }
    }

    public override void _Ready()
    {
        if (_instance != this) return;

        if (!Packsize.Test())
        {
            GD.PushError("[Steamworks.NET] Packsize Test returned false. The wrong version of Steamworks.NET is being run on this platform.");
        }

        if (!DllCheck.Test())
        {
            GD.PushError("[Steamworks.NET] DllCheck Test returned false. One or more of the Steamworks binaries seems to be the wrong version.");
        }

        try
        {
            // If Steam is not running or the game wasn't started through Steam, RestartAppIfNecessary
            // starts the Steam client and re-launches this game (a rudimentary form of DRM).
            // Replace AppId_t.Invalid with your real App ID once Valve assigns one
            // (and remove steam_appid.txt from the depot).
            if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
            {
                GetTree().Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e)
        {
            GD.PushError(
                "[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. " +
                "It must sit next to the Godot executable (or your exported game's exe). " +
                "Refer to Steamworks.NET README.\n" + e);

            GetTree().Quit();
            return;
        }

        // Initialize the Steamworks API.
        // Returns false if Steam isn't running, steam_appid.txt is missing,
        // user doesn't own the App ID on the active account, etc.
        // https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
        _initialized = SteamAPI.Init();
        if (!_initialized)
        {
            GD.PushError("[Steamworks.NET] SteamAPI.Init() failed. Is Steam running, and is steam_appid.txt next to the executable?");
            return;
        }

        _everInitialized = true;

        // Set up our warning-message hook. Launch with "-debug_steamapi" to see anything.
        if (_steamApiWarningMessageHook == null)
        {
            _steamApiWarningMessageHook = SteamAPIDebugTextHook;
            SteamClient.SetWarningMessageHook(_steamApiWarningMessageHook);
        }
    }

    public override void _Process(double delta)
    {
        if (!_initialized) return;
        SteamAPI.RunCallbacks();
    }

    public override void _ExitTree()
    {
        if (_instance != this) return;
        _instance = null;

        if (!_initialized) return;
        SteamAPI.Shutdown();
        _initialized = false;
    }
    
    public string ClientName => SteamFriends.GetPersonaName();
#else
    public static bool Initialized => false;

    public override void _Ready()
    {
        GD.Print("[SteamManager] Steamworks is disabled for this build/platform.");
    }
#endif
}