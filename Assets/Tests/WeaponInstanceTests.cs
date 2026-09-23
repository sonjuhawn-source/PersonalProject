using NUnit.Framework;
using UnityEngine;

namespace Game.Gameplay.Tests
{
    public class WeaponInstanceTests
    {
        private WeaponData data;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(data);
        }

        [Test]
        public void 강화_0레벨이면_원본_그대로다()
        {
            data = SoFixture.Weapon(new[] { 10 });
            var weapon = new WeaponInstance(data, 0);

            Assert.AreEqual(10, weapon.GetAttack(0).damage);
        }

        [Test]
        public void 강화_1레벨에_데미지가_최소_1은_오른다()
        {
            data = SoFixture.Weapon(new[] { 1 });
            var weapon = new WeaponInstance(data, 1);

            Assert.AreEqual(2, weapon.GetAttack(0).damage);
        }

        [Test]
        public void 원본이_크면_배율이_가드를_이긴다()
        {
            data = SoFixture.Weapon(new[] { 100 });
            var weapon = new WeaponInstance(data, 1);

            Assert.AreEqual(115, weapon.GetAttack(0).damage);
        }

        [Test]
        public void 강화해도_activeTime_은_안_변한다()
        {
            data = SoFixture.Weapon(new[] { 10 }, activeTime: 0.1f);
            var weapon = new WeaponInstance(data, 3);

            Assert.AreEqual(0.1f, weapon.GetAttack(0).activeTime, 0.0001f);
        }

        [Test]
        public void 강화하면_선딜과_후딜이_줄어든다()
        {
            data = SoFixture.Weapon(new[] { 10 }, startup: 0.2f, recovery: 0.2f);
            var plain = new WeaponInstance(data, 0);
            var upgraded = new WeaponInstance(data, 2);

            Assert.Less(upgraded.GetAttack(0).startup, plain.GetAttack(0).startup);
            Assert.Less(upgraded.GetAttack(0).recovery, plain.GetAttack(0).recovery);
        }

        [Test]
        public void GetAttack_은_원본_SO_를_바꾸지_않는다()
        {
            data = SoFixture.Weapon(new[] { 10 });
            var weapon = new WeaponInstance(data, 5);

            AttackData 사본 = weapon.GetAttack(0);

            Assert.Greater(사본.damage, 10, "강화가 사본에 반영되지 않았다");
            Assert.AreEqual(10, data.GetAttack(0).damage, "원본 SO 가 바뀌었다");
        }

        [Test]
        public void 생성자에_음수_레벨을_주면_0_이_된다()
        {
            data = SoFixture.Weapon(new[] { 10 });
            var weapon = new WeaponInstance(data, -3);

            Assert.AreEqual(0, weapon.UpgradeLevel);
            Assert.AreEqual(10, weapon.GetAttack(0).damage);
        }

        [Test]
        public void Upgrade_는_0_이하를_무시한다()
        {
            data = SoFixture.Weapon(new[] { 10 });
            var weapon = new WeaponInstance(data, 1);

            weapon.Upgrade(0);
            weapon.Upgrade(-5);

            Assert.AreEqual(1, weapon.UpgradeLevel);
        }
    }
}