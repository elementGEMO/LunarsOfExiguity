using RoR2;
using R2API;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace LunarsOfExiguity;
public class BrittleCrownHooks
{
    private static readonly string InternalName = "BrittleCrownHooks";
    public static bool ReworkItemEnabled;
    public static bool PureItemEnabled;

    public static GameObject ProvidenceSymbol;
    public static GameObject SymbolExplodeEffect;
    public static readonly Color BaseSymbolColor = new Color32(0, 93, 85, 255);

    private static readonly Color HudMoneyColor = new Color32(255, 252, 196, 255);
    private static readonly Color BarMoneyColor = new Color32(248, 252, 151, 255);
    private static readonly Color DebtColor = new Color32(255, 121, 126, 255);

    private static readonly Color AlphaMoneyColor = new Color32(197, 189, 144, 34);
    private static readonly Color AlphaDebtColor = new Color32(255, 121, 126, 34);

    public BrittleCrownHooks()
    {
        ReworkItemEnabled = BrittleCrownRework.Rework_Enabled.Value;
        PureItemEnabled = PureCrownItem.Item_Enabled.Value;

        if (ReworkItemEnabled)
        {
            new Hook(typeof(CharacterMaster).GetMethod("set_money"), SetMoney);

            On.RoR2.UI.HUD.Update += UpdateHud;
            On.RoR2.UI.ScoreboardStrip.UpdateMoneyText += UpdateStripHud;

            IL.RoR2.GlobalEventManager.ProcessHitEnemy += DisableGold;
            IL.RoR2.HealthComponent.TakeDamageProcess += ModifyDamage;
            IL.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += AllowDebt;
        }
        if (PureItemEnabled)
        {
            CreateFreeVisual();
            IL.RoR2.PurchaseInteraction.OnInteractionBegin += RefundFreeUnlock;
            On.RoR2.PurchaseInteraction.Awake += AddFreeComponent;
        }
    }

    private static void SetMoney(Action<CharacterMaster, uint> orig, CharacterMaster self, uint value)
    {
        if (value == self._money) return;

        CharacterBody selfBody = self.GetBody();
        if (selfBody)
        {
            int buffCount = selfBody.GetBuffCount(DebtCountBuff.BuffDef);
            int intValue = Mathf.Abs((int)value);

            if ((int)value < 0)
            {
                for (int i = 0; i < intValue; i++) selfBody.AddBuff(DebtCountBuff.BuffDef);
                value = 0U;
            }
            else if (buffCount > 0)
            {
                for (int i = 0; i < value; i++) selfBody.RemoveBuff(DebtCountBuff.BuffDef);
                value = (uint)Mathf.Max(intValue - buffCount, 0);
            }
        }

        if (value == self._money) return;

        self.SetDirtyBit(2U);
        self._money = value;
    }

    private void UpdateHud(On.RoR2.UI.HUD.orig_Update orig, RoR2.UI.HUD self)
    {
        orig(self);

        if (self.moneyText)
        {
            int targetMoney = 0;
            if (self.targetMaster) targetMoney = (int) self.targetMaster.money - self.targetMaster.GetBody().GetBuffCount(DebtCountBuff.BuffDef);
            self.moneyText.targetValue = targetMoney;

            if (targetMoney >= 0) self.moneyText.targetText.color = HudMoneyColor;
            else self.moneyText.targetText.color = DebtColor;

            var gradientPanel = self.moneyText.transform.Find("BackgroundPanel")?.GetComponent<RawImage>();
            if (gradientPanel)
            {
                if (targetMoney >= 0) gradientPanel.color = AlphaMoneyColor;
                else gradientPanel.color = AlphaDebtColor;
            }

            var cashSymbol = self.moneyText.transform.Find("DollarSign")?.GetComponent<RoR2.UI.HGTextMeshProUGUI>();
            if (cashSymbol)
            {
                if (targetMoney >= 0) cashSymbol.color = HudMoneyColor;
                else cashSymbol.color = DebtColor;
            }
        }
    }
    private void UpdateStripHud(On.RoR2.UI.ScoreboardStrip.orig_UpdateMoneyText orig, RoR2.UI.ScoreboardStrip self)
    {
        if (self.master)
        {
            self.previousMoney = (uint)((int)self.master.money - self.master.GetBody().GetBuffCount(DebtCountBuff.BuffDef));

            string cashPlacement = (int)self.previousMoney >= 0 ? "$" : "-$";
            self.moneyText.text = string.Format("{0}{1}", cashPlacement, Mathf.Abs((int)self.previousMoney));

            if ((int)self.previousMoney >= 0) self.moneyText.color = BarMoneyColor;
            else self.moneyText.color = DebtColor;
        }
    }

