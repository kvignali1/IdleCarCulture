# Null Reference Audit

## Critical Scripts Reviewed

### GameManager.cs
- ✅ Singleton pattern with null check
- ✅ Creates instance if missing
- ⚠️ Added check in Save() for _playerProfile

### CityHUD.cs
- ⚠️ All OnEnable/OnDisable have GameManager null checks
- ⚠️ Optional UI elements checked before use
- ⚠️ RefreshAll() handles missing carData

### RaceSceneController.cs
- ⚠️ Added null check for missing RaceOpportunity
- ⚠️ Optional car sprites checked before animation
- ⚠️ All manager lookups protected

### GarageUI.cs
- ⚠️ Added null checks for all scroll rect references
- ⚠️ Handles missing upgrade definitions gracefully
- ⚠️ Optional UI elements safe

### RaceOpportunityUI.cs
- ⚠️ Optional popup panel and texts checked
- ⚠️ Handles null opportunity gracefully

### EconomyManager.cs
- ✅ Manager and profile null checks on all methods
- ✅ FindObjectOfType protected

### HeatSystem.cs
- ✅ All manager lookups checked
- ✅ Optional EconomyManager handled

### PrestigeSystem.cs
- ✅ All manager lookups checked

### UpgradeSystem.cs
- ✅ All carId and manager references checked
- ✅ Optional PrestigeSystem handled

## Defensive Coding Pattern Used

```csharp
// Check GameObject/Component
if (componentRef == null)
{
    Debug.LogError($"[ComponentName] {componentRef} is not assigned in Inspector.");
    return;
}

// Optional components
if (optionalComponent != null)
{
    optionalComponent.DoSomething();
}
else
{
    Debug.LogWarning($"[ComponentName] Optional {optionalComponent} not assigned.");
}
```

## Scene Compatibility

All scenes should work even if optional UI panels are missing:
- CityHUD advanced panel is optional
- RaceOpportunityUI popup can be missing (races won't trigger)
- GarageUI can be disabled without breaking game logic
