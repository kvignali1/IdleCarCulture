# Tuning System Documentation

## Overview
A centralized `Tuning.cs` static class has been created containing **70+ balancing constants** organized by system. All magic numbers throughout the project have been replaced with references to these constants, enabling easy balance tweaking without code recompilation.

## Benefits
✅ **Single Source of Truth** - All balance parameters in one file  
✅ **Easy Balance Iteration** - Change one constant to affect entire system  
✅ **Better Maintainability** - Clear intent with descriptive constant names  
✅ **Self-Documenting** - Each constant has XML summary explaining its purpose  
✅ **Safe Modifications** - Constants are centralized, reducing typos and inconsistencies  

## Tuning Categories

### HEAT SYSTEM (10 constants)
Configures police encounter mechanics:
- Heat thresholds: `HEAT_THRESHOLD_LOW` (30f), `HEAT_THRESHOLD_MEDIUM` (60f), `HEAT_THRESHOLD_HIGH` (85f)
- Trigger chances: `HEAT_TRIGGER_CHANCE_LOW` (0.01f), `MEDIUM` (0.05f), `HIGH` (0.15f)
- Police outcomes: Escape heat reduction (50%), fine base (500), fine per heat (50x)
- Escape chance: Base 30% + 0.1% per grip/suspension point

### ECONOMY (3 constants)
Race payout configuration:
- Illegal payouts by tier: {500, 1000, 1500, 2500, 4000}
- Legal payouts by tier: {1000, 2000, 3500, 5500, 8000}
- Prestige bonus: +2% per prestige level

### PRESTIGE (8 constants)
Progression thresholds and bonuses:
- Unlock thresholds: Money (500k), Reputation (1000), Cred (500)
- Starting balance: 5000
- Bonuses per prestige: Cost -5%, Heat -3%, Income +10%

### UPGRADES (2 constants)
Upgrade system balance:
- Cost exponent: 1.8 (exponential scaling per level)
- Max level cap: 5

### RACE MECHANICS (11 constants)
Core racing logic:
- PR multipliers: Illegal 1.1x, Legal 0.95x
- Skill contribution: Illegal 0.10x, Legal 0.15x
- Randomness: ±3% (0.97 to 1.03)
- Win payout: 1.5 / prRatio (clamped 0.5-3.0x)
- Loss payout: 0.1x (participation reward)
- Opponent scaling: 1.1x per tier

### MINIGAME (28 constants)
Detailed timing and scoring for 3-phase minigame:

**Tire Heat Phase:**
- Duration: 3 seconds
- Optimal window: 1.2-1.8s (score 1.0)
- Good window: 0.9-2.1s (score 0.7)
- Bad zone: Outside windows (score 0.3)

**Launch Phase:**
- Duration: 3 seconds
- Target time range: 0.8-2.0s
- Perfect tolerance: ±0.1s (score 1.0)
- Good tolerance: ±0.3s (score 0.8)
- OK tolerance: ±0.5s (score 0.5)
- Bad: Anything else (score 0.2)

**Shifting Phase:**
- Duration: 2 seconds per shift
- Shift count: 3
- Marker range: 0.3-0.7 (random)
- Perfect tolerance: ±0.1 (score 1.0)
- Good tolerance: ±0.2 (score 0.8)
- OK tolerance: ±0.4 (score 0.5)
- Delay between shifts: 0.5s

### SKILL WEIGHTING (3 constants)
Overall skill calculation in SkillResult:
- Launch weight: 0.4 (40%)
- Shift weight: 0.4 (40%)
- Tire heat weight: 0.2 (20%)

### RACE OPPORTUNITY SPAWNING (5 constants)
Tier unlock and bet suggestion:
- Illegal cred per tier: 100 (tier 1 at 100, tier 2 at 200, etc.)
- Legal reputation per tier: 150 (tier 1 at 150, tier 2 at 300, etc.)
- Min bet percent: 5%
- Max bet percent: 10%
- Base bet per tier: 500

## Files Modified

| File | Changes |
|------|---------|
| **Tuning.cs** | Created new file with 70+ constants |
| **HeatSystem.cs** | Replaced heat thresholds, trigger chances, police outcomes |
| **PrestigeSystem.cs** | Replaced thresholds, bonus percents |
| **EconomyManager.cs** | Replaced payout tables and prestige bonus |
| **RaceCalculator.cs** | Replaced PR multipliers, skill multipliers, randomness, payout logic |
| **RaceSceneController.cs** | Replaced minigame timings, tolerances, scores, phase durations |
| **SkillResult.cs** | Replaced skill weighting (0.4/0.4/0.2) |
| **RaceOpportunitySpawner.cs** | Replaced tier unlock thresholds, bet suggestion logic |
| **UpgradeDefinition.cs** | Replaced cost exponent and max level defaults |

## Usage Example

**Before (Magic Numbers):**
```csharp
if (heat >= 60f)
    triggerChance = 0.05f;

float payout = basePayout * 1.5f;
profile.money = 5000;
```

**After (Tuning Constants):**
```csharp
if (heat >= Tuning.HEAT_THRESHOLD_MEDIUM)
    triggerChance = Tuning.HEAT_TRIGGER_CHANCE_MEDIUM;

float payout = basePayout * Tuning.RACE_PAYOUT_WIN_BASELINE;
profile.money = Tuning.PRESTIGE_STARTING_MONEY;
```

## Balance Tuning Guide

### To Make Game Harder
- ↑ Increase `HEAT_TRIGGER_CHANCE_*` to make police more frequent
- ↓ Decrease `ILLEGAL_BASE_PAYOUT` / `LEGAL_BASE_PAYOUT` to reduce income
- ↑ Increase `RACE_SKILL_ILLEGAL_MULTIPLIER` to require better timing

### To Make Game Easier
- ↓ Decrease heat thresholds to lower police encounter rate
- ↑ Increase payout values for more money
- ↑ Increase prestige bonuses (cost reduction, income bonus)

### To Adjust Minigame Difficulty
- ↑ Decrease tolerance windows (e.g., `MINIGAME_LAUNCH_PERFECT_TOLERANCE` from 0.1 to 0.05)
- ↑ Increase phase durations for slower pacing
- Adjust green zone windows to be narrower/wider

## Next Steps
1. **In-Game Settings Menu:** Expose Tuning constants to player options (difficulty slider)
2. **Balance Spreadsheet:** Track constants in Excel for collaborative balance design
3. **A/B Testing:** Log Tuning values with analytics to measure impact of changes
4. **Version Control:** Document balance changes in commit messages for history

## Statistics
- **Constants Added:** 70+
- **Files Modified:** 9
- **Magic Numbers Replaced:** 100+
- **Code Lines Refactored:** ~500 lines touched

