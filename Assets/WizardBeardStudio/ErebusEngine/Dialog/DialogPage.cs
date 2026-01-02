using System.Collections.Generic;
using UnityEngine;
using WizardBeardStudio.ErebusEngine.Core;

namespace WizardBeardStudio.ErebusEngine.Dialog
{
    public class DialogPage : MonoBehaviour
    {
        [field: SerializeField] public bool IsStartPage { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public List<Actor> Actors { get; private set; }
        [field: SerializeField] public Sprite Portrait { get; private set; }
        [field: SerializeField] public Sprite HeaderBG { get; private set; }
        [TextArea(minLines: 4, maxLines: 8)][field: SerializeField] public string DialogText { get; private set; }
    }
}