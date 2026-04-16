using UnityEngine;
using Match3.Core;
using Match3.Data;
using Match3.UI;

namespace Match3.Steps
{
    public class FailStep : BaseStep
    {
        public override bool IsItemStep => false;

        public FailStep(MatchManager matchManager) : base(matchManager) { }

        public override void Step_Play()
        {
            Debug.Log("[FailStep] Mission failed!");

            if (PopupManager.Instance)
            {
                PopupManager.Instance.Show<DefeatPopup>(
                    p => p.SetContent("¡Sin movimientos!"),
                    onClosed: () => RetryLevel());
            }
            else
            {
                RetryLevel();
            }
        }

        private void RetryLevel()
        {
            Debug.Log("[FailStep] Retrying level...");
        }
    }
}
