using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System.Numerics;

namespace QCS
{
    public class QuantumComputer : MonoBehaviour
    {
        private EntityManager entityManager;
        private int QubitCount = 0;
        private int GateCount = 0;

        public static List<Entity> qubitList = new List<Entity>();

        public static List<Entity> gateList = new List<Entity>();

        private void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));
            //gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 2));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 2, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 2, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 4));

            //gateList.Add(CreateGate(entityManager, 1, 1));
        }

        public Entity CreateQubit(EntityManager entityManager)
        {
            EntityArchetype archetype = CreateQubitArchetype(entityManager);
            var qubit = entityManager.CreateEntity(archetype);
            entityManager.SetName(qubit, "qubit");
            entityManager.AddComponentData(qubit, new QubitComponent { Id = QubitCount });
            QubitCount++;
            return qubit;
        }

        private Entity CreateGate(EntityManager entityManager)
        {
            EntityArchetype archetype = CreateGateArchetype(entityManager);
            var gate = entityManager.CreateEntity(archetype);
            entityManager.SetName(gate, "gate");
            GateCount++;
            return gate;
        }

        public Entity CreateGate(EntityManager entityManager, int gateCode, int qubit)
        {
            var gate = CreateGate(entityManager);
            entityManager.AddComponentData(gate, new SingleQubitGate { GateCode = gateCode, Qubit = qubit });
            return gate;
        }

        public Entity CreateGate(EntityManager entityManager, int gateCode, int controlQubit, int targetQubit)
        {
            var gate = CreateGate(entityManager);
            entityManager.AddComponentData(gate, new DoubleQubitGate { GateCode = gateCode, ControlQubit = controlQubit, TargetQubit = targetQubit });
            return gate;
        }

        public Entity CreateGate(EntityManager entityManager, int gateCode, int c1Qubit, int c2Qubit, int targetQubit)
        {
            var gate = CreateGate(entityManager);
            entityManager.AddComponentData(gate, new TripleQubitGate { GateCode = gateCode, C1Qubit = c1Qubit, C2Qubit = c2Qubit, TargetQubit = targetQubit });
            return gate;
        }

        [Obsolete]
        public Entity CreateGate(EntityManager entityManager, int gateCode, int[] qubits)
        {
            /* var gate = CreateGate(entityManager);
             entityManager.AddComponentData(gate, new SingleQubitGate { GateCode = gateCode, QubitId = qubits});
             return gate;*/
            return default;
        }

        private EntityArchetype CreateQubitArchetype(EntityManager entityManager)
        {
            return entityManager.CreateArchetype(
               typeof(Translation),
               typeof(Rotation),
               typeof(QuantumState),
               typeof(QubitComponent));
        }

        private EntityArchetype CreateGateArchetype(EntityManager entityManager)
        {
            return entityManager.CreateArchetype();
        }
    }
}