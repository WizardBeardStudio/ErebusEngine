namespace WizardBeardStudio.Events.Dialog
{
    public readonly struct EndDialogEvent
    {
        public readonly string DialogManagerName;

        public EndDialogEvent(string dialogManagerName)
        {
            DialogManagerName = dialogManagerName;
        }
    }
}
