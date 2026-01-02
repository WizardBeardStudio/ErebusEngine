using UnityEngine;
using WizardBeardStudio.ErebusEngine.Core;

namespace WizardBeardStudio.ErebusEngine.PlayerCharacter
{
    public class Player : Singleton<Player>
    {
        [field: SerializeField] public int Level { get; private set; }

        private void Start()
        {
            
        }
    }
}