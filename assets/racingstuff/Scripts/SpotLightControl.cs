using UnityEngine;
using System.Collections;

public class SpotLightControl : MonoBehaviour {

    public GameObject spotLight;
    public Transform playerCar;
    public Transform steeringWheel;
    GameObject car;
    // Use this for initialization
    void Start () {
        car = GameObject.FindGameObjectWithTag("Player");
        //spotLight = GameObject.FindGameObjectWithTag("SpotLight");
      //  steeringWheel = car.transform.FindChild("SteeringWheel");       
	}
	
	// Update is called once per frame
	void Update () {
     
        //spotLight.transform.LookAt(steeringWheel);
        spotLight.transform.position = car.transform.position - 0.3f * car.transform.forward + 0.1f * Vector3.up;
        spotLight.transform.forward = car.transform.forward;
        spotLight.transform.LookAt(steeringWheel);
    }
}
