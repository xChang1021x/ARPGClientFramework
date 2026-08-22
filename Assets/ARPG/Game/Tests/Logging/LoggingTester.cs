// using System;
// using ARPG.Framework.Logging;
// using ARPG.Game.Bootstrap;
// using UnityEngine;

// namespace ARPG.Game.Tests.Logging
// {
//     public sealed class LoggingTester : MonoBehaviour
//     {
//         private LogService _logService;

//         private void Awake()
//         {
//             _logService =
//                 GameLauncher.Instance
//                     .GameContext
//                     .LogService;
//         }

//         private void Update()
//         {
//             if (Input.GetKeyDown(KeyCode.Alpha1))
//             {
//                 _logService.Debug(
//                     "LoggingTest",
//                     "Debug message.");
//             }

//             if (Input.GetKeyDown(KeyCode.Alpha2))
//             {
//                 _logService.Info(
//                     "LoggingTest",
//                     "Info message.");
//             }

//             if (Input.GetKeyDown(KeyCode.Alpha3))
//             {
//                 _logService.Warning(
//                     "LoggingTest",
//                     "Warning message.");
//             }

//             if (Input.GetKeyDown(KeyCode.Alpha4))
//             {
//                 TestError();
//             }

//             if (Input.GetKeyDown(KeyCode.Alpha5))
//             {
//                 ToggleMinimumLevel();
//             }
//         }

//         private void TestError()
//         {
//             try
//             {
//                 throw new InvalidOperationException(
//                     "Logging test exception.");
//             }
//             catch (Exception exception)
//             {
//                 _logService.Error(
//                     "LoggingTest",
//                     "Operation failed.",
//                     exception);
//             }
//         }

//         private void ToggleMinimumLevel()
//         {
//             _logService.MinimumLevel =
//                 _logService.MinimumLevel == LogLevel.Debug
//                     ? LogLevel.Warning
//                     : LogLevel.Debug;

//             UnityEngine.Debug.Log(
//                 $"Minimum log level: " +
//                 $"{_logService.MinimumLevel}");
//         }
//     }
// }