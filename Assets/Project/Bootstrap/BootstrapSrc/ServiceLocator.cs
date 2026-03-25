using System;
using System.Collections.Generic;
using RainbowTower.MainUi;
using UnityEngine;

namespace RainbowTower.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class ServiceLocator : MonoBehaviour
    {
        [SerializeField] private ConfigurationProvider configurationProvider;
        [SerializeField] private MainUiProvider mainUiProvider;

        private readonly Dictionary<Type, object> services = new();
        private readonly List<Type> registrationOrder = new();

        public ConfigurationProvider ConfigurationProvider => configurationProvider;
        public MainUiProvider MainUiProvider => mainUiProvider;
        public IReadOnlyList<Type> RegistrationOrder => registrationOrder;

        public void Initialize(GameManager gameManager)
        {
            if (gameManager == null)
            {
                throw new ArgumentNullException(nameof(gameManager));
            }

            if (configurationProvider == null)
            {
                Debug.LogError("ServiceLocator requires a ConfigurationProvider reference.", this);
                return;
            }

            if (mainUiProvider == null)
            {
                Debug.LogError("ServiceLocator requires a MainUiProvider reference.", this);
                return;
            }

            services.Clear();
            registrationOrder.Clear();

            Register(gameManager);
            Register(configurationProvider);
            Register(mainUiProvider);
            Register(this);
        }

        public void Register<TService>(TService service) where TService : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            var serviceType = typeof(TService);
            services[serviceType] = service;

            if (!registrationOrder.Contains(serviceType))
            {
                registrationOrder.Add(serviceType);
            }
        }

        public bool TryGet<TService>(out TService service) where TService : class
        {
            if (services.TryGetValue(typeof(TService), out var instance))
            {
                service = instance as TService;
                return service != null;
            }

            service = null;
            return false;
        }

        public TService Get<TService>() where TService : class
        {
            if (TryGet<TService>(out var service))
            {
                return service;
            }

            throw new InvalidOperationException($"Service of type {typeof(TService).Name} is not registered.");
        }
    }
}
