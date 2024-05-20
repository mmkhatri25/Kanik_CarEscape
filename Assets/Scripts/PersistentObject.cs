using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private void Start()
    {
        // Make sure this GameObject persists across scene changes
        DontDestroyOnLoad(gameObject);
    }

    // Add any other scripts or functionality you want for the persistent object
}