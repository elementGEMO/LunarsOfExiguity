using R2API;
using RoR2;

namespace LunarsOfExiguity;

public abstract class ItemReworkBase : GenericBase<ItemDef>
{
    protected virtual string RelicNameOverride { get; }
    protected virtual string CursedNameOverride { get; }
    
    protected virtual string PickupOverride { get; }
    protected virtual string DescriptionOverride { get; }
    protected virtual string LoreOverride { get; }

    protected override void Initialize() => ItemCatalog.availability.CallWhenAvailable(DelayedInitialize);

    private void DelayedInitialize()
    {
        ItemIndex itemIndex = ItemCatalog.FindItemIndex(Name);
        if (itemIndex == ItemIndex.None)
        {
            Log.Warning($"Failed to find ItemIndex for {Name}.");
            return;
        }

        Value = ItemCatalog.GetItemDef(itemIndex);
        if (Value)
        {
            switch (MainConfig.ItemNameStyle.Value)
            {
                case MainConfig.NameStyle.Relic:
                    if (!string.IsNullOrWhiteSpace(RelicNameOverride)) LanguageAPI.Add(Value.nameToken, RelicNameOverride);
                    break;
                case MainConfig.NameStyle.Cursed:
                    if (!string.IsNullOrWhiteSpace(CursedNameOverride)) LanguageAPI.Add(Value.nameToken, CursedNameOverride);
                    break;
            }
            
            if (!string.IsNullOrWhiteSpace(Value.pickupToken)) LanguageAPI.Add(Value.pickupToken, PickupOverride);
            if (!string.IsNullOrWhiteSpace(Value.descriptionToken)) LanguageAPI.Add(Value.descriptionToken, DescriptionOverride);
            if (!string.IsNullOrWhiteSpace(Value.loreToken)) LanguageAPI.Add(Value.loreToken, LoreOverride);
        }
    }
}