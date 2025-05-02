using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using System;
using System.Collections.Generic;
using UnboundLib.Cards;
using UnityEngine;
using RarityLib.Utils;
using RarityBundle;
using System.Linq;
using System.Diagnostics.SymbolStore;
using HarmonyLib;


namespace OwlCards.Cards
{
	internal abstract class AOwlCard : CustomCard
	{
		public static Dictionary<string, Func<float, bool>> conditions = new Dictionary<string, Func<float, bool>>();
		public abstract void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block);

		public sealed override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			cardInfo.categories = new CardCategory[] { OwlCardCategory.modCategory };
			SetupCard_child(cardInfo, gun, cardStats, statModifiers, block);
		}

		protected string RarityToColorString(CardInfo.Rarity rarity)
		{
			Rarity rarityObj = RarityUtils.GetRarityData(rarity);
			if (rarity == Rarities.Common)
				return rarityObj.name;
			Color color = rarityObj.color;
			int r = (int)(0xFF * color.r);
			int g = (int)(0xFF * color.g);
			int b = (int)(0xFF * color.b);

			string coloredRarity = "<#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2") + ">" + rarityObj.name + "</color>";
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
		public static CardCategory soulCondition = CustomCardCategories.instance.CardCategory(OwlCards.ModName + "-SoulCondition");

		public static Dictionary<CardInfo.Rarity, CardCategory> rarityCategories = new Dictionary<CardInfo.Rarity, CardCategory>();

		public static void InitializeRarityCategories()
		{
			//already initialized
			if (rarityCategories.Count != 0)
				return;

			Rarity[] rarities = RarityUtils.Rarities.Values.ToArray();
			foreach (Rarity rarity in rarities)
			{
				rarityCategories.Add(rarity.value, CustomCardCategories.instance.CardCategory("OwlCards-" + rarity.name));
			}
		}
		public static CardCategory[] GetRarityCategoryAndLower(CardInfo.Rarity rarity, bool bGetReverseArray)
		{
			return GetRarityCategories(CardInfo.Rarity.Common, rarity, bGetReverseArray);
		}
		public static CardCategory[] GetRarityCategoryAndHigher(CardInfo.Rarity rarity, bool bGetReverseArray)
		{
			return GetRarityCategories(rarity, Rarities.Divine, bGetReverseArray);
		}

		public static CardCategory[] GetRarityCategories(CardInfo.Rarity lowestRarity, CardInfo.Rarity highestRarity, bool bGetReverseArray)
		{
			CardCategory[] cardCategory = new CardCategory[0];
			float lowRelativeRarity = RarityUtils.GetRarityData(lowestRarity).relativeRarity;
			float highRelativeRarity = RarityUtils.GetRarityData(highestRarity).relativeRarity;
			if (lowRelativeRarity < highRelativeRarity)
				throw new OwlCardsException("GetRarityCategories called with invalid low/high parameters");
			foreach (var (rarity, category) in rarityCategories)
			{
				float categoryRelativeRarity = RarityUtils.GetRarityData(rarity).relativeRarity;

				// common card woul be 0.75 and very rare 0.003
				// we want it to be lower than lowRelativeRarity
				// and higher then highRelativeRarity
				if (categoryRelativeRarity <= lowRelativeRarity &&
					categoryRelativeRarity >= highRelativeRarity)
				{
					if (!bGetReverseArray)
						cardCategory = cardCategory.Append(category).ToArray();
				}
				else if (bGetReverseArray)
				{
					cardCategory = cardCategory.Append(category).ToArray();
				}
			}
			return cardCategory;
		}
	}
}
