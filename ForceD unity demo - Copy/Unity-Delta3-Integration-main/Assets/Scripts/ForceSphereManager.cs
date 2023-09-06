using System.Collections.Generic;
using UnityEngine;

public class ForceSphereManager : MonoBehaviour
{
    private static ForceSphereManager instance;

    public static ForceSphereManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ForceSphereManager>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<ForceSphereManager>();
                    singletonObject.name = "ForceSphereManager (Singleton)";
                }
            }
            return instance;
        }
    }

    public List<GameObject> spawnedForceSpheres = new List<GameObject>();

    public void AddForceSphere(GameObject sphere)
    {
        spawnedForceSpheres.Add(sphere);
    }

    public void RemoveForceSphere(GameObject sphere)
    {
        spawnedForceSpheres.Remove(sphere);
    }

}
