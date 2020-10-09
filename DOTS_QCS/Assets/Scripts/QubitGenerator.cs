using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

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

public struct SphericalCoords
{
    public float theta;
    public float phi;
}

//TODO: Physical-To-Quantum Transform system
//TODO: Proper Qubit Generator
//TODO: X Gate system

public class PhysicalToQuantumSystem : ComponentSystem
{
    /// <summary>
    /// Calculate quantum state amplitudes based on the rotation of the qubit
    /// cos(theta/2)* state_0 + e^(i*fi)*sin(theta/2)* state_1
    /// </summary>
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Rotation rotation, ref QuantumState quantumState) => 
        {
            var coords = ExtraMath.QuaternionToSpherical(rotation.Value);
            quantumState.Alpha = math.cos(coords.theta / 2);
            quantumState.Beta = math.sqrt(1 - math.pow(quantumState.Alpha, 2));
        });
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
        coords.theta = math.atan2(quaternion.value.y, quaternion.value.w);
        coords.phi = math.acos(quaternion.value.z / 1); // since we're working with a unit sphere, our radius is equal to 1
        return coords;
    }
}

public class QubitGenerator : MonoBehaviour
{
    private EntityManager entityManager;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype archetype = CreateQubitArchetype(entityManager);
        Entity entity = entityManager.CreateEntity(archetype);
    }

    private EntityArchetype CreateQubitArchetype(EntityManager entityManager)
    {
        return entityManager.CreateArchetype(
           typeof(Translation),
           typeof(Rotation),
           typeof(Energy),
           typeof(QuantumState));
    }
}
