using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Boids
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private Material unitMaterial;
        [SerializeField] private Material targetMaterial;
        [SerializeField] private Mesh quadMesh;

        private static EntityManager _entityManager;

        public float WorldSize = 1;

        private int entityCount;

        [SerializeField] private TextMeshProUGUI EntityCountText;

        private World _defaultWorld;
        private List<ComponentSystemBase> _systems;

        public int EntityCount
        {
            get => entityCount;
            set
            {
                entityCount = value;
                EntityCountText.text = $"Entities: {entityCount}";
            }
        }

        void Start()
        {
            _systems = new List<ComponentSystemBase>();
            _defaultWorld = World.DefaultGameObjectInjectionWorld;
            _entityManager = _defaultWorld.EntityManager;

            //_systems.Add(_defaultWorld.GetOrCreateSystem<FindTargetJobSystem>());
            //_systems.Add(_defaultWorld.GetOrCreateSystem<UnitMoveToTargetJobSystem>());

            for (int i = 0; i < 5000; i++)
            {
                SpawnUnitEntity();
            }

            for (int i = 0; i < 1000; i++)
            {
                SpawnTargetEntity();
            }
        }

        private float spawnTimer;

        private void Update()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].Update();
            }

            EntityCount = _entityManager.GetAllEntities().Length;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer < 0)
            {
                spawnTimer = 0.1f;

                for (int i = 0; i < 300; i++)
                {
                    SpawnTargetEntity();
                }
            }
        }

        private void SpawnUnitEntity()
        {
            SpawnUnitEntity(new float3(new float3(UnityEngine.Random.Range(-12f, 12f),
                UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-10f, 10f)) * WorldSize));
        }

        private void SpawnUnitEntity(float3 position)
        {
            Entity entity = _entityManager.CreateEntity(
                typeof(Translation),
                typeof(LocalToWorld),
                typeof(RenderMesh),
                typeof(RenderBounds),
                typeof(Scale),
                typeof(Boid)
            );
            SetEntityComponentData(entity, position, quadMesh, unitMaterial);
            _entityManager.SetComponentData(entity, new Scale {Value = 1.5f});
        }

        private void SpawnTargetEntity()
        {
            Entity entity = _entityManager.CreateEntity(
                typeof(Translation),
                typeof(LocalToWorld),
                typeof(RenderMesh),
                typeof(RenderBounds),
                typeof(Scale),
                typeof(Target)
            );
            SetEntityComponentData(entity,
                new float3(UnityEngine.Random.Range(-12f, 12f), UnityEngine.Random.Range(-8f, 8f),
                    UnityEngine.Random.Range(-10f, 10f)) * WorldSize, quadMesh, targetMaterial);
            _entityManager.SetComponentData(entity, new Scale {Value = 0.5f});
        }

        private void SetEntityComponentData(Entity entity, float3 spawnPosition, Mesh mesh, Material material)
        {
            _entityManager.SetSharedComponentData(entity, new RenderMesh()
            {
                material = material,
                mesh = mesh
            });

            _entityManager.SetComponentData(entity,
                new Translation
                {
                    Value = spawnPosition
                });
        }
    }
}