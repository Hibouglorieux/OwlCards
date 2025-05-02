using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using System;
using System.Collections.Generic;
using UnboundLib.Cards;
using UnityEngine;
using RarityLib.Utils;
using RarityBundle;


namespace OwlCards.Cards
{
	internal abstract class AOwlCard : CustomCard
	{
		public static Dictionary<string, Func<float, bool>> conditions = new Dictionary<string, Func<float, bool>>();
		public abstract void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block);

		public sealed override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			cardInfo.categories = new CardCategory[] { OwlCardCategory.modCategory};
			SetupCard_child(cardInfo, gun, cardStats, statModifiers, block);
		}

		protected string RarityToColorString(CardInfo.Rarity rarity)
		{
			Rarity rarityObj = RarityUtils.GetRarityData(rarity);
			Color color = rarityObj.color;
			int r = (int)(0xFF / color.r);
			int g = (int)(0xFF / color.g);
			int b = (int)(0xFF / color.b);

			string coloredRarity = "<#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2") + ">" + rarityObj.name + "</color>";
			OwlCards.Log(coloredRarity);
			return coloredRarity;
		}
		protected GameObject GetCardArt(string name)
		{
			try
			{
				return OwlCards.instance.Bundle.LoadAsset<GameObject>(name);
			}
			catch
			{
				return null;
			}
		}

		protected override GameObject GetCardArt()
		{
			return null;
		}
		public override string GetModName()
		{
			return OwlCards.ModInitials;
		}

		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
		}
	}

	static internal class OwlCardCategory
	{
		public static CardCategory modCategory = CustomCardCategories.instance.CardCategory(OwlCards.ModName);
		public static CardCategory soulCondition = CustomCardCategories.instance.CardCategory(OwlCards.ModName);
	}
}
