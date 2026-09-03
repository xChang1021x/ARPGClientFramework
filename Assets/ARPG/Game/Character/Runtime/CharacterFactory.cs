using System;
using System.Threading;
using System.Threading.Tasks;
using ARPG.Game.Character.Movement;
using ARPG.Game.Resource;
using UnityEngine;

namespace ARPG.Game.Character
{
    /// <summary>
    /// Character实例创建工厂。
    ///
    /// 负责：
    /// 1. 获取Character配置；
    /// 2. 加载Prefab；
    /// 3. Instantiate；
    /// 4. 验证Character组件；
    /// 5. 创建CharacterContext；
    /// 6. Initialize；
    /// 7. 完成Ownership Transfer。
    /// </summary>
    public sealed class CharacterFactory
    {
        private readonly IResourceService
            _resourceService;

        public CharacterFactory(
            IResourceService resourceService)
        {
            _resourceService =
                resourceService
                ?? throw new ArgumentNullException(
                    nameof(resourceService));
        }

        public async Task<CharacterHandle> CreateAsync<TCharacter>(
            Vector3 position,
            Quaternion rotation,
            Transform parent = null,
            CancellationToken cancellationToken = default)
            where TCharacter : CharacterEntity
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            CharacterConfig config =
                CharacterRegistry.Get<TCharacter>();

            ResourceHandle<GameObject> resourceHandle =
                null;

            GameObject instance =
                null;

            try
            {
                resourceHandle =
                    await _resourceService
                        .LoadAsync<GameObject>(
                            config.Address,
                            cancellationToken);

                cancellationToken
                    .ThrowIfCancellationRequested();

                instance =
                    UnityEngine.Object.Instantiate(
                        resourceHandle.Asset,
                        position,
                        rotation,
                        parent);

                TCharacter character =
                    instance.GetComponent<TCharacter>();

                if (character == null)
                {
                    throw new InvalidOperationException(
                        $"Character prefab '{config.Address}' " +
                        $"does not contain component " +
                        $"'{typeof(TCharacter).Name}'.");
                }

                CharacterController controller =
                    character.GetComponent<CharacterController>();

                if (controller == null)
                {
                    throw new InvalidOperationException(
                        $"Character prefab '{config.Address}' " +
                        "does not contain a CharacterController.");
                }

                var motor =
                    new CharacterMotor(
                        controller,
                        config.MoveSpeed,
                        config.Gravity);

                var context =
                    new CharacterContext(
                        config,
                        motor);

                character.Initialize(
                    context);

                var handle =
                    new CharacterHandle(
                        character,
                        resourceHandle);

                /*
                 * Ownership Transfer:
                 *
                 * CharacterFactory
                 *      ↓
                 * CharacterHandle
                 */
                resourceHandle = null;
                instance = null;

                return handle;
            }
            catch
            {
                if (instance != null)
                {
                    UnityEngine.Object.Destroy(
                        instance);
                }

                resourceHandle?.Dispose();

                throw;
            }
        }
    }
}