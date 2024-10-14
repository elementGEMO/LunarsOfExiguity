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
using R2API.Networking;

namespace LunarsOfExiguity
{
    public class FocusedConvergence : ItemBaseRework
    {
        public FocusedConvergence(bool configValue = true) : base(configValue) { }
        protected override string Token => "ITEM_FOCUSEDCONVERGENCE_";

        public static string Internal = "Focused Convergence";
        public static ConfigEntry<bool> Enable_Rework;

        protected override void LanguageTokens()
        {
            if (MainConfig.RelicNameRewrite.Value == MainConfig.RewriteOptions.RelicRewrite) LanguageAPI.Add(Token + "NAME", "Relic of Focus");
            /*
            if (MainConfig.RelicNameRewrite.Value == MainConfig.RewriteOptions.RelicRewrite) LanguageAPI.Add(Token + "NAME", "Relic of the Drowned");
            LanguageAPI.Add(Token + "PICKUP", "Equipments no longer use charge... " + "BUT activating your Equipment disables all skills temporarily".Style(FontColor.cDeath) + ".");
            LanguageAPI.Add(Token + "DESC", string.Format(
                "Equipments no longer use charge".Style(FontColor.cIsUtility) + ". Activating your Equipment temporarily " + "disables all skills ".Style(FontColor.cIsHealth) + "for " + "{0}% ".Style(FontColor.cIsHealth) + "of the " + "Equipment cooldown ".Style(FontColor.cIsUtility) + "on " + "each use".Style(FontColor.cIsHealth) + ", up to a " + "maximum ".Style(FontColor.cIsHealth) + "of " + "{1}%".Style(FontColor.cIsHealth) + ".",
                RoundVal(Base_Equip_Percent.Value), RoundVal(Max_Equip_Percent.Value)
            ));
            */
        }
        protected override void DisabledTokens()
        {
            /*
            if (MainConfig.RelicNameRewrite.Value == MainConfig.RewriteOptions.RelicRewrite) LanguageAPI.Add(Token + "NAME", "Relic of the Drowned");
            LanguageAPI.Add(Token + "DESC", "Reduce Equipment cooldown ".Style(FontColor.cIsUtility) + "by " + "50%".Style(FontColor.cIsUtility) + ". Forces your Equipment to " + "activate ".Style(FontColor.cIsUtility) + "whenever it is off " + "cooldown".Style(FontColor.cIsUtility) + ".");
            */
        }
        protected override void Methods()
        {
            //new DrownedDebuff();

            CharacterBody.onBodyAwakeGlobal += Invincibility;
        }
        private static void Invincibility(CharacterBody self) => self.gameObject.AddComponent<InvincibleDuringHoldout>();
    }
    public class InvincibleDuringHoldout : NetworkBehaviour
    {
        private CharacterBody Self;
        private void Awake() => Self = GetComponent<CharacterBody>();
        public static bool HasConvergence;
        private void FixedUpdate()
        {
            if (Self)
            {
                HasConvergence = false;
                if (Self.inventory && !HasConvergence) HasConvergence = Self.inventory.GetItemCount(RoR2Content.Items.FocusConvergence) > 0;

                if (HasConvergence)
                {
                    float holdoutCharge = TeleporterInteraction.instance.holdoutZoneController.charge;
                    Log.Debug(holdoutCharge + " .. " + Self.name);
                    if (holdoutCharge > 0 && holdoutCharge < 1)
                    {
                        if (Self.teamComponent && Self.teamComponent.teamIndex != TeamIndex.Player)
                        {
                            Self.AddTimedBuff(RoR2Content.Buffs.Immune, 1);
                        }
                    }
                }
            }
        }
    }
}
