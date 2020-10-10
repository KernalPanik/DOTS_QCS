using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public enum GateCodes
{
    IDENTITY = 0,
    X = 1
}

public static class Gates
{
    /// <summary>
    /// Apply gate by passing it's code
    /// </summary>
    public static void ApplyGate(int gateCode, ref Rotation qubitRotation)
    {
        switch(gateCode)
        {
            case (int)GateCodes.IDENTITY:
                ApplyI(ref qubitRotation);
                break;
            case (int)GateCodes.X:
                ApplyX(ref qubitRotation);
                break;
        }
    }

    /// <summary>
    /// Apply identity matrix (nop)
    /// </summary>
    public static void ApplyI(ref Rotation qubitRotation)
    {
        qubitRotation.Value = math.mul(math.normalizesafe(qubitRotation.Value), quaternion.identity);
    }

    /// <summary>
    /// Apply X gate (NOT gate)
    /// </summary>
    public static void ApplyX(ref Rotation qubitRotation)
    {
        qubitRotation.Value = math.mul(math.normalizesafe(qubitRotation.Value), quaternion.RotateX(math.PI));
    }
}
