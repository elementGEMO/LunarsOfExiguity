using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace LunarsOfExiguity;
public class SkillDisableDebuff : BuffBase
{
    protected override string Name => "RelicDisableSkills";
    public static BuffDef BuffDef;

    protected override Sprite IconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("DrownedDebuffIcon");
    protected override Color Color => new(0.706f, 0.753f, 0.976f);
    protected override bool IsStackable => true;

    protected override void Initialize()
    {
        BuffDef = Value;

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
        }
        else Log.Warning(BuffDef.name + " - #1 (DisableSkills) Failure");
    }
    private static void HandleDrownedDebuff(CharacterBody self)
    {
        if (!self.hasAuthority || self.HasBuff(DLC2Content.Buffs.DisableAllSkills)) return;

        bool flag = self.HasBuff(BuffDef);
        var disabledSkill = LegacyResourcesAPI.Load<SkillDef>("Skills/DisabledSkills");
        if (!disabledSkill)
        {
            Log.Warning(BuffDef.name + "- Failed to find DisabledSkills SkillDef");
            return;
        }

        if (flag)
        {
            if (self.skillLocator.primary) self.skillLocator.primary.SetSkillOverride(self, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
            if (self.skillLocator.secondary) self.skillLocator.secondary.SetSkillOverride(self, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
            if (self.skillLocator.utility) self.skillLocator.utility.SetSkillOverride(self, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
            if (self.skillLocator.special) self.skillLocator.special.SetSkillOverride(self, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
            return;
        }

        if (self.skillLocator.primary) self.skillLocator.primary.UnsetSkillOverride(self, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
        if (self.skillLocator.secondary) self.skillLocator.secondary.UnsetSkillOverride(self, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
        if (self.skillLocator.utility) self.skillLocator.utility.UnsetSkillOverride(self, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
        if (self.skillLocator.special) self.skillLocator.special.UnsetSkillOverride(self, disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
    }
}