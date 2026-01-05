namespace WizardBeardStudio.Events.Dialog
{
    public readonly struct StartDialogEvent
    {
        public readonly string DialogManagerName;

        public StartDialogEvent(string dialogManagerName)
        {
            DialogManagerName = dialogManagerName;
        }
    }
}
