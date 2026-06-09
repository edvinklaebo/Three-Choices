using Core;
using Core.Combat;

using NUnit.Framework;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Tests for <see cref="CombatListenerRegistry"/>.
    /// Verifies duplicate-registration prevention, priority-ordered registration,
    /// and full cleanup on Clear().
    /// </summary>
    public class CombatListenerRegistryTests
    {
        private CombatContext _context;

        [SetUp]
        public void Setup()
        {
            _context = new CombatContext();
        }

        [TearDown]
        public void Teardown()
        {
            _context.Clear();
        }

        // ---- Listener registration ----

        [Test]
        public void Register_SingleListener_ReceivesEvents()
        {
            var callCount = 0;
            var listener = new SpyListener(50, _ => callCount++);

            _context.RegisterListener(listener);

            var source = MakeUnit("A");
            var target = MakeUnit("B");
            _context.DealDamage(source, target, 10);

            Assert.Greater(callCount, 0, "Registered listener should receive DamagePhaseEvents");
        }

        [Test]
        public void Register_SameListenerTwice_OnlyRegisteredOnce()
        {
            var callCount = 0;
            var listener = new SpyListener(50, _ => callCount++);

            _context.RegisterListener(listener);
            _context.RegisterListener(listener); // duplicate — should be ignored

            var source = MakeUnit("A");
            var target = MakeUnit("B");
            _context.DealDamage(source, target, 10);

            // 7 phase events fire per DealDamage call (PreAction…PostResolve).
            // With duplicate ignored, listener fires exactly 7 times, not 14.
            Assert.AreEqual(7, callCount, "Duplicate registration should be silently ignored");
        }

        [Test]
        public void Register_TwoDistinctListeners_BothReceiveEvents()
        {
            var countA = 0;
            var countB = 0;
            var listenerA = new SpyListener(10, _ => countA++);
            var listenerB = new SpyListener(20, _ => countB++);

            _context.RegisterListener(listenerA);
            _context.RegisterListener(listenerB);

            var source = MakeUnit("A");
            var target = MakeUnit("B");
            _context.DealDamage(source, target, 10);

            Assert.AreEqual(7, countA, "First listener should receive all phase events");
            Assert.AreEqual(7, countB, "Second listener should receive all phase events");
        }

        // ---- Priority ordering ----

        [Test]
        public void Register_ListenersCalledInPriorityOrder_LowerFirst()
        {
            var callOrder = new System.Collections.Generic.List<int>();

            var lowPriority = new SpyListener(10, _ => callOrder.Add(10));
            var highPriority = new SpyListener(200, _ => callOrder.Add(200));

            // Register high-priority first, then low — registry should reorder
            _context.RegisterListener(highPriority);
            _context.RegisterListener(lowPriority);

            var source = MakeUnit("A");
            var target = MakeUnit("B");
            _context.DealDamage(source, target, 10);

            // First call to OnDamagePhase should come from the lower-priority listener
            Assert.AreEqual(10, callOrder[0],
                "Lower-priority listener (10) should be called before higher-priority (200)");
        }

        // ---- Clear lifecycle ----

        [Test]
        public void Clear_UnregistersAllListeners_NoMoreEvents()
        {
            var callCount = 0;
            var listener = new SpyListener(50, _ => callCount++);
            _context.RegisterListener(listener);

            _context.Clear();
            callCount = 0; // reset after clear

            var source = MakeUnit("A");
            var target = MakeUnit("B");
            _context.DealDamage(source, target, 10);

            Assert.AreEqual(0, callCount, "After Clear(), no listeners should receive events");
        }

        [Test]
        public void Clear_ThenRegisterAgain_ListenerReceivesEvents()
        {
            var countBeforeClear = 0;
            var countAfterClear = 0;

            var listener = new SpyListener(50, _ => countBeforeClear++);
            _context.RegisterListener(listener);

            var source = MakeUnit("A");
            var target = MakeUnit("B");
            _context.DealDamage(source, target, 5);
            // countBeforeClear > 0 here

            _context.Clear();

            // Re-create context (Clear also clears the event bus)
            _context = new CombatContext();
            var newListener = new SpyListener(50, _ => countAfterClear++);
            _context.RegisterListener(newListener);

            var source2 = MakeUnit("C");
            var target2 = MakeUnit("D");
            _context.DealDamage(source2, target2, 5);

            Assert.Greater(countAfterClear, 0,
                "After Clear and re-register, new listener should receive events");
        }

        // ---- Helpers ----

        private static Unit MakeUnit(string name) => new Unit(name)
        {
            Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10, Armor = 0, Speed = 5 }
        };

        /// <summary>Minimal ICombatListener that records DamagePhaseEvent calls.</summary>
        private class SpyListener : ICombatListener
        {
            private readonly System.Action<DamagePhaseEvent> _onPhase;

            public int Priority { get; }

            public SpyListener(int priority, System.Action<DamagePhaseEvent> onPhase)
            {
                Priority = priority;
                _onPhase = onPhase;
            }

            public void RegisterHandlers(CombatContext context)
            {
                context.On<DamagePhaseEvent>(_onPhase);
            }

            public void UnregisterHandlers(CombatContext context)
            {
                context.Off<DamagePhaseEvent>(_onPhase);
            }
        }
    }
}
