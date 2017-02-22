using UnityEngine;
using System;
using System.Collections;

public class ProgressTracker : MonoBehaviour
{
    private WaypointCircuit circuit; // A reference to the waypoint-based route we should follow
    private float lookAheadForTargetOffset = 20f;
    private float lookAheadForTargetFactor = 0.1f;
    private float lookAheadForSpeedOffset = 20;
    private float lookAheadForSpeedFactor = 0.5f;
    public Transform target;
    public float progressDistance;
    public float raceCompletion;
    private Vector3 lastPosition;
    private float speed;
    public WaypointCircuit.RoutePoint targetPoint { get; private set; }
    public WaypointCircuit.RoutePoint speedPoint { get; private set; }
    public WaypointCircuit.RoutePoint progressPoint { get; private set; }


    void Start()
    {
        if (!RaceManager.instance)
            return;

        target = new GameObject("New Progress Tracker").transform;
        circuit = GameObject.FindObjectOfType(typeof(WaypointCircuit)) as WaypointCircuit;

    }


    void Update()
    {
        if (!target) return;

        if (Time.deltaTime > 0)
        {
            speed = Mathf.Lerp(speed, (lastPosition - transform.position).magnitude / Time.deltaTime, Time.deltaTime);
        }

        target.position = circuit.GetRoutePoint(progressDistance + lookAheadForTargetOffset + lookAheadForTargetFactor * speed).position;

        target.rotation = Quaternion.LookRotation(circuit.GetRoutePoint(progressDistance + lookAheadForSpeedOffset + lookAheadForSpeedFactor * speed).direction);

        // get our current progress along the route
        progressPoint = circuit.GetRoutePoint(progressDistance);

        Vector3 progressDelta = progressPoint.position - transform.position;

        if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
        {
            progressDistance += progressDelta.magnitude * 0.5f;
        }

        if (Vector3.Dot(progressDelta, progressPoint.direction) > 5.0f)
        {
            progressDistance -= progressDelta.magnitude * 0.5f;
        }

        lastPosition = transform.position;

        raceCompletion = ((progressDistance / RaceManager.instance.raceDistance) * 100) / RaceManager.instance.totalLaps;
        raceCompletion = Mathf.Clamp(raceCompletion, -Mathf.Infinity, 100);
        raceCompletion = Mathf.Round(raceCompletion * 100) / 100;
    }

    void OnDestroy()
    {
        if (target)
            Destroy(target.gameObject);
    }
}
