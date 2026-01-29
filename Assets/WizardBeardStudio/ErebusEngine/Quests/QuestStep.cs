namespace WizardBeardStudio.ErebusEngine.Quests
{
    [Serializable]
    public sealed class QuestStep
    {
        public string Id;
        public string Title;
        [TextArea] public string Description;
    }
}
