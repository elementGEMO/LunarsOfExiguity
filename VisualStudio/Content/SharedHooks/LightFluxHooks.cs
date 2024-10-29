using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using RoR2;
using UnityEngine.Networking;

namespace LunarsOfExiguity;
public class LightFluxHooks
{
    private static readonly string InternalName = "LightFluxHooks";
    public static bool ReworkItemEnabled;
    public static bool PureItemEnabled;

    public LightFluxHooks()
    {
        ReworkItemEnabled = LightFluxRework.Rework_Enabled.Value;
        PureItemEnabled = PureLightFluxItem.Item_Enabled.Value;

        if (ReworkItemEnabled)
        {
            CharacterBody.onBodyInventoryChangedGlobal += AddDirtySkills;
            On.RoR2.CharacterBody.RecalculateStats += IncreaseFluxCharge;
            IL.RoR2.CharacterBody.RecalculateStats += ReplaceFluxEffect;
        }
        if (PureItemEnabled)
        {
            On.RoR2.CharacterBody.RecalculateStats += IncreasePureCharge;
        }
    }

    private void AddDirtySkills(CharacterBody self)
    {
        if (NetworkServer.active) self.AddItemBehavior<FluxDirtySkills>(self.inventory.GetItemCount(DLC1Content.Items.HalfAttackSpeedHalfCooldowns));
    }
    private void IncreaseFluxCharge(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
    {
        orig(self);

        if (self.inventory)
        {
            bool hasItem = self.inventory.GetItemCount(DLC1Content.Items.HalfAttackSpeedHalfCooldowns) > 0;

            if (hasItem)
            {
                int allMissing = 0;
                float cooldownReduction = (100f - LightFluxRework.Cooldown_Reduction.Value) / 100f;

                if (self.skillLocator.primary)
                {
                    self.skillLocator.primary.cooldownScale *= cooldownReduction;
                    self.skillLocator.primaryBonusStockSkill.SetBonusStockFromBody(LightFluxRework.Charge_Amount.Value);
                    allMissing += self.skillLocator.primary.maxStock - self.skillLocator.primary.stock;
                }
                if (self.skillLocator.secondary)
                {
                    self.skillLocator.secondary.cooldownScale *= cooldownReduction;
                    self.skillLocator.secondaryBonusStockSkill.SetBonusStockFromBody(LightFluxRework.Charge_Amount.Value);
                    allMissing += self.skillLocator.secondary.maxStock - self.skillLocator.secondary.stock;
                }
                if (self.skillLocator.utility)
                {
                    self.skillLocator.utility.cooldownScale *= cooldownReduction;
                    self.skillLocator.utilityBonusStockSkill.SetBonusStockFromBody(LightFluxRework.Charge_Amount.Value);
                    allMissing += self.skillLocator.utility.maxStock - self.skillLocator.utility.stock;
                }
                if (self.skillLocator.special)
                {
                    self.skillLocator.special.cooldownScale *= cooldownReduction;
                    self.skillLocator.specialBonusStockSkill.SetBonusStockFromBody(LightFluxRework.Charge_Amount.Value);
                    allMissing += self.skillLocator.special.maxStock - self.skillLocator.special.stock;
                }

                if (allMissing > 0)
                {
                    float attackSpeedMod = 1f - MathF.Pow(LightFluxRework.Attack_Speed_Percent.Value / 100f, 1f / allMissing);
                    self.attackSpeed *= attackSpeedMod;
                }
            }
        }
    }
    private void ReplaceFluxEffect(ILContext il)
    {
        ILCursor cursor = new(il);
        int itemIndex = -1;

        cursor.TryGotoNext(
            x => x.MatchLdsfld(typeof(DLC1Content.Items), nameof(DLC1Content.Items.HalfAttackSpeedHalfCooldowns)),
            x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
            x => x.MatchStloc(out itemIndex)
        );

        if (itemIndex != -1 && cursor.TryGotoNext(x => x.MatchLdarg(0)))
        {
            cursor.Emit(OpCodes.Ldloc, itemIndex);
            cursor.EmitDelegate<Func<int, int>>(self => 0);
            cursor.Emit(OpCodes.Stloc, itemIndex);
        }
        else Log.Warning(InternalName + " - #1 (ReplaceFluxEffect) Failure");
    }

    private void IncreasePureCharge(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
    {
        orig(self);

        if (self.inventory)
        {
            bool hasItem = self.inventory.GetItemCount(PureLightFluxItem.ItemDef) > 0;

            if (hasItem)
            {
                if (self.skillLocator.primary) self.skillLocator.primaryBonusStockSkill.SetBonusStockFromBody(PureLightFluxItem.Charge_Amount.Value);
                if (self.skillLocator.secondary) self.skillLocator.secondaryBonusStockSkill.SetBonusStockFromBody(PureLightFluxItem.Charge_Amount.Value);
                if (self.skillLocator.utility) self.skillLocator.utilityBonusStockSkill.SetBonusStockFromBody(PureLightFluxItem.Charge_Amount.Value);
                if (self.skillLocator.special) self.skillLocator.specialBonusStockSkill.SetBonusStockFromBody(PureLightFluxItem.Charge_Amount.Value);
            }
        }
    }

    public class FluxDirtySkills : CharacterBody.ItemBehavior
    {
        private void Awake() => enabled = false;
        private void OnEnable()
        {
            if (body) body.onSkillActivatedServer += OnSkillActivated;
        }
        private void OnDisable()
        {
            if (body) body.onSkillActivatedServer -= OnSkillActivated;
        }
        private void OnSkillActivated(GenericSkill skill) => body.statsDirty = true;
    }
}