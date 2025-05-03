using Unity.Entities;
using UnityEngine;

namespace FlipperHockey
{
    public struct PuckSpawner : IComponentData
    {
        public Entity Puck;
    }
    
    [DisallowMultipleComponent]
    public class PuckSpawnerAuthoring : MonoBehaviour
    {
        public GameObject Puck;
        
        class SpawnerBaker : Baker<PuckSpawnerAuthoring>
        {
            public override void Bake(PuckSpawnerAuthoring authoring)
            {
                PuckSpawner component = default(PuckSpawner);
                component.Puck = GetEntity(authoring.Puck,
                    TransformUsageFlags.Dynamic);
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, component);
            }
        }
    }
}