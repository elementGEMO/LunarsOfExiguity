using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Skills;
using RoR2;
using R2API;
using System;
using UnityEngine;
using UnityEngine.Networking;

using static LunarsOfExiguity.ColorCode;
using static LunarsOfExiguity.ItemUtils;

namespace LunarsOfExiguity
{
    public class GestureOfTheDrowned : ItemBaseRework
    {
        public GestureOfTheDrowned(bool configValue = true) : base(configValue) { }
        protected override string Token => "ITEM_AUTOCASTEQUIPMENT_";

        public static string Internal = "Gesture of the Drowned";
        public static ConfigEntry<bool> Enable_Rework;
        public static ConfigEntry<float> Base_Equip_Percent;
        public static ConfigEntry<float> Max_Equip_Percent;

        protected override void LanguageTokens()
        {
            if (MainConfig.RelicNameRewrite.Value == MainConfig.RewriteOptions.RelicRewrite) LanguageAPI.Add(Token + "NAME", "Relic of the Drowned");
            LanguageAPI.Add(Token + "PICKUP", "Equipments no longer use charge... " + "BUT activating your Equipment disables all skills temporarily".Style(FontColor.cDeath) + ".");
            LanguageAPI.Add(Token + "DESC", string.Format(
                "Equipments no longer use charge".Style(FontColor.cIsUtility) + ". Activating your Equipment temporarily " + "disables all skills ".Style(FontColor.cIsHealth) + "for " + "{0}% ".Style(FontColor.cIsHealth) + "of the " + "Equipment cooldown ".Style(FontColor.cIsUtility) + "on " + "each use".Style(FontColor.cIsHealth) + ", up to a " + "maximum ".Style(FontColor.cIsHealth) + "of " + "{1}%".Style(FontColor.cIsHealth) + ".",
                RoundVal(Base_Equip_Percent.Value), RoundVal(Max_Equip_Percent.Value)
            ));
        }
        protected override void DisabledTokens()
        {
            if (MainConfig.RelicNameRewrite.Value == MainConfig.RewriteOptions.RelicRewrite) LanguageAPI.Add(Token + "NAME", "Relic of the Drowned");
            LanguageAPI.Add(Token + "DESC", "Reduce Equipment cooldown ".Style(FontColor.cIsUtility) + "by " + "50%".Style(FontColor.cIsUtility) + ". Forces your Equipment to " + "activate ".Style(FontColor.cIsUtility) + "whenever it is off " + "cooldown".Style(FontColor.cIsUtility) + ".");
        }
        protected override void Methods()
        {
            new DrownedDebuff();

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
                } else Log.Warning(Internal + " - #2 (DisableAutoCast) Failure");
            } else Log.Warning(Internal + " - #1 (DisableAutoCast) Failure");
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
                } else Log.Warning(Internal + " - #2 (DisableCooldownReduction) Failure");
            } else Log.Warning(Internal + " - #1 (DisableCooldownReduction) Failure");
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
                } else Log.Warning(Internal + " - #2 (NoChargeUse) Failure");
            } else Log.Warning(Internal + " - #1 (NoChargeUse) Failure");
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
    }
    public class DrownedDebuff
    {
        private static readonly string Internal = "Gesture of the Drowned";
        private static readonly Sprite DrownedIcon = AssetStatics.bundle.LoadAsset<Sprite>("DrownedDebuffIcon");
        public static BuffDef DrownedDebuffDef;
        public DrownedDebuff()
        {
            DrownedDebuffDef = ScriptableObject.CreateInstance<BuffDef>();
            DrownedDebuffDef.name = "DrownDisabledDebuff";
            DrownedDebuffDef.isCooldown = false;
            DrownedDebuffDef.canStack = true;
            DrownedDebuffDef.isDebuff = false;
            DrownedDebuffDef.isHidden = false;
            DrownedDebuffDef.buffColor = new Color(0.706f, 0.753f, 0.976f);
            DrownedDebuffDef.iconSprite = DrownedIcon;

            ContentAddition.AddBuffDef(DrownedDebuffDef);

            IL.RoR2.CharacterBody.RecalculateStats += DisableSkills;
        }
        private static void DisableSkills(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(
                x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HandleDisableAllSkillsDebuff))
            ))
            {
                cursor.Emit(OpCodes.Ldarg, 0);
                cursor.EmitDelegate(HandleDrownedDebuff);
            } else Log.Warning(Internal + " - #1 (DisableSkills) Failure");
        }
        private static void HandleDrownedDebuff(CharacterBody self)
        {
            if (!self) return;
            if (self.HasBuff(DrownedDebuffDef))
            {
                HandleDrownedState(self, true);
            }
            else
            {
                HandleDrownedState(self, false);
            }
        }
        private static void HandleDrownedState(CharacterBody self, bool disable)
        {
            if (self.hasAuthority)
            {
                SkillDef disableSkill = LegacyResourcesAPI.Load<SkillDef>("Skills/DisabledSkills");
                if (!disableSkill) Log.Warning(Internal + " - #2 (DisableSkills) Failure");
                else if (disable && self.skillLocator)
                {
                    if (self.skillLocator.primary) self.skillLocator.primary.SetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                    if (self.skillLocator.secondary) self.skillLocator.secondary.SetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                    if (self.skillLocator.utility) self.skillLocator.utility.SetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                    if (self.skillLocator.special) self.skillLocator.special.SetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                }
                else if (self.skillLocator)
                {
                    if (self.skillLocator.primary) self.skillLocator.primary.UnsetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                    if (self.skillLocator.secondary) self.skillLocator.secondary.UnsetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                    if (self.skillLocator.utility) self.skillLocator.utility.UnsetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                    if (self.skillLocator.special) self.skillLocator.special.UnsetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
        }
    }
    public class DrownedHandler : NetworkBehaviour
    {
        private float Duration;
        private CharacterBody Self;
        private void Awake() => Self = GetComponent<CharacterBody>();
        private void FixedUpdate()
        {
            if (Self && Duration > 0)
            {
                Duration -= Time.deltaTime;
                Self.SetBuffCount(DrownedDebuff.DrownedDebuffDef.buffIndex, (int) Math.Round(Duration));
            }
            else if (Self && Self.HasBuff(DrownedDebuff.DrownedDebuffDef))
            {
                Self.RemoveBuff(DrownedDebuff.DrownedDebuffDef.buffIndex);
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
            Duration = Math.Min(Duration + duration, baseDuration * GestureOfTheDrowned.Max_Equip_Percent.Value / 100f);
        }
    }
}
