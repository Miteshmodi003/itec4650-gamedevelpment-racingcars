using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Car_Controller))]
public class AiController : MonoBehaviour {

    public Transform target;
    [Range(0, 0.5f)]
    public float steerSensitivity;
    private Car_Controller controller;

	void Start () {
        controller = GetComponent<Car_Controller>();
        
	}
	
	void Update () {

        //Find the progress target
        if (!target){
            target = GetComponent<ProgressTracker>().target;
        }

        //Naviagte the Ai
        Navigate();
	}

    void Navigate()
    {
        //No logic added here, just step on it and steer correctly!
        if (!target) return;

        //Get steer verctor
        Vector3 steerVector = transform.InverseTransformPoint(target.position);
        float targetAngle = Mathf.Atan2(steerVector.x, steerVector.z) * Mathf.Rad2Deg;
        float newSteer = Mathf.Clamp(targetAngle * steerSensitivity, -1, 1) * Mathf.Sign(controller.currentSpeed);

        controller.motorInput = 1.0f; //accelerate
        controller.steerInput = newSteer;
    }
}
