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
    }
}