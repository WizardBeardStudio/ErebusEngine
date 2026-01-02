namespace WizardBeardStudio.Events.PlayerCharacter
{
    public readonly struct SetTargetXpEvent
    {
        public readonly int Value;

        public SetTargetXpEvent(int value)
        {
            Value = value;
        }
    }
}