using System;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class GestureDrownedRework : ItemReworkBase
{
    public static ConfigEntry<float> Base_Equip_Percent;
    public static ConfigEntry<float> Max_Equip_Percent;
    protected override string Name => "AutoCastEquipment";
    private static string InternalName;

    protected override string RelicNameOverride => "Relic of the Drowned";
    protected override string CursedNameOverride => RelicNameOverride;
    protected override string PickupOverride => "Equipments no longer use charge... " + "BUT activating your Equipment disables all skills temporarily".Style(FontColor.cDeath) + ".";//"Equipment use requires no charges... <style=cDeath>BUT activation disables all skills temporarily</style>.";
    protected override string DescriptionOverride => string.Format( 
        "Equipments no longer use charge".Style(FontColor.cIsUtility) + ". Activating your Equipment temporarily " + "disables all skills ".Style(FontColor.cIsHealth) + "for " + "{0}% ".Style(FontColor.cIsHealth) + "of the " + "Equipment cooldown ".Style(FontColor.cIsUtility) + "on " + "each use".Style(FontColor.cIsHealth) + ", up to a " + "maximum ".Style(FontColor.cIsHealth) + "of " + "{1}%".Style(FontColor.cIsHealth) + ".",
        RoundVal(Base_Equip_Percent.Value), RoundVal(Max_Equip_Percent.Value)
    );

    protected override bool IsEnabled()
    {
        Base_Equip_Percent = LoEPlugin.Instance.Config.Bind(
            Name + " - Rework",
            "Rework - Percent Duration", 15f,
            "[ 15 = 15% Duration | Per Equipment Use ]"
        );
        Max_Equip_Percent = LoEPlugin.Instance.Config.Bind(
            Name + " - Rework",
            "Rework - Max Percent", 100f,
            "[ 100 = 100% Max Duration | Per Equipment Use]"
        );
        return base.IsEnabled();
    }

    protected override void Initialize()
    {
        InternalName = Name;

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
            }
            else Log.Warning(InternalName + " - #2 (DisableAutoCast) Failure");
        }
        else Log.Warning(InternalName + " - #1 (DisableAutoCast) Failure");
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
            }
            else Log.Warning(InternalName + " - #2 (DisableCooldownReduction) Failure");
        }
        else Log.Warning(InternalName + " - #1 (DisableCooldownReduction) Failure");
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
            }
            else Log.Warning(InternalName + " - #2 (NoChargeUse) Failure");
        }
        else Log.Warning(InternalName + " - #1 (NoChargeUse) Failure");
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
                drownedHandler.IncreaseDuration(duration * Base_Equip_Percent.Value / 100f, duration);
            }
        }
    }
    private static void DebuffAdded(CharacterBody self) => self.gameObject.AddComponent<DrownedHandler>();

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
                Self.SetBuffCount(buffIndex, (int)Math.Round(Duration));
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
            Duration = Math.Min(Duration + duration, baseDuration * Max_Equip_Percent.Value / 100f);
        }

    }
}