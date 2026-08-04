using ARPG.Framework.Timer;
using ARPG.Game.Bootstrap;
using UnityEngine;

namespace ARPG.Game.Tests.Timer
{
    public sealed class TimerTester : MonoBehaviour
    {
        private TimerService _timerService;

        private TimerHandle _delayHandle;
        private TimerHandle _repeatHandle;

        private void Awake()
        {
            _timerService =
                GameLauncher.Instance
                    .GameContext
                    .TimerService;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestDelay();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestRepeat();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                CancelRepeat();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
        }

        private void TestDelay()
        {
            _delayHandle?.Cancel();

            _delayHandle = _timerService.Delay(
                delay: 2f,
                callback: () =>
                {
                    Debug.Log(
                        "[Timer] Delay completed.");
                });

            Debug.Log(
                "[Timer] Delay started.");
        }

        private void TestRepeat()
        {
            _repeatHandle?.Cancel();

            _repeatHandle = _timerService.Repeat(
                interval: 1f,
                callback: () =>
                {
                    Debug.Log(
                        "[Timer] Repeat callback.");
                },
                repeatCount: 5);

            Debug.Log(
                "[Timer] Repeat started.");
        }

        private void CancelRepeat()
        {
            _repeatHandle?.Cancel();
            _repeatHandle = null;

            Debug.Log(
                "[Timer] Repeat cancelled.");
        }

        private static void TogglePause()
        {
            Time.timeScale =
                Time.timeScale > 0f
                    ? 0f
                    : 1f;

            Debug.Log(
                $"[Timer] TimeScale={Time.timeScale}");
        }

        private void OnDestroy()
        {
            _delayHandle?.Cancel();
            _repeatHandle?.Cancel();
        }
    }
}