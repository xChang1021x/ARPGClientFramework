using System;
using System.Collections.Generic;
using ARPG.Framework.Core;

namespace ARPG.Framework.Config
{
    /// <summary>
    /// 运行时配置服务。
    /// 负责注册和获取强类型配置对象。
    /// </summary>
    public sealed class ConfigService : IGameService
    {
        private readonly Dictionary<Type, IConfig> _configs = new();

        private bool _isInitialized;

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
        }

        /// <summary>
        /// 注册配置。
        /// 同一种配置类型只能注册一次。
        /// </summary>
        public void Register<TConfig>(TConfig config)
            where TConfig : class, IConfig
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Type configType = typeof(TConfig);

            if (_configs.ContainsKey(configType))
            {
                throw new InvalidOperationException(
                    $"Config '{configType.Name}' has already been registered.");
            }

            _configs.Add(
                configType,
                config);
        }

        /// <summary>
        /// 获取指定类型的配置。
        /// </summary>
        public TConfig Get<TConfig>()
            where TConfig : class, IConfig
        {
            Type configType = typeof(TConfig);

            if (!_configs.TryGetValue(
                    configType,
                    out IConfig config))
            {
                throw new InvalidOperationException(
                    $"Config '{configType.Name}' has not been registered.");
            }

            return (TConfig)config;
        }

        /// <summary>
        /// 尝试获取配置。
        /// </summary>
        public bool TryGet<TConfig>(
            out TConfig config)
            where TConfig : class, IConfig
        {
            if (_configs.TryGetValue(
                    typeof(TConfig),
                    out IConfig storedConfig))
            {
                config = (TConfig)storedConfig;
                return true;
            }

            config = null;
            return false;
        }

        public void Shutdown()
        {
            _configs.Clear();
            _isInitialized = false;
        }
    }
}