    private void DisableGold(ILContext il)
    {
        ILCursor cursor = new(il);

        if (cursor.TryGotoNext(
            x => x.MatchNewobj(typeof(RoR2.Orbs.GoldOrb)),
            x => x.MatchStloc(out _),
            x => x.MatchLdloc(out _),
            x => x.MatchLdarg(1)
        ))
        {
            var previousIndex = cursor.Index;

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdflda<DamageInfo>(nameof(DamageInfo.procChainMask)),
                x => x.MatchLdcI4(out _)
            ))
            {
                var skipLabel = cursor.MarkLabel();
                cursor.Goto(previousIndex);
                cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Br, skipLabel.Target);
            }
        }
        else Log.Warning(InternalName + " - #1 (DisableGold) Failure");
    }
    private void ModifyDamage(ILContext il)
    {
        ILCursor cursor = new(il);
        int damageIndex = -1;

        cursor.TryGotoNext(
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.damage)),
            x => x.MatchStloc(out damageIndex)
        );

        if (damageIndex != -1 && cursor.TryGotoNext(
            x => x.MatchLdloc(out _),
            x => x.MatchLdarg(0),
            x => x.MatchCall<HealthComponent>("get_fullCombinedHealth"),
            x => x.MatchDiv(),
            x => x.MatchLdloc(out _),
            x => x.MatchCallOrCallvirt<CharacterMaster>("get_money")
        ))
        {
            cursor.Emit(OpCodes.Ldarg, 0);
            cursor.Emit(OpCodes.Ldarg, 1);
            cursor.Emit(OpCodes.Ldloc, damageIndex);

            cursor.EmitDelegate<Func<HealthComponent, DamageInfo, float, float>>((self, damageInfo, damage) =>
            {
                float setDamage = damage;
                int buffCount = self.body.GetBuffCount(DebtCountBuff.BuffDef);

                if (buffCount > 0)
                {
                    setDamage *= 1f + (buffCount * BrittleCrownRework.Debt_Damage.Value / 100f) / Run.instance.difficultyCoefficient;
                    damageInfo.damageColorIndex = DamageColorIndex.WeakPoint;

                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.loseCoinsImpactEffectPrefab, new EffectData()
                    {
                        origin = self.body.corePosition,
                        scale = self.body.radius * 3f
                    }, true);
                }

                return setDamage;
            });

            cursor.Emit(OpCodes.Stloc, damageIndex);

            var previousIndex = cursor.Index;

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdflda<HealthComponent>(nameof(HealthComponent.itemCounts)),
                x => x.MatchLdfld(typeof(HealthComponent.ItemCounts), nameof(HealthComponent.ItemCounts.goldOnHurt))
            ))
            {
                var skipLabel = cursor.MarkLabel();
                cursor.Goto(previousIndex);
                cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Br, skipLabel.Target);
            }
        }
        else Log.Warning(InternalName + " - #1 (ModifyDamage) Failure");
    }
    private void AllowDebt(ILContext il)
    {
        ILCursor cursor = new(il);
        int numIndex = -1;

        cursor.TryGotoNext(
            x => x.MatchLdfld<PurchaseInteraction>(nameof(PurchaseInteraction.cost)),
            x => x.MatchStloc(out numIndex)
        );

        if (numIndex != -1 && cursor.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PurchaseInteraction>(nameof(PurchaseInteraction.costType)),
            x => x.MatchCallOrCallvirt(typeof(CostTypeCatalog), nameof(CostTypeCatalog.GetCostTypeDef))
        ))
        {
            cursor.Emit(OpCodes.Ldarg, 1);
            cursor.Emit(OpCodes.Ldloc, numIndex);

            cursor.EmitDelegate<Func<Interactor, int, int>>((activator, cost) =>
            {
                int modCost = cost;
                if (activator.GetComponent<CharacterBody>().inventory?.GetItemCount(RoR2Content.Items.GoldOnHit) > 0) modCost = 0;
                return modCost;
            });

            cursor.Emit(OpCodes.Stloc, numIndex);
        }
        else Log.Warning(InternalName + " - #1 (AllowDebt) Failure");
    }

    private void CreateFreeVisual()
    {
        ProvidenceSymbol = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ShrineChance/ShrineChance.prefab").WaitForCompletion().InstantiateClone("TempShrinePrefab", true).transform.FindChild("Symbol").gameObject;
        MeshRenderer mesh = ProvidenceSymbol.GetComponent<MeshRenderer>();
        Material newMat = new(mesh.sharedMaterial);

        newMat.mainTexture = LoEPlugin.Bundle.LoadAsset<Sprite>("PureCrownSymbol").texture;
        newMat.SetColor("_TintColor", BaseSymbolColor);
        newMat.SetFloat("_AlphaBoost", 0.6f);
        mesh.sharedMaterial = newMat;

        ProvidenceSymbol.transform.localScale *= 1.5f;

        SymbolExplodeEffect = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Effects/MonstersOnShrineUse"), "PureCrownExplode", true);
        UnityEngine.Object.Destroy(SymbolExplodeEffect.transform.FindChild("Chunks01_Ps").gameObject);
        UnityEngine.Object.Destroy(SymbolExplodeEffect.transform.FindChild("Chunks01_Ps (1)").gameObject);
        UnityEngine.Object.Destroy(SymbolExplodeEffect.transform.FindChild("Rings").gameObject);

        foreach (ParticleSystemRenderer particle in SymbolExplodeEffect.transform.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            if (!particle.sharedMaterial) continue;
            Material particleMat = new(particle.sharedMaterial);
            particleMat.DisableKeyword("VERTEXCOLOR");
            particleMat.SetColor("_TintColor", BaseSymbolColor);
            particle.sharedMaterial = particleMat;
        }

        SymbolExplodeEffect.transform.GetComponentInChildren<Light>().color = BaseSymbolColor;
        SymbolExplodeEffect.transform.FindChild("BlastDark_Ps").localScale = Vector3.one * 1.25f;

        new EffectDef()
        {
            prefab = SymbolExplodeEffect,
            prefabName = "PureCrownExplode",
            prefabEffectComponent = SymbolExplodeEffect.GetComponent<EffectComponent>()
        };

        ContentAddition.AddEffect(SymbolExplodeEffect);
    }

    private void RefundFreeUnlock(ILContext il)
    {
        ILCursor cursor = new(il);

        if (cursor.TryGotoNext(
            x => x.MatchLdcI4(out _),
            x => x.MatchStloc(out _),
            x => x.MatchLdarg(1),
            x => x.MatchCallOrCallvirt<Component>(nameof(Component.GetComponent)),
            x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.FreeUnlocks))
        ))
        {
            cursor.Emit(OpCodes.Ldarg, 0);
            cursor.Emit(OpCodes.Ldarg, 1);

            cursor.EmitDelegate<Action<PurchaseInteraction, Interactor>>((self, activator) =>
            {
                if (self.cost == 0) activator.GetComponent<CharacterBody>().AddBuff(DLC2Content.Buffs.FreeUnlocks);
            });
        }
        else Log.Warning(InternalName + " - #1 (RefundFreeUnlock) Failure");
    }
    private void AddFreeComponent(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
    {
        orig(self);
        if (self.costType == CostTypeIndex.Money) self.gameObject.AddComponent<CrownFreePurchase>();
    }
}

