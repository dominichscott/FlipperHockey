using Unity.Entities;
using UnityEngine;

namespace FlipperHockey
{
    /// <summary>
    /// Flag component to mark an entity as a Bullet.
    /// </summary>
    public struct Bullet : IComponentData
    {
        public float timer;
        public byte hasHit;
        public bool hittable;
        public float ownerNetworkID;
        public float damageMult;
        public float hitPlayerNetworkID;
        internal bool killed;
    }

    /// <summary>
    /// The authoring component for the Bullet.
    /// </summary>
    [DisallowMultipleComponent]
    public class BulletAuthoring : MonoBehaviour
    {
        class Baker : Baker<BulletAuthoring>
        {
            public override void Bake(BulletAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Bullet>(entity, new Bullet
                {
                    timer = 5f,
                    hasHit = 0,
                    hittable = false,
                    ownerNetworkID = -1f,
                    damageMult = 1f,
                    hitPlayerNetworkID = -1f,
                    killed = false
                });
            }
        }
    }
}
