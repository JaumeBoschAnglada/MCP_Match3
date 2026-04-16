using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>
    /// Base class for all turn steps in the match-3 state machine.
    /// Each step represents one phase of the turn cycle.
    /// </summary>
    public abstract class BaseStep
    {
        protected MatchManager MatchMgr;

        public virtual bool IsItemStep => true;

        public BaseStep(MatchManager matchManager)
        {
            MatchMgr = matchManager;
        }

        /// <summary>Called once when the step system is initialized.</summary>
        public virtual void Step_Init() { }

        /// <summary>Called once when this step becomes active (transition in).</summary>
        public virtual void Step_Play() { }

        /// <summary>Called every frame while this step is active.</summary>
        public virtual void Step_Process() { }
    }
}
