using QCS;
using System;
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
    IDENTITY = 0,
    X = 1,
    HADAMARD = 2,
    CNOT = 3,
    MEASUREMENT = 4 // Measurement is not a 'gate', but it is convenient to interpret it as it is a gate
}

public static class Gates
{
    /// <summary>
    /// Apply Qubit gate by passing an entityManager to retrieve essential qubit data, 
    /// qubit entity, and gate code
    /// </summary>
    /// <returns>
    /// If Measurement gate is applied, measured bit value is returned, -1 is returned otherwise
    /// </returns>
    public static int ApplyGate(EntityManager em, int gateCode, ref Entity Qubit)
    {
        Rotation qubitRotation = em.GetComponentData<Rotation>(Qubit);
        QuantumState qubitQuantumState = em.GetComponentData<QuantumState>(Qubit);
        int classicalData = -1;

        switch (gateCode)
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
            case (int)GateCodes.MEASUREMENT:
                classicalData = ApplyMeasurement(ref qubitRotation, ref qubitQuantumState);
                break;
        }

        em.SetComponentData(Qubit, qubitRotation);
        em.SetComponentData(Qubit, qubitQuantumState);
        return classicalData;
    }

    /// <summary>
    /// Apply double qubit gate by passing it's code and qubit entities
    /// </summary>
    public static void ApplyGate(EntityManager em, int gateCode, in Entity controlQubit,
        ref Entity targetQubit)
    {
        switch(gateCode)
        {
            case (int)GateCodes.CNOT:
                var targetRotation = em.GetComponentData<Rotation>(targetQubit);
                var controlState = em.GetComponentData<QuantumState>(controlQubit);
                ApplyCNOT(in controlState, ref targetRotation);
                em.SetComponentData(targetQubit, targetRotation);
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

    /// <summary>
    /// Apply CNOT gate
    /// This gate requires to know state of the control qubit and the rotation of the target,
    /// so passing only them
    /// </summary>
    public static void ApplyCNOT(in QuantumState controlState, ref Rotation targetRotation)
    {
        // TODO: (Luke) figuring out states and their amps will can be wrapped in a function
        float[] stateAmps = new float[] { controlState.Alpha, controlState.Beta };
        int[] states = new int[] { 0, 1 };
        float2 stateFromAmplitudes = ExtraMath.PickValueFromAmplitudes(stateAmps, states);

        if (stateFromAmplitudes.y == 1f)
        {
            ApplyX(ref targetRotation);
        }
    }

    /// <summary>
    /// Apply Measurement 'gate'
    /// </summary>
    /// <returns>
    /// Measured classical bit value
    /// </returns>
    public static int ApplyMeasurement(ref Rotation qubitRotation, ref QuantumState qubitState)
    {
        float[] stateAmps = new float[] { qubitState.Alpha, qubitState.Beta };
        int[] states = new int[] { 0, 1 };
        float2 measuredState = ExtraMath.PickValueFromAmplitudes(stateAmps, states);
        qubitRotation.Value = quaternion.identity;
        qubitState.Alpha = 0f;
        qubitState.Beta = 0f;

        return (int)measuredState.y;
    }
}
