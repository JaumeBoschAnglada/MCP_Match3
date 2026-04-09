using System.Collections.Generic;
using UnityEngine;
using Match3.Data;

namespace Match3.Effects
{
    /// <summary>
    /// Manages particle effects with object pooling.
    /// Each piece color has its own pool of ParticleSystem prefabs.
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        private static EffectManager instance;
        public static EffectManager Instance => instance;

        [SerializeField] private GameObject particleSystemPrefab;
        [SerializeField] private Transform effectsParent;
        
        private Dictionary<ColorType, Queue<ParticleSystem>> effectPool = new Dictionary<ColorType, Queue<ParticleSystem>>();
        private Dictionary<ParticleSystem, float> activeEffects = new Dictionary<ParticleSystem, float>();
        
        private const int INITIAL_POOL_SIZE = 10;
        private Color[] colorMap;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            // If no parent specified, create one
            if (effectsParent == null)
            {
                GameObject parent = new GameObject("Effects");
                effectsParent = parent.transform;
            }

            InitializeColorMap();
            InitializePools();
        }

        private void InitializeColorMap()
        {
            colorMap = new Color[(int)ColorType.Yellow + 1];
            colorMap[(int)ColorType.Red] = new Color(0.9f, 0.15f, 0.15f);
            colorMap[(int)ColorType.Blue] = new Color(0.15f, 0.4f, 0.9f);
            colorMap[(int)ColorType.Green] = new Color(0.15f, 0.8f, 0.2f);
            colorMap[(int)ColorType.Yellow] = new Color(0.95f, 0.85f, 0.1f);
        }

        private GameObject CreateDefaultParticlePrefab()
        {
            GameObject go = new GameObject("DefaultParticle");
            go.SetActive(false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = 0.6f;
            main.startSpeed = 3f;
            main.startSize = 0.15f;
            main.gravityModifier = 0.5f;
            main.loop = false;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12) });
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            DontDestroyOnLoad(go);
            return go;
        }

        private void InitializePools()
        {
            if (particleSystemPrefab == null)
            {
                Debug.LogWarning("[EffectManager] No particleSystemPrefab assigned. Creating programmatic particles.");
                particleSystemPrefab = CreateDefaultParticlePrefab();
            }

            ColorType[] colorTypes = { ColorType.Red, ColorType.Blue, ColorType.Green, ColorType.Yellow };
            
            foreach (ColorType type in colorTypes)
            {
                effectPool[type] = new Queue<ParticleSystem>();
                
                for (int i = 0; i < INITIAL_POOL_SIZE; i++)
                {
                    CreateParticleSystem(type);
                }
            }
        }

        private void CreateParticleSystem(ColorType type)
        {
            if (particleSystemPrefab == null)
            {
                Debug.LogError("[EffectManager] ParticleSystem prefab not assigned!");
                return;
            }

            GameObject go = Instantiate(particleSystemPrefab, effectsParent);
            go.name = $"Particle_{type}";
            go.SetActive(false);
            
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                ps = go.AddComponent<ParticleSystem>();
            }

            // Set particle color based on type
            var main = ps.main;
            main.startColor = colorMap[(int)type];

            effectPool[type].Enqueue(ps);
        }

        /// <summary>
        /// Spawn a particle effect at the specified position with the given color.
        /// </summary>
        public void SpawnEffect(Vector3 position, ColorType colorType)
        {
            if (colorType == ColorType.Empty)
            {
                Debug.LogWarning($"[EffectManager] Cannot spawn effect for type {colorType}");
                return;
            }

            ParticleSystem ps = GetFromPool(colorType);
            if (ps == null)
            {
                Debug.LogError($"[EffectManager] Failed to get particle system from pool for type {colorType}");
                return;
            }

            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            ps.Play();

            // Store duration for cleanup
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            activeEffects[ps] = Time.time + duration + 0.5f; // Extra buffer
        }

        /// <summary>
        /// Spawn a larger burst effect when a special piece is created.
        /// </summary>
        public void SpawnCreationEffect(Vector3 position, ColorType colorType)
        {
            if (colorType == ColorType.Empty) return;

            ParticleSystem ps = GetFromPool(colorType);
            if (ps == null) return;

            ps.transform.position = position;
            ps.gameObject.SetActive(true);

            // Temporarily boost size and count for creation effect
            var main = ps.main;
            main.startSize = 0.25f;
            main.startSpeed = 5f;

            var emission = ps.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });

            ps.Play();

            float duration = main.duration + main.startLifetime.constantMax;
            activeEffects[ps] = Time.time + duration + 0.5f;
        }

        private ParticleSystem GetFromPool(ColorType type)
        {
            if (!effectPool.TryGetValue(type, out var queue))
            {
                Debug.LogWarning($"[EffectManager] No pool for type {type}");
                return null;
            }

            if (queue.Count > 0)
            {
                return queue.Dequeue();
            }

            // Create new if pool is empty
            CreateParticleSystem(type);
            return queue.Dequeue();
        }

        private void Update()
        {
            // Return finished effects to pool
            List<ParticleSystem> finishedEffects = new List<ParticleSystem>();

            foreach (var (ps, endTime) in activeEffects)
            {
                if (ps == null || !ps.gameObject.activeSelf)
                {
                    finishedEffects.Add(ps);
                }
                else if (Time.time >= endTime)
                {
                    finishedEffects.Add(ps);
                }
            }

            foreach (var ps in finishedEffects)
            {
                if (ps != null)
                {
                    ps.gameObject.SetActive(false);
                    
                    // Determine color type from particle color
                    var main = ps.main;
                    ColorType type = GetColorType(main.startColor.color);
                    
                    if (effectPool.ContainsKey(type))
                    {
                        effectPool[type].Enqueue(ps);
                    }
                }
                activeEffects.Remove(ps);
            }
        }

        private ColorType GetColorType(Color color)
        {
            for (int i = (int)ColorType.Red; i <= (int)ColorType.Yellow; i++)
            {
                if (colorMap[i] == color) return (ColorType)i;
            }
            return ColorType.Red;
        }
    }
}
