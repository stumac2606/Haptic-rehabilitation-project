using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.PlayerLoop;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using Valve.Newtonsoft.Json;
using System.IO;
using System.IO.Enumeration;
using Valve.VR.InteractionSystem;
using System.Runtime.InteropServices;
using System;

public class DataClass
{
    public List<int> frameNum = new List<int>();
    public List<string> userID = new List<string>();
    public List<int> trialNumber = new List<int>();
    public List<string> phase = new List<string>();
    public List<string> force_type= new List<string>();
    public List<float> positional_error = new List<float>();
    /*public List<float> target_velocity = new List<float>();
    public List<float> endEff_velocity = new List<float>();*/

    public List<float> x_target_pos = new List<float>();
    public List<float> y_target_pos = new List<float>();
    public List<float> z_target_pos = new List<float>();

    public List<float> x_user_pos = new List<float>();
    public List<float> y_user_pos = new List<float>();
    public List<float> z_user_pos = new List<float>();

    public List<float> x_pos_error = new List<float>();
    public List<float> y_pos_error = new List<float>();
    public List<float> z_pos_error = new List<float>();

    /*public List<float> velocity_error= new List<float>();*/

    /* public List<float> x_vel_error = new List<float>();
     public List<float> y_vel_error = new List<float>();
     public List<float> z_vel_error = new List<float>();*/

    //public List<int> force_active = new List<int>();
    public List<float> time = new List<float>();
}

public class ExperimentManager : MonoBehaviour
{
    [DllImport("dhd64.dll")]
    extern static int dhdGetButton(int index, IntPtr ID);

    [DllImport("dhd64.dll")]
    extern static int dhdEnableForce(byte val, IntPtr id);

    const byte DHD_ON = 1;
    const byte DHD_OFF = 0;

    private DataClass dataClass = new DataClass(); 

    public int userID; 
    public TMPro.TMP_Text infoText;
    public int numberOfBaselineTrials = 20;
    public int numberOfTrainingTrials = 20;
    public int numberOfTestingTrials = 20;
    public float trialDuration = 15f; 
    public DLLImportTest dllImpTest; 

    Coroutine ExperimentSequence, SaveFileRoutine;
    public bool buttonNotPressed = false;
    private bool recording = false;

    public Transform endEffectorSphere;
    public Transform targetSphere;
    int frame = 0;
    int trialNum = 0;
    string phase = "";
    string forceType = "";

    private Vector3 prev_target_position;
    private Vector3 prev_endEff_position;

    int buttonStatus; 

