using System;
using System.Threading;
using ARPG.Framework.Core;
using ARPG.Game.Bootstrap;
using ARPG.Game.Resource;
using UnityEngine;

namespace ARPG.Game.Tests.Resource
{
    public sealed class ResourceServiceTester
        : MonoBehaviour
    {
        private IResourceService _resourceService;

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
                TestLoad();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestTryLoadMissingResource();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestLoadMissingResource();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestAsyncLoad();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                CancelAsyncLoad();
            }

            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                TestHandle();
            }

            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                ReleaseHandle();
            }

            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                TestHandle2();
            }

            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                ReleaseHandle2();
            }
        }

        private void TestLoad()
        {
            GameObject prefab =
                _resourceService.Load<GameObject>(
                    "ARPG/Test/ResourceTestCube");

            GameObject instance =
                Instantiate(prefab);

            instance.name =
                "ResourceServiceTestInstance";
        }

        private void TestTryLoadMissingResource()
        {
            bool succeeded =
                _resourceService.TryLoad<GameObject>(
                    "ARPG/Test/DoesNotExist",
                    out GameObject asset);

            Debug.Log(
                $"TryLoad result: {succeeded}, " +
                $"Asset: {asset}");
        }

        private void TestLoadMissingResource()
        {
            try
            {
                _resourceService.Load<GameObject>(
                    "ARPG/Test/DoesNotExist");
            }
            catch (ResourceLoadException exception)
            {
                Debug.Log(
                    $"Expected exception: " +
                    $"{exception.Message}");
            }
        }

        private CancellationTokenSource _loadCancellation;

        private async void TestAsyncLoad()
        {
            _loadCancellation?.Cancel();
            _loadCancellation?.Dispose();

            _loadCancellation =
                new CancellationTokenSource();

            try
            {
                GameObject prefab =
                    await _resourceService
                        .LoadAsync<GameObject>(
                            "ARPG/Test/ResourceTestCube",
                            _loadCancellation.Token);

                Instantiate(prefab);

                Debug.Log(
                    "Async resource load succeeded.");
            }
            catch (OperationCanceledException)
            {
                Debug.Log(
                    "Async resource load cancelled.");
            }
            catch (ResourceLoadException exception)
            {
                Debug.LogError(
                    exception);
            }
        }

        private void CancelAsyncLoad()
        {
            _loadCancellation?.Cancel();
        }

        private ResourceHandle<GameObject> _handle;
        private ResourceHandle<GameObject> _handle2;

        private async void TestHandle()
        {
            _handle?.Dispose();

            _handle =
                await _resourceService
                    .LoadHandleAsync<GameObject>(
                        "ARPG/Test/ResourceTestCube");

            Instantiate(
                _handle.Asset);
        }

        private async void TestHandle2()
        {
            _handle2?.Dispose();

            _handle2 =
                await _resourceService
                    .LoadHandleAsync<GameObject>(
                        "ARPG/Test/ResourceTestCube");

            Instantiate(
                _handle2.Asset);
        }

        private void ReleaseHandle()
        {
            _handle?.Dispose();
            _handle = null;
        }

        private void ReleaseHandle2()
        {
            _handle2?.Dispose();
            _handle2 = null;
        }
    }
}