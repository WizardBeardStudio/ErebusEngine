# Rules Engine DSL

## 1. Formal DSL Grammar

This is an **EBNF-style** grammar describing the implemented v1 DSL. Whitespace and comments may appear between tokens.

### Lexical Notes

* **Identifiers**: `[A-Za-z_][A-Za-z0-9_]*`
* **Strings**: `"` … `"` with escapes `\" \\ \n \r \t`
* **Numbers**: digits with optional decimal point (parsed as `int` when no dot; `double` when dot present)
* **Comments**:
  * Line: `// ...`
  * Block: `/* ... */`

### Grammar (EBNF)

```ebnf
RuleSet         ::= { Rule } EOF ;

Rule            ::= "rule" String
                    [ "priority" Number ]
                    "when" Expr
                    "then" "outcome" String
                    [ "with" Payload ]
                    [ "stop" ] ;

Payload         ::= "{" [ PayloadPairs ] "}" ;

PayloadPairs    ::= PayloadPair { "," PayloadPair } ;

PayloadPair     ::= PayloadKey ":" Literal ;

PayloadKey      ::= Identifier | String ;

Literal         ::= Number | String | "true" | "false" | "null" ;

Expr            ::= OrExpr ;

OrExpr          ::= AndExpr { "or" AndExpr } ;
AndExpr         ::= RelExpr { "and" RelExpr } ;

RelExpr         ::= AddExpr [ RelOp AddExpr ] ;
RelOp           ::= "==" | "!=" | "<" | "<=" | ">" | ">=" ;

AddExpr         ::= MulExpr { ("+" | "-") MulExpr } ;
MulExpr         ::= UnaryExpr { ("*" | "/" | "%") UnaryExpr } ;

UnaryExpr       ::= [ "not" | "-" ] Primary ;

Primary         ::= Literal
                  | SymbolOrCall
                  | "(" Expr ")" ;

SymbolOrCall    ::= SymbolPath | Call ;

SymbolPath      ::= Identifier { "." Identifier } ;

Call            ::= Identifier "(" [ ArgList ] ")" ;

ArgList         ::= Expr { "," Expr } ;
```

### Semantics (v1)

* Rules are evaluated in **priority-descending** order (stable among ties).
* Conditional truth uses `RuleValue.IsTruthy()`:
  * `null`, `false`, `0`, `0.0`, `""` → false
  * otherwise → true
* Missing symbols resolve to `null` and (optionally) appear in trace.
* Payload values are **literals only** (no expressions) in v1.
* `stop` is parsed but currently redundant under **first-match** evaluation (reserved for forward compatibility).

## 2. Designer Cheat Sheet

### Rule Template

```txt
rule "Name"
priority 100
when <condition>
then outcome "SomeOutcome" with { key: "value", n: 3 }
stop
```

### Condition Operators

* Boolean: `and`, `or`, `not`
* Compare: `==`, `!=`, `<`, `<=`, `>`, `>=`
* Math: `+`, `-`, `*`, `/`, `%`
* Grouping: `( ... )`

### Values

* Strings: `"Boss"`, `"legendary"`
* Numbers: `10`, `1.5`
* Booleans: `true`, `false`
* Null: `null`

### Symbols

* Use dotted paths for facts, e.g.:
  * `player.level`
  * `enemy.tag`
  * `inventory.gold`

### Common Patterns

#### Fallback

```txt
rule "Default"
when true
then outcome "Allow" with { reason: "fallback" }
```

#### Gate

```txt
rule "GateFeature"
priority 50
when player.level >= 15
then outcome "Unlock" with { feature: "AdvancedZone" }
```

#### Tiering

```txt
rule "Tier3"
priority 30
when score >= 300
then outcome "Tier" with { value: 3 }

rule "Tier2"
priority 20
when score >= 200
then outcome "Tier" with { value: 2 }

rule "Tier1"
priority 10
when score >= 100
then outcome "Tier" with { value: 1 }
```

### Authoring Tips

* Use **priority** to order rules; do not rely on file order when priorities differ.
* Keep conditions readable; prefer multiple rules over giant expressions.
* Always include a **Default** rule.

## 3. “Dos and Don’ts” for Production Authoring

### Do 

* **Do use one decision intent per rule**
  * Each rule should answer one question cleanly (e.g., “SpawnElite?”).
