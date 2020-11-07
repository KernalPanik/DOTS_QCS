using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace QCS
{
    public class RotateSystem : JobComponentSystem
    {

        /// <summary>
        /// Test System to induce rotation of qubits to test PhysicalToQuantum Transform
        /// </summary>
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            /*float deltaTime = Time.DeltaTime;
            float ROTATION_SPEED = 3f; // rad/s

            Entities.ForEach((ref Rotation rotation, ref QuantumState quantumState, ref QubitComponent qubitComponent) =>
            {
            //TODO: Verify how state should be changing on rotation along X Z
            //normalize returns NaN, using normalizesafe instead
            rotation.Value = math.mul(math.normalizesafe(rotation.Value), quaternion.RotateY(ROTATION_SPEED * deltaTime));
            rotation.Value = math.mul(math.normalizesafe(rotation.Value), quaternion.RotateX(ROTATION_SPEED * deltaTime));
            rotation.Value = math.mul(math.normalizesafe(rotation.Value), quaternion.RotateZ(ROTATION_SPEED * deltaTime));
            
            }).Run();*/

            return default;
        }
    }
}