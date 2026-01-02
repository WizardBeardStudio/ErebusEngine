using UnityEngine;
using WizardBeardStudio.ErebusEngine.EventBus;

namespace WizardBeardStudio.ErebusEngine.Core
{
    /// <summary>
    /// Transform any class into a GameObject Singleton. 
    /// </summary>
    /// <typeparam name="T">Must inherit from <see cref="MonoBehaviour"/></typeparam>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }
        
        /// <summary>
        /// Singleton Instance management is handled when a <see cref="Singleton{T}"/> gameObject calls Awake().
        /// Attaching a <see cref="MonoBehaviour"/> of <see cref="T"/> in scene is required to set Instance.
        /// </summary>
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}