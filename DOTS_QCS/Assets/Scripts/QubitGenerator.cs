using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public struct Energy : IComponentData
{
    public float Value;
}

public struct QuantumState : IComponentData
{
    /// <summary>
    /// Alpha amplitude describes state |0>
    /// Beta amplitude describes state |1>
    /// </summary>
    public float Alpha;
    public float Beta;
}

public struct GateComponent : IComponentData
{
    public int ExecutionOrder;
}

public struct NewGateComponent : IComponentData
{
    public int ExecutionOrder;
    public quaternion GateQuaternion;
}

public struct QubitComponent : IComponentData
{
    public int QubitId;
}

public struct SphericalCoords
{
    public float Theta;
    public float Phi;
}

//TODO: X Gate system

public class PhysicalToQuantumSystem : ComponentSystem
{
    /// <summary>
    /// Calculate quantum state amplitudes based on the rotation of the qubit
    /// cos(theta/2)* state_0 + e^(i*fi)*sin(theta/2)* state_1
    /// </summary>
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState) => 
        {
            var coords = ExtraMath.QuaternionToSpherical(rotation.Value);
            quantumState.Alpha = math.cos(coords.Theta / 2);
            quantumState.Beta = math.sqrt(1 - math.pow(quantumState.Alpha, 2));
        });
    }
}

public class QuantumCircuitSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Rotation rotation, ref QuantumState quantumState, ref QubitComponent qubitComponent)=>
        {
            // Here we can call gates and they will modify our qubits!
            // But as you see, the approach is super lame :(
            if(qubitComponent.QubitId == 0)
                Gates.ApplyI(ref rotation);
            if (qubitComponent.QubitId == 1)
                Gates.ApplyX(ref rotation);
        });
    }
}

public static class Gates
{
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

public static class ExtraMath
{
    /// <summary>
    /// a function to convert a given quaternion into vector of longitude and lattitude
    /// </summary>
    public static SphericalCoords QuaternionToSpherical(quaternion quaternion)
    {
        SphericalCoords coords = new SphericalCoords();
        //TODO: Verify if such conversion is correct
        coords.Theta = math.atan2(quaternion.value.y, quaternion.value.w);
        coords.Phi = math.acos(quaternion.value.z / 1); // since we're working with a unit sphere, our radius is equal to 1
        return coords;
    }
}

public class QubitGenerator : MonoBehaviour
{
    private EntityManager entityManager;
    private DynamicBuffer<Entity> qubits;
    private int QubitCount = 0;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        CreateQubit(entityManager);
        CreateQubit(entityManager);
    }

    public Entity CreateQubit(EntityManager entityManager)
    {
        EntityArchetype archetype = CreateQubitArchetype(entityManager);
        var qubit = entityManager.CreateEntity(archetype);
        entityManager.AddComponentData(qubit, new QubitComponent { QubitId = QubitCount });
        QubitCount++;
        return qubit;
    }

    private EntityArchetype CreateQubitArchetype(EntityManager entityManager)
    {
        return entityManager.CreateArchetype(
           typeof(Translation),
           typeof(Rotation),
           typeof(Energy),
           typeof(QuantumState),
           typeof(QubitComponent));
    }
}
