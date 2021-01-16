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
    MEASUREMENT = 4, // Measurement is not a 'gate', but it is convenient to interpret it as it is a gate
    TOFFOLI = 5,
    T = 6,
    T_ = 7,
    RY = 8,
    AMP_DAMP = 9,
    PHS_DAMP = 10,
    RZ = 11,
    RX = 12
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
            case (int)GateCodes.RZ:
                ApplyRzGate(ref qubitRotation, 180);
                break;
            case (int)GateCodes.RX:
                ApplyRxGate(ref qubitRotation, 180);
                break;
            case (int)GateCodes.RY:
                ApplyRyGate(ref qubitRotation, 180);
                break;
            case (int)GateCodes.HADAMARD:
                ApplyHadamard(ref qubitRotation, ref qubitQuantumState);
                break;
            case (int)GateCodes.MEASUREMENT:
                classicalData = ApplyMeasurement(ref qubitRotation, ref qubitQuantumState);
                break;
            case (int)GateCodes.AMP_DAMP:
                ApplyAmplitudeDampingGate(ref qubitRotation, ref qubitQuantumState);
                break;
            case (int)GateCodes.PHS_DAMP:
                ApplyPhaseDampingGate(ref qubitRotation);
                break;
            default:
                throw new KeyNotFoundException("Gate code is not found");
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
                ApplyCNOT(ref controlState, ref targetRotation);
                em.SetComponentData(targetQubit, targetRotation);
            break;
            default:
                throw new KeyNotFoundException("Gate code is not found");
        }
    }

    public static void ApplyGate(EntityManager em, int gateCode, in Entity qubit, float value)
    {
        Rotation qubitRotation = em.GetComponentData<Rotation>(qubit);

        switch (gateCode)
        {
            case (int)GateCodes.RY:
                ApplyRyGate(ref qubitRotation, value);
                break;
            default:
                throw new KeyNotFoundException("Gate code is not found");
        }
    }

    /// <summary>
    /// Apply triple qubit gate by passing it's code and qubit entities
    /// </summary>
    public static void ApplyGate(EntityManager em, int gateCode, in Entity c1Qubit, in Entity c2Qubit, ref Entity targetQubit)
    {
        switch (gateCode)
        { 
            case (int)GateCodes.TOFFOLI:
                var c1State = em.GetComponentData<QuantumState>(c1Qubit);
                var c2State = em.GetComponentData<QuantumState>(c2Qubit);
                var targetRotation = em.GetComponentData<Rotation>(targetQubit);
                ApplyToffoli(ref targetRotation, in c1State, in c2State);
                em.SetComponentData(targetQubit, targetRotation);
                break;
            default:
                throw new KeyNotFoundException("Gate code is not found");
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
    public static void ApplyHadamard(ref Rotation qubitRotation, ref QuantumState quantumState)
    {
        qubitRotation.Value = math.mul(math.normalizesafe(qubitRotation.Value), quaternion.RotateY(math.PI));
        qubitRotation.Value = math.mul(math.normalizesafe(qubitRotation.Value), quaternion.RotateZ(math.PI/2));
    }

    /// <summary>
    /// Apply CNOT gate
    /// This gate requires to know state of the control qubit and the rotation of the target,
    /// so passing only them
    /// </summary>
    public static void ApplyCNOT(ref QuantumState controlState, ref Rotation targetRotation)
    {
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
        // Qubit state should be modified and fixed here, since we already measured it
        float[] stateAmps = new float[] { qubitState.Alpha, qubitState.Beta };
        int[] states = new int[] { 0, 1 };
        float2 measuredState = ExtraMath.PickValueFromAmplitudes(stateAmps, states);
        qubitRotation.Value = quaternion.identity;

        if (measuredState.y == 1)
        {
            qubitState.Alpha = 0;
            qubitState.Beta = 1;
        }
        else
        {
            qubitState.Beta = 0;
            qubitState.Alpha = 1;
        }
        //qubitState.Alpha = measuredState.x;
        qubitState.Locked = 1;
        
        return (int)measuredState.y;
    }

    /// <summary>
    /// Apply Toffoli gate
    /// This gate requires to know the state of two control qubits, so passing only them and a
    /// rotation of the target qubit
    /// </summary>
    public static void ApplyToffoli(ref Rotation target, in QuantumState c1, in QuantumState c2)
    {
        float[] c1_stateAmps = new float[] { c1.Alpha, c1.Beta };
        int[] c1_states = new int[] { 0, 1 };
        float[] c2_stateAmps = new float[] { c2.Alpha, c2.Beta };
        int[] c2_states = new int[] { 0, 1 };

        float2 c1_stateFromAmps = ExtraMath.PickValueFromAmplitudes(c1_stateAmps, c1_states);
        float2 c2_stateFromAmps = ExtraMath.PickValueFromAmplitudes(c2_stateAmps, c2_states);

        if(c1_stateFromAmps.y == 1f && c2_stateFromAmps.y == 1f)
        {
            ApplyX(ref target);
        }
    }

    public static void ApplyRyGate(ref Rotation target, float angle)
    {
        target.Value = math.mul(math.normalizesafe(target.Value), quaternion.RotateZ(angle));
    }

    public static void ApplyRxGate(ref Rotation target, float angle)
    {
        target.Value = math.mul(math.normalizesafe(target.Value), quaternion.RotateX(angle));
    }

    public static void ApplyRzGate(ref Rotation target, float angle)
    {
        target.Value = math.mul(math.normalizesafe(target.Value), quaternion.RotateY(angle));
    }

    public static void ApplyAmplitudeDampingGate(ref Rotation target, ref QuantumState quantumState, float rate = 0.5f)
    {
        QuantumNoiseSystem.ApplyAmplitudeDamping(rate, ref quantumState, ref target);
    }

    public static void ApplyPhaseDampingGate(ref Rotation target, float angle = 5f)
    {
        // delegate the behavior to the Rz, as it is basically the same.
        angle = ExtraMath.NextGauss(60, 0, -180, 180);
        ApplyRyGate(ref target, angle);
    }
}
