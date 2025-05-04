using Unity.Entities;
using FlipperHockey;
using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

namespace FlipperHockey
{
    public struct Goal : IComponentData
    {
        public int GoalID;
    }

    public class GoalAuthoring : MonoBehaviour
    {
        public int setGoalID;
        
        class Baker : Baker<GoalAuthoring>
        {
            public override void Bake(GoalAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Goal>(entity, new Goal
                {
                    GoalID = authoring.setGoalID,
                });
                
            }

        }
    }
}