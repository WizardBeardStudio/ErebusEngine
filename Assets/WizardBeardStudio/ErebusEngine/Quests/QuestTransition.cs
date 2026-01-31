using System;
using System.Collections.Generic;

namespace WizardBeardStudio.ErebusEngine.Quests
{
    [Serializable]
    public sealed class QuestTransition
    {
        public string Label;              // button label
        public List<string> RequiresTags; // gating tags, inventory flags, etc.
    }
}
