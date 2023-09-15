using UnityEngine;

public class SphereColorChanger : MonoBehaviour
{
    public GameObject TargetSphere;
    public GameObject EndEffector;
    private Renderer endeffectorRenderer;
    private Material originalMaterial;
    private Material greenMaterial;
    private float distanceThreshold = 0.2f;

    private void Start()
    {
        // Get the Renderer component of the endeffector GameObject
        endeffectorRenderer = GetComponent<Renderer>();

        // Store the original material of the endeffector
        originalMaterial = endeffectorRenderer.material;

        // Create a green material (you can also assign it in the Inspector)
        greenMaterial = new Material(Shader.Find("Standard"));
        greenMaterial.color = Color.green;
    }

    private void Update()
    {
        // Check if the endeffector is inside the targetsphere
        float distanceToPlayer = Vector3.Distance(EndEffector.transform.position, TargetSphere.transform.position);

        if (distanceToPlayer <= distanceThreshold)
        {
            // Change the endeffector material to green
            endeffectorRenderer.material = greenMaterial;
        }
        else
        {
            // Revert to the original material
            endeffectorRenderer.material = originalMaterial;
        }
    }
}
