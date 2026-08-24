using System;
using System.Threading;
using System.Threading.Tasks;
using ARPG.Framework.Core;
using ARPG.Game.Bootstrap;
using ARPG.Game.Resource;
using UnityEngine;

namespace ARPG.Game.Tests.Resource
{
    /// <summary>
    /// AddressablesResourceService集成测试脚本。
    ///
    /// 建议挂在独立测试场景中的GameObject上。
    /// </summary>
    public sealed class AddressablesResourceServiceTester
        : MonoBehaviour
    {
        [Header("Addressables Test Data")]

        [SerializeField]
        private string _prefabAddress =
            "ARPG/Test/AddressableCube";

        [SerializeField]
        private string _invalidAddress =
            "ARPG/Test/DoesNotExist";

        private IResourceService _resourceService;

        private ResourceHandle<GameObject> _persistentHandleA;
        private ResourceHandle<GameObject> _persistentHandleB;

        private CancellationTokenSource _cancellationTokenSource;

        private void Awake()
        {
            ServiceContainer services =
                GameLauncher.Instance
                    .GameContext
                    .Services;

            _resourceService =
                services.Get<IResourceService>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestSingleLoadAsync();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestMultipleHandlesAsync();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestConcurrentLoadAsync();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                ReleasePersistentHandleA();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                ReleasePersistentHandleB();
            }

            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                TestCancelledLoadAsync();
            }

            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                TestInvalidAddressAsync();
            }
        }

        /// <summary>
        /// 测试：
        /// 单次异步加载 -> Instantiate -> Dispose。
        /// </summary>
        private async void TestSingleLoadAsync()
        {
            try
            {
                Debug.Log(
                    "[Addressables Test] Single load started.");

                ResourceHandle<GameObject> handle =
                    await _resourceService
                        .LoadAsync<GameObject>(
                            _prefabAddress);

                try
                {
                    GameObject instance =
                        Instantiate(
                            handle.Asset);

                    instance.name =
                        "AddressablesSingleLoadInstance";

                    Debug.Log(
                        "[Addressables Test] " +
                        "Single load succeeded.");
                }
                finally
                {
                    handle.Dispose();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        /// <summary>
        /// 测试：
        /// 同一个资源获得两个独立ResourceHandle。
        ///
        /// 预期：
        /// 两个Handle.Asset引用相同；
        /// 业务引用计数应该变成2。
        /// </summary>
        private async void TestMultipleHandlesAsync()
        {
            ReleasePersistentHandles();

            try
            {
                _persistentHandleA =
                    await _resourceService
                        .LoadAsync<GameObject>(
                            _prefabAddress);

                _persistentHandleB =
                    await _resourceService
                        .LoadAsync<GameObject>(
                            _prefabAddress);

                bool sameAsset =
                    ReferenceEquals(
                        _persistentHandleA.Asset,
                        _persistentHandleB.Asset);

                Debug.Log(
                    "[Addressables Test] " +
                    $"Two handles acquired. " +
                    $"Same asset = {sameAsset}.");

                GameObject instanceA =
                    Instantiate(
                        _persistentHandleA.Asset);

                instanceA.name =
                    "AddressablesHandleAInstance";

                GameObject instanceB =
                    Instantiate(
                        _persistentHandleB.Asset);

                instanceB.name =
                    "AddressablesHandleBInstance";
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);

                ReleasePersistentHandles();
            }
        }

        /// <summary>
        /// 测试：
        /// 同一时刻发起两个相同Address异步请求。
        ///
        /// 用于验证in-flight请求合并。
        /// </summary>
        private async void TestConcurrentLoadAsync()
        {
            try
            {
                Debug.Log(
                    "[Addressables Test] " +
                    "Concurrent load started.");

                Task<ResourceHandle<GameObject>> taskA =
                    _resourceService
                        .LoadAsync<GameObject>(
                            _prefabAddress);

                Task<ResourceHandle<GameObject>> taskB =
                    _resourceService
                        .LoadAsync<GameObject>(
                            _prefabAddress);

                ResourceHandle<GameObject>[] handles =
                    await Task.WhenAll(
                        taskA,
                        taskB);

                try
                {
                    bool sameAsset =
                        ReferenceEquals(
                            handles[0].Asset,
                            handles[1].Asset);

                    Debug.Log(
                        "[Addressables Test] " +
                        $"Concurrent load completed. " +
                        $"Same asset = {sameAsset}.");

                    GameObject instanceA =
                        Instantiate(
                            handles[0].Asset);

                    instanceA.name =
                        "ConcurrentAddressablesInstanceA";

                    GameObject instanceB =
                        Instantiate(
                            handles[1].Asset);

                    instanceB.name =
                        "ConcurrentAddressablesInstanceB";
                }
                finally
                {
                    handles[0].Dispose();
                    handles[1].Dispose();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        /// <summary>
        /// 释放第一个持久Handle。
        ///
        /// 如果Handle B仍然存在，
        /// 底层Addressables资源不应该真正Release。
        /// </summary>
        private void ReleasePersistentHandleA()
        {
            if (_persistentHandleA == null)
            {
                Debug.Log(
                    "[Addressables Test] " +
                    "Handle A is already null.");

                return;
            }

            _persistentHandleA.Dispose();
            _persistentHandleA = null;

            Debug.Log(
                "[Addressables Test] " +
                "Handle A released.");
        }

        /// <summary>
        /// 释放第二个持久Handle。
        ///
        /// 如果A已经释放，
        /// 此时应该触发最终底层Release。
        /// </summary>
        private void ReleasePersistentHandleB()
        {
            if (_persistentHandleB == null)
            {
                Debug.Log(
                    "[Addressables Test] " +
                    "Handle B is already null.");

                return;
            }

            _persistentHandleB.Dispose();
            _persistentHandleB = null;

            Debug.Log(
                "[Addressables Test] " +
                "Handle B released.");
        }

        /// <summary>
        /// 测试：
        /// 已经取消的Token不应该取得ResourceHandle。
        /// </summary>
        private async void TestCancelledLoadAsync()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            _cancellationTokenSource =
                new CancellationTokenSource();

            _cancellationTokenSource.Cancel();

            try
            {
                await _resourceService
                    .LoadAsync<GameObject>(
                        _prefabAddress,
                        _cancellationTokenSource.Token);

                Debug.LogError(
                    "[Addressables Test] " +
                    "Cancellation test failed: " +
                    "load unexpectedly completed.");
            }
            catch (OperationCanceledException)
            {
                Debug.Log(
                    "[Addressables Test] " +
                    "Cancellation succeeded.");
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        /// <summary>
        /// 测试：
        /// 无效Address应抛ResourceLoadException。
        /// </summary>
        private async void TestInvalidAddressAsync()
        {
            try
            {
                ResourceHandle<GameObject> handle =
                    await _resourceService
                        .LoadAsync<GameObject>(
                            _invalidAddress);

                handle.Dispose();

                Debug.LogError(
                    "[Addressables Test] " +
                    "Invalid address test failed: " +
                    "load unexpectedly succeeded.");
            }
            catch (ResourceLoadException exception)
            {
                Debug.Log(
                    "[Addressables Test] " +
                    "Expected ResourceLoadException received: " +
                    exception.Message);
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        private void ReleasePersistentHandles()
        {
            _persistentHandleA?.Dispose();
            _persistentHandleA = null;

            _persistentHandleB?.Dispose();
            _persistentHandleB = null;
        }

        private void OnDestroy()
        {
            ReleasePersistentHandles();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}