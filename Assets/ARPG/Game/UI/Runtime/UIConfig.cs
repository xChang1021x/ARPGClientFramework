using System;

namespace ARPG.Game.UI
{
    /// <summary>
    /// 一个UI类型的运行时静态配置。
    ///
    /// UIConfig描述“这个UI是什么”，
    /// 而不是某一次Open调用临时传递的参数。
    /// </summary>
    public readonly struct UIConfig
    {
        public UIConfig(
            string address,
            UILayer layer,
            bool participateInNavigation)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException(
                    "UI address cannot be null or empty.",
                    nameof(address));
            }

            Address = address;
            Layer = layer;
            ParticipateInNavigation =
                participateInNavigation;
        }

        /// <summary>
        /// ResourceService使用的资源地址。
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// UI所属显示层级。
        /// </summary>
        public UILayer Layer { get; }

        /// <summary>
        /// 是否参与Back导航。
        ///
        /// 注意：
        /// UI Layer和Navigation是两个独立维度。
        /// </summary>
        public bool ParticipateInNavigation { get; }
    }
}