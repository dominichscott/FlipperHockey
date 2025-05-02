using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
namespace FlipperHockey
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    partial struct ShootManager : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BulletSpawner>();
            state.RequireForUpdate<NetworkTime>();
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Ensure NetworkTime singleton exists before continuing
            state.RequireForUpdate<Unity.NetCode.NetworkTime>();
            
            NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var prefab = SystemAPI.GetSingleton<BulletSpawner>().Bullet;
            EntityCommandBuffer ecb = new
                EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach ((
                         var playerInput,
                         var localTransform,
                         var ghostOwner,
                         var poweredUpComp)
                     in SystemAPI.Query<
                         RefRW<CubeInput>,
                         RefRO<LocalTransform>,
                         RefRO<GhostOwner>,
                         RefRO<PoweredUpComponent>>().WithAll<Simulate>())
            {
                if (networkTime.IsFirstTimeFullyPredictingTick)
                {
                    if (playerInput.ValueRO.shoot.IsSet)
                    {
                        //Debug.LogWarning("Shoot Input");
                        Entity bulletEntity = ecb.Instantiate(prefab);

                        int bulletOffset = 3;
                        var forwardDir = math.mul(localTransform.ValueRO.Rotation, Vector3.forward) * bulletOffset;
                        ecb.SetComponent(bulletEntity, new Bullet { hasHit = 0, hittable = false, ownerNetworkID = ghostOwner.ValueRO.NetworkId, timer = 5f, damageMult = poweredUpComp.ValueRO.poweredUpMultiplier });

                        ecb.SetComponent(bulletEntity, LocalTransform.FromPositionRotation(localTransform.ValueRO.Position + forwardDir, localTransform.ValueRO.Rotation));
                        ecb.SetComponent(bulletEntity, new GhostOwner { NetworkId = ghostOwner.ValueRO.NetworkId });
                    }
                }
            }
            ecb.Playback(state.EntityManager);
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}