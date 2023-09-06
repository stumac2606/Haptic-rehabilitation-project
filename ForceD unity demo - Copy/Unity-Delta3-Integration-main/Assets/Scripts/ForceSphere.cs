/*using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static DLLImportTest;
using System.Collections;

public class DLLImportTest1 : MonoBehaviour
{

    public enum DHD_STATUS_ENUM
    {
        DHD_STATUS_POWER,
        DHD_STATUS_CONNECTED,
        DHD_STATUS_STARTED,
        DHD_STATUS_RESET,
        DHD_STATUS_IDLE,
        DHD_STATUS_FORCE,
        DHD_STATUS_BRAKE,
        DHD_STATUS_TORQUE,
        DHD_STATUS_WRIST_DETECTED,
        DHD_STATUS_ERROR,
        DHD_STATUS_GRAVITY,
        DHD_STATUS_TIMEGUARD,
        DHD_STATUS_WRIST_INIT,
        DHD_STATUS_REDUNDANCY,
        DHD_STATUS_FORCE_OFF_CAUSE,
        DHD_STATUS_LOCKS,
        DHD_STATUS_AXIS_CHECKED
    }

    private bool forcesOn = false;

    public double forceTest = 1.0;
    public enum deviceStatus { DELTA_OPEN, DELTA_CLOSED };

    public Transform targetTransform;

    public deviceStatus DeviceStatus = deviceStatus.DELTA_CLOSED;

    public GameObject EndEffector;

    public GameObject TargetSphere;

    public float distanceThreshold = 1.8f;

    //variables for the regular attractive force with applying resistive force 
    private float areaSize = 5.0f; // The size of the area in all three dimensions (5x5x5)
    public float timeInsideSphereForForce = 5.0f; // Time user is inside the sphere before resistive force is applied 
    public float constantForceDuration = 5.0f; // time for constant resistive force 
    public float maxRandomForceDurationLimit = 10.0f; // Maximum duration for the random force 
    private float timeInsideTargetSphere = 0.0f;
    private float timeSinceRandomForceStart = 0.0f;
    private bool applyingRandomForce = false;
    private bool applyingRegularForce = true;
    private bool isRandomForceOn = false;

    public float repelForceTest = 1.0f;
    private float repellingForceMultiplier = 1.0f;
    public float maxRepellingforceDistance = 2.0f; // The distance where repelling force is at its maximum

    public GameObject forceSphere;
    private bool repellingForceOn = false;

    public Vector3 DhdPosition = Vector3.zero;

    IntPtr defaultId = new IntPtr(1);

    const int DHD_MAX_STATUS = 17;

    int[] DHD_STATUS_RESULT = new int[DHD_MAX_STATUS];

    public List<DHD_STATUS_ENUM> dhdStatus = new List<DHD_STATUS_ENUM>();

    [DllImport("dhd64.dll")]
    extern static int dhdOpen1();

    [DllImport("dhd64.dll")]
    extern static int dhdStop1(IntPtr id);

    [DllImport("dhd64.dll")]
    extern static int dhdClose1(IntPtr id);

    [DllImport("dhd64.dll")]
    extern static int dhdGetStatus1(int[] dhdStatus, IntPtr id);

    [DllImport("dhd64.dll")]
    extern static int dhdSetBrakes1(int val, IntPtr id);

    [DllImport("dhd64.dll")]
    extern static IntPtr dhdErrorGetLastStr1();

    [DllImport("dhd64.dll")]
    extern static void dhdSleep1(double sec);

    [DllImport("dhd64.dll")]
    extern static int dhdGetPosition1(ref double px, ref double py, ref double pz, IntPtr id);

    [DllImport("dhd64.dll")]
    extern static int dhdEnableForce1(UIntPtr val, IntPtr id);

    [DllImport("dhd64.dll")]
    extern static int dhdSetStandardGravity1(double g, IntPtr id);

    [DllImport("dhd64.dll")]
    extern static int dhdSetForce1(double fx, double fy, double fz, IntPtr id);


    // Start is called before the first frame update
    void Start()
    {
        DhdOpen();
        if (dhdEnableForce(new UIntPtr(1), defaultId) >= 0)
        {
            Debug.Log("Forces set to on");
            forcesOn = true;
        }
        else
        {
            Debug.LogError("ERROR SETTING FORCES TO ON");
        }

        UpdateDHDStatus();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            DhdClose();
            UpdateDHDStatus();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            DhdOpen();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            GetDHDPosition();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            SetDHDBrake(false);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            SetDHDBrake(true);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            UpdateDHDStatus();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            GetLastDHDError();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetDHDGravity(0.0);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            //ApplyForceTest(true, new Vector3((float)forceTest, (float)forceTest, (float)forceTest));
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            //ApplyForceTest(false, new Vector3((float)0.0f, (float)0.0f, (float)0.0f));
        }

    }


    private void FixedUpdate()
    {
        if (DeviceStatus == deviceStatus.DELTA_OPEN)
        {
            if (GetDHDPosition() >= 0) //check to see if information from haptic device successful
            {
                //EndEffector is unity object, this matches the position of haptic device (DhdPosition)
                EndEffector.transform.position = DhdPosition;
            }

          


            //Applying a repelling force to target sphere
            double px = 0, py = 0, pz = 0;
            if (dhdGetPosition(ref px, ref py, ref pz, defaultId) >= 0)
            {
                Vector3 heading = TargetSphere.transform.position - EndEffector.transform.position;
                float distance = heading.magnitude;
                Vector3 direction = heading.normalized;

                // Calculate the repelling force based on the distance
                float repellingForce = CalculateRepellingForce(distance);

                // Apply the force in the opposite direction of the heading
                Vector3 repellingForceVector = -direction * repellingForce * repellingForceMultiplier;

                // Apply the repelling force
                ApplyForceToHapticDevice(repellingForceVector);
            }

            //applying forces to the forceSpheres
            // Check if targetSphere is within range of forceSphere
            if (IsTargetInRangeOfForceSphere())
            {
                forcesOn = true; // Turn on the forces if targetSphere is within range
            }
            else
            {
                forcesOn = false; // Turn off the forces if targetSphere is not within range
            }

            if (forcesOn && IsEndEffectorNearForceSphere())
            {
                // Calculate repelling force based on distance from forceSphere
                //float repellingForce = CalculateRepellingForceFromForceSphere();
                Vector3 heading = TargetSphere.transform.position - EndEffector.transform.position;
                float distance = heading.magnitude;
                Vector3 direction = heading.normalized;

                // Calculate the repelling force based on the distance
                float repellingForce = CalculateRepellingForce(distance);

                // Apply the force in the opposite direction of the forceSphere
                Vector3 repellingForceVector = -direction * repellingForce * repellingForceMultiplier;

                // Apply the repelling force to the haptic device
                ApplyForceToHapticDevice(repellingForceVector);
            }
        }

    }

    private void ApplyForceToHapticDevice(Vector3 force)
    {
        dhdSetForce(force.x, force.z, force.y, defaultId);
    }





    private float CalculateRepellingForce(float distance)
    {

        float maxDistanceCalc = maxRepellingforceDistance * distanceThreshold;
        float clampedDistance = Mathf.Clamp(distance, distanceThreshold, maxDistanceCalc);

        // Calculate the normalized force (0 to 1) based on distance
        float normalizedForce = 1.0f - (clampedDistance - distanceThreshold) / (maxDistanceCalc - distanceThreshold);

        // Adjust the normalized force to control the strength of the repelling force
        float repellingForce = normalizedForce * (float)repelForceTest;

        return repellingForce;
    }

    private float CalculateRepellingForceFromForceSphere()
    {
        Vector3 heading = TargetSphere.transform.position - EndEffector.transform.position;
        float distance = heading.magnitude;

        return CalculateRepellingForce(distance);


    }

    private bool IsTargetInRangeOfForceSphere()
    {
        float distanceToForceSphere = Vector3.Distance(TargetSphere.transform.position, forceSphere.transform.position);
        return distanceToForceSphere <= 2.0f;
    }

    private bool IsEndEffectorNearForceSphere()
    {
        float distanceToEndEffector = Vector3.Distance(EndEffector.transform.position, forceSphere.transform.position);
        return distanceToEndEffector <= distanceThreshold; // Set your desired threshold value here
    }

    private Vector3 GetForceSphereDirection()
    {
        return (forceSphere.transform.position - EndEffector.transform.position).normalized;
    }

    private void ProduceRandomForce()
    {
        // Generate random values within the specified area
        float randomX = UnityEngine.Random.Range(-areaSize / 2.0f, areaSize / 2.0f);
        float randomY = UnityEngine.Random.Range(-areaSize / 2.0f, areaSize / 2.0f);
        float randomZ = UnityEngine.Random.Range(-areaSize / 2.0f, areaSize / 2.0f);

        // Create a random direction vector within the area
        Vector3 randomDirection = new Vector3(randomX, randomY, randomZ).normalized;

        // Generate a force in random direction
        Vector3 randomForce = randomDirection * (float)forceTest * 2;

        // Apply the random force
        ApplyForceToHapticDevice(randomForce);

        // Start the coroutine to stop applying the random force after the duration
        if (!applyingRandomForce)
        {
            StartCoroutine(ApplyRandomForceCoroutine());
        }
        applyingRegularForce = false;
    }

    IEnumerator ApplyRandomForceCoroutine()
    {
        applyingRandomForce = true;

        //float forceDuration = UnityEngine.Random.Range(minRandomForceDuration, maxRandomForceDuration); //random duration
        float forceDuration = constantForceDuration; // constant duration

        while (timeSinceRandomForceStart < forceDuration)
        {
            // Wait for the next fixed update
            yield return new WaitForFixedUpdate();

            timeSinceRandomForceStart += Time.fixedDeltaTime;
        }

        // After the random force duration, stop applying the force
        ApplyForceToHapticDevice(Vector3.zero);

        applyingRandomForce = false;
        applyingRegularForce = true;
        timeSinceRandomForceStart = 0.0f;
    }


    // Method to check if the EndEffector is inside the TargetSphereArea
    private bool IsInsideTargetSphereArea()
    {
        Vector3 heading = TargetSphere.transform.position - EndEffector.transform.position;
        float distance = heading.magnitude;
        return distance < distanceThreshold;
    }

    public int DhdOpen()
    {
        // open the first available device
        if (dhdOpen() < 0)
        {
            DeviceStatus = deviceStatus.DELTA_CLOSED;
            IntPtr intPtr = dhdErrorGetLastStr();
            string myErrorString = Marshal.PtrToStringAnsi(intPtr);
            Debug.LogError(String.Format("error: cannot open device {0}\n", myErrorString));
            dhdSleep(2.0);
            return -1;
        }
        else
        {
            Debug.Log(String.Format("Device Succesfully Opened"));
            DeviceStatus = deviceStatus.DELTA_OPEN;
            UpdateDHDStatus();
            return 0;
        }
    }

    private void OnDestroy()
    {
        DhdClose();
    }

    public int DhdClose()
    {
        if (dhdClose(defaultId) < 0)
        {
            IntPtr intPtr = dhdErrorGetLastStr();
            string myErrorString = Marshal.PtrToStringAnsi(intPtr);
            Debug.LogError(String.Format("error: Failed to stop device {0}\n", myErrorString));
            return -1;
        }
        else
        {
            DeviceStatus = deviceStatus.DELTA_CLOSED;
            Debug.Log(String.Format("Device Closed!"));
            return 0;
        }
    }

    private int UpdateDHDStatus()
    {

        if (dhdGetStatus(DHD_STATUS_RESULT, defaultId) < 0)
        {
            dhdStatus.Clear();
            return -1;
        }
        else
        {
            //Debug.Log(String.Format("Succesfully Got Status"));
            dhdStatus.Clear();
            for (int i = 0; i < DHD_MAX_STATUS; i++)
            {
                int currentResult = DHD_STATUS_RESULT[i];
                if (currentResult > 0)
                {
                    dhdStatus.Add((DHD_STATUS_ENUM)i);
                }
            }
            return 0;
        }
    }

    public int GetDHDPosition()
    {
        double px = 0;
        double py = 0;
        double pz = 0;
        if (dhdGetPosition(ref px, ref py, ref pz, defaultId) < 0)
        {
            Debug.LogError("ERROR GETTING POSITION");
            return -1;
        }
        else
        {
            //Debug.Log(String.Format("{0:0.000},{1:0.000},{2:0.000}", px, py, pz));
            DhdPosition = new Vector3((float)px, (float)pz, (float)py);
            return 0;
        }
    }

    public int SetDHDBrake(bool brakeOn)
    {
        int success = -1;

        if (brakeOn)
        {
            dhdEnableForce(new UIntPtr(1), defaultId);
            if (dhdSetBrakes(1, defaultId) < 0)
            {
                Debug.LogError("ERROR TURNING ON BRAKE");
                success = -1;
            }
            else
            {
                success = 0;
            }
        }
        else
        {
            dhdEnableForce(new UIntPtr(0), defaultId);
            if (dhdSetBrakes(0, defaultId) < 0)
            {
                Debug.LogError("ERROR TURNING OFF BRAKE");
                success = -1;
            }
            else
            {
                success = 0;
            }
        }
        UpdateDHDStatus();
        return success;
    }

    public void GetLastDHDError()
    {
        IntPtr intPtr = dhdErrorGetLastStr();
        string myErrorString = Marshal.PtrToStringAnsi(intPtr);
        Debug.LogError(String.Format("Last Error: {0}\n", myErrorString));
    }

    public void SetDHDGravity(double g)
    {
        dhdSetStandardGravity(g, defaultId);
    }
}
*/