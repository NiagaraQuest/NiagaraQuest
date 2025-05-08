using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class BackgroundVideo : MonoBehaviour
{
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private RawImage rawImage;
    
    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;

    void Awake()
    {
        // Add Video Player component if not exists
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
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
        rawImage.texture = renderTexture;
    }
}