    private void Start()
    {
        /*prev_target_position = targetSphere.transform.position;
        prev_endEff_position = endEffectorSphere.transform.position;*/
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S)) 
        {
            if(ExperimentSequence != null)
            {
                StopCoroutine(ExperimentSequence);
            }
            ExperimentSequence = StartCoroutine(ExperimentRoutine());

        }

        //buttonNotPressed = false;

        // Reference the delta robot button to set it to true for a moment
        buttonStatus = dhdGetButton(0, new IntPtr(1));
        if (buttonStatus == 1)
        {
            buttonNotPressed = true; 
        }
    }


    //public List<int> frameNum = new List<int>();
    //public List<string> gameObjectName = new List<string>();
    //public List<float> positional_error = new List<float>();
    //public List<float> velocity_error = new List<float>();
    //public List<int> force_active = new List<int>();
    //public List<float> time = new List<float>();


    void FixedUpdate()
    {
        // Recoding the motion data and performance calculations 
        float target_x_pos = targetSphere.position.x;
        float target_y_pos = targetSphere.position.y;
        float target_z_pos = targetSphere.position.z;

        float user_x_pos = endEffectorSphere.position.x;
        float user_y_pos = endEffectorSphere.position.y;
        float user_z_pos = endEffectorSphere.position.z;

        float posError = Vector3.Distance(targetSphere.position, endEffectorSphere.position);
        float x_pos_err = targetSphere.position.x - endEffectorSphere.position.x;
        float y_pos_err = targetSphere.position.y - endEffectorSphere.position.y;
        float z_pos_err = targetSphere.position.z - endEffectorSphere.position.z;

        /*float target_velocity = calaculateVelocity(targetSphere, prev_target_position);
        float endEff_velocity = calaculateVelocity(endEffectorSphere, prev_endEff_position);*/
        prev_target_position = targetSphere.transform.position;
        prev_endEff_position = endEffectorSphere.transform.position;

        /*float velocityError = target_velocity - endEff_velocity;*/

        //Debug.Log("velocity " + target_velocity);


        //float velocityErrorMag = velocityError.magnitude;



        /*float x_vel_error = velocityError.x;
        float y_vel_error = velocityError.y;
        float z_vel_error = velocityError.z;*/

        if (recording)
        {
            
            dataClass.frameNum.Add(frame);
            dataClass.positional_error.Add(posError); //  Summary of positioanl error 

            /*dataClass.target_velocity.Add(target_velocity); // "" "" velocity error 
            dataClass.endEff_velocity.Add(endEff_velocity);
            dataClass.velocity_error.Add(velocityError);*/

            dataClass.trialNumber.Add(trialNum); // "" "" velocity error 
            dataClass.phase.Add(phase); // "" "" velocity error 
            dataClass.userID.Add("User_" + userID); // "" "" velocity error 
            dataClass.force_type.Add(forceType);

            dataClass.x_target_pos.Add(target_x_pos);
            dataClass.y_target_pos.Add(target_y_pos);
            dataClass.z_target_pos.Add(target_z_pos);

            dataClass.x_user_pos.Add(user_x_pos);
            dataClass.y_user_pos.Add(user_y_pos);
            dataClass.z_user_pos.Add(user_z_pos);

            dataClass.x_pos_error.Add(x_pos_err);
            dataClass.y_pos_error.Add(y_pos_err);
            dataClass.z_pos_error.Add(z_pos_err);

            dataClass.time.Add(Time.time);

            frame++; 
        }
    }

    private float calaculateVelocity(Transform movement, Vector3 prev_pos)
    {
        float velocity = 0;

        Vector3 currentPos = movement.transform.position; // this is position at frame x

        Vector3 vel_displacment = currentPos - prev_pos; 

        float deltaTime = Time.deltaTime;

        velocity = vel_displacment.magnitude / deltaTime;     

        return velocity; 
    }

    void SaveFileNow(string fileInfo)
    {
        if (SaveFileRoutine != null)
        {
            StopCoroutine(SaveFileRoutine);
        }
        SaveFileRoutine = StartCoroutine(SaveFile(fileInfo));
        frame = 0;
    }

    IEnumerator ExperimentRoutine()
    {
        // Intro/Welcome message
        /*infoText.text = "Welcome to this study. \n Please follow these instructions."; yield return new WaitForSeconds(5);
        infoText.text = "Please take a moment to look around the room to get \n comfortable with your virtual surroundings. \n press the button when you're ready to continue";
        if (buttonStatus == 1)
        {
            buttonNotPressed = true;
        }
        while (!buttonNotPressed)
        {
            yield return null;
        }
        infoText.text = "Someone should have explained how \n you will use the haptic device \n postioned infront of you "; yield return new WaitForSeconds(5);
        buttonNotPressed = false;
        infoText.text = "If you have any further questions or \n conecerns about the haptic device \n please ask, if not press the \n button to continue "; 
        
        if (buttonStatus == 1)
        {
            buttonNotPressed = true;
        }
        while (!buttonNotPressed)
        {
            yield return null;
        }
        
        
        infoText.text = "The white ball position correlates \n with the postion of the ball on \n the haptic device "; yield return new WaitForSeconds(5);
        infoText.text = "The white ball will turn green when it comes into \n close proximity of the target  "; yield return new WaitForSeconds(5);
        buttonNotPressed = false;
        infoText.text = "The blue transparent sphere is your target,\n Your task is to move the white ball as \n close to the target sphere as possible, \n using the haptic device "; yield return new WaitForSeconds(5);
        infoText.text = "Both objects are located \n above this text "; yield return new WaitForSeconds(5);
        infoText.text = "You will perform this task " + numberOfBaselineTrials + " times \n for " + trialDuration + "s, with \n 3s breaks betwen trials  "; yield return new WaitForSeconds(5);
        infoText.text = "The study will be split into 3 phases - \n baseline, training and testing phase"; yield return new WaitForSeconds(5);
        infoText.text = "You can take a break inbetween phases \n until you are ready for the next phase ";
        infoText.text = "There will be no forces applied in the \n baseline or training phases, however the training \n phase may have a repelling force applied "; yield return new WaitForSeconds(5);
       */ infoText.text = "If you have any questions, please ask.\n If you are ready to begin, please press \n the button on the haptic device ";
        
        if (buttonStatus == 1)
        {
            buttonNotPressed = true;
        }
        while (!buttonNotPressed)
        {
            yield return null;
        }
        
        
        infoText.text = null;

        // Start baseline (pre-test) routine
        for (int i = 0; i < numberOfBaselineTrials; i++) 
        {
            buttonNotPressed = false;
            dhdEnableForce(DHD_ON, new IntPtr(1));
            Debug.Log("Running baseline trial: " + i);
            // Here you send out a command to the DLLImportTest.cs script to run one trial 
            
            dllImpTest.RunTrial(-1, trialDuration);
            recording = true;
            phase = "Baseline";
            trialNum = i;
            forceType = "";
            dllImpTest.movement = true;
            yield return new WaitForSeconds(trialDuration);
            recording = false;
            SaveFileNow("ID_" + userID + "_" + "Phase_Baseline_" + "Trial_0" + i);

            dllImpTest.RunTrial(-2, trialDuration);

            dllImpTest.movement = false; 
            infoText.text = "Please wait: 3";
            yield return new WaitForSeconds(1);
            infoText.text = "Please wait: 2";
            yield return new WaitForSeconds(1);
            infoText.text = "Please wait: 1";
            yield return new WaitForSeconds(1);
            infoText.text = null;

        }

        infoText.text = "Press the button to continue to the next phase \n warning: forces may apply immediatley \n after button push";

        if (buttonStatus == 1)
        {
            buttonNotPressed = true;
        }
        while(!buttonNotPressed)
        {
            yield return null; 
        }
        buttonNotPressed = false;
        infoText.text = null;

        // Start training (w/o forces) 
        for (int i = 0; i < numberOfTrainingTrials; i++)
        {
            Debug.Log("Running training trial: " + i);

            recording = true;
            phase = "Training";
            trialNum = i;
            string force_trial = "";
            if (userID % 3 == 0) // Depending on odd/even user id number, either initially use attractive (1) or no forces (-1)
            {
                dllImpTest.RunTrial(0, trialDuration);
                Debug.Log("Even user, repelling forces in training!");
                force_trial = "Repelling_Forces";
                forceType = "Repelling";
            }

            else if (userID % 3 == 1)
            {
                dllImpTest.RunTrial(-1, trialDuration);
                Debug.Log("Odd user! i.e. no forces in training!");
                force_trial = "No_Force";
                forceType = "No_Force";
            }
            else
            {
                dllImpTest.RunTrial(1, trialDuration);
                Debug.Log("Odd user! i.e. no forces in training!");
                force_trial = "No_Force";
                forceType = "No_Force";
            }
            dllImpTest.movement = true;
            yield return new WaitForSeconds(trialDuration);
            recording = false;
            SaveFileNow("ID_" + userID + "_" + "_Phase_Training_" + force_trial + "_Trial_0" + i);

            dllImpTest.RunTrial(-2, trialDuration);

            dllImpTest.movement = false;
            infoText.text = "Please wait: 3";
            yield return new WaitForSeconds(1);
            infoText.text = "Please wait: 2";
            yield return new WaitForSeconds(1);
            infoText.text = "Please wait: 1";
            yield return new WaitForSeconds(1);
            infoText.text = null;
            buttonNotPressed = false;

        }

        infoText.text = "Press button to continue to the next phase \n forces will be turned off ";

        if (buttonStatus == 1)
        {
            buttonNotPressed = true;
        }
        while (!buttonNotPressed)
        {
            yield return null;
        }
        buttonNotPressed = false;
        infoText.text = null;

        // Test (no forces present) to look at tracking performance (accuracy/speed etc) 
        for (int i = 0; i < numberOfTestingTrials; i++)
        {
            Debug.Log("Running testing trial: " + i);
            dllImpTest.RunTrial(-1, trialDuration);
            recording = true;
            phase = "Testing";
            trialNum = i;
            forceType = "";
            dllImpTest.movement = true;
            yield return new WaitForSeconds(trialDuration);
            recording = false;
            SaveFileNow("ID_" + userID + "_" + "Phase_Test_" + "Trial_0" + i);

            dllImpTest.RunTrial(-2, trialDuration);

            dllImpTest.movement = false;
            infoText.text = "Please wait: 3";
            yield return new WaitForSeconds(1);
            infoText.text = "Please wait: 2";
            yield return new WaitForSeconds(1);
            infoText.text = "Please wait: 1";
            yield return new WaitForSeconds(1);
            infoText.text = null;
            buttonNotPressed = false;
        }


        infoText.text = "Study Complete. Thanks for your time.";


        yield return null;
    }


    IEnumerator SaveFile(string fileInfo)
    {
        string path = "C:\\Users\\StuartM\\Downloads\\MscProject-main\\MscProject-main\\MSC Project\\ForceD unity demo\\Data";
        string fileName = fileInfo + ".json";

        string jsonString = JsonConvert.SerializeObject(dataClass, Formatting.Indented);
        File.WriteAllText(path + "/" + fileName, jsonString);

        yield return new WaitForSecondsRealtime(0.5f);

        dataClass = new DataClass();

        yield return null; 
    }

}
