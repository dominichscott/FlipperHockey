using FlipperHockey;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;
using Unity.NetCode;

public class HealthBarUIManager : MonoBehaviour
{
    public UIDocument uiDocument;
    private ProgressBar healthSlider;

    private EntityManager entityManager;
    private EntityQuery ghostQuery;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Get the Slider from the UI Document
        var root = uiDocument.rootVisualElement;
        healthSlider = root.Q<ProgressBar>("HealthBar");

        if (healthSlider == null)
        {
            Debug.LogError("HealthSlider not found in UI Document!");
            return;
        }

        // Query for entities with HealthComponent (Ghost)
        ghostQuery = entityManager.CreateEntityQuery(typeof(HealthComponent));
    }

    private void Update()
    {

        // Check if we have any Ghost entities
        if(ghostQuery == null)
        {
            ghostQuery = entityManager.CreateEntityQuery(typeof(HealthComponent));
        }
        var entities = ghostQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

        if (entities.Length > 0)
        {
            var entity = entities[0]; // Assuming we track the first ghosted entity

            if (entityManager.HasComponent<HealthComponent>(entity))
            {
                var health = entityManager.GetComponentData<HealthComponent>(entity);

                // Update UI Slider
                healthSlider.value = health.CurrentHealth / health.MaxHealth;
                Debug.LogWarning(">>"+healthSlider.value);
            }
        }
        else
        {
           // Debug.LogWarning("No Entities from query");
        }
        entities.Dispose();
    }
}
