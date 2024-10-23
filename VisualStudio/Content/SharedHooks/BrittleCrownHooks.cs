using RoR2;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Networking;
using UnityEngine;

namespace LunarsOfExiguity;
public class BrittleCrownHooks
{
    private static readonly string InternalName = "BrittleCrownHooks";
    public static bool ReworkItemEnabled;
    public static bool PureItemEnabled;

    public BrittleCrownHooks()
    {
        IL.RoR2.GlobalEventManager.ProcessHitEnemy += DisableGold;
        IL.RoR2.HealthComponent.TakeDamageProcess += ModifyDamage;
        IL.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += AllowDebt;
        On.RoR2.UI.ScoreboardStrip.UpdateMoneyText += FixNegativeMoney;
    }

    private void DisableGold(ILContext il)
    {
        ILCursor cursor = new(il);

        if (cursor.TryGotoNext(
            x => x.MatchNewobj(typeof(RoR2.Orbs.GoldOrb)),
            x => x.MatchStloc(out _),
            x => x.MatchLdloc(out _),
            x => x.MatchLdarg(1)
        ))
        {
            var previousIndex = cursor.Index;

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdflda<DamageInfo>(nameof(DamageInfo.procChainMask)),
                x => x.MatchLdcI4(out _)
            ))
            {
                var skipLabel = cursor.MarkLabel();
                cursor.Goto(previousIndex);
                cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Br, skipLabel.Target);
            }
        }
        else Log.Warning(InternalName + " - #1 (DisableGold) Failure");
    }

    private void ModifyDamage(ILContext il)
    {
        ILCursor cursor = new(il);
        int damageIndex = -1;

        cursor.TryGotoNext(
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.damage)),
            x => x.MatchStloc(out damageIndex)
        );

        if (damageIndex != -1 && cursor.TryGotoNext(
            x => x.MatchLdloc(out _),
            x => x.MatchLdarg(0),
            x => x.MatchCall<HealthComponent>("get_fullCombinedHealth"),
            x => x.MatchDiv(),
            x => x.MatchLdloc(out _),
            x => x.MatchCallOrCallvirt<CharacterMaster>("get_money")
        ))
        {
            cursor.Emit(OpCodes.Ldarg, 0);
            cursor.Emit(OpCodes.Ldloc, damageIndex);

            cursor.EmitDelegate<Func<HealthComponent, float, float>>((self, damage) =>
            {
                float damageMod = damage;
                int debtMoney = (int) self.body.master.money;

                if (debtMoney < 0)
                {
                    Log.Debug("Damage Before Debt: " + damageMod);

                    damageMod *= 1f + Mathf.Abs(debtMoney * BrittleCrownRework.Debt_Damage.Value) / 100f / Run.instance.difficultyCoefficient;

                    Log.Debug("Damage After Debt: " + damageMod);

                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.loseCoinsImpactEffectPrefab, new EffectData()
                    {
                        origin = self.body.corePosition,
                        scale = self.body.radius * 3f
                    }, true);
                }

                return damageMod;
            });

            cursor.Emit(OpCodes.Stloc, damageIndex);

            var previousIndex = cursor.Index;

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdflda<HealthComponent>(nameof(HealthComponent.itemCounts)),
                x => x.MatchLdfld(typeof(HealthComponent.ItemCounts), nameof(HealthComponent.ItemCounts.goldOnHurt))
            ))
            {
                var skipLabel = cursor.MarkLabel();
                cursor.Goto(previousIndex);
                cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Br, skipLabel.Target);
            }
        }
        else Log.Warning(InternalName + " - #1 (ModifyDamage) Failure");
    }

    private void FixNegativeMoney(On.RoR2.UI.ScoreboardStrip.orig_UpdateMoneyText orig, RoR2.UI.ScoreboardStrip self)
    {
        if (self.master && self.master.money != self.previousMoney)
        {
            self.previousMoney = self.master.money;
            string cashPlacement = (int) self.previousMoney >= 0 ? "$" : "-$";
            self.moneyText.text = string.Format("{0}{1}", cashPlacement, Mathf.Abs((int) self.previousMoney));
        }
    }

    private void AllowDebt(ILContext il)
    {
        ILCursor cursor = new(il);
        int numIndex = -1;

        cursor.TryGotoNext(
            x => x.MatchLdfld<PurchaseInteraction>(nameof(PurchaseInteraction.cost)),
            x => x.MatchStloc(out numIndex)
        );

        if (numIndex != -1 && cursor.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PurchaseInteraction>(nameof(PurchaseInteraction.costType)),
            x => x.MatchCallOrCallvirt(typeof(CostTypeCatalog), nameof(CostTypeCatalog.GetCostTypeDef))
        ))
        {
            cursor.Emit(OpCodes.Ldarg, 1);
            cursor.Emit(OpCodes.Ldloc, numIndex);

            cursor.EmitDelegate<Func<Interactor, int, int>>((activator, cost) =>
            {
                int modCost = cost;
                if (activator.GetComponent<CharacterBody>().inventory?.GetItemCount(RoR2Content.Items.GoldOnHit) > 0) modCost = 0;
                return modCost;
            });

            cursor.Emit(OpCodes.Stloc, numIndex);
        }
        else Log.Warning(InternalName + " - #1 (AllowDebt) Failure");
    }
}