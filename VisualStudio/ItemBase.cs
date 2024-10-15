using R2API;
using RoR2;
using System;
using UnityEngine;

namespace LunarsOfExiguity
{
    public abstract class ItemBaseRework
    {
        protected virtual string Token => null;
        public ItemBaseRework(bool configValue = true) => Initialize(configValue);

        private void Initialize(bool configValue)
        {
            if (configValue) {
                LanguageTokens();
                Methods();
            }
            else
            {
                DisabledTokens();
            }
        }
        protected virtual void LanguageTokens() { }
        protected virtual void DisabledTokens() { }
        protected virtual void Methods() { }
    }
    public static class ItemUtils
    {
        public static string SignVal(this float value) => value >= 0 ? "+" + value : "-" + value;
        public static float RoundVal(float value) => MathF.Round(value, MainConfig.RoundNumber.Value);
    }
}
