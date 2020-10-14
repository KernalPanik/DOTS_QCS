using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Gate codes that map given code to the gate it represents
/// It is used in Gates.ApplyGate() method
/// </summary>
public enum GateCodes
{
    IDENTITY = 1,
    X = 2,
    HADAMARD = 3,
    CNOT = 4
}

public static class Gates
{
    /// <summary>
    /// Apply double qubit gate by passing it's code and qubit rotation
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
            case (int)GateCodes.HADAMARD:
                ApplyHadamard(ref qubitRotation);
                break;
        }
    }

    /// <summary>
    /// Apply double qubit gate by passing it's code and qubit entities
    /// </summary>
    public static void ApplyGate(EntityManager em, int gateCode, ref Entity controlQubit,
        ref Entity targetQubit)
    {
        switch(gateCode)
        {
            case (int)GateCodes.CNOT:
                var control = em.GetComponentData<Rotation>(controlQubit);
                var target = em.GetComponentData<Rotation>(targetQubit);
                ApplyCNOT(ref control, ref target);
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

    /// <summary>
    /// Apply Hadamard gate
    /// </summary>
    public static void ApplyHadamard(ref Rotation qubitRotation)
    {
        qubitRotation.Value = math.mul(math.normalizesafe(qubitRotation.Value), quaternion.RotateZ(math.PI/2));
        qubitRotation.Value = math.mul(math.normalizesafe(qubitRotation.Value), quaternion.RotateY(math.PI));
    }

    public static void ApplyCNOT(ref Rotation control, ref Rotation target)
    {
        throw new System.NotImplementedException();
    }
}
