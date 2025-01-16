﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public Drone agentPrefab;
    List<Drone> agents = new List<Drone>();
    public FlockBehavior behavior;

    [Range(10, 5000)]
    public int startingCount = 250;
    const float AgentDensity = 0.08f;

    [Range(1f, 100f)]
    public float driveFactor = 10f;
    [Range(1f, 100f)]
    public float maxSpeed = 5f;
    [Range(1f, 10f)]
    public float neighborRadius = 1.5f;
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = 0.5f;

    float squareMaxSpeed;
    float squareNeighborRadius;
    float squareAvoidanceRadius;
    public float SquareAvoidanceRadius { get { return squareAvoidanceRadius; } }
    float deltaTime = 0.0f;
    bool firstWrite = true;

    // Start is called before the first frame update
    void Start()
    {
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        for (int i = 0; i < startingCount; i++)
        {
            Drone newAgent = Instantiate(
                agentPrefab,
                UnityEngine.Random.insideUnitCircle * startingCount * AgentDensity,
                Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(0f, 360f)),
                transform
                );
            newAgent.name = "Agent " + i;
            newAgent.Initialize(this);
            agents.Add(newAgent);
        }

    }

    void BubbleSort(Drone[] arr, int n) // O(N^2)
    {
        int i, j;
        Drone temp;
        bool swapped;               // let n =10
        for (i = 0; i < n - 1; i++)  // i=0..9
        {
            swapped = false;
            for (j = 0; j < n - i - 1; j++)   // i=0: j=0..9
                                              // i=1; j=0..8
                                              // i=2; j=0..7
                                              // i
            {
                if (arr[j].Temperature > arr[j + 1].Temperature) // check whether to swap
                {

                    // Swap arr[j] and arr[j+1]
                    temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                    swapped = true;
                }
            }

            // If no two elements were
            // swapped by inner loop, then break
            //if (swapped == false)
            //    break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Drone[] drones = agents.ToArray();
        //BubbleSort(drones, drones.Length);
        //BubbleSort(drones, drones.Length);
        //BubbleSort(drones, drones.Length);
        //Array.Sort(tempArray);

        foreach (Drone agent in agents)
        {
            // decide on next movement direction
            List<Transform> context = GetNearbyObjects(agent);

            Vector2 move = behavior.CalculateMove(agent, context, this);
            move *= driveFactor;
            if (move.sqrMagnitude > squareMaxSpeed)
            {
                move = move.normalized * maxSpeed;
            }
            agent.Move(move);
        }

        //Records the fps
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        //Stopwatch record how much time passes
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        PartitionDrones(); //Calls to Partition
        stopwatch.Stop();
        TimeSpan Timing = stopwatch.Elapsed;
        WriteCSV(Timing, fps, append: !firstWrite); //Write data to file
        firstWrite = false;
    }

    List<Transform> GetNearbyObjects(Drone agent)
    {
        List<Transform> context = new List<Transform>();
        Collider2D[] contextColliders = Physics2D.OverlapCircleAll(agent.transform.position, neighborRadius);
        foreach (Collider2D c in contextColliders)
        {
            if (c != agent.AgentCollider)
            {
                context.Add(c.transform);
            }
        }
        return context;
    }

    public Sprite highBattery,lowBattery;

    //Function to partition based on the batterylevel of the drone
    public void PartitionDrones()
    {
        Drone firstDrone = agents[0];
        float BatteryPivot = firstDrone.BatteryLevel;

        List<Drone> highBatteryDrones = new List<Drone>();
        List<Drone> lowBatteryDrones = new List<Drone>();

        foreach (Drone drone in agents)
        {
            SpriteRenderer droneSpriteRenderer = drone.gameObject.GetComponent<SpriteRenderer>();
            if (drone.BatteryLevel >= BatteryPivot)
            {
                highBatteryDrones.Add(drone);
                droneSpriteRenderer.sprite = highBattery;
            }
            else
            {
                lowBatteryDrones.Add(drone);
                droneSpriteRenderer.sprite = lowBattery;
            }
        }
    }

    //Write a csv file recording Time taken and FPS
    void WriteCSV(TimeSpan Timing, float fps, bool append = true)
    {
        string FilePath = Path.Combine(Application.dataPath, "Results.csv");

        using (StreamWriter Writer = new StreamWriter(FilePath, append))
        {
            if (!append)
            {
                Writer.WriteLine("Time Taken(ms), FPS");
            }
            Writer.WriteLine($"{Timing.TotalMilliseconds}, {fps}");
        }
    }
}

