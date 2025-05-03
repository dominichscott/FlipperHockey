using Unity.Entities;
using UnityEngine;
namespace FlipperHockey
{
    public class PuckSpawnPointAuthoring : MonoBehaviour
    {
        public class Baker : Baker<PuckSpawnPointAuthoring>
        {
            public override void Bake(PuckSpawnPointAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PuckSpawnPoint());
            }
        }
    }
    
    // Spawn Points used to determine potential placements for the Pucks
    public struct PuckSpawnPoint : IComponentData
    {
    }
}