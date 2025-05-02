
using System.Collections.Generic;
using System.Collections;
using FlipperHockey;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UIElements;
using UnityEngine;
using Unity.NetCode;
using System;
partial struct TurretControllerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    { 
    }
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;  // Get time step

        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        //get turret
        foreach ((
               RefRW<LocalTransform> localTransform,
               RefRW<TurretComponent> turret,
               Entity entity)
               in SystemAPI.Query<
                   RefRW<LocalTransform>,
                   RefRW<TurretComponent>>().WithEntityAccess().WithAll<Simulate>())
        {
            
            List<float3> playerLocations = new List<float3>();
            //get targets
            foreach ((
                RefRW<LocalTransform> playerTransform,
                RefRW<HealthComponent> target,
                Entity playerEntity)
                in SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRW<HealthComponent>>().WithEntityAccess().WithAll<Simulate>())
            {
                playerLocations.Add(playerTransform.ValueRW.Position);
            }
            //rotate toward closest player
            float3 closestTarget = GetClosesetTarget(localTransform.ValueRW.Position, playerLocations);
            quaternion targetRot = quaternion.LookRotationSafe(closestTarget - localTransform.ValueRO.Position, math.up());

            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRot, 2f * deltaTime);
           


            if (!turret.ValueRO.reloading)
            {
                //Debug.Log("Turret Firing");
                // If already activated, just wait until the delay is over
                turret.ValueRW.reloadTimer += deltaTime;

                // Once the delay is over, reset the flag and timer
                if (turret.ValueRO.reloadTimer >= turret.ValueRO.reloadTime)
                {
                    turret.ValueRW.reloading = false;
                    turret.ValueRW.reloadTimer = 0f;
                    // Perform your action here
                    //fire a bullet
                    var prefab = SystemAPI.GetSingleton<BulletSpawner>().Bullet;
                    Entity bulletEntity = ecb.Instantiate(prefab);

                    int bulletOffset = 3;
                    var forwardDir = math.mul(localTransform.ValueRO.Rotation, Vector3.forward) * bulletOffset;
                    ecb.SetComponent(bulletEntity, new Bullet { hasHit = 0, hittable = false, ownerNetworkID = 999, timer = 5f, damageMult = 1f });

                    ecb.SetComponent(bulletEntity, LocalTransform.FromPositionRotation(localTransform.ValueRO.Position + forwardDir, localTransform.ValueRO.Rotation));
                    ecb.SetComponent(bulletEntity, new GhostOwner { NetworkId = 999 });
                }
            }
            else
            {
                // If not activated, you can reset delay if needed
                turret.ValueRW.reloadTimer = 0f;
                turret.ValueRW.reloading = false;
                Debug.Log("Turret Bugged");
            }




        }
        ecb.Playback(state.EntityManager);
    }

    private object ActivateAfterDelay()
    {
        throw new NotImplementedException();
    }

    private float3 GetClosesetTarget(float3 position, List<float3> playerLocations)
    {
        float3 closestTar = new float3(9999,9999,9999);
        foreach (float3 playerLoc in playerLocations)
        {
            float temp = math.distance(position, playerLoc);
            if (temp < math.distance(position, closestTar))
            {
                closestTar = playerLoc;
            }
        }               
        return closestTar;
    }
}
