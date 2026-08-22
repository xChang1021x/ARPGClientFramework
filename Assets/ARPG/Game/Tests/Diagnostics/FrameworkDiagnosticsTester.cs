// using System;
// using System.Collections.Generic;
// using ARPG.Framework.Diagnostics;
// using ARPG.Framework.Event;
// using ARPG.Framework.Logging;
// using ARPG.Framework.Timer;
// using ARPG.Game.Bootstrap;
// using UnityEngine;

// namespace ARPG.Game.Tests.Diagnostics
// {
//     /// <summary>
//     /// EventBus、Timer和日志异常隔离的集成测试入口。
//     ///
//     /// 按键：
//     /// 1：测试EventBus监听者异常隔离
//     /// 2：测试Timer回调异常隔离
//     /// 3：测试NullLogger
//     /// 4：测试无Reporter时的Fail Fast
//     /// </summary>
//     public sealed class FrameworkDiagnosticsTester : MonoBehaviour
//     {
//         private readonly List<IDisposable> _eventSubscriptions = new();

//         private EventBus _eventBus;
//         private TimerService _timerService;
//         private LogService _logService;

//         private TimerHandle _failedTimerHandle;
//         private TimerHandle _normalTimerHandle;
//         private TimerHandle _verificationTimerHandle;

//         private int _listenerACallCount;
//         private int _listenerCCallCount;

//         private bool _normalTimerExecuted;

//         private void Awake()
//         {
//             var gameContext =
//                 GameLauncher.Instance.GameContext;

//             _eventBus = gameContext.EventBus;
//             _timerService = gameContext.TimerService;
//             _logService = gameContext.LogService;

//             _logService.Info(
//                 "Diagnostics",
//                 "Framework diagnostics tester initialized.");
//         }

//         private void Update()
//         {
//             if (Input.GetKeyDown(KeyCode.Alpha1))
//             {
//                 TestEventBusExceptionIsolation();
//             }

//             if (Input.GetKeyDown(KeyCode.Alpha2))
//             {
//                 TestTimerExceptionIsolation();
//             }

//             if (Input.GetKeyDown(KeyCode.Alpha3))
//             {
//                 TestNullLogger();
//             }

//             if (Input.GetKeyDown(KeyCode.Alpha4))
//             {
//                 TestFailFastWithoutReporter();
//             }
//         }

//         /// <summary>
//         /// 验证Listener B异常时，Listener C仍然能够执行。
//         /// </summary>
//         private void TestEventBusExceptionIsolation()
//         {
//             ClearEventSubscriptions();

//             _listenerACallCount = 0;
//             _listenerCCallCount = 0;

//             _eventSubscriptions.Add(
//                 _eventBus.Subscribe<DiagnosticsTestEvent>(
//                     OnEventListenerA));

//             _eventSubscriptions.Add(
//                 _eventBus.Subscribe<DiagnosticsTestEvent>(
//                     OnEventListenerB));

//             _eventSubscriptions.Add(
//                 _eventBus.Subscribe<DiagnosticsTestEvent>(
//                     OnEventListenerC));

//             _logService.Info(
//                 "Diagnostics",
//                 "Starting EventBus exception isolation test.");

//             try
//             {
//                 _eventBus.Publish(
//                     new DiagnosticsTestEvent(testId: 1));
//             }
//             catch (Exception exception)
//             {
//                 _logService.Fatal(
//                     "Diagnostics",
//                     "EventBus exception escaped to publisher. " +
//                     "Exception isolation failed.",
//                     exception);

//                 return;
//             }

//             if (_listenerACallCount == 1 &&
//                 _listenerCCallCount == 1)
//             {
//                 _logService.Info(
//                     "Diagnostics",
//                     "EventBus exception isolation test passed.");
//             }
//             else
//             {
//                 _logService.Error(
//                     "Diagnostics",
//                     $"EventBus test failed. " +
//                     $"ListenerA={_listenerACallCount}, " +
//                     $"ListenerC={_listenerCCallCount}.");
//             }
//         }

//         private void OnEventListenerA(
//             DiagnosticsTestEvent eventData)
//         {
//             _listenerACallCount++;

//             _logService.Info(
//                 "Diagnostics",
//                 $"Listener A executed. TestId={eventData.TestId}");
//         }

//         private static void OnEventListenerB(
//             DiagnosticsTestEvent eventData)
//         {
//             throw new InvalidOperationException(
//                 $"Listener B failed intentionally. " +
//                 $"TestId={eventData.TestId}");
//         }

//         private void OnEventListenerC(
//             DiagnosticsTestEvent eventData)
//         {
//             _listenerCCallCount++;

//             _logService.Info(
//                 "Diagnostics",
//                 $"Listener C executed. TestId={eventData.TestId}");
//         }

//         /// <summary>
//         /// 验证错误Timer被移除，同时其他Timer继续执行。
//         /// </summary>
//         private void TestTimerExceptionIsolation()
//         {
//             CancelTimerTests();

