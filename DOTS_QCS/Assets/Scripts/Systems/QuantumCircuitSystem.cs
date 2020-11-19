using QCS;
using System.Collections;
using System.Collections.Generic;
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

            if (executed < 1500)
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
                        var coords = ExtraMath.QuaternionToSpherical(rotation.Value);
                        quantumState.Alpha = math.cos(coords.Theta / 2);
                        quantumState.Beta = math.sqrt(1 - math.pow(quantumState.Alpha, 2));
                    });
                }
                executed += 1;
            }
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
                    int gateResult = Gates.ApplyGate(em, gate.GateCode, ref entity);
                    if (gateResult != -1)
                    {
                        Debug.Log(string.Format("Measurement on qubit {0} returned {1}", qubitComponent.Id, gateResult));
                    }
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