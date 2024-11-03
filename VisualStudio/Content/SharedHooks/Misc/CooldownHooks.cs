using BepInEx.Configuration;
using RoR2;
using UnityEngine;

namespace LunarsOfExiguity;
public class CooldownHooks
{
    public static ConfigEntry<float> Cooldown_Minimum;

    public CooldownHooks() => On.RoR2.GenericSkill.CalculateFinalRechargeInterval += ReplaceCharge;

    private float ReplaceCharge(On.RoR2.GenericSkill.orig_CalculateFinalRechargeInterval orig, GenericSkill self)
    {
        return Mathf.Min(self.baseRechargeInterval, Mathf.Max(Cooldown_Minimum.Value, self.baseRechargeInterval * self.cooldownScale - self.flatCooldownReduction)) + self.temporaryCooldownPenalty;
    }
}