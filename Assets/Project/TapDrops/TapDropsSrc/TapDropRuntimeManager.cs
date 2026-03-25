using System.Collections.Generic;
using RainbowTower.Bootstrap;
using RainbowTower.CrystalSystem;
using RainbowTower.GameplayField;
using RainbowTower.ManaSystem;
using UnityEngine;

namespace RainbowTower.TapDrops
{
    public sealed class TapDropRuntimeManager : IRuntimeManager
    {
        private readonly ManaRuntimeManager manaRuntimeManager;
        private readonly CrystalRuntimeManager crystalRuntimeManager;
        private readonly TapDropInputBridge tapInputBridge = new();
        private readonly List<ActiveDrop> activeDrops = new();
        private readonly ManaColor[] unlockedColorBuffer = new ManaColor[ManaColorUtility.TotalColorCount];
        private readonly int[] pendingEmpoweredShots = new int[ManaColorUtility.TotalColorCount];

        private TapDropConfig tapDropConfig;
        private Transform towerAnchor;
        private Transform dropRoot;
        private Camera worldCamera;
        private bool isReady;

        public TapDropRuntimeManager(
            ManaRuntimeManager manaRuntimeManager,
            CrystalRuntimeManager crystalRuntimeManager)
        {
            this.manaRuntimeManager = manaRuntimeManager;
            this.crystalRuntimeManager = crystalRuntimeManager;
        }

        public void Initialize(ServiceLocator serviceLocator)
        {
            tapDropConfig = serviceLocator.ConfigurationProvider.GetConfiguration<TapDropConfig>();
            if (tapDropConfig == null)
            {
                Debug.LogWarning("TapDropRuntimeManager: TapDropConfig is not assigned. Feature remains disabled.");
                isReady = false;
                return;
            }

            if (!serviceLocator.TryGet<GameplayFieldProvider>(out var gameplayFieldProvider) || gameplayFieldProvider == null)
            {
                Debug.LogError("TapDropRuntimeManager requires GameplayFieldProvider.");
                isReady = false;
                return;
            }

            towerAnchor = gameplayFieldProvider.TowerAnchor;
            worldCamera = Camera.main;

            var rootObject = new GameObject("RuntimeTapDrops");
            dropRoot = rootObject.transform;
            dropRoot.SetParent(gameplayFieldProvider.FieldVisualRoot, false);

            activeDrops.Clear();
            ClearEmpoweredShots();
            isReady = true;
        }

