using System;
using UnityEngine;
using WizardBeardStudio.ErebusEngine.Core;
using WizardBeardStudio.ErebusEngine.EventBus;
using WizardBeardStudio.Events.PlayerCharacter;

namespace WizardBeardStudio.ErebusEngine.PlayerCharacter
{
    public class ExperienceManager : Singleton<ExperienceManager>
    {
        private static SharedEventBus _sharedEventBus;
        
        [field: SerializeField] public int CurrentXp { get; private set; }
        [field: SerializeField] public int TargetXp { get; private set; }
        
        private void Start()
        {
            GetCurrentXp();
            GetTargetXp();
        }

        private void GetTargetXp()
        {
            try
            {
                if (PlayerPrefs.HasKey("TargetXp"))
                {
                    TargetXp = PlayerPrefs.GetInt("TargetXp");    
                }
                else
                {
                    TargetXp = 0;
                    PlayerPrefs.SetInt("TargetXp", TargetXp);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ExperienceManager] Exception in GetTargetXp(): {ex.Message}");
            }
        }

        private void GetCurrentXp()
        {
            try
            {
                if (PlayerPrefs.HasKey("CurrentXp"))
                {
                    CurrentXp = PlayerPrefs.GetInt("CurrentXp");    
                }
                else
                {
                    CurrentXp = 0;
                    PlayerPrefs.SetInt("CurrentXp", CurrentXp);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ExperienceManager] Exception in GetCurrentXp(): {ex.Message}");
            }
        }

        private void OnEnable()
        {
            _sharedEventBus = SharedEventBus.Instance;
            _sharedEventBus.Subscribe<GainExperienceEvent>(OnGainExperienceEvent);
            _sharedEventBus.Subscribe<SetTargetXpEvent>(OnSetTargetXpEvent);
        }

        private void OnDisable()
        {
            _sharedEventBus.Unsubscribe<GainExperienceEvent>(OnGainExperienceEvent);
            _sharedEventBus.Unsubscribe<SetTargetXpEvent>(OnSetTargetXpEvent);
        }

        private void OnGainExperienceEvent(GainExperienceEvent e)
        {
            int calculatedValue = e.Calculate();
            CurrentXp += calculatedValue;
            PlayerPrefs.SetInt("CurrentXp", CurrentXp);
            Debug.Log($"[Event] Gained XP Total: {e.Total.ToString()} with Multiplier: {e.Multiplier.ToString()} calculated as: {calculatedValue.ToString()}");
            Debug.Log($"[ExperienceManager] Current XP: {CurrentXp}");
        }

        private void OnSetTargetXpEvent(SetTargetXpEvent e)
        {
            Debug.Log($"[Event] Setting new Target XP Value: {e.Value.ToString()}");
            TargetXp = e.Value;
            PlayerPrefs.SetInt("TargetXp", TargetXp);
        }
    }
}