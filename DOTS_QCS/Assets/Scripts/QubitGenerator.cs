using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public struct Energy : IComponentData
{
    public float Value;
}

public struct QuantumState : IComponentData
{
    public float Value;
}

//TODO: Physical-To-Quantum Transform system
//TODO: Proper Qubit Generator
//TODO: X Gate system

public class PhysicalToQuantumSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Energy energy, ref QuantumState quantumState) => 
        {

        });
    }
}

public class QubitGenerator : MonoBehaviour
{
    private EntityManager entityManager;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityArchetype archetype = CreateQubitArchetype(entityManager);

        Entity entity = entityManager.CreateEntity(archetype);
        // TODO: Implement a quick degree to radian conversion extension
    }

    private EntityArchetype CreateQubitArchetype(EntityManager entityManager)
    {
        //TODO: What should be done in such a case? Create an archetype with a lot of different components, or create a signle component to hold them all?
        return entityManager.CreateArchetype(
           typeof(Translation),
           typeof(Rotation),
           typeof(Energy),
           typeof(QuantumState));
    }
}
