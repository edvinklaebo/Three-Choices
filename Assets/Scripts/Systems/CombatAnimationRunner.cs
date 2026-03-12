using System.Collections;
using System.Collections.Generic;

using Core;

using Interfaces;

using UnityEngine;

using Utils;

namespace Systems
{
    /// <summary>
    ///     Responsible for sequential playback of combat actions.
    ///     Manages animation queue and provides playback control.
    /// </summary>
    public class CombatAnimationRunner : MonoBehaviour
    {
        private readonly Queue<ICombatAction> _queue = new();
        private Coroutine _runCoroutine;
        private bool _skipRequested;
        private float _speedMultiplier = 1f;

        /// <summary>
        ///     Whether the animation runner is currently playing actions.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        ///     Returns a yield instruction that waits until animation playback completes.
        ///     Prefer this over polling <see cref="IsRunning"/> directly.
        /// </summary>
        public WaitUntil WaitForCompletion() => new WaitUntil(() => !IsRunning);

        /// <summary>
        ///     Speed multiplier for animations. Higher values = faster playback.
        /// </summary>
        public float SpeedMultiplier
        {
            get => this._speedMultiplier;
            set => this._speedMultiplier = Mathf.Max(0.1f, value);
        }

        private void OnDisable()
        {
            // Clean up if disabled during playback
            if (IsRunning) Cancel();
        }

        /// <summary>
        ///     Enqueues a single combat action for playback.
        /// </summary>
        public void Enqueue(ICombatAction action)
        {
            if (action == null)
            {
                Log.Error("CombatAnimationRunner: Cannot enqueue null action");
                return;
            }

            this._queue.Enqueue(action);
            Log.Info("Action enqueued", new { actionType = action.GetType().Name, queueSize = this._queue.Count });
        }

        /// <summary>
        ///     Enqueues multiple combat actions for playback.
        /// </summary>
        public void EnqueueRange(IEnumerable<ICombatAction> actions)
        {
            if (actions == null)
            {
                Log.Error("CombatAnimationRunner: Cannot enqueue null action collection");
                return;
            }

            foreach (var action in actions)
                if (action != null)
                    this._queue.Enqueue(action);

            Log.Info("Actions enqueued", new { queueSize = this._queue.Count });
        }

        /// <summary>
        ///     Starts playing all queued actions sequentially.
        /// </summary>
        public void PlayAll(AnimationContext ctx)
        {
            if (ctx == null)
            {
                Log.Error("CombatAnimationRunner: Cannot play with null context");
                return;
            }

            if (IsRunning)
            {
                Log.Warning("CombatAnimationRunner: Already running, cannot start playback");
                return;
            }

            this._runCoroutine = StartCoroutine(Run(ctx));
        }

        /// <summary>
        ///     Requests to skip all remaining animations.
        ///     Current action will complete, but subsequent actions will be skipped.
        /// </summary>
        public void SkipAnimations()
        {
            this._skipRequested = true;
            Log.Info("Skip requested");
        }

        /// <summary>
        ///     Cancels the animation playback immediately.
        ///     Clears the queue and stops the runner.
        /// </summary>
        public void Cancel()
        {
            if (this._runCoroutine != null)
            {
                StopCoroutine(this._runCoroutine);
                this._runCoroutine = null;
            }

            this._queue.Clear();
            IsRunning = false;
            this._skipRequested = false;

            Log.Info("Animation playback cancelled");
        }

        /// <summary>
        ///     Clears all queued actions without stopping current playback.
        /// </summary>
        public void ClearQueue()
        {
            this._queue.Clear();
            Log.Info("Queue cleared");
        }

        private IEnumerator Run(AnimationContext ctx)
        {
            IsRunning = true;
            this._skipRequested = false;

            Log.Info("Animation playback started", new { queueSize = this._queue.Count });

            while (this._queue.Count > 0 && !this._skipRequested)
            {
                var action = this._queue.Dequeue();

                // Apply speed multiplier by scaling time
                var originalTimeScale = Time.timeScale;

                try
                {
                    Time.timeScale = this._speedMultiplier;
                    yield return action.Play(ctx);
                }
                finally
                {
                    // Restore original timescale
                    Time.timeScale = originalTimeScale;
                }
            }

            if (this._skipRequested)
            {
                Log.Info("Animation playback skipped", new { remainingActions = this._queue.Count });
                this._queue.Clear();
            }

            IsRunning = false;
            this._skipRequested = false;

            Log.Info("Animation playback completed");
        }
    }
}