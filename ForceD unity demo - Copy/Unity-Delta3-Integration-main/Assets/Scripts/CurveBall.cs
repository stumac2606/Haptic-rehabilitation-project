using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static DLLImportTest;
using System.Collections;

public class BubbleMotion : MonoBehaviour
{
    public float speed = 0.5f;
    public float frequency = 0.5f;
    public float magnitude = 3.0f;
    public Transform targetTransform;

    public Transform forceSphere;
    public Transform forceSphere1;
    public Transform forceSphere2;
    public Transform forceSphere3;
    public Vector3 movementAreaSize = new Vector3(5f, 5f, 5f);

    

    private Vector3 initialPosition;
    private float timeOffset;

    

    private void Start()
    {
        initialPosition = transform.position;
        timeOffset = UnityEngine.Random.Range(0f, 100f); // Randomize the starting point of the Perlin noise.
    }


    private void Update()
    {
        Vector3 newPosition = initialPosition;

        // Calculate the Perlin noise values for each axis.
        float xNoise = Mathf.PerlinNoise(Time.time * frequency + timeOffset, 0) * 2 - 1;
        float yNoise = Mathf.PerlinNoise(0, Time.time * frequency + timeOffset) * 2 - 1;
        float zNoise = Mathf.PerlinNoise(Time.time * frequency + timeOffset, Time.time * frequency + timeOffset) * 2 - 1;

        // Apply the Perlin noise values to the position within the specified movement area.
        newPosition += new Vector3(xNoise, yNoise, zNoise) * magnitude;
        newPosition = Vector3.Scale(newPosition - initialPosition, movementAreaSize) + initialPosition;

        // Clamp the newPosition values to ensure they stay within the specified range.
        newPosition.x = Mathf.Clamp(newPosition.x, -10f, 10f);
        newPosition.y = Mathf.Clamp(newPosition.y, -10f, 10f);
        newPosition.z = Mathf.Clamp(newPosition.z, -10f, 10f);

        // Move the object smoothly.
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * speed);

        forceCyclinderFollow();
    }

    private void forceCyclinderFollow()
    {
        forceSphere1.transform.LookAt(forceSphere.position);
        if ((forceSphere1.transform.position - forceSphere.position).magnitude > 1.0f)
        {
            forceSphere1.Translate(0.0f, 0.0f, 5 * Time.deltaTime);
        }

        targetTransform.transform.LookAt(forceSphere1.position);
        if ((targetTransform.transform.position - forceSphere1.position).magnitude > 1.0f)
        {
            targetTransform.Translate(0.0f, 0.0f, 5 * Time.deltaTime);
        }

        forceSphere2.transform.LookAt(targetTransform.position);
        if ((forceSphere2.transform.position - targetTransform.position).magnitude > 1.0f)
        {
            forceSphere2.Translate(0.0f, 0.0f, 5 * Time.deltaTime);
        }

        forceSphere3.transform.LookAt(forceSphere2.position);
        if ((forceSphere3.transform.position - forceSphere2.position).magnitude > 1.0f)
        {
            forceSphere3.Translate(0.0f, 0.0f, 5 * Time.deltaTime);
        }

    }

}
