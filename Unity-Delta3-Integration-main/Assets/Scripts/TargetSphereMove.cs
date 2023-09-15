using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    public float moveSpeed = 1f; // Speed at which the sphere moves
    public float minX = -5f;     // Minimum X position within 
    public float maxX = 5f;      // Maximum X position within 
    public float minY = -5f;     // Minimum Y position within 
    public float maxY = 5f;    // Maximum Y position within 
    public float minZ = -5f;     // Minimum Z position within 
    public float maxZ = 5f;      // Maximum Z position within 

    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        RandomizeTargetPosition();
    }

    void Update()
    {
        if (isMoving)
        {
            // Move towards the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // If the "TargetSphere" reaches the target position, choose a new random target position
            if (transform.position == targetPosition)
            {
                RandomizeTargetPosition();
            }
        }

        // Check for input to start/stop the movement
        if (Input.GetKeyDown(KeyCode.M))
        {
            StartRandomMovement();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            StopRandomMovement();
        }
    }

    // Function to set a new random target position within the 1x1x1 area
    void RandomizeTargetPosition()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        float randomZ = Random.Range(minZ, maxZ);
        targetPosition = new Vector3(randomX, randomY, randomZ);
    }

    // Call this function to start the random movement
    public void StartRandomMovement()
    {
        isMoving = true;
    }

    // Call this function to stop the random movement
    public void StopRandomMovement()
    {
        isMoving = false;
    }
}
