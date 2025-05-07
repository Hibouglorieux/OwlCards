using ModdingUtils.Extensions;
using RarityBundle;
using RarityLib.Utils;
using UnityEngine;
using System.Linq;
using OwlCards.Extensions;
using OwlCards.Dependencies;

namespace OwlCards.Cards.Curses
{
	internal class Burden : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			if (!cardInfo.categories.Contains(CurseHandler.CurseCategory))
				cardInfo.categories = cardInfo.categories.Append(CurseHandler.CurseCategory).ToArray();
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			OwlCardsData.GetData(player.playerID).soulConsumptionFactor *= 0.5f;
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			OwlCardsData.GetData(player.playerID).soulConsumptionFactor /= 0.5f;
			//Run when the card is removed from the player
		}
		public override string GetModName()
		{
			return "OWL-Curse";
		}

		protected override string GetTitle()
		{
			return "Burden";
		}
		protected override string GetDescription()
		{
			return "Increase the cost of soul rerolls";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				/*
				new CardInfoStat()
				{
					positive = true,
					stat = "Effect",
					amount = "No",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
				*/
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_Burden");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return Rarities.Uncommon;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CurseHandler.CursedPink;
		}
	}
}
