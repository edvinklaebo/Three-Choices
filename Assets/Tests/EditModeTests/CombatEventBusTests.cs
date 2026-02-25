using NUnit.Framework;
using System.Collections.Generic;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Tests for <see cref="CombatEventBus"/> in isolation.
    /// Verifies subscription, dispatch, unsubscription, and clear behaviour.
    /// </summary>
    public class CombatEventBusTests
    {
        private static Unit CreateUnit(string name) => new Unit(name)
        {
            Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10, Armor = 0, Speed = 10 }
        };

        [Test]
        public void On_RegisteredHandler_ReceivesRaisedEvent()
        {
            var bus = new CombatEventBus();
            var source = CreateUnit("A");
            var target = CreateUnit("B");
            var received = new List<OnHitEvent>();

            bus.On<OnHitEvent>(received.Add);
            bus.Raise(new OnHitEvent(source, target, 10));

            Assert.AreEqual(1, received.Count);
            Assert.AreEqual(10, received[0].Damage);
        }

        [Test]
        public void Off_RemovesHandler_StopsReceivingEvents()
        {
            var bus = new CombatEventBus();
            var source = CreateUnit("A");
            var target = CreateUnit("B");
            var callCount = 0;

            void Handler(OnHitEvent _) => callCount++;

            bus.On<OnHitEvent>(Handler);
            bus.Off<OnHitEvent>(Handler);
            bus.Raise(new OnHitEvent(source, target, 10));

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void Clear_RemovesAllHandlers_NoEventsDelivered()
        {
            var bus = new CombatEventBus();
            var source = CreateUnit("A");
            var target = CreateUnit("B");
            var callCount = 0;

            bus.On<OnHitEvent>(_ => callCount++);
            bus.Clear();
            bus.Raise(new OnHitEvent(source, target, 10));

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void Raise_WithNoHandlers_DoesNotThrow()
        {
            var bus = new CombatEventBus();
            var source = CreateUnit("A");
            var target = CreateUnit("B");

            Assert.DoesNotThrow(() => bus.Raise(new OnHitEvent(source, target, 5)));
        }

        [Test]
        public void MultipleHandlers_AllReceiveRaisedEvent()
        {
            var bus = new CombatEventBus();
            var source = CreateUnit("A");
            var target = CreateUnit("B");
            var callCount = 0;

            bus.On<OnHitEvent>(_ => callCount++);
            bus.On<OnHitEvent>(_ => callCount++);
            bus.Raise(new OnHitEvent(source, target, 10));

            Assert.AreEqual(2, callCount);
        }

        [Test]
        public void DifferentEventTypes_AreDispatchedIndependently()
        {
            var bus = new CombatEventBus();
            var source = CreateUnit("A");
            var target = CreateUnit("B");
            var hitCount = 0;
            var afterCount = 0;

            bus.On<OnHitEvent>(_ => hitCount++);
            bus.On<AfterAttackEvent>(_ => afterCount++);

            bus.Raise(new OnHitEvent(source, target, 10));

            Assert.AreEqual(1, hitCount);
            Assert.AreEqual(0, afterCount);
        }
    }
}
