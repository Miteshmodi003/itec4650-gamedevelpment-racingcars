//PlayerControl.cs handles user input to control the car

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    public enum InputTypes { Desktop, Mobile, Automatic }
    public enum SteerType { TiltToSteer, TouchSteer }
    private Car_Controller car_controller;

    public InputTypes inputType = InputTypes.Automatic;

     
    public SteerType steerType = SteerType.TiltToSteer;
    
    public float distanceTravelled;
    Vector3 startPosition;
    public float timeSinceStarted;
    public static float avgSpeed;
    float hours, minutes, seconds, milliSeconds;
    int speedMode = 0;
    public static Text avgSpeedText;

    void Awake()
    {
        if (GetComponent<Car_Controller>())
            car_controller = GetComponent<Car_Controller>();
    }

    void Start()
    {
        startPosition = transform.position;
        //Find Avg Speed GameObject Component
        avgSpeedText = GameObject.Find("AvgSpeed").GetComponent<Text>();

    }

    

    void Update()
    {
        switch (inputType)
        {
            case InputTypes.Desktop:
                DesktopControl();
                break;

            case InputTypes.Automatic:
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
                DesktopControl();
#endif
                break;
        }
        distanceTravelled += Vector3.Distance(transform.position, startPosition);
        startPosition = transform.position;
        timeSinceStarted = Time.time;
        avgSpeed = Mathf.Abs(((distanceTravelled / timeSinceStarted) * 3600.0f) / 1000.0f);
        float timer = Time.timeSinceLevelLoad;
        hours = (int)(timer / 3600f);
        minutes = (int)(timer / 60f);
        seconds = (int)(timer % 60);
        //milliSeconds = Mathf.Round(timer * 1000);

        if (speedMode == 1)
        {
            avgSpeedText.text = "Average Speed: " + Mathf.Round(avgSpeed) + " KPH";
        }
        if (speedMode == 0)
        {
            avgSpeedText.text = "Average Speed: " + Mathf.Round(avgSpeed / 1.61f) + " MPH";
        }

        if (Input.GetKey(KeyCode.K))
        {
            speedMode = 1;
        }
        else if (Input.GetKey(KeyCode.M))
        {
            speedMode = 0;
        }
    }

    void DesktopControl()
    {

        //send inputs
        SendInputs(Mathf.Clamp01(Input.GetAxis("Vertical")), Mathf.Clamp01(-Input.GetAxis("Vertical")), Mathf.Clamp(Input.GetAxis("Horizontal"), -1, 1), Input.GetKey(KeyCode.LeftShift));

        if (Input.GetKey(KeyCode.H))
        {
            car_controller.Stop();
        }
    }

    void SendInputs(float accel, float brake, float steer, bool nitro)
    {
        if (car_controller)
        {
            car_controller.motorInput = accel;
            car_controller.brakeInput = brake;
            car_controller.steerInput = steer;
            car_controller.usingNitro = nitro;
        }
    }
}
