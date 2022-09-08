﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Scanner : RayGunMode
{
    [SerializeField] private Transform rayContainer;
    [SerializeField] private int numOfRays;
    [SerializeField] private float verticalScanAngle;
    [SerializeField] private float horizontalScanAngle;
    [SerializeField] private float scanRate;

    // Scan ray pass either 1 or 0 - used to determine which rays to randomly sample (only draws dots for half of all
    // rays)
    private int scanRayPass;
    private GameObject[] rays;
    private float scanAngleRadians;

    private bool scanning;

    public bool Scanning
    {
        get => scanning;
        set
        {
            if (scanAngleRadians > 0)
                return;

            scanning = value;
            scanAngleRadians = scanning ? Mathf.PI : -1;
            SetRaysActive(rays, scanning);
        }
    }

    public override void InitializeRays(GameObject rayPrefab)
    {
        rays = new GameObject[numOfRays];
        scanAngleRadians = -1;
        scanRayPass = 0;

        for (int i = 0; i < numOfRays; i++)
        {
            rays[i] = Instantiate(rayPrefab, rayContainer);
        }

        Scanning = false;
    }

    // Scan the geometry, drawing horizontal lines down the geometry
    public void Scan(float deltaTime)
    {
        if (!scanning)
            return;

        scanAngleRadians -= scanRate * deltaTime;
        AdjustRays();

        Scanning = false;
    }

    private void AdjustRays()
    {
        RaycastHit hit = new RaycastHit();
        scanRayPass = (scanRayPass + 1) % 2;

        for (int i = 0; i < numOfRays; i++)
        {
            if (AdjustRayFromRaycast(
                rays[i].transform,
                horizontalScanAngle,
                verticalScanAngle,
                Mathf.PI * Random.Range(i / (float)numOfRays, i + 1 / (float)numOfRays),
                scanAngleRadians,
                ref hit)
                && i % 2 == scanRayPass)
            {
                CreateDotFromRaycast(hit);
            }
        }
    }

    // Tries to adjust the scan ray from a raycast, returning whether the raycast hit was successful
    private bool AdjustRayFromRaycast(Transform ray, float horizontalAngle, float verticalAngle,
        float horizontalRadians, float verticalRadians, ref RaycastHit hit)
    {
        bool successfulHit;

        // Orient the ray
        ray.localEulerAngles = new Vector3(
            Mathf.Cos(verticalRadians) * verticalAngle,
            Mathf.Cos(horizontalRadians) * horizontalAngle, 0);

        successfulHit = Raycast(ray.position, ray.forward, out hit);
        ResizeRay(ray, hit.distance);

        return successfulHit;
    }

    public new void SetRaysActive(Array rays, bool active)
    {
        // Adjust rays before enabling them so they don't appear to jump to the top of the screen
        if (active)
            AdjustRays();

        base.SetRaysActive(rays, active);
    }
}