* **Do make rules total** (no undecidable gaps)
  * Always add a final `when true` fallback outcome.
* **Do standardize naming**
  * Use consistent prefixes or categories:
    * `"Spawn.HighLevelBoss"`
    * `"UI.LowHealthWarning"`
    * `"Economy.Discount.Whale"`
* **Do keep payloads declarative**
  * Payload is for “what to do,” not “how to do it.”
* **Do treat rules as pure logic**
  * No side effects during evaluation; apply effects in gameplay code from the returned decision.
* **Do define a canonical fact vocabulary**
  * Publish a list of supported symbol paths and types:
    * `player.level` (int)
    * `player.health` (int)
    * `enemy.tag` (string)
* **Do add trace in non-production builds**
  * Enable trace in editor/dev to debug rule behavior quickly.

### Don't

* **Don’t encode side effects into the DSL**
  * Avoid “delete item,” “spawn object,” etc. in rules; return decisions and let gameplay systems act.
* **Don’t depend on missing symbols**
  * Missing symbols resolve to `null` (falsey); rely on explicit facts.
* **Don’t write extremely long conditions**
  * Prefer splitting into multiple rules with priorities.
* **Don’t reuse the same rule name**
  * It complicates tracing, debugging, and regression tracking.
* **Don’t make outcomes ambiguous**
  * Decide whether outcomes are:
    * a closed enum set (recommended), or
    * free-form strings (allowed, but requires governance).
* **Don’t mix numeric and string comparisons unintentionally**
  * `"10"` is a string; `10` is a number. Keep types consistent in facts and rules.

### Governance Recommendations

* Maintain a version header outside the DSL (file metadata) and keep rule sets versioned in source control.
* Require code review for:
  * new outcomes
  * new symbol paths
  * priority changes in high-impact rule sets

## 4. CI Lint Checklist (Practical and Implementable)

This is a set of checks you can run in CI (or as an editor tool) to keep rule sets safe and maintainable.

### A. Syntax and Parse Checks

* [ ] DSL parses successfully (no tokenizer/parser exceptions).
* [ ] Each rule contains:
  * [ ] `rule "Name"`
  * [ ] `when <expr>`
  * [ ] `then outcome "X"`
* [ ] Payload braces are balanced and keys/values are valid literals.
* [ ] No illegal tokens (e.g., single `=` instead of `==`).

### B. Structural Checks

* [ ] Rule names are unique within the file / ruleset.
* [ ] At least one fallback rule exists: `when true`
* [ ] Priorities are within a defined range (example policy): `-1000..1000`
* [ ] No more than `N` rules per file (example policy): `<= 200` (tune to your needs)

### C. Expression Complexity Checks (maintainability)

* [ ] Maximum expression depth (example): `<= 12`
* [ ] Maximum operator count per condition (example): `<= 25`
* [ ] Maximum symbol references per condition (example): `<= 12`
* [ ] Disallow suspicious patterns (policy-driven):
  * [ ] chained comparisons that are hard to read
  * [ ] excessive parentheses

### D. Fact Vocabulary and Type Checks (requires a fact schema)

If you maintain a schema like:

* `player.level`: number
* `enemy.tag`: string
* `player.isPoisoned`: boolean

Then lint can enforce:

* [ ] All referenced symbols exist in the schema.
* [ ] Comparisons are type-valid:
  * string compared only with string
  * numeric comparisons use numeric operands
* [ ] Boolean operators operate on truthy expressions (or explicitly boolean, if you enforce strictness).

### E. Outcome Governance Checks

* [ ] Outcomes are in an allow-list (recommended) OR follow a naming convention.
* [ ] Payload keys are in an allow-list per outcome (optional but useful).
* [ ] Payload value types match expected schema per key (optional).

### F. Behavioral Checks (optional but high value)

With a small suite of test contexts:

* [ ] Golden tests: known inputs yield expected outcomes.
* [ ] Regression tests for high-impact rules.
* [ ] Coverage check: fallback triggers on empty context.

### G. Trace Quality Checks (developer ergonomics)

* [ ] Rule names are descriptive and stable (no random IDs).
* [ ] High-priority rules include a short payload reason field (policy choice).

### H. Suggested “Rule Set Header” Convention

```txt
// ruleset: Combat.Spawn
// version: 1.3.0
// owner: GameplayTeam
// lastReviewed: 2026-01-13
```
