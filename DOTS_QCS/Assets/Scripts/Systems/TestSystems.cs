using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class RotateSystem : JobComponentSystem
{
    
    /// <summary>
    /// Test System to induce rotation of qubits to test PhysicalToQuantum Transform
    /// </summary>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;
        float ROTATION_SPEED = 0.5f; // deg/s

        Entities.ForEach((ref Rotation rotation, ref QuantumState quantumState, ref Energy energy) =>
        {
            //TODO: Verify how state should be changing on rotation along X Z
            //normalize returns NaN, using normalizesafe instead
            rotation.Value = math.mul(math.normalizesafe(rotation.Value), quaternion.RotateX(ROTATION_SPEED * deltaTime));
        }).Run();

        return default;
    }
}