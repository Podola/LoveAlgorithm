using System;
using System.Collections.Generic;
using LoveAlgo.Data;
using LoveAlgo.Services;

namespace LoveAlgo.Core
{
    public sealed class LoveAlgoContext : IDisposable
    {
        private static LoveAlgoContext instance;
        private readonly Dictionary<Type, object> services = new();

        private LoveAlgoContext(LoveAlgoConfiguration configuration)
        {
            Configuration = configuration;
            Register(new GameModeController());

            var clock = new GameClockService(configuration.Schedule, configuration.MaxFreeActionsPerDay);
            var stats = new StatsService(configuration.MinFatigue, configuration.MaxFatigue);
            var affinity = new AffinityService(configuration.HeroineRoster);
            var content = new ContentService(configuration.EpisodeCatalog, configuration.Schedule);
            var inventory = new InventoryService();
            var shop = new ShopService(configuration.GiftTierCatalog);
            var miniGame = new MiniGameGateway();
            var freeActionService = new FreeActionService(configuration.FreeActionCatalog, stats, clock);
            var dialogueBridge = new DialogueBridgeService(stats, affinity, Get<GameModeController>(), freeActionService);
            var save = new SaveService(stats, affinity, clock);

            Register(clock);
            Register(stats);
            Register(affinity);
            Register(content);
            Register(inventory);
            Register(shop);
            Register(miniGame);
            Register(freeActionService);
            Register(dialogueBridge);
            Register(save);
        }

        public LoveAlgoConfiguration Configuration { get; }

        public static bool Exists => instance != null;
        public static LoveAlgoContext Instance => instance ?? throw new InvalidOperationException("LoveAlgoContext has not been created");

        public static LoveAlgoContext Create(LoveAlgoConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (instance == null)
            {
                instance = new LoveAlgoContext(configuration);
            }

            return instance;
        }

        public static void DisposeInstance()
        {
            instance?.Dispose();
            instance = null;
        }

        public void Dispose()
        {
            services.Clear();
        }

        public T Get<T>() where T : class
        {
            if (!services.TryGetValue(typeof(T), out var service))
            {
                throw new InvalidOperationException($"Service {typeof(T).Name} is not registered");
            }

            return (T)service;
        }

        private void Register<T>(T service) where T : class
        {
            services[typeof(T)] = service;
        }
    }
}
