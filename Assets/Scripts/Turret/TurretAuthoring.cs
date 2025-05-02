using Unity.Entities;
using UnityEngine;



    /// <summary>
    /// Flag component to mark an entity as a Bullet.
    /// </summary>
    public struct TurretComponent : IComponentData
    {
        public float shootTimer;
        public bool reloading;     // Flag for activation state
        public float reloadTime;      // Time to wait
        public float reloadTimer;
    }

    /// <summary>
    /// The authoring component for the Bullet.
    /// </summary>
    [DisallowMultipleComponent]
    public class TurretAuthoring : MonoBehaviour
    {
        class Baker : Baker<TurretAuthoring>
        {
            public override void Bake(TurretAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<TurretComponent>(entity, new TurretComponent
            {
                shootTimer = 2f,
                reloading = false,
                reloadTime = 4,
                reloadTimer =4
            });
            }
        }
    }



