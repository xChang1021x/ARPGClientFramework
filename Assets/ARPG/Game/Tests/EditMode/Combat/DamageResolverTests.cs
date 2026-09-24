using System;
using ARPG.Game.Character.Attribute;
using ARPG.Game.Combat.Damage;
using NUnit.Framework;

namespace ARPG.Game.Tests.EditMode.Combat
{
    public sealed class DamageResolverTests
    {
        private DamageResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _resolver =
                new DamageResolver();
        }

        [Test]
        public void Resolve_ShouldApplyRequestedDamage()
        {
            var health =
                new CharacterHealth(100);

            DamageResult result =
                _resolver.Resolve(
                    health,
                    new DamageRequest(30));

            Assert.AreEqual(
                30,
                result.RequestedDamage);

            Assert.AreEqual(
                30,
                result.AppliedDamage);

            Assert.AreEqual(
                70,
                result.RemainingHealth);

            Assert.IsTrue(
                result.Applied);

            Assert.IsFalse(
                result.Killed);
        }

        [Test]
        public void Resolve_ShouldReportActualDamage_WhenDamageExceedsHealth()
        {
            var health =
                new CharacterHealth(20);

            DamageResult result =
                _resolver.Resolve(
                    health,
                    new DamageRequest(100));

            Assert.AreEqual(
                100,
                result.RequestedDamage);

            Assert.AreEqual(
                20,
                result.AppliedDamage);

            Assert.AreEqual(
                0,
                result.RemainingHealth);

            Assert.IsTrue(
                result.Killed);
        }

        [Test]
        public void Resolve_ShouldReportKilled_WhenDamageExactlyReachesZero()
        {
            var health =
                new CharacterHealth(100);

            DamageResult result =
                _resolver.Resolve(
                    health,
                    new DamageRequest(100));

            Assert.AreEqual(
                100,
                result.AppliedDamage);

            Assert.AreEqual(
                0,
                result.RemainingHealth);

            Assert.IsTrue(
                result.Killed);
        }

        [Test]
        public void Resolve_ShouldNotReportKilled_WhenTargetWasAlreadyDead()
        {
            var health =
                new CharacterHealth(100);

            health.TakeDamage(100);

            DamageResult result =
                _resolver.Resolve(
                    health,
                    new DamageRequest(30));

            Assert.AreEqual(
                0,
                result.AppliedDamage);

            Assert.AreEqual(
                0,
                result.RemainingHealth);

            Assert.IsFalse(
                result.Applied);

            Assert.IsFalse(
                result.Killed);
        }

        [Test]
        public void Resolve_ShouldThrow_WhenTargetHealthIsNull()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    _resolver.Resolve(
                        null,
                        new DamageRequest(10));
                });
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-100)]
        public void DamageRequest_ShouldThrow_WhenDamageIsNotPositive(
            int damage)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = new DamageRequest(
                        damage);
                });
        }
    }
}