//             _normalTimerExecuted = false;

//             _logService.Info(
//                 "Diagnostics",
//                 "Starting Timer exception isolation test.");

//             _failedTimerHandle = _timerService.Delay(
//                 delay: 0.5f,
//                 callback: ThrowTimerException);

//             _normalTimerHandle = _timerService.Delay(
//                 delay: 1f,
//                 callback: OnNormalTimerExecuted);

//             _verificationTimerHandle = _timerService.Delay(
//                 delay: 1.5f,
//                 callback: VerifyTimerResult);
//         }

//         private static void ThrowTimerException()
//         {
//             throw new InvalidOperationException(
//                 "Timer callback failed intentionally.");
//         }

//         private void OnNormalTimerExecuted()
//         {
//             _normalTimerExecuted = true;

//             _logService.Info(
//                 "Diagnostics",
//                 "Normal Timer executed after failed Timer.");
//         }

//         private void VerifyTimerResult()
//         {
//             if (_normalTimerExecuted)
//             {
//                 _logService.Info(
//                     "Diagnostics",
//                     "Timer exception isolation test passed.");
//             }
//             else
//             {
//                 _logService.Error(
//                     "Diagnostics",
//                     "Timer exception isolation test failed. " +
//                     "Normal Timer was not executed.");
//             }
//         }

//         /// <summary>
//         /// 验证NullLogger不会因为异常上报而产生二次异常。
//         /// </summary>
//         private void TestNullLogger()
//         {
//             var silentLogService =
//                 new LogService(
//                     new NullLogger(),
//                     LogLevel.None);

//             IExceptionReporter silentReporter =
//                 new LoggingExceptionReporter(
//                     silentLogService);

//             var silentEventBus =
//                 new EventBus(silentReporter);

//             var silentTimerService =
//                 new TimerService(silentReporter);

//             IDisposable subscription =
//                 silentEventBus.Subscribe<DiagnosticsTestEvent>(
//                     _ =>
//                     {
//                         throw new InvalidOperationException(
//                             "Silent EventBus exception.");
//                     });

//             try
//             {
//                 silentEventBus.Publish(
//                     new DiagnosticsTestEvent(testId: 3));

//                 silentTimerService.Delay(
//                     delay: 0.1f,
//                     callback: () =>
//                     {
//                         throw new InvalidOperationException(
//                             "Silent Timer exception.");
//                     });

//                 /*
//                  * 纯C#手动驱动Timer。
//                  * 不需要TimerDriver。
//                  */
//                 silentTimerService.Tick(
//                     deltaTime: 0.1f,
//                     unscaledDeltaTime: 0.1f);

//                 _logService.Info(
//                     "Diagnostics",
//                     "NullLogger test passed. " +
//                     "Silent exceptions did not escape.");
//             }
//             catch (Exception exception)
//             {
//                 _logService.Error(
//                     "Diagnostics",
//                     "NullLogger test failed.",
//                     exception);
//             }
//             finally
//             {
//                 subscription.Dispose();
//                 silentTimerService.Dispose();
//             }
//         }

//         /// <summary>
//         /// 验证没有Reporter时保留Fail Fast行为。
//         /// </summary>
//         private void TestFailFastWithoutReporter()
//         {
//             var failFastEventBus = new EventBus();

//             IDisposable subscription =
//                 failFastEventBus.Subscribe<DiagnosticsTestEvent>(
//                     _ =>
//                     {
//                         throw new InvalidOperationException(
//                             "Expected fail-fast exception.");
//                     });

//             try
//             {
//                 failFastEventBus.Publish(
//                     new DiagnosticsTestEvent(testId: 4));

//                 _logService.Error(
//                     "Diagnostics",
//                     "Fail-fast test failed. " +
//                     "Expected exception was not thrown.");
//             }
//             catch (EventDispatchException exception)
//             {
//                 _logService.Info(
//                     "Diagnostics",
//                     "Fail-fast test passed.");

//                 _logService.Debug(
//                     "Diagnostics",
//                     exception.Message);
//             }
//             catch (Exception exception)
//             {
//                 _logService.Error(
//                     "Diagnostics",
//                     "Fail-fast test threw an unexpected " +
//                     "exception type.",
//                     exception);
//             }
//             finally
//             {
//                 subscription.Dispose();
//             }
//         }

//         private void ClearEventSubscriptions()
//         {
//             foreach (IDisposable subscription
//                      in _eventSubscriptions)
//             {
//                 subscription.Dispose();
//             }

//             _eventSubscriptions.Clear();
//         }

//         private void CancelTimerTests()
//         {
//             _failedTimerHandle?.Cancel();
//             _normalTimerHandle?.Cancel();
//             _verificationTimerHandle?.Cancel();

//             _failedTimerHandle = null;
//             _normalTimerHandle = null;
//             _verificationTimerHandle = null;
//         }

//         private void OnDestroy()
//         {
//             ClearEventSubscriptions();
//             CancelTimerTests();
//         }
//     }
// }