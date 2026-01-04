using System.Collections.Generic;
using UnityEngine;
using WizardBeardStudio.ErebusEngine.Core;

namespace WizardBeardStudio.ErebusEngine.Dialog
{
    /// <summary>
    /// Stack individual <see cref="DialogPage"/> <see cref="GameObject"/> instances.
    /// An instance of <see cref="DialogNavButton"/> dyanmically creates an instance of <see cref="GameObjectTree{T}"/> where T : <see cref="DialogPage"/>
    /// </summary>
    public class DialogPage : MonoBehaviour
    {
        [field: SerializeField] public bool IsStartPage { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public List<Actor> Actors { get; private set; }
        [field: SerializeField] public Sprite Portrait { get; private set; }
        [field: SerializeField] public Sprite HeaderBg { get; private set; }
        [TextArea(minLines: 4, maxLines: 8)][field: SerializeField] public string DialogText { get; private set; }
    }
}