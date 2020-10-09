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
        Entities.ForEach((ref Rotation rotation, ref QuantumState quantumState, ref Energy energy) =>
        {
            //normalize returns NaN, using normalizesafe instead
            rotation.Value = math.mul(math.normalizesafe(rotation.Value), quaternion.RotateX(1 * deltaTime));
        }).Run();
        return default;
    }
}