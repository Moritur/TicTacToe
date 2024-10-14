#nullable enable
using System;

namespace TicTac.Gameplay.Time
{
    /// <summary>Can be used to check current time and schedule operations based on time.</summary>
    public interface ITimeSource
    {
        /// <summary>Current time in seconds.</summary>
        public float CurrentTime { get; }

        /// <summary>Invokes <paramref name="action"/> after <paramref name="delay"/> seconds.</summary>
        public IScheduledAction Schedule(Action action, float delay);
    }
}
