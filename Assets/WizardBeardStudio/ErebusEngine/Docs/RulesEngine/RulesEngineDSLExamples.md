# Rules Engine DSL — Examples

This document provides worked examples of the Rules Engine DSL.
Each example is valid against the current parser and evaluator.

## 1. Minimal Rule (Hello World)

```txt
rule "AlwaysAllow"
when true
then outcome "Allow"
```

### Behavior

* Always matches.
* Returns `Decision.Outcome = "Allow"`.
* No payload data.
* Useful as a fallback or sanity check.

## 2. Priority and Fallback

```txt
rule "HighPriority"
priority 10
when score >= 100
then outcome "Win"

rule "Fallback"
when true
then outcome "Continue"
```

### Behavior

* Rules are sorted by priority (descending).
* "HighPriority" is evaluated first.
* "Fallback" guarantees a decision if nothing else matches.

## 3. Player Level Gating

```txt
rule "UnlockAdvancedZone"
priority 50
when player.level >= 15
then outcome "UnlockZone" with { zone: "Advanced" }
```

### Payload

```json
{
  "zone": "Advanced"
}
```

### Typical Usage

* Progression gates
* Feature unlocks
* Difficulty tiers

## 4. Multi-Condition (AND / OR)

```txt
rule "HardModeBoss"
priority 100
when player.level >= 20 and enemy.type == "Boss"
then outcome "SpawnElite"
```

```txt
rule "LowHealthWarning"
when player.health < 25 or player.isPoisoned == true
then outcome "ShowWarning"
```

### Notes

* `and` and `or` short-circuit.
* Truthiness rules:
  * `0`, `false`, `null`, empty string → false
  * Non-zero numbers and non-empty strings → true

## 5. String Comparison

```txt
rule "FactionCheck"
when player.faction == "Alliance"
then outcome "FriendlyNPC"
```

* String comparisons are **case-sensitive**.
* Comparison uses ordinal semantics.

## 6. Numeric Comparisons

```txt
rule "CriticalHealth"
priority 80
when player.health <= 10
then outcome "CriticalState"
```

### Supported operators:

* `==`, `!=`
* `<`, `<=`
* `>`, `>=`

Numeric types (`int`, `double`, `bool`) are safely coerced.

## 7. Arithmetic Expressions

```txt
rule "HighThreatEnemy"
when enemy.attack * enemy.speed > 150
then outcome "MarkHighThreat"
```

### Supported operators:

* `+`, `-`, `*`, `/`, `%`
* Parentheses supported: `(a + b) * c`

## 8. Payload Data (`with { ... }`)

```txt
rule "SpawnElite"
priority 100
when player.level >= 20 and enemy.tag == "Boss"
then outcome "SpawnEnemy" with {
  tier: 3,
  loot: "legendary",
  scale: 1.5,
  isElite: true
}
```

### Rules

* Payload values must be **literals** (v1).
* Supported payload types:
  * number
  * string
  * boolean
  * null

Payloads are returned verbatim in `Decision.Data`.

## 9. Stop Keyword

```txt
rule "TutorialGate"
priority 200
when player.hasCompletedTutorial == false
then outcome "ForceTutorial"
stop

rule "NormalFlow"
when true
then outcome "ContinueGame"
```

### Behavior

* `stop` signals that no further rules should be *conceptually* considered.
* Current engine uses **first-match semantics after priority sort**.
* `stop` exists for forward compatibility and documentation clarity.

## 10. Missing Symbols (Safe by Default)

```txt
rule "OptionalFeature"
when player.hasPremium == true
then outcome "EnablePremiumUI"
```

If `player.hasPremium` is missing:

* Evaluates as `null`
* Condition becomes false
* Rule does not match
* No exception thrown
* Missing symbol recorded in trace (if enabled)

## 11. Default Catch-All Rule

```txt
rule "Default"
when true
then outcome "Allow" with { reason: "fallback" }
```

**Strongly recommended**:

* Always include a final default rule.
* Makes engine behavior explicit and deterministic.

## 12. Full Example (Documentation Reference)

```txt
rule "HighLevelBoss"
priority 100
when player.level >= 20 and enemy.tag == "Boss"
then outcome "SpawnElite" with {
  tier: 3,
  loot: "legendary"
}
stop

rule "MidLevelEnemy"
priority 50
when player.level >= 10
then outcome "SpawnNormal" with {
  tier: 2
}

rule "Default"
when true
then outcome "Allow" with {
  reason: "fallback"
}
```

## 13. Mapping to C# Evaluation

```csharp
var decision = engine.Evaluate(context);

decision.Outcome;     // string
decision.Matched;     // bool
decision.Data["tier"] // RuleValue
decision.Trace;       // optional diagnostics
```

## Recommended Authoring Guidelines

* One **decision intent per rule**.
* Prefer **priority** over deeply nested conditions.
* Keep payloads small and declarative.
* Avoid encoding side effects in rule names.
* Treat rules as **pure decision logic**.
