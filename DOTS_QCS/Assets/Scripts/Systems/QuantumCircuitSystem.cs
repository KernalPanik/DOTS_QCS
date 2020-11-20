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
        protected override void OnUpdate()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (executed < 5000)
            {
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

                    // Physical rotation quaternion to quantum state mapping
                    Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState) =>
                    {
                        if (quantumState.Locked == 0)
                        {
                            var coords = ExtraMath.QuaternionToSpherical(rotation.Value);
                            quantumState.Alpha = math.cos(coords.Theta / 2);
                            quantumState.Beta = math.sqrt(1 - math.pow(quantumState.Alpha, 2));
                        }
                    });
                }

                var statevector = GenerateStateVector();
                Debug.Log($"statevector: {string.Join("", statevector.ToList().ConvertAll(i => i.ToString()).ToArray())}");
                executed += 1;
                
                // Reset all states
                Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState) => {
                    rotation = default;
                    quantumState = default;
                });
            }
        }

        /// <summary>
        /// Generate a statevector of qubits after circuit was executed.s
        /// </summary>
        /// <returns></returns>
        private int[] GenerateStateVector()
        {
            var statevector = new int[QuantumComputer.qubitList.Count];
            var currentQubit = 0;
            Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState) => { 
                if(quantumState.Locked == 0)
                {
                    statevector[currentQubit] = (byte)ExtraMath.PickValueFromAmplitudes(
                        new float[] { quantumState.Alpha, quantumState.Beta },
                        new int[] { 0, 1 });
                }
                else
                {
                    // We already have a measured state
                    if(quantumState.Alpha == 1)
                    {
                        statevector[currentQubit] = 0;
                    }
                    else
                    {
                        statevector[currentQubit] = 1;
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
                    // Add singleQubitGate component to this entity
                    //em.AddComponentData(entity, new QuantumGate { GateCode = gate.GateCode });

                    var str = $"qubit 4 alpha {quantumState.Alpha} beta {quantumState.Beta}";
                    int gateResult = Gates.ApplyGate(em, gate.GateCode, ref entity);
                    /*if (gateResult != -1 && qubitComponent.Id == 4)
                    {
                        Debug.Log(string.Format("{0} Measurement on qubit {1} returned {2}", str, qubitComponent.Id, gateResult));
                    }*/
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