public class CrownFreePurchase : NetworkBehaviour
{
    [SyncVar]
    public bool ShowIcon;
    private PurchaseInteraction Self;

    private GameObject ProvIcon;
    private GameObject Hologram;

    public void Awake()
    {
        Self = GetComponent<PurchaseInteraction>();
        int itemCount = Util.GetItemCountGlobal(PureCrownItem.ItemDef.itemIndex, false, false);

        if (itemCount > 0 && Util.CheckRoll(PureCrownItem.Chance_Free.Value, itemCount - 1))
        {
            CreateVisual();
            ShowIcon = true;
            Hologram = transform.FindChild("HologramPivot")?.gameObject;
        }
        else
        {
            enabled = false;
            Destroy(this);
        }
    }
    public void LateUpdate()
    {
        if (!ProvIcon || !Self) return;

        if (Self.Networkcost != 0) Self.Networkcost = 0;
        if (Self.cost != 0) Self.cost = 0;

        if (ShowIcon != Self.available)
        {
            ShowIcon = Self.available;
            ProvIcon.SetActive(ShowIcon);

            if (!ShowIcon)
            {
                EffectManager.SpawnEffect(BrittleCrownHooks.SymbolExplodeEffect, new EffectData
                {
                    origin = ProvIcon.transform.position,
                    rotation = UnityEngine.Random.rotation
                }, true);
            }
        }

        if (Hologram && Hologram.transform.GetComponentInChildren<TextMeshPro>())
        {
            TextMeshPro text = Hologram.transform.GetComponentInChildren<TextMeshPro>();
            text.text = "...";
            text.color = BrittleCrownHooks.BaseSymbolColor;
        }
    }
    private void CreateVisual()
    {
        ProvIcon = Instantiate(BrittleCrownHooks.ProvidenceSymbol);
        ProvIcon.name = "PureHonorSymbol";
        ProvIcon.transform.parent = gameObject.transform;
        ProvIcon.transform.localPosition = new Vector3(0f, 5f, 0f);
    }
}