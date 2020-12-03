using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System.Numerics;
using UnityEditor;

namespace QCS
{
    public class QuantumComputer : MonoBehaviour
    {
        private EntityManager entityManager;
        public static int LogicalQubitCount = 0;
        private int QubitCount = 0;
        private int GateCount = 0;

        public static List<Entity> qubitList = new List<Entity>();

        public static List<Entity> gateList = new List<Entity>();

        private void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //TODO: Create a better interface to compose circuits
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));

            /*qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));*/

            /*gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            */

            //gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            //gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));


            /*gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2));

            /*
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 2));
            */

            /*gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));

            /*gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 2));*/

            /*gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 2, 1, 0));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));
            */

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0, noisy:false));



            // PHASE FLIP CODE
            /*
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 2));
            

            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 2));
            
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 2, 1, 0, noisy: false));
            */


            //SHOR'S CODE
            /*gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 3, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 6, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 3, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 6, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 3, 4, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 3, 5, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 6, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 6, 8, noisy: false));*/

            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));

            /*gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 2, 1, 0, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 3, 4, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 3, 5, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 5, 4, 3, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 6, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 6, 8, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 8, 7, 6, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 3, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 6, noisy: false));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 3, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 6, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 6, 3, 0, noisy: false));*/

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));

            /*
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
            */
            //gateList.Add(CreateGate(entityManager, 1, 1));
        }

        [MenuItem("QCS/Calculate fidelity")]
        public static void CalculateFidelity()
        {
            List<float> statevectorDistances = new List<float>();
            List<string> statevectorStrings = new List<string>();

            if (!QuantumCircuitSystem.CircuitWorking)
            {
                // Get all statevectors and perform computation analysis
                var statevectors = QuantumCircuitSystem.StatevectorList;

                foreach (var statevector in statevectors)
                {
                    var expandedStatevector = ExtraMath.ExpandStatevector(statevector);
                    statevectorStrings.Add(string.Join("", statevector.ToList().ConvertAll(i => i.ToString()).ToArray()));
                }
            }
            else
            {
                Debug.LogWarning("Quantum computer simulation is still running, can't calculate fidelity.");
            }

            var benchmark = string.Join("", QuantumCircuitSystem.BenchhmarkStatevector.ToList().ConvertAll(i => i.ToString()).ToArray());
            Debug.Log($"benchmark statevector: {benchmark}");

            var correctStateCount = 0;
            var overallStateCount = 0;
            var groupedStatevectors = statevectorStrings.GroupBy(i => i);
            foreach(var group in groupedStatevectors)
            {
                Debug.Log($"occurrences of {group.Key}: {group.Count()}");
                if(group.Key.Equals(benchmark))
                {
                    correctStateCount = group.Count();
                }

                overallStateCount += group.Count();
            }
            Debug.Log($"It means that the fidelity of the circuit is: {(float)correctStateCount/overallStateCount}");
        }

        public Entity CreateQubit(EntityManager entityManager, bool qecQubit=false)
        {
            EntityArchetype archetype = CreateQubitArchetype(entityManager);
            var qubit = entityManager.CreateEntity(archetype);
            entityManager.SetName(qubit, "qubit");
            entityManager.AddComponentData(qubit, new QubitComponent { Id = QubitCount });
            if(qecQubit)
            {
                entityManager.AddComponentData(qubit, new ErrorCorrectionComponent {});
            }
            else
            { 
                LogicalQubitCount++;
            }
            QubitCount++;
            return qubit;
        }

        private Entity CreateGate(EntityManager entityManager, bool noisy = true)
        {
            EntityArchetype archetype = CreateGateArchetype(entityManager);
            var gate = entityManager.CreateEntity(archetype);
            entityManager.SetName(gate, "gate");

            if(noisy)
            {
                entityManager.AddComponent<NoisyChannel>(gate);
            }

            GateCount++;
            return gate;
        }

        public Entity CreateGate(EntityManager entityManager, int gateCode, int qubit, bool noisy = true)
        {
            var gate = CreateGate(entityManager, noisy);
            entityManager.AddComponentData(gate, new SingleQubitGate { GateCode = gateCode, Qubit = qubit });
            return gate;
        }

        public Entity CreateGate(EntityManager entityManager, int gateCode, int controlQubit, int targetQubit, bool noisy = true)
        {
            var gate = CreateGate(entityManager, noisy);
            entityManager.AddComponentData(gate, new DoubleQubitGate { GateCode = gateCode, ControlQubit = controlQubit, TargetQubit = targetQubit });
            return gate;
        }

        public Entity CreateGate(EntityManager entityManager, int gateCode, int c1Qubit, int c2Qubit, int targetQubit, bool noisy = true)
        {
            var gate = CreateGate(entityManager, noisy);
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