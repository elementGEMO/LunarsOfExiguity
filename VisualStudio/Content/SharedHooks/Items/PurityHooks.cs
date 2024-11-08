using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using RoR2;
using R2API;
using UnityEngine;
using MonoMod.RuntimeDetour;

namespace LunarsOfExiguity;
public class PurityHooks
{
    private static readonly string InternalName = "PurityHooks";
    public static bool ReworkItemEnabled;
    public static bool PureItemEnabled;

    public PurityHooks()
    {
        ReworkItemEnabled = PurityRework.Rework_Enabled.Value;
        PureItemEnabled = PurePurityItem.Item_Enabled.Value;

        if (ReworkItemEnabled)
        {
            IL.RoR2.CharacterMaster.OnInventoryChanged += DisableLunarLuck;
            On.RoR2.GlobalEventManager.ProcessHitEnemy += DisableProcHit;
            On.RoR2.HealthComponent.TakeDamageProcess += DisableProcTake;
            IL.RoR2.CharacterBody.RecalculateStats += ReplaceBehavior;
            On.RoR2.CharacterBody.RecalculateStats += NoCooldown;
            RecalculateStatsAPI.GetStatCoefficients += BoostStats;

            new Hook(typeof(CharacterBody).GetMethod(nameof(CharacterBody.RecalculateStats)), RemoveCrit, new HookConfig { Priority = int.MinValue });
        }
        if (ReworkItemEnabled)
        {
            On.RoR2.CharacterBody.RecalculateStats += CooldownReduce;
            RecalculateStatsAPI.GetStatCoefficients += PureBoost;
        }
    }

    private void DisableLunarLuck(ILContext il)
    {
        ILCursor cursor = new(il);

        if (cursor.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<CharacterMaster>("get_luck"),
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<CharacterMaster>("get_inventory"),
            x => x.MatchLdsfld(typeof(RoR2Content.Items), nameof(RoR2Content.Items.LunarBadLuck))
        ))
        {
            var previousIndex = cursor.Index;

            if (cursor.TryGotoNext(
                x => x.MatchCallOrCallvirt(out _),
                x => x.MatchBrfalse(out _)
            ))
            {
                var skipLabel = cursor.MarkLabel();
                cursor.Goto(previousIndex);
                cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Br, skipLabel.Target);
            }
            else Log.Warning(InternalName + " - #2 (ReplaceLuck) Failure");
        }
        else Log.Warning(InternalName + " - #1 (ReplaceLuck) Failure");
    }

    private void DisableProcHit(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
    {
        if (damageInfo.attacker && damageInfo.attacker.GetComponent<CharacterBody>())
        {
            if (damageInfo.attacker.GetComponent<CharacterBody>().inventory?.GetItemCount(RoR2Content.Items.LunarBadLuck) > 0) damageInfo.procCoefficient = float.Epsilon;//return;
        }

        orig(self, damageInfo, victim);
    }
    private void DisableProcTake(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
    {
        if (damageInfo.attacker && damageInfo.attacker.GetComponent<CharacterBody>())
        {
            if (damageInfo.attacker.GetComponent<CharacterBody>().inventory?.GetItemCount(RoR2Content.Items.LunarBadLuck) > 0) damageInfo.procCoefficient = float.Epsilon;//damageInfo.procCoefficient = 0;
        }

        orig(self, damageInfo);
    }
    private void ReplaceBehavior(ILContext il)
    {
        ILCursor cursor = new(il);
        int itemIndex = -1;

        cursor.TryGotoNext(
            x => x.MatchLdsfld(typeof(RoR2Content.Items), nameof(RoR2Content.Items.LunarBadLuck)),
            x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
            x => x.MatchStloc(out itemIndex)
        );

        if (itemIndex != -1 && cursor.TryGotoNext(x => x.MatchLdarg(0)))
        {
            cursor.Emit(OpCodes.Ldloc, itemIndex);
            cursor.EmitDelegate<Func<int, int>>(self => 0);
            cursor.Emit(OpCodes.Stloc, itemIndex);
        }
        else Log.Warning(InternalName + " - #1 (ReplaceBehavior) Failure");
    }
    private void NoCooldown(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
    {
        orig(self);

        if (self.inventory)
        {
            bool hasItem = self.inventory.GetItemCount(RoR2Content.Items.LunarBadLuck) > 0;
            if (hasItem)
            {
                float cooldownReduce = 1f - (PurityRework.Cooldown_Reduce.Value / 100f);

                if (self.skillLocator.primary) self.skillLocator.primary.cooldownScale *= cooldownReduce;
                if (self.skillLocator.secondary) self.skillLocator.secondary.cooldownScale *= cooldownReduce;
                if (self.skillLocator.utility) self.skillLocator.utility.cooldownScale *= cooldownReduce;
                if (self.skillLocator.special) self.skillLocator.special.cooldownScale *= cooldownReduce;
            }
        }
    }
    private void BoostStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (sender.inventory?.GetItemCount(RoR2Content.Items.LunarBadLuck) > 0)
        {
            float statMultiplier = PurityRework.Stat_Modifier.Value / 100f;

            args.attackSpeedMultAdd += statMultiplier;
            args.healthMultAdd += statMultiplier;
            args.moveSpeedMultAdd += statMultiplier;
            args.damageMultAdd += statMultiplier;
            args.regenMultAdd += statMultiplier;
        }
    }
    private void RemoveCrit(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
    {
        orig(self); if (self.inventory?.GetItemCount(RoR2Content.Items.LunarBadLuck) > 0) self.crit = 0;
    }

    private void CooldownReduce(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
    {
        orig(self);

        if (self.inventory)
        {
            bool hasItem = self.inventory.GetItemCount(PurePurityItem.ItemDef) > 0;
            if (hasItem)
            {
                float cooldownReduce = 1f - (PurePurityItem.Cooldown_Reduce.Value / 100f);

                if (self.skillLocator.primary) self.skillLocator.primary.cooldownScale *= cooldownReduce;
                if (self.skillLocator.secondary) self.skillLocator.secondary.cooldownScale *= cooldownReduce;
                if (self.skillLocator.utility) self.skillLocator.utility.cooldownScale *= cooldownReduce;
                if (self.skillLocator.special) self.skillLocator.special.cooldownScale *= cooldownReduce;

                self.crit *= 1 + PurePurityItem.Stat_Modifier.Value / 100f;
            }
        }
    }
    private void PureBoost(CharacterBody self, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (self.inventory?.GetItemCount(PurePurityItem.ItemDef) > 0)
        {
            float statMultiplier = PurePurityItem.Stat_Modifier.Value / 100f;

            args.attackSpeedMultAdd += statMultiplier;
            args.healthMultAdd += statMultiplier;
            args.moveSpeedMultAdd += statMultiplier;
            args.damageMultAdd += statMultiplier;
            args.regenMultAdd += statMultiplier;
        }
    }
}