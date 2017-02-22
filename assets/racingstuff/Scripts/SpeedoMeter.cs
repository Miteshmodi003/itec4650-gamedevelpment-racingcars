using UnityEngine;
using System.Collections;

public class SpeedoMeter : MonoBehaviour
{

    //Speedometer GUI
    public RectTransform needle;
    public float minNeedleAngle = -20.0f;
    public float maxNeedleAngle = 160.0f;
    public float rotationMultiplier = 0.85f;
    private float needleRotation;
    GameObject player;

    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (needle)
        {
            float fraction = 0;
            if (player.GetComponent<Car_Controller>())
            {
                fraction = player.GetComponent<Car_Controller>().currentSpeed / maxNeedleAngle;
            }
            needleRotation = Mathf.Lerp(minNeedleAngle, maxNeedleAngle, (fraction * rotationMultiplier));
            // need to make needleRotation negative on z-axis in order to achieve the needle rotation from 0 mph onwards
            needle.transform.eulerAngles = new Vector3(0, 0, -needleRotation);
            //  print(needle.transform.eulerAngles);
        }
       
    }
}
