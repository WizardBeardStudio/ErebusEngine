namespace WizardBeardStudio.Events.PlayerCharacter
{
    /// <summary>
    /// Simple struct to broadcast XP gains. 
    /// </summary>
    public readonly struct GainExperienceEvent
    {
        public readonly int Total;
        public readonly int Multiplier;

        public GainExperienceEvent(int total, int multiplier)
        {
            Total = total;
            Multiplier = multiplier;
        }
        
        public int Calculate()
        {
            return Total * Multiplier;
        }
    }
}