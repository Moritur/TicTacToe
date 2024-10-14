#nullable enable
namespace TicTac.Gameplay.Time
{
    /// <summary>Action that was scheduled to be invoked after some time.</summary>
    public interface IScheduledAction
    {
        /// <summary>Time that has to pass since the action was scheduled or restarted for it to be invoked.</summary>
        public float Delay { get; }
        /// <summary>Time remaining before action will be invoked if it's not canceled or restarted before that.</summary>
        /// <remarks>If action was already invoked or canceled this will be less than or equal to 0.</remarks>
        public float TimeRemaining { get; }

        /// <summary>Cancels the action, so it won't be invoked.</summary>
        /// <remarks>Canceling an action that was already invoked or canceled has no effect.</remarks>
        public void Cancel();

        /// <summary>Resets the timer to its original value and invokes the action once it runs to completion.</summary>
        /// <remarks>Can be used when action is in any state, including not yet invoked, already invoked and canceled.</remarks>
        public void Restart();
    }
}
