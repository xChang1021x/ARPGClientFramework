using System;
using ARPG.Game.Character.Attribute;
using NUnit.Framework;

namespace ARPG.Game.Tests.EditMode.Character
{
    public sealed class CharacterHealthTests
    {
        [Test]
        public void Constructor_ShouldInitializeAtMaxHealth()
        {
            var health =
                new CharacterHealth(
                    100);

            Assert.AreEqual(
                100,
                health.MaxHealth);

            Assert.AreEqual(
                100,
                health.CurrentHealth);

            Assert.IsTrue(
                health.IsAlive);
        }

        [Test]
        public void Constructor_ShouldThrow_WhenMaxHealthIsZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = new CharacterHealth(0);
                });
        }

        [Test]
        public void Constructor_ShouldThrow_WhenMaxHealthIsNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = new CharacterHealth(-1);
                });
        }

        [Test]
        public void TakeDamage_ShouldReduceCurrentHealth()
        {
            var health =
                new CharacterHealth(
                    100);

            bool changed =
                health.TakeDamage(
                    30);

            Assert.IsTrue(
                changed);

            Assert.AreEqual(
                70,
                health.CurrentHealth);

            Assert.IsTrue(
                health.IsAlive);
        }

        [Test]
        public void TakeDamage_ShouldClampAtZero()
        {
            var health =
                new CharacterHealth(
                    100);

            health.TakeDamage(
                999);

            Assert.AreEqual(
                0,
                health.CurrentHealth);

            Assert.IsFalse(
                health.IsAlive);
        }

        [Test]
        public void TakeDamage_ShouldDoNothing_WhenDamageIsZero()
        {
            var health =
                new CharacterHealth(
                    100);

            bool changed =
                health.TakeDamage(
                    0);

            Assert.IsFalse(
                changed);

            Assert.AreEqual(
                100,
                health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ShouldDoNothing_WhenDamageIsNegative()
        {
            var health =
                new CharacterHealth(
                    100);

            bool changed =
                health.TakeDamage(
                    -10);

            Assert.IsFalse(
                changed);

            Assert.AreEqual(
                100,
                health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ShouldDoNothing_WhenAlreadyDead()
        {
            var health =
                new CharacterHealth(
                    100);

            health.TakeDamage(
                100);

            bool changed =
                health.TakeDamage(
                    20);

            Assert.IsFalse(
                changed);

            Assert.AreEqual(
                0,
                health.CurrentHealth);
        }

        [Test]
        public void Heal_ShouldIncreaseCurrentHealth()
        {
            var health =
                new CharacterHealth(
                    100);

            health.TakeDamage(
                50);

            bool changed =
                health.Heal(
                    20);

            Assert.IsTrue(
                changed);

            Assert.AreEqual(
                70,
                health.CurrentHealth);
        }

        [Test]
        public void Heal_ShouldClampAtMaxHealth()
        {
            var health =
                new CharacterHealth(
                    100);

            health.TakeDamage(
                20);

            health.Heal(
                999);

            Assert.AreEqual(
                100,
                health.CurrentHealth);
        }

        [Test]
        public void Heal_ShouldDoNothing_WhenAlreadyAtMaxHealth()
        {
            var health =
                new CharacterHealth(
                    100);

            bool changed =
                health.Heal(
                    10);

            Assert.IsFalse(
                changed);

            Assert.AreEqual(
                100,
                health.CurrentHealth);
        }

        [Test]
        public void Heal_ShouldDoNothing_WhenAmountIsZero()
        {
            var health =
                new CharacterHealth(
                    100);

            health.TakeDamage(
                20);

            bool changed =
                health.Heal(
                    0);

            Assert.IsFalse(
                changed);

            Assert.AreEqual(
                80,
                health.CurrentHealth);
        }

        [Test]
        public void Heal_ShouldDoNothing_WhenCharacterIsDead()
        {
            var health =
                new CharacterHealth(
                    100);

            health.TakeDamage(
                100);

            bool changed =
                health.Heal(
                    50);

            Assert.IsFalse(
                changed);

            Assert.AreEqual(
                0,
                health.CurrentHealth);

            Assert.IsFalse(
                health.IsAlive);
        }
    }
}