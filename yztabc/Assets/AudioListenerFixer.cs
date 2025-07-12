using UnityEngine;

public class AudioListenerFixer : MonoBehaviour
{
    void Start()
    {
        // Find all AudioListeners in the scene
        AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();
        
        if (audioListeners.Length > 1)
        {
            Debug.Log($"Found {audioListeners.Length} AudioListeners. Disabling duplicates...");
            
            // Keep the first one enabled, disable the rest
            for (int i = 1; i < audioListeners.Length; i++)
            {
                audioListeners[i].enabled = false;
                Debug.Log($"Disabled AudioListener on: {audioListeners[i].gameObject.name}");
            }
        }
        else if (audioListeners.Length == 1)
        {
            Debug.Log("Found exactly one AudioListener. No action needed.");
        }
        else
        {
            Debug.LogWarning("No AudioListeners found in the scene!");
        }
    }
} 