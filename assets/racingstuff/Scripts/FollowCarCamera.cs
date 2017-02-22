using UnityEngine;
using System.Collections;

public class FollowCarCamera : MonoBehaviour {

    public GameObject speedoMeter;
    GameObject camObject;
    //GameObject driverView;
    public Transform playerCar;
    int mode = 0, i = 0;
    // Use this for initialization
    void Start()
    {
        camObject = GameObject.FindGameObjectWithTag("MainCamera");
        //  driverView = GameObject.Find("DriverCamera");


    }

    // Update is called once per frame
    void Update()
    {
        //camObject.transform.LookAt(playerCar);
        camObject = GameObject.FindGameObjectWithTag("MainCamera");

        DriverPointOfView();
        NormalView();
       
        if (mode == 1)
        {
            // Cockpit view
            //speedoMeter.SetActive(false);
            
            camObject.transform.position = playerCar.transform.position - 0.1f * playerCar.transform.forward + 0.32f * Vector3.up;
            camObject.transform.forward = playerCar.transform.forward;
        }
        if (mode == 0)
        {
            // behind the car
             //speedoMeter.SetActive(true);
  //          camObject.transform.position = playerCar.transform.position - 10 * playerCar.transform.forward + 2 * Vector3.up;
    //        camObject.transform.forward = playerCar.transform.forward;
        }
    }

   void DriverPointOfView()
    {
        if(mode == 0)
        {
            if (Input.GetKey(KeyCode.V))
            {
                mode = 1;
                 
            }
        }        
    }
    void NormalView()
    {
        if(mode == 1)
        {
            if (Input.GetKey(KeyCode.N))
            {
                mode = 0;
            }
        }        
    }    
}

