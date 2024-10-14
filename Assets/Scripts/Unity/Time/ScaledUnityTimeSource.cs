using System;
using TicTac.Gameplay.Time;
using UnityEngine;

namespace TicTac.Unity.Time
{
    /// <summary>Exposes scaled time values from <see cref="UnityEngine.Time"/> and allows scheduling actions based on that time.</summary>
    public class ScaledUnityTimeSource : ITimeSource
    {
        /// <summary>Used to start coroutines.</summary>
        /// <remarks>When this object is destroyed or becomes disabled <see cref="Schedule(Action, float)"/> and all actions created with it will stop working.</remarks>
        readonly MonoBehaviour behaviour;

        public float CurrentTime => UnityEngine.Time.time;

        /// <summary>Creates new instance that will use <paramref name="behaviour"/> to start coroutines.</summary>
        /// <param name="behaviour">Used to start coroutines. Should not be destroyed or disabled as long as this instance is used.</param>
        public ScaledUnityTimeSource(MonoBehaviour behaviour) => this.behaviour = behaviour;

        /// <inheritdoc cref="ITimeSource.Schedule(Action, float)"/>
        public ScheduledScaledCoroutineAction Schedule(Action action, float delay) => new ScheduledScaledCoroutineAction(behaviour, delay, action);

        IScheduledAction ITimeSource.Schedule(Action action, float delay) => this.Schedule(action, delay);
    }
}
