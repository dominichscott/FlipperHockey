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
        state.RequireForUpdate<PuckManager>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        Entity puckManagerEntity = entityManager
            .CreateEntityQuery(typeof(PuckManagerState))
            .GetSingletonEntity();
        
        CollisionSimulationJob simulationJob = new CollisionSimulationJob
        {
            PlayerHealthLookup = SystemAPI.GetComponentLookup<HealthComponent>(),
            //BulletLookup = SystemAPI.GetComponentLookup<Bullet>(),
            SpeedBoxLookup = SystemAPI.GetComponentLookup<SpeedBox>(),
            LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
            PuckLookup = SystemAPI.GetComponentLookup<Puck>(),
            GoalLookup = SystemAPI.GetComponentLookup<Goal>(),
            PuckManagerStateLookup = SystemAPI.GetComponentLookup<PuckManagerState>(),
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
    public Entity PuckManagerEntity;
    public ComponentLookup<HealthComponent> PlayerHealthLookup;
    public ComponentLookup<SpeedBox> SpeedBoxLookup;
    public ComponentLookup<LocalTransform> LocalTransformLookup;
    public ComponentLookup<Puck> PuckLookup;
    public ComponentLookup<Goal> GoalLookup;
    public ComponentLookup<PuckManagerState> PuckManagerStateLookup;

    public void Execute(CollisionEvent collisionEvent)
    {
        Debug.Log("Puck Collision!");
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
        
        
        if (PuckLookup.TryGetComponent(puckEntity, out var puck))
        {
            puck.ShouldReflect = true;
            puck.SurfaceNormal = normal;

            // If collision happens with the Speed Box
            if (SpeedBoxLookup.TryGetComponent(otherEntity, out var otherSpeedBox))
            {
                if (otherSpeedBox.hasbeenPickedUp != 1)
                {
                    puck.Speed = puck.MaxSpeed;
                    otherSpeedBox.hasbeenPickedUp = 1;
                    otherSpeedBox.destroy = true;
                    SpeedBoxLookup[otherEntity] = otherSpeedBox;
                }
            }
            else if (GoalLookup.TryGetComponent(otherEntity, out var foundGoal))
            {
                puck.destroy = true;
                // Change stat to indicate Puck has despawned
                var puckManagerState = PuckManagerStateLookup[PuckManagerEntity];
                puckManagerState.puckSpawned = false;
                PuckManagerStateLookup[PuckManagerEntity] = puckManagerState;

                
                
            }
            else if (LocalTransformLookup.TryGetComponent(otherEntity, out var otherTransform))
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