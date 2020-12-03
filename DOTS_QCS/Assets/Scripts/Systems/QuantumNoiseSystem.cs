using QCS;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class QuantumNoiseSystem : ComponentSystem
{

    /// <summary>
    /// Apply Noise simulation by calling Pauli Noise
    /// Pauli Noise allows to model qubit interaction with an environment, this noise
    /// system is applied in an ComponentSystem because noise can occur even during qubit idling time
    /// </summary>
    protected override void OnUpdate()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        /*Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState, ref NoiseComponent noiseComponent) =>
        {
            //var randAxis = Random.Range(0, 2);
            // generating random angle for simplicity for now.
            //var randAngle = Random.Range(0, 90);

            var randAxis = 0;
            var randAngle = 0.75f;

            switch (randAxis)
            {
                case 0:
                    Gates.ApplyRxGate(ref rotation, randAngle);
                    break;
                case 1:
                    Gates.ApplyRyGate(ref rotation, randAngle);
                    break;
                case 2:
                    Gates.ApplyRzGate(ref rotation, randAngle);
                    break;
            }
        });*/
    }
}
