using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Template Singleton, Template also inherits from MonoBehaviour
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance;
    static bool _isApplicationQuitting = false;
    static object _lock = new object();

    /// <summary>
    /// Get the Instance of the Singleton or makes one if none exists yet
    /// </summary>
    public static T Instance()
    {
        // Check if quitting, if so return null
        if (_isApplicationQuitting)
        {
            Debug.LogWarning("Singleton Instance of " + typeof(T) + " is already destroyed by the application");
            return null;
        }

        lock (_lock)
        {
            // No instance
            if (_instance == null)
            {
                // Check in scene if it exists
                _instance = FindObjectOfType(typeof(T)) as T;

                // If there are more, problem!
                if (FindObjectsOfType(typeof(T)).Length > 1)
                {
                    Debug.LogError("More than one instance of singleton " + typeof(T));
                    // Prevent crash in release build
                    return _instance;
                }

                // Still no instance, create one
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).ToString());
                    _instance = obj.AddComponent<T>();
                }
            }
        }

        return _instance;
    }

    // Protect the use of new
    protected Singleton() { }

    // Marks singleton as destroyed
    public void OnDestroy()
    {
        _isApplicationQuitting = true;
    }
}
