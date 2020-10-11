using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using System.Collections.Generic;

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

public struct GateComponent : IComponentData
{
    /// <summary>
    /// GateCode is an opcode of a gate, see the list //TODO:
    /// QubitIds is the array of qubits a gate should be applied to
    /// </summary>
    public int GateCode;
    public int QubitId;
}

public struct QubitComponent : IComponentData
{
    public int QubitId;
}

public struct SphericalCoords
{
    public float Theta;
    public float Phi;
}

public struct SingleQubitGate : IComponentData
{
    public int ExecutionOrder;
    public int GateCode;
}

public class PhysicalToQuantumSystem : ComponentSystem
{
    /// <summary>
    /// Calculate quantum state amplitudes based on the rotation of the qubit
    /// cos(theta/2)* state_0 + e^(i*fi)*sin(theta/2)* state_1
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

}

public class QuantumCircuit : ComponentSystem
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
                var gateComponent = em.GetComponentData<GateComponent>(gate);

                // Find qubits which should be modified
                Entities.ForEach((Entity entity, ref QuantumState quantumState, ref Rotation rotation, ref QubitComponent qubitComponent) =>
                {
                    if (gateComponent.QubitId == qubitComponent.QubitId)
                    {
                    // Add singleQubitGate component to this entity
                    // Pretty sure that ExecutionOrder is not needed in this case
                    em.AddComponentData(entity, new SingleQubitGate { ExecutionOrder = 0, GateCode = gateComponent.GateCode });
                    }
                });

                // Go through all entities with attached SingleQubitGate components
                Entities.ForEach((Entity entity, ref QuantumState quantumState, ref Rotation rotation,
                    ref QubitComponent qubitComponent, ref SingleQubitGate singleQubitGate) =>
                {
                    Gates.ApplyGate(singleQubitGate.GateCode, ref rotation);
                });

                // Remove all Attached single qubit gate components 
                Entities.ForEach((Entity entity) =>
                {
                //em.RemoveComponent<SingleQubitGate>(entity);
                });
            }
            //executed = 1;
        }
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

        /*gateList.Add(CreateGate(entityManager, 0, 0));
        gateList.Add(CreateGate(entityManager, 0, 2));*/
        //gateList.Add(CreateGate(entityManager, 2, 2));

        //Apply I gate to the qubit with id 0
        /*gateList.Add(CreateGate(entityManager, 2, 0));
        //gateList.Add(CreateGate(entityManager, 1, 0));
        gateList.Add(CreateGate(entityManager, 0, 0));*/
        //gateList.Add(CreateGate(entityManager, 1, 0));
    }

    public Entity CreateQubit(EntityManager entityManager)
    {
        EntityArchetype archetype = CreateQubitArchetype(entityManager);
        var qubit = entityManager.CreateEntity(archetype);
        entityManager.SetName(qubit, "qubit");
        entityManager.AddComponentData(qubit, new QubitComponent { QubitId = QubitCount });
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
        entityManager.AddComponentData(gate, new GateComponent { GateCode = gateCode, QubitId = qubit});
        return gate;
    }

    public Entity CreateGate(EntityManager entityManager, int gateCode, int[] qubits)
    {
        /* var gate = CreateGate(entityManager);
         entityManager.AddComponentData(gate, new GateComponent { GateCode = gateCode, QubitId = qubits});
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
            typeof(GateComponent));
    }
}
