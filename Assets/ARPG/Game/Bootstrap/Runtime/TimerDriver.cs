using System;
using ARPG.Framework.Timer;
using UnityEngine;

namespace ARPG.Game.Bootstrap
{
    /// <summary>
    /// 负责使用Unity Update驱动TimerService。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TimerDriver : MonoBehaviour
    {
        private TimerService _timerService;

        public void Initialize(TimerService timerService)
        {
            _timerService = timerService
                ?? throw new ArgumentNullException(
                    nameof(timerService));
        }

        private void Update()
        {
            if (_timerService == null)
            {
                return;
            }

            _timerService.Tick(
                Time.deltaTime,
                Time.unscaledDeltaTime);
        }
    }
}