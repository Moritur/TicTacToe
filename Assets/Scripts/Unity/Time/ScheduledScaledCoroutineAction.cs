using System;
using System.Collections;
using TicTac.Gameplay.Time;
using UnityEngine;

namespace TicTac.Unity.Time
{
    /// <summary>Action that was scheduled to be invoked after some scaled time using a coroutine.</summary>
    public class ScheduledScaledCoroutineAction : IScheduledAction
    {
        /// <summary>Used to wait in coroutine.</summary>
        /// <remarks>Reused after restart to save allocations.</remarks>
        readonly WaitForSeconds wait;
        /// <summary>Used to start coroutines.</summary>
        readonly MonoBehaviour behaviour;
        /// <summary>Will be invoked after the delay.</summary>
        readonly Action action;

        /// <summary>Coroutine used to delay action in time.</summary>
        Coroutine coroutine;
        /// <summary>Time when the action was scheduled or restarted in seconds.</summary>
        float startTime;

        public float Delay { get; }

        public float TimeRemaining => (startTime + Delay) - UnityEngine.Time.time;

        /// <summary>Creates and schedules new action to be invoked after <paramref name="delay"/>.</summary>
        /// <param name="behaviour">Used to start coroutines. Should not be disabled or destroyed until this object is no longer in use.</param>
        /// <param name="delay">Number of seconds after which <paramref name="action"/> will be invoked.</param>
        public ScheduledScaledCoroutineAction(MonoBehaviour behaviour, float delay, Action action)
        {
            this.behaviour = behaviour;
            this.action = action;
            Delay = delay;
            wait = new(delay);

            ThrowIfDisabledOrDestroyed();

            Restart();
        }

        public void Cancel()
        {
            if (coroutine == null) return; //If already finished or canceled there is no need to do anything

            ThrowIfDisabledOrDestroyed();

            behaviour.StopCoroutine(coroutine);
            coroutine = null;
            startTime = float.MinValue; //Make sure TimeRemaining will not be positive after this
        }

        public void Restart()
        {
            ThrowIfDisabledOrDestroyed();

            Cancel(); //If previous coroutine is still running it should be canceled
            startTime = UnityEngine.Time.time;
            coroutine = behaviour.StartCoroutine(InvokeCoroutine());
        }

        /// <summary>Coroutine that invokes <see cref="action"/> after <see cref="Delay"/>.</summary>
        IEnumerator InvokeCoroutine()
        {
            yield return wait;
            action();
            coroutine = null;
        }

        void ThrowIfDisabledOrDestroyed()
        {
            if (behaviour == null)             throw new InvalidOperationException($"Can't perform this operation after {nameof(behaviour)} was destoryed.");
            if (!behaviour.isActiveAndEnabled) throw new InvalidOperationException($"Can't perform this operation when {nameof(behaviour)} '{behaviour.name}' is disabled.");
        }
    }
}