        public void Tick(float deltaTime)
        {
            if (!isReady || tapDropConfig == null || !tapDropConfig.IsEnabled)
            {
                return;
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            CleanupDestroyedDrops();
            ProcessTapInput();
            TickLifetimes(deltaTime);
        }

        public void LateTick(float deltaTime)
        {
        }

        public void Deinitialize()
        {
            for (var index = 0; index < activeDrops.Count; index++)
            {
                if (activeDrops[index].View != null)
                {
                    Object.Destroy(activeDrops[index].View.gameObject);
                }
            }

            activeDrops.Clear();

            if (dropRoot != null)
            {
                Object.Destroy(dropRoot.gameObject);
            }

            ClearEmpoweredShots();
            dropRoot = null;
            towerAnchor = null;
            worldCamera = null;
            tapDropConfig = null;
            isReady = false;
        }

        public bool TrySpawnDropFromShot(Vector3 towerPosition)
        {
            if (!isReady || tapDropConfig == null || !tapDropConfig.IsEnabled)
            {
                return false;
            }

            if (activeDrops.Count >= tapDropConfig.MaxSimultaneousDrops)
            {
                return false;
            }

            if (Random.value > tapDropConfig.SpawnChancePerShot)
            {
                return false;
            }

            if (!TrySelectDropColor(out var color))
            {
                return false;
            }

            var spawnAnchor = towerAnchor != null ? towerAnchor.position : towerPosition;
            var spawnPosition = spawnAnchor + (Vector3)SampleSpawnOffset();
            var view = CreateDropView(spawnPosition);
            if (view == null)
            {
                return false;
            }

            view.Initialize(
                color,
                TapDropConfig.GetColor(color),
                tapDropConfig.GetDropSprite(color),
                tapDropConfig.DropWorldSize,
                tapDropConfig.SortingOrder,
                tapDropConfig.SpawnFeedbackDuration,
                tapDropConfig.PulseScaleAmplitude,
                tapDropConfig.PulseFrequency,
                tapDropConfig.CollectFeedbackDuration,
                tapDropConfig.ExpireFeedbackDuration);

            activeDrops.Add(new ActiveDrop
            {
                View = view,
                Color = color,
                RemainingLifetime = tapDropConfig.LifetimeSeconds
            });

            return true;
        }

        public void ResolveShotModifierForNextShot(ManaColor color, int currentMana, out int manaCost, out int damageMultiplier)
        {
            manaCost = 1;
            damageMultiplier = 1;

            if (!isReady || tapDropConfig == null || !tapDropConfig.IsEnabled)
            {
                return;
            }

            var colorIndex = color.ToIndex();
            if (pendingEmpoweredShots[colorIndex] <= 0)
            {
                return;
            }

            pendingEmpoweredShots[colorIndex]--;
            if (currentMana > 1)
            {
                manaCost = 2;
                damageMultiplier = 2;
            }
        }

        private void ProcessTapInput()
        {
            if (!tapInputBridge.TryGetTapWorldPosition(worldCamera, out var tapWorldPosition))
            {
                return;
            }

            var hitIndex = FindDropAtPoint(tapWorldPosition, tapDropConfig.TapRadiusWorld);
            if (hitIndex < 0)
            {
                return;
            }

            CollectDrop(hitIndex);
        }

        private void CollectDrop(int dropIndex)
        {
            var drop = activeDrops[dropIndex];
            if (drop.View == null)
            {
                activeDrops.RemoveAt(dropIndex);
                return;
            }

            manaRuntimeManager.AddMana(drop.Color, 1);
            pendingEmpoweredShots[drop.Color.ToIndex()]++;
            drop.View.TriggerCollect();
            activeDrops.RemoveAt(dropIndex);
        }

        private void TickLifetimes(float deltaTime)
        {
            for (var index = activeDrops.Count - 1; index >= 0; index--)
            {
                var drop = activeDrops[index];
                if (drop.View == null)
                {
                    activeDrops.RemoveAt(index);
                    continue;
                }

                drop.RemainingLifetime -= deltaTime;
                if (drop.RemainingLifetime > 0f)
                {
                    activeDrops[index] = drop;
                    continue;
                }

                drop.View.TriggerExpire();
                activeDrops.RemoveAt(index);
            }
        }

        private int FindDropAtPoint(Vector2 point, float radius)
        {
            var radiusSqr = radius * radius;
            var bestIndex = -1;
            var bestDistance = float.MaxValue;

            for (var index = 0; index < activeDrops.Count; index++)
            {
                var view = activeDrops[index].View;
                if (view == null)
                {
                    continue;
                }

                var dropPosition = view.transform.position;
                var distanceSqr = (new Vector2(dropPosition.x, dropPosition.y) - point).sqrMagnitude;
                if (distanceSqr > radiusSqr || distanceSqr >= bestDistance)
                {
                    continue;
                }

                bestDistance = distanceSqr;
                bestIndex = index;
            }

            return bestIndex;
        }

        private bool TrySelectDropColor(out ManaColor color)
        {
            var unlockedCount = crystalRuntimeManager.FillUnlockedColors(unlockedColorBuffer);
            if (unlockedCount <= 0)
            {
                color = default;
                return false;
            }

            color = unlockedColorBuffer[Random.Range(0, unlockedCount)];
            return true;
        }

        private Vector2 SampleSpawnOffset()
        {
            var radius = Random.Range(tapDropConfig.MinSpawnRadiusWorld, tapDropConfig.MaxSpawnRadiusWorld);
            var angle = Random.value * Mathf.PI * 2f;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private TapDropView CreateDropView(Vector3 worldPosition)
        {
            TapDropView view = null;

            if (tapDropConfig.DropPrefab != null)
            {
                view = Object.Instantiate(tapDropConfig.DropPrefab, worldPosition, Quaternion.identity, dropRoot);
            }
            else
            {
                var dropObject = new GameObject("TapDrop");
                dropObject.transform.SetParent(dropRoot, false);
                dropObject.transform.position = worldPosition;
                view = dropObject.AddComponent<TapDropView>();
            }

            return view;
        }

        private void CleanupDestroyedDrops()
        {
            for (var index = activeDrops.Count - 1; index >= 0; index--)
            {
                if (activeDrops[index].View == null)
                {
                    activeDrops.RemoveAt(index);
                }
            }
        }

        private void ClearEmpoweredShots()
        {
            for (var index = 0; index < pendingEmpoweredShots.Length; index++)
            {
                pendingEmpoweredShots[index] = 0;
            }
        }

        private struct ActiveDrop
        {
            public TapDropView View;
            public ManaColor Color;
            public float RemainingLifetime;
        }
    }
}

