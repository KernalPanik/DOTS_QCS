using QCS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace QCS
{
    public class QuantumCircuitSystem : ComponentSystem
    {
        private int executed = 0;
        private bool benchmarkFound = false;

        public static List<int[]> StatevectorList = new List<int[]>();
        public static bool CircuitWorking = true;
        public static int[] BenchhmarkStatevector { get; set; }

        protected override void OnUpdate()
        {
            if (!benchmarkFound)
            {
                // TODO: Fix incorrect benchmar statevector calculation
                // Noise is now disabled, get benchmark statevector
                IterateOverGates();
                BenchhmarkStatevector = GenerateStateVector();
                Debug.Log($"benchmark statevector: {string.Join("", BenchhmarkStatevector.ToList().ConvertAll(i => i.ToString()).ToArray())}");
                benchmarkFound = true;
                EnableNoise();

                Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState) =>
                {
                    rotation = default;
                    quantumState = default;
                });
            }

            if (executed < 10000)
            {
                IterateOverGates();

                var statevector = GenerateStateVector();
                Debug.Log($"statevector: {string.Join("", statevector.ToList().ConvertAll(i => i.ToString()).ToArray())}");
                StatevectorList.Add(statevector);
                executed += 1;

                // Reset all states
                Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState) =>
                {
                    rotation = default;
                    quantumState = default;
                });
            }

            if (executed == 10000 && CircuitWorking)
            {
                CircuitWorking = false;
                Debug.Log("Quantum computer simulation is finished, use QCS menu for more tasks");
            }
        }

        private void EnableNoise()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState, ref QubitComponent qubitComponent) =>
            {
                if (!em.HasComponent<ErrorCorrectionComponent>(entity))
                {
                    em.AddComponentData(entity, new NoiseComponent { });
                }
            });
        }



        private void IterateOverGates()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (var gate in QuantumComputer.gateList)
            {
                if (em.HasComponent<SingleQubitGate>(gate))
                {
                    ExecuteSingleQubitGate(em, em.GetComponentData<SingleQubitGate>(gate));
                }
                else if (em.HasComponent<DoubleQubitGate>(gate))
                {
                    ExecuteDoubleQubitGate(em, em.GetComponentData<DoubleQubitGate>(gate));
                }
                else if (em.HasComponent<TripleQubitGate>(gate))
                {
                    ExecuteTripleQubitGate(em, em.GetComponentData<TripleQubitGate>(gate));
                }

                if (em.HasComponent<NoisyChannel>(gate))
                {
                    Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState, ref NoiseComponent noiseComponent) =>
                    {
                        //var randAxis = Random.Range(0, 2);
                        // generating random angle for simplicity for now.
                        //var randAngle = Random.Range(0, 90);

                        var randAxis = 0;
                        var randAngle = 1f;

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
                    });
                }

                // Physical rotation quaternion to quantum state mapping
                Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState) =>
                {
                    if (quantumState.Locked == 0)
                    {
                        var coords = ExtraMath.QuaternionToSpherical(rotation.Value);
                        // TODO: (Luke) Check if alpha is for 0 or 1
                        quantumState.Alpha = math.cos(coords.Theta / 2);
                        quantumState.Beta = math.sqrt(1 - math.pow(quantumState.Alpha, 2));
                    }
                });
            }
        }

        /// <summary>
        /// Generate a statevector of qubits after circuit was executed.s
        /// </summary>
        /// <returns></returns>
        private int[] GenerateStateVector()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var statevector = new int[QuantumComputer.LogicalQubitCount];
            var currentQubit = 0;
            Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState) =>
            {
                if (!em.HasComponent<ErrorCorrectionComponent>(entity))
                {
                    if (quantumState.Locked == 0)
                    {
                        statevector[currentQubit] = (byte)ExtraMath.PickValueFromAmplitudes(
                            new float[] { quantumState.Alpha, quantumState.Beta },
                            new int[] { 0, 1 });
                    }
                    else
                    {
                        // We already have a measured state
                        if (quantumState.Alpha == 1)
                        {
                            statevector[currentQubit] = 0;
                        }
                        else
                        {
                            statevector[currentQubit] = 1;
                        }
                    }
                }
            });

            return statevector;
        }

        private void ExecuteDoubleQubitGate(EntityManager em, DoubleQubitGate gate)
        {
            Entity controlQubit = new Entity();
            Entity targetQubit = new Entity();

            Entities.ForEach((Entity entity, ref QuantumState quantumState, ref Rotation rotation, ref QubitComponent qubitComponent) =>
            {
                if (gate.ControlQubit == qubitComponent.Id)
                {
                    controlQubit = entity;
                }
                else if (gate.TargetQubit == qubitComponent.Id)
                {
                    targetQubit = entity;
                }
            });

            //Apply gate
            Gates.ApplyGate(em, gate.GateCode, in controlQubit, ref targetQubit);
        }

        private void ExecuteSingleQubitGate(EntityManager em, SingleQubitGate gate)
        {
            Entities.ForEach((Entity entity, ref QuantumState quantumState, ref Rotation rotation, ref QubitComponent qubitComponent) =>
            {
                if (gate.Qubit == qubitComponent.Id)
                {
                    int gateResult = Gates.ApplyGate(em, gate.GateCode, ref entity);
                }
            });
        }

        private void ExecuteTripleQubitGate(EntityManager em, TripleQubitGate gate)
        {
            Entity c1Qubit = new Entity();
            Entity c2Qubit = new Entity();
            Entity targetQubit = new Entity();

            Entities.ForEach((Entity entity, ref QuantumState quantumState, ref Rotation rotation, ref QubitComponent qubitComponent) =>
            {
                if (gate.C1Qubit == qubitComponent.Id)
                {
                    c1Qubit = entity;
                }
                else if (gate.C2Qubit == qubitComponent.Id)
                {
                    c2Qubit = entity;
                }
                else if (gate.TargetQubit == qubitComponent.Id)
                {
                    targetQubit = entity;
                }
            });

            //Apply gate
            Gates.ApplyGate(em, gate.GateCode, in c1Qubit, in c2Qubit, ref targetQubit);
        }
    }
}