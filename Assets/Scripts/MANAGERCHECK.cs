using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MANAGERCHECK : MonoBehaviour
{
    public GameObject prefabToCreate;

    public static MANAGERCHECK Instance;

    void Awake()
    {
        Instance = this;
        // Check if a GameObject of type exists
        PersistentObject existingObject = GameObject.FindObjectOfType<PersistentObject>();

        if (existingObject == null)
        {
            // Instantiate the prefab if it doesn't exist
            Instantiate(prefabToCreate);
        }
    }
}
