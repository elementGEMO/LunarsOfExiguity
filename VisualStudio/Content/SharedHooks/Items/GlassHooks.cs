using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;

namespace LunarsOfExiguity;
public class GlassHooks
{
    private static readonly string InternalName = "GlassHooks";
    public static bool ReworkItemEnabled;
    public static bool PureItemEnabled;

    public GlassHooks()
    {
        ReworkItemEnabled = GlassRework.Rework_Enabled.Value;
        PureItemEnabled = PureGlassItem.Item_Enabled.Value;

        if (ReworkItemEnabled)
        {
            IL.RoR2.CharacterBody.RecalculateStats += DisableHealth;
            IL.RoR2.HealthComponent.TakeDamageProcess += ReduceHealth;
            RecalculateStatsAPI.GetStatCoefficients += GlassDamage;
        }
        if (PureItemEnabled)
        {
            RecalculateStatsAPI.GetStatCoefficients += BezerkDamage;
        }
    }

    private void BezerkDamage(CharacterBody self, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (NetworkServer.active && self.inventory)
        {
            bool hasItem = self.inventory.GetItemCount(PureGlassItem.ItemDef) > 0;

            if (hasItem)
            {
                float damageMod = PureGlassItem.Damage_Modifier.Value / 100f;
                float healthFraction = self.healthComponent.combinedHealth / self.healthComponent.fullCombinedHealth;

                args.damageMultAdd += damageMod * (1f - healthFraction);

                /*
                float currentHealth = self.healthComponent.fullCombinedHealth;
                float baseHealth = currentHealth * self.cursePenalty;

                float healthFraction = currentHealth / baseHealth;
                float damageMod = GlassRework.Damage_Modifier.Value / 100f;

                args.damageMultAdd += Mathf.Pow(GlassRework.Exponent_Coefficient.Value, damageMod * (1f - healthFraction)) - 1f;
                */


            }
        }
    }

    private void GlassDamage(CharacterBody self, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (NetworkServer.active && self.inventory)
        {
            bool hasItem = self.inventory.GetItemCount(RoR2Content.Items.LunarDagger) > 0;

            if (hasItem)
            {
                float currentHealth = self.healthComponent.fullCombinedHealth;
                float baseHealth = currentHealth * self.cursePenalty;

                float healthFraction = currentHealth / baseHealth;
                float damageMod = GlassRework.Damage_Modifier.Value / 100f;

                args.damageMultAdd += Mathf.Pow(GlassRework.Exponent_Coefficient.Value, damageMod * (1f - healthFraction)) - 1f;
            }
        }
    }

    private void DisableHealth(ILContext il)
    {
        ILCursor cursor = new(il);
        int itemIndex = -1;

        cursor.TryGotoNext(
            x => x.MatchLdsfld(typeof(RoR2Content.Items), nameof(RoR2Content.Items.LunarDagger)),
            x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
            x => x.MatchStloc(out itemIndex)
        );

        if (itemIndex != -1 && cursor.TryGotoNext(x => x.MatchLdarg(0)))
        {
            cursor.Emit(OpCodes.Ldloc, itemIndex);
            cursor.EmitDelegate<Func<int, int>>(self => 0);
            cursor.Emit(OpCodes.Stloc, itemIndex);

            cursor.Emit(OpCodes.Ldarg, 0);
            cursor.EmitDelegate<Action<CharacterBody>>(self =>
            {
                if (self.inventory?.GetItemCount(RoR2Content.Items.LunarDagger) > 0) self.isGlass = true;
            });
        }
        else Log.Warning(InternalName + " - #1 (DisableHealth) Failure");
    }
    private void ReduceHealth(ILContext il)
    {
        ILCursor cursor = new(il);
        int damageIndex = -1;

        cursor.TryGotoNext(
            x => x.MatchStfld<HealthComponent>(nameof(HealthComponent.adaptiveArmorValue)),
            x => x.MatchLdloc(out _),
            x => x.MatchStloc(out damageIndex)
        );

        if (damageIndex != -1 && cursor.TryGotoNext(
            x => x.MatchLdcR4(out _),
            x => x.MatchBleUn(out _),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.barrier))
        ))
        {
            cursor.Emit(OpCodes.Ldarg, 0);
            cursor.Emit(OpCodes.Ldloc, damageIndex);

            cursor.EmitDelegate<Action<HealthComponent, float>>((self, damage) =>
            {
                if (self.body.inventory?.GetItemCount(RoR2Content.Items.LunarDagger) > 0 && damage < self.fullCombinedHealth)
                {
                    float applyAmount = damage / self.fullCombinedHealth * self.body.cursePenalty * 100f;
                    applyAmount *= 1f + (self.body.cursePenalty - 1f);

                    for (int i = 0; i < Mathf.FloorToInt(applyAmount * (GlassRework.Permanent_Modifier.Value / 100f)); i++)
                    {
                        self.body.AddBuff(RoR2Content.Buffs.PermanentCurse);
                    }
                }
            });
        }
        else Log.Warning(InternalName + " - #1 (ReduceHealth) Failure");
    }
}