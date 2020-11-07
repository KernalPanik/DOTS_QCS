using QCS;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class QuantumNoiseSystem : ComponentSystem
{
    /// <summary>
    /// Apply Noise simulation by calling Ry gate as it was described in phase damping circuit
    /// </summary>
    protected override void OnUpdate()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState) =>
        {
            // randControl can be 0 ir 1 to reflect control state. If it is 1, then we apply rotation in this frame
            var randControl = Random.Range(0, 1);
            // generating random angle for simplicity for now.
            var randAngle = Random.Range(15, 45);
            if(randControl == 1)
            {
                Gates.ApplyRyGate(ref rotation, randAngle);
            }
        });
    }
}
