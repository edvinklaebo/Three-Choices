using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class UnitViewTests
    {
        private static Unit CreateUnit(string name, int hp, int attack, int armor, int speed)
        {
            return new Unit(name)
            {
                Stats = new Stats
                {
                    MaxHP = hp,
                    CurrentHP = hp,
                    AttackPower = attack,
                    Armor = armor,
                    Speed = speed
                }
            };
        }

        [Test]
        public void Initialize_WithValidUnit_SetsUnitReference()
        {
            var go = new GameObject("TestUnitView");
            var unitView = go.AddComponent<UnitView>();

            var unit = CreateUnit("TestUnit", 100, 10, 5, 5);
            unitView.Initialize(unit, isPlayer: true, null);

            Assert.AreEqual(unit, unitView.Unit);
            Assert.IsTrue(unitView.IsPlayer);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Initialize_AsPlayer_SetsCorrectFlag()
        {
            var go = new GameObject("TestUnitView");
            var unitView = go.AddComponent<UnitView>();

            var unit = CreateUnit("Player", 100, 10, 5, 5);
            unitView.Initialize(unit, isPlayer: true, null);

            Assert.IsTrue(unitView.IsPlayer);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Initialize_AsEnemy_SetsCorrectFlag()
        {
            var go = new GameObject("TestUnitView");
            var unitView = go.AddComponent<UnitView>();

            var unit = CreateUnit("Enemy", 80, 8, 3, 4);
            unitView.Initialize(unit, isPlayer: false, null);

            Assert.IsFalse(unitView.IsPlayer);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Initialize_WithIdlePoint_PositionsAtIdlePoint()
        {
            var go = new GameObject("TestUnitView");
            var unitView = go.AddComponent<UnitView>();

            var idlePointGo = new GameObject("IdlePoint");
            idlePointGo.transform.SetParent(go.transform);
            idlePointGo.transform.position = new Vector3(5, 0, 0);

            var field = typeof(UnitView).GetField("_idlePoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(unitView, idlePointGo.transform);

            var unit = CreateUnit("TestUnit", 100, 10, 5, 5);
            unitView.Initialize(unit, isPlayer: true, null);

            Assert.AreEqual(new Vector3(5, 0, 0), unitView.transform.position);

            // Cleanup: idlePointGo is a child of go and will be destroyed with parent
            Object.DestroyImmediate(go);
        }

        [Test]
        public void IdlePoint_ReturnsCorrectTransform()
        {
            var go = new GameObject("TestUnitView");
            var unitView = go.AddComponent<UnitView>();

            var idlePointGo = new GameObject("IdlePoint");
            idlePointGo.transform.SetParent(go.transform);

            var field = typeof(UnitView).GetField("_idlePoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(unitView, idlePointGo.transform);

            Assert.AreEqual(idlePointGo.transform, unitView.IdlePoint);

            // Cleanup: idlePointGo is a child of go and will be destroyed with parent
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Initialize_WithPortrait_SetsSprite()
        {
            var go = new GameObject("TestUnitView");
            var unitView = go.AddComponent<UnitView>();

            var spriteGo = new GameObject("Sprite");
            spriteGo.transform.SetParent(go.transform);
            var spriteRenderer = spriteGo.AddComponent<SpriteRenderer>();

            var field = typeof(UnitView).GetField("_spriteRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(unitView, spriteRenderer);

            var texture = new Texture2D(1, 1);
            var portrait = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero);

            var unit = CreateUnit("Player", 100, 10, 5, 5);
            unitView.Initialize(unit, isPlayer: true, portrait);

            Assert.AreEqual(portrait, spriteRenderer.sprite);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void LungePoint_ReturnsCorrectTransform()
        {
            var go = new GameObject("TestUnitView");
            var unitView = go.AddComponent<UnitView>();

            var lungePointGo = new GameObject("LungePoint");
            lungePointGo.transform.SetParent(go.transform);

            var field = typeof(UnitView).GetField("_lungePoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(unitView, lungePointGo.transform);

            Assert.AreEqual(lungePointGo.transform, unitView.LungePoint);

            Object.DestroyImmediate(go);
        }
    }
}
