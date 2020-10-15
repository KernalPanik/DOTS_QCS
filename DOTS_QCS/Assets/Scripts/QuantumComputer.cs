using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace QCS
{
    public struct Energy : IComponentData
    {
        public float Value;
    }

    public struct QuantumState : IComponentData
    {
        /// <summary>
        /// Alpha amplitude describes state |0>
        /// Beta amplitude describes state |1>
        /// </summary>
        public float Alpha;
        public float Beta;
    }

    public struct GateIdComponent : IComponentData
    {
        public int Value;
    }

    public struct SingleQubitGate : IComponentData
    {
        /// <summary>
        /// GateCode is an opcode of a gate, see the list //TODO:
        /// QubitIds is the array of qubits a gate should be applied to
        /// </summary>
        public int GateCode;
        public int Qubit;
    }

    public struct DoubleQubitGate : IComponentData
    {
        public int GateCode;
        public int ControlQubit;
        public int TargetQubit;
    }

    public struct QubitComponent : IComponentData
    {
        public int Id;
    }

    public struct SphericalCoords
    {
        public float Theta;
        public float Phi;
    }

    public struct QuantumGate : IComponentData
    {
        public int GateCode;
    }

    public struct TwoQubitEntanglementState : IComponentData
    {
        public int Alpha;
        public int Beta;
        public int Gamma;
        public int Zeta;
    }

    public class PhysicalToQuantumSystem : ComponentSystem
    {
        /// <summary>
        /// Calculate single qubit quantum state amplitudes based on the rotation of the qubit
        /// </summary>
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref Rotation rotation, ref QuantumState quantumState) =>
            {
                var coords = ExtraMath.QuaternionToSpherical(rotation.Value);
                quantumState.Alpha = math.cos(coords.Theta / 2);
                quantumState.Beta = math.sqrt(1 - math.pow(quantumState.Alpha, 2));
            });
        }
    }

    public static class ExtraMath
    {
        /// <summary>
        /// A method that converts a given quaternion into spherical coordinates (theta & phi) by 
        /// calculating a direction vector and translating it's coordinates into theta and phi //TODO: rework doc
        /// </summary>
        public static SphericalCoords QuaternionToSpherical(quaternion quat)
        {
            float3 v = math.normalize(math.mul(quat, Vector3.up));
            SphericalCoords coords = new SphericalCoords();

            coords.Phi = math.atan(v.x / v.z); // lon
            coords.Theta = math.acos(v.y / 1); // lat

            return coords;
        }


        public static float3 ToEulerAngles(quaternion q)
        {
            float3 angles;

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.value.w * q.value.x + q.value.y * q.value.z);
            double cosr_cosp = 1 - 2 * (q.value.x * q.value.x + q.value.y * q.value.y);
            angles.x = (float)math.atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.value.w * q.value.y - q.value.z * q.value.x);
            if (math.abs(sinp) >= 1)
                angles.y = (float)CopySign(math.PI / 2, sinp); // use 90 degrees if out of range
            else
                angles.y = (float)math.asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.value.w * q.value.z + q.value.x * q.value.y);
            double cosy_cosp = 1 - 2 * (q.value.y * q.value.y + q.value.z * q.value.z);
            angles.z = (float)math.atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        private static double CopySign(double a, double b)
        {
            return math.abs(a) * math.sign(b);
        }

        public static float4 TensorProduct2x2(float2 a, float2 b)
        {
            float2 r1 = a.x * b;
            float2 r2 = a.y * b;
            float4 result = new float4(r1.x, r1.y, r2.x, r2.y);
            return result;
        }

        public static float GenerateStates(int qubitCount)
        {
            return math.pow(2, qubitCount);
        }

        //Isn't it a measurement?
        public static float PickValueFromAmplitudes(float[] stateAmplitudes, int[] states)
        {
            // Find non zero states
            Dictionary<int, float> nonZeroStatePairs = new Dictionary<int, float>();
            for(int i = 0; i < states.Length; i++)
            {
                if(stateAmplitudes[i] != 0f)
                {
                    nonZeroStatePairs.Add(states[i], math.abs(math.pow(stateAmplitudes[i], 2)));
                }
            }

            // Order non zero states by amplitudes
            float largestState = 0f;
            List<KeyValuePair<int, float>> stateAmpPairs = nonZeroStatePairs.ToList();
            stateAmpPairs.Sort(
                delegate(KeyValuePair<int, float> p1, 
                KeyValuePair<int, float> p2)
                {
                    int order = p1.Value.CompareTo(p2.Value);
                    if (order > 0)
                        largestState = p1.Value;
                    else
                        largestState = p2.Value;
                    return order;
                });

            // Generate random number between 0, 99
            System.Random rand = new System.Random();
            var randomNum = (rand.NextDouble() * 99f) / 100f;

            // Pick a state based on random number
            int pickedState = 0;
            float tempAmp = 0f;
            foreach(var stateAmpPair in stateAmpPairs)
            {
                if (tempAmp < randomNum && randomNum < stateAmpPair.Value)
                {
                    pickedState = stateAmpPair.Key;
                    break;
                }
                else if (randomNum > largestState)
                {
                    pickedState = stateAmpPair.Key;
                    break;
                }
                else
                {
                    tempAmp = stateAmpPair.Value;
                }
            }

            return pickedState;
        }
    }

    public class QuantumCircuitSystem : ComponentSystem
    {
        private int executed = 0;
        protected override void OnUpdate()
        {
            //TODO: Find out how to add OR check to foreach
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (executed == 0)
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
                }
                executed = 1;
            }
        }

        private void ExecuteDoubleQubitGate(EntityManager em, DoubleQubitGate gate)
        {
            Entity controlQubit = new Entity();
            Entity targetQubit = new Entity();


            //Find Control qubit
            Entities.ForEach((Entity entity, ref QuantumState quantumState, ref Rotation rotation, ref QubitComponent qubitComponent) =>
            {
                if (gate.ControlQubit == qubitComponent.Id)
                {
                // Add singleQubitGate component to this entity
                em.AddComponentData(entity, new QuantumGate { GateCode = gate.GateCode });
                    controlQubit = entity;
                }
            });

            //Find Target qubit
            Entities.ForEach((Entity entity, ref QuantumState quantumState, ref Rotation rotation, ref QubitComponent qubitComponent) =>
            {
                if (gate.TargetQubit == qubitComponent.Id)
                {
                // Add singleQubitGate component to this entity
                em.AddComponentData(entity, new QuantumGate { GateCode = gate.GateCode });
                    targetQubit = entity;
                }
            });

            //Entangle qubits


            //Apply gate
            Gates.ApplyGate(em, gate.GateCode, ref controlQubit, ref targetQubit);
        }

        private void ExecuteSingleQubitGate(EntityManager em, SingleQubitGate gate)
        {
            Entities.ForEach((Entity entity, ref QuantumState quantumState, ref Rotation rotation, ref QubitComponent qubitComponent) =>
            {
                if (gate.Qubit == qubitComponent.Id)
                {
                    // Add singleQubitGate component to this entity
                    em.AddComponentData(entity, new QuantumGate { GateCode = gate.GateCode });
                }
            });

            // Go through all entities with attached SingleQubitGate components
            Entities.ForEach((Entity entity, ref QuantumState quantumState, ref Rotation rotation,
                ref QubitComponent qubitComponent, ref QuantumGate singleQubitGate) =>
            {
                Gates.ApplyGate(singleQubitGate.GateCode, ref rotation);
            });
        }
    }

    public class QuantumComputer : MonoBehaviour
    {
        private EntityManager entityManager;
        private int QubitCount = 0;
        private int GateCount = 0;

        public Dictionary<int, Entity> qubitDictionary = new Dictionary<int, Entity>();

        public static List<Entity> gateList = new List<Entity>();

        private void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //maybe encapsulate the dictionary?
            qubitDictionary.Add(QubitCount, CreateQubit(entityManager));

            //gateList.Add(CreateGate(entityManager, 0, 0));
            gateList.Add(CreateGate(entityManager, 2, 0));
            gateList.Add(CreateGate(entityManager, 1, 0));
            gateList.Add(CreateGate(entityManager, 0, 0));
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
            entityManager.AddComponentData(gate, new GateIdComponent { Value = GateCount });
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
               typeof(Energy),
               typeof(QuantumState),
               typeof(QubitComponent));
        }

        private EntityArchetype CreateGateArchetype(EntityManager entityManager)
        {
            return entityManager.CreateArchetype(
                typeof(GateIdComponent),
                typeof(SingleQubitGate));
        }
    }
}