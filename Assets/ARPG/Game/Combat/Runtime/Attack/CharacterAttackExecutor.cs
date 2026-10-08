using System;
using System.Collections.Generic;
using ARPG.Game.Character;
using ARPG.Game.Combat.Damage;
using UnityEngine;

namespace ARPG.Game.Combat.Attack
{
    public sealed class CharacterAttackExecutor
    {
        private const int ColliderBufferSize = 16;

        private readonly CharacterEntity _owner;
        private readonly Transform _ownerTransform;

        private readonly int _attackDamage;
        private readonly float _attackRange;
        private readonly float _attackRadius;
        private readonly LayerMask _targetLayerMask;

        private readonly Collider[]
            _colliderBuffer =
                new Collider[ColliderBufferSize];

        private readonly HashSet<CharacterEntity>
            _hitTargets =
                new();

        public CharacterAttackExecutor(
            CharacterEntity owner,
            int attackDamage,
            float attackRange,
            float attackRadius,
            LayerMask targetLayerMask)
        {
            _owner =
                owner
                ? owner
                : throw new ArgumentNullException(
                    nameof(owner));

            if (attackDamage <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(attackDamage));
            }

            if (attackRange < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(attackRange));
            }

            if (attackRadius <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(attackRadius));
            }

            _ownerTransform =
                owner.transform;

            _attackDamage =
                attackDamage;

            _attackRange =
                attackRange;

            _attackRadius =
                attackRadius;

            _targetLayerMask =
                targetLayerMask;
        }

        public int Execute()
        {
            _hitTargets.Clear();

            Vector3 center =
                _ownerTransform.position +
                _ownerTransform.forward *
                _attackRange;

            int count =
                Physics.OverlapSphereNonAlloc(
                    center,
                    _attackRadius,
                    _colliderBuffer,
                    _targetLayerMask,
                    QueryTriggerInteraction.Collide);

            int hitCount = 0;

            for (int i = 0; i < count; i++)
            {
                Collider collider =
                    _colliderBuffer[i];

                CharacterEntity target =
                    collider
                        .GetComponentInParent<CharacterEntity>();

                if (target == null ||
                    target == _owner ||
                    !target.IsInitialized)
                {
                    continue;
                }

                if (!_hitTargets.Add(target))
                {
                    continue;
                }

                target.Context
                    .DamageReceiver
                    .Receive(
                        new DamageRequest(
                            _attackDamage));

                hitCount++;
            }

            return hitCount;
        }
    }
}