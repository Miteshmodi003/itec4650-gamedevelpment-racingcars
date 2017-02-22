using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class LapTracker : MonoBehaviour
{
    public Transform[] checkPointArray;
    public int position;
    public int lap = 0;
    public int currentCheckpoint = 0;
    public int totalCheckpoints;
    private int checkpointIndex;
    private List<Transform> path = new List<Transform>();


    void Start()
    {
        lap = 1;

        //Get the path object to find total checkpoints
        Transform pathContainer = RaceManager.instance.pathContainer;
        Transform[] nodes = pathContainer.GetComponentsInChildren<Transform>();
        foreach (Transform p in nodes)
        {

            if (p != pathContainer)
            {
                path.Add(p);
            }
        }
        totalCheckpoints = nodes.Length - 1;
    }

    // Update is called once per frame
    void Update()
    {
        Transform node = path[checkpointIndex] as Transform;
        Vector3 nodeVector = GetComponent<ProgressTracker>().target.InverseTransformPoint(node.position);

        //register that we have passed this node
        if (nodeVector.magnitude <= 10)
        {
            
            checkpointIndex++;
            currentCheckpoint++;
            
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //When triggers Finish line
        if (other.tag == "FinishLine")
        {
            if (currentCheckpoint > totalCheckpoints - 1)
            {
                NewLap();
            }
        }
    }

    public void NewLap()
    {
       
        //handle new lap stuff
        if (lap < RaceManager.instance.totalLaps)
        {
            //increment lap
            lap++;

            //clear checkpoints
            currentCheckpoint = 0;
            checkpointIndex = 0;
        }

        //we finish the race after we've done all laps
        else {
            RaceManager.instance.raceCompleted = true;
            EndRace();
        }
    }

    void EndRace()
    {
        //Disable control
        GetComponent<Car_Controller>().controllable = false;

        //Update finish text
        if(tag == "Player")
            RaceManager.instance.finish.text = "Finished!";
    }

}
