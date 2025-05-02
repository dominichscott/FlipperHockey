using Unity.Entities;
using UnityEngine;

namespace FlipperHockey
{
    public struct PickupSpawnerPowerUp : IComponentData
    {
        public Entity PickupObjAsEnt;
        public byte hasObject;
        public float timer;
    }

    [DisallowMultipleComponent]
    public class PickupSpawnerPowerUpAuthoring : MonoBehaviour
    {
        public GameObject PickupObj;

        class SpawnerBaker : Baker<PickupSpawnerPowerUpAuthoring>
        {
            public override void Bake(PickupSpawnerPowerUpAuthoring authoring)
            {
                PickupSpawnerPowerUp component = default(PickupSpawnerPowerUp);
                component.PickupObjAsEnt = GetEntity(authoring.PickupObj, TransformUsageFlags.Dynamic);
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, component);
            }
        }
    }
}
