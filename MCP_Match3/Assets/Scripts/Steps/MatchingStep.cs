using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>
    /// Matching step: Runs the burst → drop → cascade loop each frame
    /// until no more matches remain, then advances to the next step.
    /// 
    /// The actual burst/drop/cascade is driven by Coroutine_Switching in MatchManager.
    /// This step simply prevents player input while matching is in progress.
    /// When the coroutine finishes, it calls SetStep(Mission) to advance.
    /// </summary>
    public class MatchingStep : BaseStep
    {
        public MatchingStep(MatchManager matchManager) : base(matchManager) { }

        public override void Step_Play()
        {
            // Matching is driven by the Coroutine_Switching coroutine.
            // This step exists to block player input during the sequence.
        }

        public override void Step_Process()
        {
            // The coroutine handles everything. Nothing to do per-frame here.
            // When the coroutine finishes, it transitions to the next step.
        }
    }
}
