using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

namespace FlipperHockey
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial struct PuckManager : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PuckSpawner>();
            state.RequireForUpdate<NetworkTime>();
            
            Entity stateEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(stateEntity, new PuckManagerState { puckSpawned = false });
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Ensure NetworkTime singleton exists before continuing
            state.RequireForUpdate<Unity.NetCode.NetworkTime>();
            var puckSpawnPointsQuery = SystemAPI.QueryBuilder().WithAll<PuckSpawnPoint, LocalToWorld>().Build();
            var puckSpawnPointList = puckSpawnPointsQuery.ToComponentDataArray<LocalToWorld>(Allocator.Temp);
            
            NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var prefab = SystemAPI.GetSingleton<PuckSpawner>().Puck;
            EntityCommandBuffer ecb = new
                EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach ((
                         var playerInput,
                         var localTransform,
                         var ghostOwner)
                     in SystemAPI.Query<
                         RefRW<CubeInput>,
                         RefRO<LocalTransform>,
                         RefRO<GhostOwner>>().WithAll<Simulate>())
            {
                if (networkTime.IsFirstTimeFullyPredictingTick)
                {
                    /*
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
                    */

                    var puckState = SystemAPI.GetSingletonRW<PuckManagerState>();
                    
                    if (playerInput.ValueRO.spawnPuck.IsSet && !puckState.ValueRO.puckSpawned)
                    {
                        Debug.Log("Spawning Puck");
                        Entity puckEntity = ecb.Instantiate(prefab);

                        int puckSelectionIndex = UnityEngine.Random.Range(0, puckSpawnPointList.Length);
                        int puckDirectionIndex = UnityEngine.Random.Range(0, 2);
                        float puckDirection = 0;
                        
                        if (puckDirectionIndex == 0)
                        {
                            puckDirection = -1;
                        }
                        else
                        {
                            puckDirection = 1;
                        }
                        
                        ecb.SetComponent(puckEntity, new Puck{ Direction = new float3( 0, 0, puckDirection), ShouldReflect = false, SurfaceNormal = new float3(0, 0, 0), Speed = 5f, MaxSpeed = 10f, destroy = false});
                        ecb.SetComponent(puckEntity, LocalTransform.FromPositionRotation(puckSpawnPointList[puckSelectionIndex].Position, puckSpawnPointList[puckSelectionIndex].Rotation));
                        ecb.SetComponent(puckEntity, new GhostOwner { NetworkId = ghostOwner.ValueRO.NetworkId });

                        puckState.ValueRW.puckSpawned = true;
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
    
    public struct PuckManagerState : IComponentData
    {
        public bool puckSpawned;
    }
}