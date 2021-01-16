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

            //SanityCheck();

            //InitialTest_NoEncoding_AmpDamping();
            //InitialTest_NoEncoding_PhaseDamping();
            //InitialTest_BitFlip_AmpDamping();
            //InitialTest_BitFlip_PhaseDamping();
            //InitialTest_PhaseFlip_AmpDamping();
            //InitialTest_PhaseFlip_PhaseDamping();
            //InitialTest_Shor_AmpDamping();
            //InitialTest_Shor_PhaseDamping();

            //FourQubit_NoEncoding_PhaseDamping();
            //FourQubit_NoEncoding_AmpDamping();
            //FourQubit_BitFlip_PhaseDamping();
            //FourQubit_BitFlip_AmpDamping();
            //FourQubit_PhaseFlip_PhaseDamping();
            //FourQubit_PhaseFlip_AmpDamping();

            //FiveQubit_NoEncoding_PhaseDamping();
            //FiveQubit_NoEncoding_AmpDamping();
            //FiveQubit_BitFlip_PhaseDamping();
            //FiveQubit_BitFlip_AmpDamping();
            //FiveQubit_PhaseFlip_PhaseDamping();
            //FiveQubit_PhaseFlip_AmpDamping();

            //Enable errors for this
            FourQubit_RandomError();
            //FiveQubit_RandomError();
            //FourQubit_RandomError_ShorEncodedQubit();
        }

        public void SanityCheck()
        {
            qubitList.Add(CreateQubit(entityManager));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
           // gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));
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
            //Debug.Log($"benchmark statevector: {benchmark}");

            System.IO.File.AppendAllText($"{Application.dataPath}/FidelityData.txt", $"Generated statevectors:\n");

            foreach (var state in statevectorStrings)
            {
                System.IO.File.AppendAllText($"{Application.dataPath}/FidelityData.txt", $"{state}\n");
            }

            var correctStateCount = 0;
            var overallStateCount = 0;
            var groupedStatevectors = statevectorStrings.GroupBy(i => i);
            foreach (var group in groupedStatevectors)
            {
                Debug.Log($"occurrences of {group.Key}: {group.Count()}");
                System.IO.File.AppendAllText($"{Application.dataPath}/FidelityData.txt", $"occurrences of {group.Key}: {group.Count()}\n");

                if (group.Key.Equals(benchmark))
                {
                    correctStateCount = group.Count();
                }

                overallStateCount += group.Count();
            }
            Debug.Log($"It means that the fidelity of the circuit is: {(float)correctStateCount / overallStateCount}");
        }

        public Entity CreateQubit(EntityManager entityManager, bool qecQubit = false)
        {
            EntityArchetype archetype = CreateQubitArchetype(entityManager);
            var qubit = entityManager.CreateEntity(archetype);
            entityManager.SetName(qubit, "qubit");
            entityManager.AddComponentData(qubit, new QubitComponent { Id = QubitCount });
            if (qecQubit)
            {
                entityManager.AddComponentData(qubit, new ErrorCorrectionComponent { });
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

            if (noisy)
            {
                entityManager.AddComponent<NoisyChannel>(gate);
            }

            GateCount++;
            return gate;
        }

        public Entity CreateGate(EntityManager entityManager, int gateCode, int qubit, bool noisy = true, bool dampingGate = false)
        {
            var gate = CreateGate(entityManager, noisy);

            if (dampingGate)
            {
                entityManager.AddComponentData(gate, new DampingGateComponent { });
            }

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

        private void TestAmplitudeNoise()
        {
            qubitList.Add(CreateQubit(entityManager));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.PHS_DAMP, 0, true, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));
        }

        private void TestPhaseNoise()
        {
            qubitList.Add(CreateQubit(entityManager));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.PHS_DAMP, 0, true, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));
        }

        private void TestFullAdder()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));


            //gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));
            //gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
           
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 2));
            
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 2));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));
        }

        /*Initial tests*/
        private void InitialTest_NoEncoding_PhaseDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.PHS_DAMP, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));
        }

        private void InitialTest_NoEncoding_AmpDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.AMP_DAMP, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));
        }

        private void InitialTest_BitFlip_PhaseDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.PHS_DAMP, 0, true, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 2, 1, 0, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));
        }

        private void InitialTest_BitFlip_AmpDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.AMP_DAMP, 0, true, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 2, 1, 0, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));
        }

        private void InitialTest_PhaseFlip_PhaseDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 2));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.PHS_DAMP, 0, true, true));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 2));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 2, 1, 0, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));
        }

        private void InitialTest_PhaseFlip_AmpDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 2));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.AMP_DAMP, 0, true, true));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 2));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 2, 1, 0, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));
        }

        private void InitialTest_Shor_PhaseDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 3, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 6, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 3, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 6, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 3, 4, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 3, 5, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 6, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 6, 8, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.PHS_DAMP, 0, true, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: false));
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
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 6, 3, 0, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));
        }

        private void InitialTest_Shor_AmpDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 3, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 6, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 3, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 6, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 2, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 3, 4, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 3, 5, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 6, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 6, 8, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.IDENTITY, 0));
            //gateList.Add(CreateGate(entityManager, (int)GateCodes.AMP_DAMP, 0, true, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1, noisy: false));
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
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 6, 3, 0, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 0));
        }


        /*4 qubit full adder tests*/

        private void FourQubit_NoEncoding_PhaseDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.PHS_DAMP, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 2));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 2));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));
        }

        private void FourQubit_NoEncoding_AmpDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.AMP_DAMP, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 2));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 2));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));
        }

        private void FourQubit_BitFlip_PhaseDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 4, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.PHS_DAMP, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 4, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 5, 4, 1, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 2));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 2));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));
        }

        private void FourQubit_BitFlip_AmpDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 4, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.AMP_DAMP, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 4, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 5, 4, 1, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 2));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 2));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));
        }

        private void FourQubit_PhaseFlip_PhaseDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 4, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 5));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.PHS_DAMP, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 5));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 4, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 5, 4, 1, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 2));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 2));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));
        }

        private void FourQubit_PhaseFlip_AmpDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 4, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 5));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.AMP_DAMP, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 5));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 4, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 5, 4, 1, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 2));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 2));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));
        }

        /*5 qubit full adder tests*/

        private void FiveQubit_NoEncoding_PhaseDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 2, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.PHS_DAMP, 0));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 2, 4));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 4));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 4));
        }

        private void FiveQubit_NoEncoding_AmpDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 2, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.AMP_DAMP, 0));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 2, 4));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 4));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 4));
        }

        private void FiveQubit_BitFlip_PhaseDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 2, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 6, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.PHS_DAMP, 0));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 6, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 6, 5, 0, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 2, 4));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 4));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 4));
        }

        private void FiveQubit_BitFlip_AmpDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 2, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 6, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.AMP_DAMP, 0));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 6, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 6, 5, 0, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 2, 4));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 4));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 4));
        }

        private void FiveQubit_PhaseFlip_PhaseDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 2, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 6, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 5));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 6));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.PHS_DAMP, 0));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 5));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 6));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 6, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 6, 5, 0, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 2, 4));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 4));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 4));
        }

        private void FiveQubit_PhaseFlip_AmpDamping()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 2, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 6, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 5));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 6));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.AMP_DAMP, 0));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 5));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 6));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 5, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 6, noisy: true));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 6, 5, 0, noisy: true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 2, 4));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 4));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 4));
        }

        
        /*Randomized error tests*/
        private void FourQubit_RandomError()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 2));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 2));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));
        }

        private void FiveQubit_RandomError()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 2, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 4));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 2, 4));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 4));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 4));
        }

        private void FourQubit_RandomError_ShorEncodedQubit()
        {
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));
            qubitList.Add(CreateQubit(entityManager));

            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));
            qubitList.Add(CreateQubit(entityManager, true));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 0));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.X, 1));

            ///////// SHOR CODING
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 10, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 10, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 6, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 7, 8, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 7, 9, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 10, 11, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 10, 12, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 6, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 6, 5, 1, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 7, 8, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 7, 9, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 9, 8, 7, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 10, 11, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 10, 12, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 12, 11, 10, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 10, noisy: false));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 10, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 10, 7, 1, noisy: false));
            /////////////////////////////

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 0, 1, 3));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            ///////// SHOR CODING
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 10, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 10, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 6, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 7, 8, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 7, 9, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 10, 11, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 10, 12, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 6, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 6, 5, 1, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 7, 8, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 7, 9, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 9, 8, 7, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 10, 11, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 10, 12, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 12, 11, 10, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 10, noisy: false));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 10, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 10, 7, 1, noisy: false));
            /////////////////////////////

            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 1, 2, 3));

            ///////// SHOR CODING
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 10, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 10, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 6, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 7, 8, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 7, 9, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 10, 11, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 10, 12, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 5, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 6, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 6, 5, 1, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 7, 8, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 7, 9, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 9, 8, 7, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 10, 11, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 10, 12, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 12, 11, 10, noisy: false));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 1, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.HADAMARD, 10, noisy: false));


            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 7, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 10, noisy: false));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.TOFFOLI, 10, 7, 1, noisy: false));
            /////////////////////////////

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 1, 2));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.CNOT, 0, 1));

            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 2));
            gateList.Add(CreateGate(entityManager, (int)GateCodes.MEASUREMENT, 3));
        }

    }
}