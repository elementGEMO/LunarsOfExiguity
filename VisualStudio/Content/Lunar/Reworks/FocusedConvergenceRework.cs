using RoR2;
using UnityEngine.Networking;

namespace LunarsOfExiguity.Content.Lunar.Reworks;

public class FocusedConvergenceRework : ItemReworkBase
{
    protected override string Name => "FocusedConvergence";

    protected override string RelicNameOverride => "Relic of Focus";

    protected override void Initialize()
    {
        CharacterBody.onBodyAwakeGlobal += Invincibility;
    }
    
    private static void Invincibility(CharacterBody self) => self.gameObject.AddComponent<InvincibleDuringHoldout>();
    
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