using FlipperHockey;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;


partial struct CollisionHandler : ISystem
{
    private EndSimulationEntityCommandBufferSystem _ecbSystem;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
        CollisionSimulationJob simulationJob = new CollisionSimulationJob
        {
            PlayerHealthLookup = SystemAPI.GetComponentLookup<HealthComponent>(),
            //BulletLookup = SystemAPI.GetComponentLookup<Bullet>(),
            HPBoxLookup = SystemAPI.GetComponentLookup<HPBox>(),
            PowerBoxLookup = SystemAPI.GetComponentLookup<PowerBox>(),
            PoweredUpComponentLookup = SystemAPI.GetComponentLookup<PoweredUpComponent>(),
            LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
            PuckLookup = SystemAPI.GetComponentLookup<Puck>(),
        };
        state.Dependency = simulationJob.Schedule(
            SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

[WithAll(typeof(Simulate))]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSimulationGroup))]
[BurstCompile]
public partial struct CollisionSimulationJob : ICollisionEventsJob
{
    public ComponentLookup<HealthComponent> PlayerHealthLookup;
    //public ComponentLookup<Bullet> BulletLookup;
    public ComponentLookup<HPBox> HPBoxLookup;
    public ComponentLookup<PowerBox> PowerBoxLookup;
    public ComponentLookup<PoweredUpComponent> PoweredUpComponentLookup;
    public ComponentLookup<LocalTransform> LocalTransformLookup;
    public ComponentLookup<Puck> PuckLookup;

    public void Execute(CollisionEvent collisionEvent)
    {
        Entity entityA = collisionEvent.EntityA;
        Entity entityB = collisionEvent.EntityB;
        
        // Only handle collisions where puck is involved
        bool aIsPuck = PuckLookup.HasComponent(entityA);
        bool bIsPuck = PuckLookup.HasComponent(entityB);
        if (!aIsPuck && !bIsPuck) return;
        
        // Assigns entities based on which one the puck is (note that if both are pucks, both will need to do an event)
        Entity puckEntity = aIsPuck ? entityA : entityB;
        Entity otherEntity = aIsPuck ? entityB : entityA;
        
        var normal = collisionEvent.Normal;
        
        Debug.Log("Wall Collision!");
        if (PuckLookup.TryGetComponent(puckEntity, out var puck))
        {
            puck.ShouldReflect = true;
            puck.SurfaceNormal = normal;

            
            if (LocalTransformLookup.TryGetComponent(otherEntity, out var otherTransform))
            {
                
            }
        }
        
        /*
        if(PlayerHealthLookup.TryGetComponent(collisionEvent.EntityB, out HealthComponent health))
        {
            if (BulletLookup.TryGetComponent(collisionEvent.EntityA, out Bullet bullet))
            {
                if (bullet.hasHit != 1 && bullet.hittable)
                {
                    health.CurrentHealth -= 50f * bullet.damageMult;
                    PlayerHealthLookup[collisionEvent.EntityB] = health;
                    Debug.Log("I am #: " + health.ownerNetworkID);
                    Debug.Log("Owww My health is: " + health.CurrentHealth);
                    Debug.Log("I was hit by: " + bullet.ownerNetworkID);
                    Debug.Log("DMG Mult: " + bullet.damageMult);

                    if(health.CurrentHealth <= 0)
                    {
                        Debug.Log("You dead?");
                        bullet.killed = true;
                    }
                    
                    bullet.hasHit = 1;
                    bullet.timer = 0;
                    bullet.hittable = false;
                    bullet.hitPlayerNetworkID = health.ownerNetworkID;
                    BulletLookup[collisionEvent.EntityA] = bullet;
                }
            }
            else if(HPBoxLookup.TryGetComponent(collisionEvent.EntityA, out HPBox hpBox))
            {
                float healthPickUpValue = 25f;
                //doo health math
                if (hpBox.hasbeenPickedUp != 1)
                {
                    if (health.CurrentHealth + healthPickUpValue >= health.MaxHealth)
                    {
                        Debug.LogWarning("Set to Max HP");
                        health.CurrentHealth = health.MaxHealth;
                    }
                    else if (health.CurrentHealth + healthPickUpValue < health.MaxHealth)
                    {
                        Debug.LogWarning("Set to Curr HP +"+healthPickUpValue);
                        health.CurrentHealth += healthPickUpValue;
                    }
                    else if (health.CurrentHealth <= 0)
                    {
                        Debug.Log("You dead");
                        //do death scene
                        bullet.killed = true;
                    }
                    PlayerHealthLookup[collisionEvent.EntityB] = health;
                    //doo stuff to HPBox
                    hpBox.hasbeenPickedUp = 1;
                    hpBox.destroy = true;
                    //add me
                    HPBoxLookup[collisionEvent.EntityA] = hpBox; 
                }
            }
            else if (PowerBoxLookup.TryGetComponent(collisionEvent.EntityA, out PowerBox powerBox) && PoweredUpComponentLookup.TryGetComponent(collisionEvent.EntityB, out PoweredUpComponent powerUp))
            {
                if (powerBox.hasbeenPickedUp != 1)
                {
                    Debug.LogWarning("Unlimited POWER!!");
                    //add mult to player damage
                    powerUp.poweredUpMultiplier = 5f;
                    PoweredUpComponentLookup[collisionEvent.EntityB] = powerUp; 

                    //destroy power cube on pickup
                    powerBox.hasbeenPickedUp = 1;
                    powerBox.destroy = true;
                    //add me
                    PowerBoxLookup[collisionEvent.EntityA] = powerBox;
                    
                }
            }
        }
       */
    }
}