using System.Collections;
using UnityEngine;
using LiveKit;
using LiveKit.Proto;
using UnityEngine.UI;
using RoomOptions = LiveKit.RoomOptions;
using System.Collections.Generic;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class LiveKitConnectionManager : MonoBehaviour
{
    [Header("Connection Settings")]
    public string serverUrl = "wss://banthry1-3hdse7yt.livekit.cloud";
    public string roomName = "xr_stream_test";

    [Header("Tokens - Paste from LiveKit Dashboard")]
    [TextArea(3, 5)]
    public string subscriberToken = "";

    [Header("UI References")]
    [SerializeField] private Button connectButton;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private Button muteButton;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_InputField serverUrlText;
    [SerializeField] private TMP_InputField roomNameText;

    [Header("XR Components")]
    public Renderer streamScreenRenderer;
    [SerializeField] private AudioSource streamAudioSource;


    [Header("Connection Status Colors")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color connectingColor = Color.yellow;
    [SerializeField] private Color disconnectedColor = Color.red;
    [SerializeField] private Color errorColor = Color.magenta;

    // Private variables
    private Room room = null;
    private RenderTexture streamRenderTexture;
    private bool isConnected = false;
    private bool isConnecting = false;
    private bool isMuted = false;
    private bool shouldUpdateVideo = false;

    // Media handling - CORRECTED: Store GameObjects and AudioSources separately
    Dictionary<string, GameObject> audioObjects = new();
    Dictionary<string, AudioSource> audioSources = new(); // Store AudioSource references
    Dictionary<string, RemoteVideoTrack> videoTrackReferences = new();
    List<VideoStream> videoStreams = new();

    // Connection states
    private enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Error
    }

    private ConnectionState currentState = ConnectionState.Disconnected;

    void Start()
    {
        InitializeComponents();
        SetupUI();
        UpdateConnectionState(ConnectionState.Disconnected);

        // testbed
        // OnConnectClicked();
    }

    private void InitializeComponents()
    {
        streamRenderTexture = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32);
        streamRenderTexture.Create();

        if (streamScreenRenderer != null)
        {
            streamScreenRenderer.material.mainTexture = streamRenderTexture;
        }

        if (streamAudioSource == null)
        {
            streamAudioSource = gameObject.AddComponent<AudioSource>();
        }
        streamAudioSource.playOnAwake = false;
    }

    private void SetupUI()
    {
        connectButton.onClick.AddListener(OnConnectClicked);
        disconnectButton.onClick.AddListener(OnDisconnectClicked);
        muteButton.onClick.AddListener(OnMuteToggled);

        if (serverUrlText != null) serverUrlText.text = $"{serverUrl}";
        if (roomNameText != null) roomNameText.text = $"{roomName}";
    }

    public void OnConnectClicked()
    {
        if (isConnecting || isConnected) return;

        if (string.IsNullOrEmpty(subscriberToken))
        {
            UpdateConnectionState(ConnectionState.Error, "Please paste subscriber token in inspector");
            return;
        }

        StartCoroutine(ConnectToRoom());
    }

    public void OnDisconnectClicked()
    {
        DisconnectFromRoom();
    }

    public void OnMuteToggled()
    {
        isMuted = !isMuted;

        // CORRECTED: Mute all audio sources using the stored references
        foreach (var audioSource in audioSources.Values)
        {
            if (audioSource != null)
            {
                audioSource.mute = isMuted;
            }
        }

        TMP_Text muteButtonText = muteButton.GetComponentInChildren<TMP_Text>();
        if (muteButtonText != null)
        {
            muteButtonText.text = isMuted ? "Unmute Audio" : "Mute Audio";
        }
    }

    private IEnumerator ConnectToRoom()
    {
        UpdateConnectionState(ConnectionState.Connecting);
        isConnecting = true;

        room = new Room();
        room.TrackSubscribed += OnTrackSubscribed;
        room.TrackUnsubscribed += OnTrackUnsubscribed;
        room.DataReceived += OnDataReceived;
        // room.Disconnected += OnRoomDisconnected;

        var options = new RoomOptions();
        var connectTask = room.Connect(serverUrl, subscriberToken, options);

        // Move yield outside of try-catch
        yield return connectTask;

        // Handle result after yielding
        try
        {
            if (!connectTask.IsError)
            {
                isConnected = true;
                isConnecting = false;
                Debug.Log("<color=green>" + isConnected + "</color>");

                UpdateConnectionState(ConnectionState.Connected);
                Debug.Log($"Successfully connected to room: {room.Name}");
            }
            else
            {
                isConnecting = false;
                isConnected = false;
                Debug.Log("<color=green>" + isConnected + "</color>");

                UpdateConnectionState(ConnectionState.Error, "Connection failed");
                Debug.LogError($"LiveKit connection error");
            }
        }
        catch (System.Exception e)
        {
            isConnecting = false;
            isConnected = false;
            UpdateConnectionState(ConnectionState.Error, $"Connection failed: {e.Message}");
            Debug.LogError($"LiveKit connection error: {e}");
        }
    }


    private void DisconnectFromRoom()
    {
        shouldUpdateVideo = false;

        if (room != null)
        {
            room.Disconnect();
            CleanupStreams();
            room = null;
        }

        isConnected = false;
        isConnecting = false;
        UpdateConnectionState(ConnectionState.Disconnected);
        Debug.Log("Disconnected from room");
    }

    private void OnTrackSubscribed(IRemoteTrack track, RemoteTrackPublication publication, RemoteParticipant participant)
    {
        Debug.Log($"Track subscribed: {track.Kind} from {participant.Identity}");

        if (track is RemoteVideoTrack videoTrack)
        {
            shouldUpdateVideo = true;
            HandleVideoTrack(videoTrack);
        }
        else if (track is RemoteAudioTrack audioTrack)
        {
            HandleAudioTrack(audioTrack);
        }
    }

    private void HandleVideoTrack(RemoteVideoTrack videoTrack)
    {
        Debug.Log($"Setting up video track: {videoTrack.Sid}");

        var videoStream = new VideoStream(videoTrack);

        videoStream.TextureReceived += (texture) =>
        {
            if (texture != null && streamRenderTexture != null)
            {
                // RenderTexture.active = streamRenderTexture;
                // Graphics.Blit(texture, streamRenderTexture);
                // RenderTexture.active = null;

                streamScreenRenderer.material.mainTexture = texture;
                Debug.Log($"Direct texture assignment - Size: {texture.width}x{texture.height}");
            }
        };

        videoStream.Start();
        StartCoroutine(ContinuousVideoUpdate(videoStream));

        videoStreams.Add(videoStream);
        videoTrackReferences[videoTrack.Sid] = videoTrack;

        Debug.Log("Video stream from mobile camera is now displaying on StreamScreen");
    }

    private IEnumerator ContinuousVideoUpdate(VideoStream videoStream)
    {
        while (videoStream != null && shouldUpdateVideo && room != null)
        {
            yield return StartCoroutine(videoStream.Update());
        }
    }

    // CORRECTED: Audio track handling following the official sample pattern
    private void HandleAudioTrack(RemoteAudioTrack audioTrack)
    {
        Debug.Log($"Setting up audio track: {audioTrack.Sid}");

        // Create GameObject with AudioSource - following official sample pattern
        GameObject audioObject = new GameObject(audioTrack.Sid);
        var audioSource = audioObject.AddComponent<AudioSource>();

        // Create AudioStream with track and AudioSource - this is the correct constructor
        var audioStream = new AudioStream(audioTrack, audioSource);

        // Apply mute state if already muted
        audioSource.mute = isMuted;

        // CORRECTED: Store references separately
        audioObjects[audioTrack.Sid] = audioObject;
        audioSources[audioTrack.Sid] = audioSource; // Store AudioSource reference for muting

        Debug.Log("Audio stream from mobile is now playing through Quest speakers");
    }

    private void OnTrackUnsubscribed(IRemoteTrack track, RemoteTrackPublication publication, RemoteParticipant participant)
    {
        Debug.Log($"Track unsubscribed: {track.Kind} from {participant.Identity}");

        if (track is RemoteVideoTrack videoTrack)
        {
            // Cleanup video streams
            for (int i = videoStreams.Count - 1; i >= 0; i--)
            {
                var stream = videoStreams[i];
                if (videoTrackReferences.ContainsValue(videoTrack))
                {
                    stream.Stop();
                    stream.Dispose();
                    videoStreams.RemoveAt(i);
                    videoTrackReferences.Remove(videoTrack.Sid); // Clean up reference
                    break;
                }
            }

            // Clear the screen
            if (streamRenderTexture != null)
            {
                RenderTexture.active = streamRenderTexture;
                GL.Clear(true, true, Color.black);
                RenderTexture.active = null;
            }
        }
        else if (track is RemoteAudioTrack audioTrack)
        {
            // CORRECTED: Cleanup audio objects following official sample pattern
            if (audioObjects.ContainsKey(audioTrack.Sid))
            {
                var audioObject = audioObjects[audioTrack.Sid];
                if (audioObject != null)
                {
                    // Stop audio source before destroying
                    var audioSource = audioObject.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.Stop();
                    }

                    Destroy(audioObject);
                }
                audioObjects.Remove(audioTrack.Sid);
            }

            // Remove from audio sources dictionary
            if (audioSources.ContainsKey(audioTrack.Sid))
            {
                audioSources.Remove(audioTrack.Sid);
            }
        }
    }

    private void OnDataReceived(byte[] data, Participant participant, DataPacketKind kind, string topic)
    {
        var message = System.Text.Encoding.UTF8.GetString(data);
        Debug.Log($"Data received from {participant.Identity}: {message}");
    }

    // CORRECTED: Cleanup following official sample pattern
    private void CleanupStreams()
    {
        // Cleanup video streams
        foreach (var videoStream in videoStreams)
        {
            videoStream.Stop();
            videoStream.Dispose();
        }
        videoStreams.Clear();

        // Cleanup audio objects - following official sample pattern
        foreach (var audioObject in audioObjects.Values)
        {
            if (audioObject != null)
            {
                var audioSource = audioObject.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
                Destroy(audioObject);
            }
        }
        audioObjects.Clear();
        audioSources.Clear();

        // Clear render texture
        if (streamRenderTexture != null)
        {
            RenderTexture.active = streamRenderTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
        }
    }

    private void UpdateConnectionState(ConnectionState state, string message = "")
    {
        currentState = state;

        string statusMessage = "";
        Color statusColor = disconnectedColor;

        switch (state)
        {
            case ConnectionState.Disconnected:
                statusMessage = "Disconnected";
                statusColor = disconnectedColor;
                connectButton.interactable = true;
                disconnectButton.interactable = false;
                muteButton.interactable = false;
                break;

            case ConnectionState.Connecting:
                statusMessage = "Connecting...";
                statusColor = connectingColor;
                connectButton.interactable = false;
                disconnectButton.interactable = true;
                muteButton.interactable = false;
                break;

            case ConnectionState.Connected:
                statusMessage = "Connected";
                statusColor = connectedColor;
                connectButton.interactable = false;
                disconnectButton.interactable = true;
                muteButton.interactable = true;
                break;

            case ConnectionState.Error:
                statusMessage = "Error";
                statusColor = errorColor;
                connectButton.interactable = true;
                disconnectButton.interactable = false;
                muteButton.interactable = false;
                break;
        }

        if (!string.IsNullOrEmpty(message))
        {
            statusMessage += $": {message}";
        }

        if (statusText != null)
        {
            statusText.text = statusMessage;
            statusText.color = statusColor;
        }

        if (instructionText != null)
        {
            switch (state)
            {
                case ConnectionState.Disconnected:
                    instructionText.text = "1. Setup mobile publisher with your token\n2. Start mobile camera stream\n3. Click Connect above\n4. Use controllers to grab/resize screen";
                    break;
                case ConnectionState.Connected:
                    instructionText.text = "✅ Connected! Mobile camera should appear on screen.\n🎮 Grab and resize screen with Quest controllers.";
                    break;
                case ConnectionState.Connecting:
                    instructionText.text = "⏳ Connecting to LiveKit server...";
                    break;
                default:
                    instructionText.text = "";
                    break;
            }
        }
    }

    private void OnRoomDisconnected()
    {
        Debug.Log("Room disconnected - attempting reconnect");
        isConnected = false;

        if (currentState == ConnectionState.Connected) // Was connected, so try to reconnect
        {
            UpdateConnectionState(ConnectionState.Reconnecting);
            StartCoroutine(AttemptReconnect());
        }
    }

    private IEnumerator AttemptReconnect()
    {
        yield return new WaitForSeconds(2f);
        if (!isConnected)
        {
            Debug.Log("Attempting to reconnect...");
            StartCoroutine(ConnectToRoom());
        }
    }

    void OnDestroy()
    {
        DisconnectFromRoom();

        if (streamRenderTexture != null)
        {
            streamRenderTexture.Release();
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && isConnected)
        {
            DisconnectFromRoom();
        }
    }
}
