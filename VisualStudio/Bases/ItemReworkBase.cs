using R2API;
using RoR2;

namespace LunarsOfExiguity;

public abstract class ItemReworkBase
{
    protected abstract string Name { get; }
    
    protected virtual string RelicNameOverride { get; }
    protected virtual string CursedNameOverride { get; }
    
    protected virtual string PickupOverride { get; }
    protected virtual string DescriptionOverride { get; }

    protected ItemReworkBase()
    {
        if (IsEnabled()) ItemCatalog.availability.CallWhenAvailable(Create);
    }

    protected virtual bool IsEnabled() => LunarsOfExiguityPlugin.Instance.Config.Bind(Name, "Enable Rework", true, "[ True = Reworked | False = Vanilla | Removed Stacking ]").Value;
    private void Create()
    {
        ItemIndex itemIndex = ItemCatalog.FindItemIndex(Name);
        if (itemIndex == ItemIndex.None)
        {
            Log.Warning($"Failed to find ItemIndex for {Name}.");
            return;
        }

        ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
        if (itemDef)
        {
            switch (LunarsOfExiguityConfig.ItemNameStyle.Value)
            {
                case LunarsOfExiguityConfig.NameStyle.Relic:
                    if (!string.IsNullOrWhiteSpace(RelicNameOverride)) LanguageAPI.Add(itemDef.nameToken, RelicNameOverride);
                    break;
                case LunarsOfExiguityConfig.NameStyle.Cursed:
                    if (!string.IsNullOrWhiteSpace(CursedNameOverride)) LanguageAPI.Add(itemDef.nameToken, CursedNameOverride);
                    break;
            }
            
            if (!string.IsNullOrWhiteSpace(itemDef.pickupToken)) LanguageAPI.Add(itemDef.pickupToken, PickupOverride);
            if (!string.IsNullOrWhiteSpace(itemDef.descriptionToken)) LanguageAPI.Add(itemDef.descriptionToken, DescriptionOverride);
            
            Initialize();
        }
    }

    protected virtual void Initialize()
    {
        
    }
}