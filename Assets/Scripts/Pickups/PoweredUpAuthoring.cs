using FlipperHockey;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[GhostComponent]
public struct PoweredUpComponent : IComponentData
{
    [GhostField] public float poweredUpMultiplier;

}

[DisallowMultipleComponent]
public class PoweredUpAuthoring : MonoBehaviour
{
    class Baker : Baker<PoweredUpAuthoring>
    {
        public override void Bake(PoweredUpAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PoweredUpComponent>(entity, new PoweredUpComponent
            {
                poweredUpMultiplier = 1f
            });


        }

    }
}