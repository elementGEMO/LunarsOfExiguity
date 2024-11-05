using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;

using static LoEOverlay;
using UnityEngine.AddressableAssets;

namespace LunarsOfExiguity;
public class GlassHooks
{
    private static readonly string InternalName = "GlassHooks";
    public static bool ReworkItemEnabled;
    public static bool PureItemEnabled;

    //private Material BrightProvOverlay = LoEPlugin.Bundle.LoadAsset<Material>("PureGlassMat");
    private GameObject SlashEffect;
    private DamageColorIndex SlashDamageIndex;

    public GlassHooks()
    {
        ReworkItemEnabled = GlassRework.Rework_Enabled.Value;
        PureItemEnabled = PureGlassItem.Item_Enabled.Value;

        SlashEffect = Addressables.LoadAsset<GameObject>("RoR2/DLC2/FalseSon/FalseSonClubSwoosh.prefab").WaitForCompletion();
        SlashDamageIndex = ColorsAPI.RegisterDamageColor(new Color32(80, 224, 186, 255));

        if (ReworkItemEnabled)
        {
            IL.RoR2.CharacterBody.RecalculateStats += DisableHealth;
            IL.RoR2.HealthComponent.TakeDamageProcess += ReduceHealth;
            RecalculateStatsAPI.GetStatCoefficients += GlassDamage;
        }
        if (PureItemEnabled)
        {
            RecalculateStatsAPI.GetStatCoefficients += BezerkDamage;
            GlobalEventManager.onServerDamageDealt += ApplyEffect;
            //On.RoR2.CharacterModel.UpdateOverlays += TestOverlay;
        }
    }

    private void ApplyEffect(DamageReport damageReport)
    {
        if (damageReport.victimBody && damageReport.attackerBody?.inventory)
        {
            bool hasItem = damageReport.attackerBody.inventory.GetItemCount(PureGlassItem.ItemDef) > 0;

            if (hasItem)
            {
                float healthFraction = damageReport.attackerBody.healthComponent.combinedHealth / damageReport.attackerBody.healthComponent.fullCombinedHealth;
                foreach (Transform transform in SlashEffect.GetComponentsInChildren<Transform>()) transform.localScale = Vector3.one * (1f - healthFraction);
                EffectManager.SimpleEffect(SlashEffect, damageReport.damageInfo.position, UnityEngine.Random.rotation, true);

                damageReport.damageInfo.damageColorIndex = SlashDamageIndex;
            }
        }
    }

    /*
    private void TestOverlay(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
    {
        orig(self);

        //if (self.GetComponent<CharacterBody>()?.inventory.GetItemCount(PureGlassItem.ItemDef) > 0) AddOverlay(self, BrightProvOverlay);
    }
    */

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