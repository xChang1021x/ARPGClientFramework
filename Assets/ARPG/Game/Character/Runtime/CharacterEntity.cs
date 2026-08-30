using System;
using UnityEngine;

namespace ARPG.Game.Character
{
    /// <summary>
    /// Character在Unity场景中的运行时实体入口。
    ///
    /// 不直接负责：
    /// Skill逻辑
    /// Buff逻辑
    /// Attribute计算
    /// AI决策
    ///
    /// 它主要负责把Unity GameObject
    /// 和Character Runtime Context连接起来。
    /// </summary>
    public abstract class CharacterEntity
        : MonoBehaviour
    {
        public CharacterContext Context
        {
            get;
            private set;
        }

        public bool IsInitialized =>
            Context != null;

        internal void Initialize(
            CharacterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} " +
                    "has already been initialized.");
            }

            Context = context;

            OnInitialized();
        }

        protected virtual void OnInitialized()
        {
        }
    }
}