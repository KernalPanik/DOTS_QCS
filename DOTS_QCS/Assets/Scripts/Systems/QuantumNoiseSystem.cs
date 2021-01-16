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
        Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState, ref NoiseComponent noiseComponent) =>
        {
            //var randAxis = Random.Range(0, 2);
            // generating random angle for simplicity for now.
            //var randAngle = Random.Range(0, 90);

            /*var randAxis = 0;
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
            }*/

            //ApplyAmplitudeDamping(0.0f, ref quantumState);
        });
    }

    public static void ApplyAmplitudeDamping(float rate, ref QuantumState quantumState, ref Rotation rotation)
    {
        // Apply amplitude damping with rate 0.1

        if(rate < 1f)
        {
            System.Random rand = new System.Random();
            var randNum = (rand.NextDouble() * 99f) / 100;
            if(randNum > rate)
            {
                return;
            }
            /*var tempTest = ExtraMath.PickValueFromAmplitudes(new float[2] { rate, 1 - rate }, new int[2] { 0, 1 });
            if(tempTest == 1)
            {
                return; 
            }*/
        }

        var ampMatrix = new float[2, 2] { { 0, 1 }, { 0, 0 } };
        ampMatrix[0, 1] = ampMatrix[0, 1] * Mathf.Sqrt(1);
        var vectorizedState = ExtraMath.VectorizeQuantumState(quantumState.Alpha, quantumState.Beta);
        var resultState = ExtraMath.Matrix2x2VectorMultiplication(ampMatrix, vectorizedState);

        var testValue = ExtraMath.PickValueFromAmplitudes(new float[2] { resultState[0], resultState[1]}, new int[2] { 0, 1 });
        if(testValue == 0)
        {
            Gates.ApplyX(ref rotation);
        }

        /*quantumState.Alpha = resultState[0];
        quantumState.Beta = resultState[1];
        */
    }
}
