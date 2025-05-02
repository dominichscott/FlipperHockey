using Unity.Entities;
using UnityEngine;
namespace FlipperHockey
{
    /// <summary>
    /// Component data that identifies a cube spawner and gives access to the cube prefab.
    /// </summary>
    public struct BulletSpawner : IComponentData
    {
        /// <summary>
        /// The Cube prefab converted to an entity.
        /// </summary>
        public Entity Bullet;
    }
    /// <summary>
    /// Baker that transforms our cube prefab into an entity and creates a spawner entity.
        /// </summary>
        [DisallowMultipleComponent]
    public class BulletSpawnerAuthoring : MonoBehaviour
    {
        /// <summary>
        /// The cube prefab to spawn.
        /// </summary>
        public GameObject Bullet;
        class SpawnerBaker : Baker<BulletSpawnerAuthoring>
        {
            public override void Bake(BulletSpawnerAuthoring authoring)
            {
                BulletSpawner component = default(BulletSpawner);
                component.Bullet = GetEntity(authoring.Bullet,
                    TransformUsageFlags.Dynamic);
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, component);
            }
        }
    }
}