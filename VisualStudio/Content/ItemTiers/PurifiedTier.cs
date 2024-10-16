using RoR2;

namespace LunarsOfExiguity.Content.ItemTiers;

public class PurifiedTier : ItemTierBase
{
    protected override string Name => "Purified";

    protected override ColorCatalog.ColorIndex Color => ColorCatalog.ColorIndex.BossItem;
    protected override ColorCatalog.ColorIndex DarkColor => ColorCatalog.ColorIndex.BossItemDark;

    // Unsure why you would want this, but to ensure no change from source, I'll mark it false,
    protected override bool CanBeRestacked => false;

    protected override void Initialize()
    {
        ItemCatalog.availability.CallWhenAvailable(() =>
        {
            ItemCatalog.GetItemDef(RoR2Content.Items.Pearl.itemIndex)._itemTierDef = Value;
        });
    }
}