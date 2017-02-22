//RankManager.cs handles setting each racer's position/rank
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RankManager : MonoBehaviour
{

    [System.Serializable]
    public class Ranker : IComparer<Ranker>
    {
        public GameObject racer;
        public float raceCompletion;

        public int Compare(Ranker x, Ranker y)
        {
            return x.raceCompletion.CompareTo(y.raceCompletion);
        }
    }

    public static RankManager instance;
    [HideInInspector]
    public List<Ranker> racerRanks = new List<Ranker>(new Ranker[100]);//allow upto 100 racers
    [HideInInspector]
    public List<ProgressTracker> racerStats = new List<ProgressTracker>();
    public int totalRacers; //number of racers when the race begins

    void Awake()
    {
        //create an instance
        instance = this;
    }


    void Start()
    {
        InvokeRepeating("SetCarRank", 0.1f, 0.5f);
    }


    //Finds the number of racers in the race.
    public void RefreshRacerCount()
    {
        LapTracker[] m_racers = GameObject.FindObjectsOfType(typeof(LapTracker)) as LapTracker[];

        for (int i = 0; i < m_racers.Length; i++)
        {
            if (!racerStats.Contains(m_racers[i].GetComponent<ProgressTracker>()))
            {
                racerStats.Add(m_racers[i].GetComponent<ProgressTracker>());
            }
        }

        totalRacers = m_racers.Length;

        //Resize the list
        racerRanks.RemoveRange(totalRacers, racerRanks.Count - totalRacers);

    }


    void Update()
    {
        //Fill & sort the list in order
        for (int i = 0; i < totalRacers; i++)
        {
            if (racerRanks[i] != null && racerStats[i] != null)
            {
                racerRanks[i].racer = racerStats[i].gameObject;
                racerRanks[i].raceCompletion = racerStats[i].raceCompletion - ((float)racerStats[i].GetComponent<LapTracker>().position / 1000);
            }
        }

        Ranker m_ranker = new Ranker();
        racerRanks.Sort(m_ranker);
        racerRanks.Reverse();
    }


    //Sets the car ranks accoding to the sorted list
    void SetCarRank()
    {
        for (int r = 0; r < totalRacers; r++)
        {
            if (racerRanks[r].racer.GetComponent<Car_Controller>())
            {
                racerRanks[r].racer.GetComponent<LapTracker>().position = r + 1;
            }
        }
    }
}
