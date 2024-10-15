using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace LunarsOfExiguity.Content.Reworks;

public class AutoCastEquipmentRework : ItemReworkBase
{
    protected override string Name => "AutoCastEquipment";
    
    protected override string RelicNameOverride => "Relic of the Drowned";
    protected override string PickupOverride => "Equipment use requires no charges... <style=cDeath>BUT activation disables all skills temporarily</style>.";
    protected override string DescriptionOverride => $"Equipment no longer requires charges. <style=cIsUtility>Activating</style> your Equipment disables all skills for <style=cIsUtility>{MainConfig.BaseDisableSkillsPercentage}%</style> of it's <style=cIsUtility>cooldown</style> for each use, up to a maximum of <style=cIsUtility>{MainConfig.MaxDisableSkillsPercentage}%</style>.";

    protected override void Initialize()
    {
        IL.RoR2.EquipmentSlot.MyFixedUpdate += DisableAutoCast;
        IL.RoR2.Inventory.CalculateEquipmentCooldownScale += DisableCooldownReduction;
        IL.RoR2.EquipmentSlot.OnEquipmentExecuted += NoChargeUse;
        On.RoR2.EquipmentSlot.OnEquipmentExecuted += DisableSkills;
        CharacterBody.onBodyAwakeGlobal += DebuffAdded;
    }
    
    
        private static void DisableAutoCast(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<EquipmentSlot>(nameof(EquipmentSlot.inventory)),
                x => x.MatchDup()
            ))
            {
                var previousIndex = cursor.Index;

                if (cursor.TryGotoNext(
                    x => x.MatchLdcI4(out _)
                ))
                {
                    var skipLabel = cursor.MarkLabel();
                    cursor.Goto(previousIndex);
                    cursor.MoveAfterLabels();
                    cursor.Emit(OpCodes.Br, skipLabel.Target);
                } else Log.Warning("AutoCastEquipment" + " - #2 (DisableAutoCast) Failure");
            } else Log.Warning("AutoCastEquipment" + " - #1 (DisableAutoCast) Failure");
        }
        private static void DisableCooldownReduction(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdloc(out _),
                x => x.MatchLdcR4(out _),
                x => x.MatchLdcR4(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchLdcI4(out _)
            ))
            {
                var previousIndex = cursor.Index;

                if (cursor.TryGotoNext(
                    x => x.MatchLdloc(out _),
                    x => x.MatchLdcI4(out _),
                    x => x.MatchBle(out _)
                ))
                {
                    var skipLabel = cursor.MarkLabel();
                    cursor.Goto(previousIndex);
                    cursor.MoveAfterLabels();
                    cursor.Emit(OpCodes.Br, skipLabel.Target);
                } else Log.Warning("AutoCastEquipment" + " - #2 (DisableCooldownReduction) Failure");
            } else Log.Warning("AutoCastEquipment" + " - #1 (DisableCooldownReduction) Failure");
        }
        private static void NoChargeUse(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<EquipmentSlot>(nameof(EquipmentSlot.inventory)),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<EquipmentSlot>("get_activeEquipmentSlot")
            ))
            {
                cursor.Emit(OpCodes.Ldarg, 0);
                cursor.EmitDelegate<Action<EquipmentSlot>>(self =>
                {
                    int chargeCost = 1;
                    if (self.characterBody)
                    {
                        bool hasItem = self.inventory.GetItemCount(RoR2Content.Items.AutoCastEquipment) > 0;
                        if (hasItem) chargeCost = 0;
                    }
                    self.inventory.DeductEquipmentCharges(self.activeEquipmentSlot, chargeCost);
                });

                var previousIndex = cursor.Index;

                if (cursor.TryGotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchCallOrCallvirt<EquipmentSlot>(nameof(EquipmentSlot.UpdateInventory))
                ))
                {
                    var skipLabel = cursor.MarkLabel();
                    cursor.Goto(previousIndex);
                    cursor.Emit(OpCodes.Br, skipLabel.Target);
                } else Log.Warning("AutoCastEquipment" + " - #2 (NoChargeUse) Failure");
            } else Log.Warning("AutoCastEquipment" + " - #1 (NoChargeUse) Failure");
        }
        private static void DisableSkills(On.RoR2.EquipmentSlot.orig_OnEquipmentExecuted orig, EquipmentSlot self)
        {
            orig(self);

            if (self.characterBody)
            {
                DrownedHandler drownedHandler = self.characterBody.GetComponent<DrownedHandler>();
                bool hasItem = self.inventory.GetItemCount(RoR2Content.Items.AutoCastEquipment) > 0;
                if (drownedHandler && hasItem)
                {
                    float duration = EquipmentCatalog.GetEquipmentDef(self.equipmentIndex).cooldown * self.inventory.CalculateEquipmentCooldownScale();
                    drownedHandler.IncreaseDuration(duration * MainConfig.BaseDisableSkillsPercentage.Value / 100f, duration);
                }
            }
        }
        private static void DebuffAdded(CharacterBody self) => self.gameObject.AddComponent<DrownedHandler>();
    }

    public class DrownedHandler : NetworkBehaviour
    {
        private float Duration;
        private CharacterBody Self;
        private void Awake() => Self = GetComponent<CharacterBody>();
        private void FixedUpdate()
        {
            BuffIndex buffIndex = BuffCatalog.FindBuffIndex("bdRelicDisableSkills");
            
            if (Self && Duration > 0)
            {
                Duration -= Time.deltaTime;
                Self.SetBuffCount(buffIndex, (int) Math.Round(Duration));
            }
            else if (Self && Self.HasBuff(buffIndex))
            {
                Self.RemoveBuff(buffIndex);
            }
        }
        public void IncreaseDuration(float duration, float baseDuration)
        {
            if (!Self) return;

            if (duration <= 0) Util.PlaySound("Play_env_meridian_primeDevestator_debuff", Self.gameObject);
            EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.knockbackFinEffect, new EffectData
            {
                origin = Self.gameObject.transform.position,
                scale = 0.5f
            }, true);
            Duration = Math.Min(Duration + duration, baseDuration * MainConfig.MaxDisableSkillsPercentage.Value / 100f);
        }
    
}