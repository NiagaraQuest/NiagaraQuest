using UnityEngine;

public class SkyManager : MonoBehaviour
{
    [Header("Sun Settings")]
    public Light sun;                          // Assign your Directional Light here
    public float dayLengthInMinutes = 2f;      // How long one full day lasts in real-time minutes

    [Header("Star Fade Settings")]
    [Range(0f, 1f)] public float nightThreshold = 0f; // When stars start appearing
    public Material fastSkyMaterial;                 // Assign the FastSky material

    void Update()
    {
        // Rotate the sun
        float rotationSpeed = 360f / (dayLengthInMinutes * 60f); // degrees per second
        sun.transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);

        // Calculate sun direction
        Vector3 sunDirection = sun.transform.forward;
        float sunDot = Vector3.Dot(sunDirection, Vector3.down); // negative means sun is above horizon

        // Set star visibility in FastSky shader (assuming "_StarVisibility" exists)
        if (fastSkyMaterial != null)
        {
            // Smooth transition based on sun direction
            float starVisibility = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((nightThreshold - sunDot) * 5f));
            fastSkyMaterial.SetFloat("_StarVisibility", starVisibility);
        }
    }
}
