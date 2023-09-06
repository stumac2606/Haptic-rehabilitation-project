using UnityEngine;
using System.Collections.Generic;

public class SphereSpawner : MonoBehaviour
{
    public GameObject EndEffector; // Reference to the bubble GameObject
    public GameObject spherePrefab; // Prefab of the sphere to be spawned
    public int numSpheres = 5; // Number of spheres to spawn initially
    public float spacingBetweenSpheres = 2.0f;

    private List<GameObject> spawnedSpheres = new List<GameObject>();
    private Vector3[] sphereOffsets; // Offsets from the bubble for sphere spawning

    private void Start()
    {
        InitializeSphereOffsets();

        for (int i = 0; i < numSpheres; i++)
        {
            Vector3 spawnPosition = EndEffector.transform.position + sphereOffsets[i];
            GameObject newSphere = Instantiate(spherePrefab, spawnPosition, Quaternion.identity);
            spawnedSpheres.Add(newSphere);
        }
    }

    private void Update()
    {
        Vector3 sphereSpawnPosition = EndEffector.transform.position + sphereOffsets[0];

        if (Vector3.Distance(transform.position, spawnedSpheres[2].transform.position) < sphereSpawnPosition.magnitude)
        {
            Destroy(spawnedSpheres[0]);
            spawnedSpheres.RemoveAt(0);

            Vector3 newSpherePosition = EndEffector.transform.position + sphereOffsets[4];
            GameObject newSphere = Instantiate(spherePrefab, newSpherePosition, Quaternion.identity);
            spawnedSpheres.Add(newSphere);
        }
    }

    private void InitializeSphereOffsets()
    {
        sphereOffsets = new Vector3[numSpheres];

        float totalSpacing = (numSpheres - 1) * spacingBetweenSpheres;

        for (int i = 0; i < numSpheres; i++)
        {
            // Calculate offsets for sphere spawning along the forward direction with spacing.
            float offset = (i - (numSpheres - 1) / 2.0f) * spacingBetweenSpheres;
            sphereOffsets[i] = Vector3.forward * offset;
        }
    }
}
