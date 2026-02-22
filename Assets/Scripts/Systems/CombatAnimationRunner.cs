using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        get => _speedMultiplier;
        set => _speedMultiplier = Mathf.Max(0.1f, value);
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

        _queue.Enqueue(action);
        Log.Info("Action enqueued", new { actionType = action.GetType().Name, queueSize = _queue.Count });
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
                _queue.Enqueue(action);

        Log.Info("Actions enqueued", new { queueSize = _queue.Count });
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

        _runCoroutine = StartCoroutine(Run(ctx));
    }

    /// <summary>
    ///     Requests to skip all remaining animations.
    ///     Current action will complete, but subsequent actions will be skipped.
    /// </summary>
    public void SkipAnimations()
    {
        _skipRequested = true;
        Log.Info("Skip requested");
    }

    /// <summary>
    ///     Cancels the animation playback immediately.
    ///     Clears the queue and stops the runner.
    /// </summary>
    public void Cancel()
    {
        if (_runCoroutine != null)
        {
            StopCoroutine(_runCoroutine);
            _runCoroutine = null;
        }

        _queue.Clear();
        IsRunning = false;
        _skipRequested = false;

        Log.Info("Animation playback cancelled");
    }

    /// <summary>
    ///     Clears all queued actions without stopping current playback.
    /// </summary>
    public void ClearQueue()
    {
        _queue.Clear();
        Log.Info("Queue cleared");
    }

    private IEnumerator Run(AnimationContext ctx)
    {
        IsRunning = true;
        _skipRequested = false;

        Log.Info("Animation playback started", new { queueSize = _queue.Count });

        while (_queue.Count > 0 && !_skipRequested)
        {
            var action = _queue.Dequeue();

            // Apply speed multiplier by scaling time
            var originalTimeScale = Time.timeScale;

            try
            {
                Time.timeScale = _speedMultiplier;
                yield return action.Play(ctx);
            }
            finally
            {
                // Restore original timescale
                Time.timeScale = originalTimeScale;
            }
        }

        if (_skipRequested)
        {
            Log.Info("Animation playback skipped", new { remainingActions = _queue.Count });
            _queue.Clear();
        }

        IsRunning = false;
        _skipRequested = false;

        Log.Info("Animation playback completed");
    }
}