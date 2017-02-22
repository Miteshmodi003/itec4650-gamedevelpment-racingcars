using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Car_Controller : MonoBehaviour {

    //GameObject speedometer; 

	public enum Propulsion{RWD,FWD}
	
	public Propulsion _propulsion = Propulsion.RWD;
    int speedMode = 0;
    
    [Header("Wheel Transforms")]
    public Transform FL_Wheel;
	public Transform FR_Wheel;
	public Transform RL_Wheel;
	public Transform RR_Wheel;
	private List<Transform> wheeltransformList = new List<Transform>();

    [Header("Wheel Colliders")]
    public WheelCollider FL_WheelCollider;
	public WheelCollider FR_WheelCollider;
	public WheelCollider RL_WheelCollider;
	public WheelCollider RR_WheelCollider;
	private List<WheelCollider> wheelcolliderList = new List<WheelCollider>();

    [Header("Engine Velues")]
    public float engineTorque = 800.0f; //avoid setting this value too high inorder to not overpower the wheels!
	public float brakeTorque = 20000.0f; //force applied to wheels to stop the car
	public float maxSteerAngle = 30.0f;
	public float topSpeed = 150.0f;
	public float boost = 100.0f;
	public float currentSpeed;
	private float currentSteerAngle;

    //Speed text
    Text speedText;

    //Gear values
    [Header("Gear Velues")]
    public int numberOfGears = 6;
	public int currentGear;
	private float[] gearRatio;

    //Stability
    [Header("Stability Velues")]
    private Rigidbody rigid;
	public Vector3 centerOfMass;
	public float antiRollAmount = 8000.0f;
	public float downforce = 50.0f;
	[Range(0,1)]public float steerHelper = 0.5f;

    //Sound
    [Header("Sound Values")]
    public AudioSource engineAudioSource;
	public AudioSource nitroAudioSource;
	public AudioClip engineSound;
	public AudioClip nitroSound;

    //Bools
    [HideInInspector]
    public bool controllable;
    [HideInInspector]
    public bool reversing;
    [HideInInspector]
    public bool nitro;
    [HideInInspector]
    public bool usingNitro;

    //Misc
    [Header("Misc Values")]
    public GameObject steeringWheel;
    public GameObject brakelightGroup;
    public float brakeForce = 2500.0f; //force added to stop the car
	private float speedLimit;
	private float impactForce = 5.0f; //the impact force required to play a collision sound
	private Vector3 velocityDir;
	private float currentRotation;
	private float drag = 0.0f;
	
	//Input values
	[HideInInspector]
    public float motorInput;
    [HideInInspector]
    public float brakeInput;
    [HideInInspector]
    public float steerInput;

	//TextMesh
	public TextMesh speedText3D;
     
	
	void Start () {
		Initialize();
	}
	
	void Initialize(){
        UnityEngine.VR.InputTracking.Recenter();
       //   speedoMeter.SetActive(false);
        rigid = GetComponent<Rigidbody>();
		rigid.centerOfMass = centerOfMass;
		//rigid.interpolation = RigidbodyInterpolation.None;

		//Find 3dText GameObject Component --Lets just assign it via the inspector
		//speedText3D = GameObject.Find("3DSpeedText").GetComponent<TextMesh>();      

        //Find White text to be displayed during game
        speedText = GameObject.Find("SpeedText").GetComponent<Text>();

		//Add wheel transfoms/colliders to the lists
		wheeltransformList.Add(FL_Wheel);
        wheelcolliderList.Add(FL_WheelCollider);
		wheeltransformList.Add(FR_Wheel);
        wheelcolliderList.Add(FR_WheelCollider);
		wheeltransformList.Add(RL_Wheel);
        wheelcolliderList.Add(RL_WheelCollider);
		wheeltransformList.Add(RR_Wheel);
        wheelcolliderList.Add(RR_WheelCollider);
		
		//Calculate gearRatio
		gearRatio = new float[numberOfGears];
		for(int i = 0; i < numberOfGears; i++){
			gearRatio[i] = Mathf.Lerp(0, topSpeed, ((float)i/(float)(numberOfGears)));
		}
		gearRatio[numberOfGears-1] = topSpeed + 50.0f; //ensure the last gear doesn't exceed topSpeed!

        // Find Speedometer
        //speedometer = GameObject.FindGameObjectWithTag("Speedometer");
		
        //set up audio
		if(engineAudioSource){
			engineAudioSource.clip = engineSound;
			engineAudioSource.loop = true;
			engineAudioSource.spatialBlend = 1.0f;
			engineAudioSource.minDistance = 5.0f;
			engineAudioSource.Play();
		}
		
		if(nitroAudioSource){
			nitroAudioSource.clip = nitroSound;
			nitroAudioSource.loop = true;
			nitroAudioSource.spatialBlend = 1.0f;
			nitroAudioSource.minDistance = 5.0f;
		}
		
			
		//set controllable to false
		if(controllable && RaceManager.instance)
			controllable = false;
        
	}
	
	
	
	void Update(){
		ShiftGears();
        if(Input.GetButtonDown("Jump"))
        {
            UnityEngine.VR.InputTracking.Recenter();
        }
	}
	
	
	
	void FixedUpdate () {
		
		if(controllable){
			Drive();
			Brake();
			MostCurrentSpeed ();
		}
		else{
			Stop();
		}
		
		WheelAllignment();
		ApplyDownforce();
		StabilizerBars();
	}
	
	
	
	void Drive(){
		
		
		switch(_propulsion){
			
		//Rear wheel drive
		case Propulsion.RWD : 
			
			if(currentSpeed <= speedLimit){
				if(!reversing){
					RL_WheelCollider.motorTorque = engineTorque * motorInput;
					RR_WheelCollider.motorTorque = engineTorque * motorInput;
				}
				else{
					RL_WheelCollider.motorTorque = engineTorque * -brakeInput;
					RR_WheelCollider.motorTorque = engineTorque * -brakeInput;
				}
			}
			else{
				rigid.velocity = (speedLimit/2.237f) * rigid.velocity.normalized;
				RL_WheelCollider.motorTorque = 0;
				RR_WheelCollider.motorTorque = 0;
			}
			
			break;
			
		//Front wheel drive
		case Propulsion.FWD :
			
			if(currentSpeed <= speedLimit){
				if(!reversing){
					FL_WheelCollider.motorTorque = engineTorque * motorInput;
					FR_WheelCollider.motorTorque = engineTorque * motorInput;
				}
				else{
					FL_WheelCollider.motorTorque = engineTorque * -brakeInput;
					FR_WheelCollider.motorTorque = engineTorque * -brakeInput;
				}
			}
			else{
				rigid.velocity = (speedLimit/2.237f) * rigid.velocity.normalized;
				FL_WheelCollider.motorTorque = 0;
				FR_WheelCollider.motorTorque = 0;
			}
			
			break;
		}

        //Boost
        if (Input.GetKey(KeyCode.Y))
        {
           // if (currentSpeed < speedLimit && motorInput > 0)
            //{
             //   print("Before Boost: " + currentSpeed );
                rigid.AddForce(transform.forward * boost);
              //  print("After Boost " + currentSpeed);
            //}
        }
		
		
		//Steering
		//Reduce our steer angle depending on how fast the car is moving
		currentSteerAngle = Mathf.Lerp(maxSteerAngle,(maxSteerAngle/2),(currentSpeed/(topSpeed * 2.0f)));
		
		FL_WheelCollider.steerAngle = Mathf.Clamp((currentSteerAngle * steerInput), -maxSteerAngle, maxSteerAngle);
		FR_WheelCollider.steerAngle = Mathf.Clamp((currentSteerAngle * steerInput), -maxSteerAngle, maxSteerAngle);
		
		SteerHelper();
		
		//calculate speed & drag values
		currentSpeed = CalculateSpeed();
		rigid.drag = CalculateDrag();
		
		//Check for reverse
		velocityDir = transform.InverseTransformDirection(rigid.velocity);
		
		if(brakeInput > 0  && velocityDir.z <= 0.01f){
			reversing = true;
			speedLimit = 25.0f;
		}
		else{
			reversing = false;
			speedLimit = topSpeed;
		}

		
		//Handle steering wheel rotation
		if(steeringWheel)
			steeringWheel.transform.rotation = transform.rotation * Quaternion.Euler( 0, 0, (FR_WheelCollider.steerAngle) * -2.0f);
		
		//Handle Nito 
		if(nitro || Input.GetKey(KeyCode.B))
            Nitroboost();
	}
	
	void Brake(){
		//Footbrake
		if(!reversing && brakeInput > 0.0f){
			
			//add a backward force to help stop the car
			rigid.AddForce(-transform.forward * brakeForce);
			
			if(_propulsion == Propulsion.RWD){
				RL_WheelCollider.brakeTorque = brakeTorque * brakeInput;
				RR_WheelCollider.brakeTorque = brakeTorque * brakeInput;
				RL_WheelCollider.motorTorque = 0;
				RR_WheelCollider.motorTorque = 0;
			}
			else{
				FL_WheelCollider.brakeTorque = brakeTorque * brakeInput;
				FR_WheelCollider.brakeTorque = brakeTorque * brakeInput;
				FL_WheelCollider.motorTorque = 0;
				FR_WheelCollider.motorTorque = 0;
			}
		}
		
		else{
				RL_WheelCollider.brakeTorque = 0;
				RR_WheelCollider.brakeTorque = 0;
				FL_WheelCollider.brakeTorque = 0;
				FR_WheelCollider.brakeTorque = 0;
		}
		
		//Decelerate
		if(motorInput == 0 && brakeInput == 0 && rigid.velocity.magnitude > 1.0f){
			if(velocityDir.z >= 0.01f)
				rigid.AddForce(-transform.forward * 250);
			else
				rigid.AddForce(transform.forward * 250);
		}
		
		//Activate brake lights if braking
		if(brakelightGroup)
			brakelightGroup.SetActive(brakeInput > 0);
	}   
        
    private float CalculateSpeed(){
		//Calculate currentSpeed(MPH)
		currentSpeed = rigid.velocity.magnitude * 2.237f;
		//Round currentSpeed
		currentSpeed = Mathf.Round(currentSpeed);
		//Never return a negative value
		return Mathf.Abs(currentSpeed);
	}

	private void MostCurrentSpeed(){
        
        if(speedMode == 0)
        {
            speedText3D.text = currentSpeed + " MPH";
            speedText.text = currentSpeed + " MPH";

        }
        
        if(speedMode == 1)
        {
            Mathf.Abs(currentSpeed * 1.61f);
            speedText3D.text = Mathf.Round(currentSpeed * 1.61f) + " KPH";
            speedText.text = Mathf.Round(currentSpeed * 1.61f) + " KPH";

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
	
	private float CalculateDrag(){
		//increase drag as the car gains speed
		drag = (currentSpeed/topSpeed) / 10;
		return Mathf.Clamp(drag, 0.0f, 0.1f);
	}
	
	
	void ShiftGears(){
		int i;
		
		for (i = 0; i < gearRatio.Length; i ++ ) {
			if ( currentSpeed < gearRatio[i]) {
				break;
			}
			currentGear = i + 1;
		}
		
		float minimumGearValue = 0.0f;
		float maximumGearValue = 0.0f;
		
		if (i == 0){
			minimumGearValue = 0;
		}
		else {
			minimumGearValue = gearRatio[i-1];
		}
		maximumGearValue = gearRatio[i];
		
		
		//engineAudioSource.pitch = ((currentSpeed + minimumGearValue)/(maximumGearValue  + minimumGearValue)) + 1.0f;
		//engineAudioSource.volume = Mathf.Lerp (engineAudioSource.volume, Mathf.Clamp (Mathf.Abs(motorInput), 0.5f, 0.8f), Time.deltaTime *  5);
	}
	
	
	void WheelAllignment(){ 
		
		for(int i = 0; i < wheelcolliderList.Count; i++){
			
			Quaternion rot;
			Vector3 pos;
			
			wheelcolliderList[i].GetWorldPose(out pos, out rot);
			
			//Set rotation & position of the wheels
			wheeltransformList[i].rotation = rot;
			wheeltransformList[i].position = pos; 
			
		}
	}
	
	
	public void StabilizerBars (){
		
		WheelHit FrontWheelHit;
		
		float travelFL = 1.0f;
		float travelFR = 1.0f;
		
		bool groundedFL= FL_WheelCollider.GetGroundHit(out FrontWheelHit);
		
		if (groundedFL)
			travelFL = (-FL_WheelCollider.transform.InverseTransformPoint(FrontWheelHit.point).y - FL_WheelCollider.radius) / FL_WheelCollider.suspensionDistance;
		
		bool groundedFR= FR_WheelCollider.GetGroundHit(out FrontWheelHit);
		
		if (groundedFR)
			travelFR = (-FR_WheelCollider.transform.InverseTransformPoint(FrontWheelHit.point).y - FR_WheelCollider.radius) / FR_WheelCollider.suspensionDistance;
		
		float antiRollForceFront= (travelFL - travelFR) * antiRollAmount;
		
		if (groundedFL)
			rigid.AddForceAtPosition(FL_WheelCollider.transform.up * -antiRollForceFront, FL_WheelCollider.transform.position); 
		if (groundedFR)
			rigid.AddForceAtPosition(FR_WheelCollider.transform.up * antiRollForceFront, FR_WheelCollider.transform.position); 
		
		WheelHit RearWheelHit;
		
		float travelRL = 1.0f;
		float travelRR = 1.0f;
		
		bool groundedRL= RL_WheelCollider.GetGroundHit(out RearWheelHit);
		
		if (groundedRL)
			travelRL = (-RL_WheelCollider.transform.InverseTransformPoint(RearWheelHit.point).y - RL_WheelCollider.radius) / RL_WheelCollider.suspensionDistance;
		
		bool groundedRR= RR_WheelCollider.GetGroundHit(out RearWheelHit);
		
		if (groundedRR)
			travelRR = (-RR_WheelCollider.transform.InverseTransformPoint(RearWheelHit.point).y - RR_WheelCollider.radius) / RR_WheelCollider.suspensionDistance;
		
		float antiRollForceRear= (travelRL - travelRR) * antiRollAmount;
		
		if (groundedRL)
			rigid.AddForceAtPosition(RL_WheelCollider.transform.up * -antiRollForceRear, RL_WheelCollider.transform.position); 
		if (groundedRR)
			rigid.AddForceAtPosition(RR_WheelCollider.transform.up * antiRollForceRear, RR_WheelCollider.transform.position);
		
		if (groundedRR && groundedRL && currentSpeed > 5.0f)
			rigid.AddRelativeTorque((Vector3.up * (steerInput)) * 5000f);
		
	}
	
	
	void ApplyDownforce(){
		rigid.AddForce(-transform.up*downforce*rigid.velocity.magnitude);
	}
	

	void SteerHelper(){
		
		for (int i = 0; i < 4; i++){
			WheelHit wheelhit;
			wheelcolliderList[i].GetGroundHit(out wheelhit);
			if (wheelhit.normal == Vector3.zero)
				return;
		}
		
		if (Mathf.Abs(currentRotation - transform.eulerAngles.y) < 10f){
			var turnadjust = (transform.eulerAngles.y - currentRotation) * (steerHelper / 2);
			Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
				rigid.velocity = velRotation * rigid.velocity;
		}
	
		currentRotation = transform.eulerAngles.y;
	}

	public void Stop(){
		
		motorInput = 0.0f;
		steerInput = 0.0f;
		currentSpeed = CalculateSpeed();
		
		FL_WheelCollider.motorTorque = 0;
		FR_WheelCollider.motorTorque = 0;
		RL_WheelCollider.motorTorque = 0;
		RR_WheelCollider.motorTorque = 0;
		
		RL_WheelCollider.brakeTorque = brakeTorque;
		RR_WheelCollider.brakeTorque = brakeTorque;
		FL_WheelCollider.brakeTorque = brakeTorque;
		FR_WheelCollider.brakeTorque = brakeTorque;
	}

    void Nitroboost()
    {
        rigid.AddForce(transform.forward * engineTorque);
    }
}
