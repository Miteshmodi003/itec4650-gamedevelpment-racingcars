using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RankManagement : MonoBehaviour {
    int totalRacers;
    GameObject player;
    float playerCurrentLap, playerCurrentCheckpoint, enemyCurrentLap, enemyCurrentCheckpoint;
    int rank = 3;
	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
    }
	
	// Update is called once per frame
	void Update () {
        AddAllCars();        
	}

    public void AddAllCars()
    {
        LapTracker[] cars = FindObjectsOfType(typeof(LapTracker)) as LapTracker[];

        totalRacers = cars.Length - 1;
        //print("Total Racers: " + totalRacers);
        foreach(LapTracker car in cars)
        {
            //print(car.name + " ChecpPoint: " + car.currentCheckpoint + " LapNum: " + car.lap);
            if(car.tag == "Player")
            {
                playerCurrentLap = car.lap;
                playerCurrentCheckpoint = car.currentCheckpoint;

            }
            if(car.tag == "Opponent")
            {
                enemyCurrentLap = car.lap;
                enemyCurrentCheckpoint = car.currentCheckpoint;
            }
            if(playerCurrentLap > enemyCurrentLap)
            {
                rank--;
                print("Rank: " + rank);
            } else if(playerCurrentLap == enemyCurrentLap && playerCurrentCheckpoint > enemyCurrentCheckpoint)
            {
                rank--;
                print("Rank: " + rank);
            } 
            
        }
    }
}
