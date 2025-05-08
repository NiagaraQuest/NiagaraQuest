using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class PersistentBackgroundVideo : MonoBehaviour
{
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private RawImage backgroundImage;
    
    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;

    // Make this class a singleton to access it from anywhere
    public static PersistentBackgroundVideo Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This makes the video player persist across scene loads
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create Video Player component
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        
        // Create render texture with appropriate resolution
        renderTexture = new RenderTexture(1920, 1080, 24);
        
        // Setup Video Player
        videoPlayer.clip = videoClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = true;
        videoPlayer.playOnAwake = true;
        
        // Assign render texture to RawImage
        backgroundImage.texture = renderTexture;
    }
}