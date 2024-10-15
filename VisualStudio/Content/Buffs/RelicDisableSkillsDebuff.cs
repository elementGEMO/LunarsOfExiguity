using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Skills;

namespace LunarsOfExiguity.Content.Buffs;

public class RelicDisableSkillsDebuff : BuffBase
{
    protected override string Name => "RelicDisableSkills";

    protected override bool IsStackable => true;

    protected override void Initialize()
    {
        IL.RoR2.CharacterBody.RecalculateStats += il =>
        {
            var cursor = new ILCursor(il);
            if (cursor.TryGotoNext(x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HandleDisableAllSkillsDebuff))))
            {
                cursor.Emit(OpCodes.Ldarg, 0);
                cursor.EmitDelegate<Action<CharacterBody>>(self =>
                {
                    if (!self.hasAuthority) return;
                    
                    var disableSkill = LegacyResourcesAPI.Load<SkillDef>("Skills/DisabledSkills");
                    if (!disableSkill)
                    {
                        Log.Warning("RelicDisableSkillsDebuff Failed to Find DisabledSkills SkillDef");
                        return;
                    }

                    if (self.skillLocator)
                    {
                        if (self.HasBuff(Get()))
                        {
                            if (self.skillLocator.primary) self.skillLocator.primary.SetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                            if (self.skillLocator.secondary) self.skillLocator.secondary.SetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                            if (self.skillLocator.utility) self.skillLocator.utility.SetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                            if (self.skillLocator.special) self.skillLocator.special.SetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                            return;
                        }
                        
                        if (self.skillLocator.primary) self.skillLocator.primary.UnsetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                        if (self.skillLocator.secondary) self.skillLocator.secondary.UnsetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                        if (self.skillLocator.utility) self.skillLocator.utility.UnsetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                        if (self.skillLocator.special) self.skillLocator.special.UnsetSkillOverride(self, disableSkill, GenericSkill.SkillOverridePriority.Contextual);
                    }
                });
                return;
            } 
            
            Log.Warning("Failed to Apply RelicDisableSkillsDebuff CharacterBody.RecalculateStats Hook.");
        };
    }
}