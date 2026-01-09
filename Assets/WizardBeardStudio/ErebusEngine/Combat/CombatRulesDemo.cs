using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WizardBeardStudio.ErebusEngine.RulesEngine.Builder;
using WizardBeardStudio.ErebusEngine.RulesEngine.Core;

namespace WizardBeardStudio.ErebusEngine.Combat
{
    public class CombatRulesDemo : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text Output { get; private set; }
        
        [field: SerializeField] public Slider Level { get; private set; }
        
        private string _rules = @"
        rule ""HighLevelBoss""
        priority 100
        when player.level >= 20 and enemy.tag == ""Boss""
        then outcome ""SpawnElite"" with { tier: 3, loot: ""legendary"" }
        stop

        rule ""Default""
        when true
        then outcome ""Allow"" with { reason: ""fallback"" }
        ";

        private IRulesEngine _engine;

        private Dictionary<string, RuleValue> _facts;

        private void OnEnable()
        {
            _engine = RulesEngineBuilder
                .FromText(_rules)
                .EnableTrace(true)
                .Build();
        }

        public void Decision()
        {
            _facts = new Dictionary<string, RuleValue>()
            {
                ["player.level"] = RuleValue.FromInt(Mathf.RoundToInt(Level.value)),
                ["enemy.tag"] = RuleValue.FromString("Boss")
            };
            
            var decision = _engine.Evaluate(new DictionaryRuleContext(_facts));
            try
            {
                var outcome = decision.Outcome;
                Debug.Log($"[Combat Rules Demo] Decision: ${outcome}");
                Output.text = "Decision: " + outcome;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Combat Rules Demo] Exception making decision: {e.Message}");
            }
        }
    }
}