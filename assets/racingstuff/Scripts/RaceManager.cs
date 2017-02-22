//Race_Manager.cs handles the race logic - countdown, spawning cars, asigning racer names, checking race status, formatting time strings etc */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{

    public static RaceManager instance;
    public int totalLaps = 3;
    public int countdownFrom = 3;
    private int currentCountdownTime;
    public float raceDistance; //Your race track's distance.
    public float countdownDelay = 3.0f;
    private float countdownTimer = 1.0f;
    private float raceTime;
    public Transform pathContainer;
    private bool startCountdown;
    public bool raceStarted; //has the race began
    public bool raceCompleted; //has the player car finished the race
    public bool racePaused; //is the game paused

    public Text countdown;
    public Text finish;
    public Text postion;
    public Text lap;
    public Text timeElapsed;

    private LapTracker player;

    void Awake()
    {
        //create an instance
        instance = this;
    }

    void Start()
    {
        InitializeRace();

        //find the player
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<LapTracker>();
    }

    void InitializeRace()
    {
        ConfigureNodes();
        RankManager.instance.RefreshRacerCount();
        StartCoroutine(Countdown(countdownDelay));
    }


    public IEnumerator Countdown(float delay)
    {

        //wait for (countdown delay) seconds
        yield return new WaitForSeconds(delay);

        //set total countdown time
        currentCountdownTime = countdownFrom + 1;
        startCountdown = true;

        while (startCountdown == true)
        {

            countdownTimer -= Time.deltaTime;

            if (currentCountdownTime >= 1)
            {
                if (countdownTimer < 0.01f)
                {
                    currentCountdownTime -= 1;
                    countdownTimer = 1;
                    if (currentCountdownTime > 0)
                    {
                        countdown.text = currentCountdownTime.ToString();

                    }
                }
            }
            else {
                //Display GO! and call StartRace();
                startCountdown = false;
                countdown.text = "G0!";

                StartRace();

                //Wait for 1 second and hide the text.
                yield return new WaitForSeconds(1);
                countdown.text = "";
            }

            yield return null;
        }
    }


    void Update()
    {
        //Pause the race with "Escape".
        if (!raceCompleted && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseRace();
        }

        //Lap / Pos text
        lap.text = "Lap " + player.lap + "/" + totalLaps;
        postion.text = "Pos " + player.position + "/" + RankManager.instance.totalRacers;

        //Time text and logic
        if (raceStarted && !raceCompleted)
        {
            raceTime += Time.deltaTime;
        }

        timeElapsed.text = FormatTime(raceTime);
    }

    public void StartRace()
    {

        //enable cars to start racing
        Car_Controller[] racers = GameObject.FindObjectsOfType(typeof(Car_Controller)) as Car_Controller[];
        foreach (Car_Controller go in racers)
        {
            go.GetComponent<Car_Controller>().controllable = true;
        }

        raceStarted = true;
    }


    public void PauseRace()
    {
        racePaused = !racePaused;

        //Freeze the game & mute volume on pause
        if (racePaused)
        {
            Time.timeScale = 0.0f;
            AudioListener.volume = 0.0f;
        }
        else {
            Time.timeScale = 1.0f;
            AudioListener.volume = 1.0f;
        }
    }


    //Format a float to a time string
    public string FormatTime(float time)
    {
        int minutes = (int)Mathf.Floor(time / 60);
        int seconds = (int)time % 60;
        int milliseconds = (int)(time * 100) % 100;

        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }


    //Used to calculate track distance(in Meters) & rotate the nodes correctly
    void ConfigureNodes()
    {
        Transform[] m_path = pathContainer.GetComponentsInChildren<Transform>();
        List<Transform> m_pathList = new List<Transform>();
        foreach (Transform node in m_path)
        {
            if (node != pathContainer)
            {
                m_pathList.Add(node);
            }
        }
        for (int i = 0; i < m_pathList.Count; i++)
        {
            if (i < m_pathList.Count - 1)
            {
                m_pathList[i].transform.LookAt(m_pathList[i + 1].transform);
                raceDistance += Vector3.Distance(m_pathList[i].position, m_pathList[i + 1].position);
            }
            else {
                m_pathList[i].transform.LookAt(m_pathList[0].transform);
            }
        }
    }

    //used to respawn a racer
    public void RespawnRacer(Transform racer, Transform node, float ignoreCollisionTime)
    {
        StartCoroutine(Respawn(racer, node, ignoreCollisionTime));
    }

    IEnumerator Respawn(Transform racer, Transform node, float ignoreCollisionTime)
    {
        //Flip the car over and place it at the last passed node
        racer.rotation = Quaternion.LookRotation(racer.forward);
        racer.GetComponent<Rigidbody>().velocity = Vector3.zero;
        racer.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        racer.position = new Vector3(node.position.x, node.position.y + 2.0f, node.position.z);
        racer.rotation = node.rotation;
        return null;
    }


    private int SetValue(int val, int otherVal)
    {
        int myVal = val;

        if (val > otherVal)
        {
            myVal = otherVal;
        }
        else if (val == 0)
        {
            myVal = 1;
        }

        return myVal;
    }
}
