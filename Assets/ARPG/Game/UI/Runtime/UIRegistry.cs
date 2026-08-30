using System;
using System.Collections.Generic;
using ARPG.Game.UI.Confirm;
using ARPG.Game.UI.Main;
using ARPG.Game.UI.Settings;

namespace ARPG.Game.UI
{
    /// <summary>
    /// UI类型到运行时配置的注册表。
    ///
    /// 第一版使用代码配置。
    ///
    /// 后续可以替换为：
    /// ScriptableObject / Luban / JSON / 配置表，
    /// 而业务层OpenAsync<TPanel>()无需改变。
    /// </summary>
    public static class UIRegistry
    {
        private static readonly Dictionary<Type, UIConfig>
            Configs = new()
            {
                {
                    typeof(MainPanel),
                    new UIConfig(
                        "ARPG/UI/MainPanel",
                        UILayer.Normal,
                        participateInNavigation: true)
                },

                {
                    typeof(SettingsPanel),
                    new UIConfig(
                        "ARPG/UI/SettingsPanel",
                        UILayer.Normal,
                        participateInNavigation: true)
                },

                {
                    typeof(ConfirmPanel),
                    new UIConfig(
                        "ARPG/UI/ConfirmPanel",
                        UILayer.Popup,
                        participateInNavigation: true)
                }
            };

        public static UIConfig Get<TPanel>()
            where TPanel : UIPanel
        {
            return Get(
                typeof(TPanel));
        }

        public static UIConfig Get(
            Type panelType)
        {
            if (panelType == null)
            {
                throw new ArgumentNullException(
                    nameof(panelType));
            }

            if (!Configs.TryGetValue(
                    panelType,
                    out UIConfig config))
            {
                throw new InvalidOperationException(
                    $"UI config for '{panelType.Name}' " +
                    "has not been registered.");
            }

            return config;
        }
    }
}