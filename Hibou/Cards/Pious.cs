using RarityBundle;
using UnityEngine;
using RarityLib.Utils;
using Photon.Pun;
using ModdingUtils.Extensions;
using OwlCards.Extensions;
using System.Linq;

namespace OwlCards.Cards
{
    internal class Pious : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			conditions[GetTitle()] = (float soul) => { return soul >= 3; };
			if (!cardInfo.categories.Contains(OwlCardCategory.soulCondition))
				cardInfo.categories = cardInfo.categories.Append(OwlCardCategory.soulCondition).ToArray();
			cardInfo.GetAdditionalData().canBeReassigned = false;
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
			{
				OwlCardsData.UpdateSoul(player.playerID, OwlCardsData.GetData(player).Soul - 3);
				Reroll.instance.Add1Reroll(player.playerID);
			}

			CardCategory[] blacklistedCategories = OwlCardCategory.GetRarityCategories(Rarities.Exotic, Rarities.Rare, true);
			for (int i = 0; i < blacklistedCategories.Length; i++)
				OwlCards.Log(blacklistedCategories[i].name);

			ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(characterStats).blacklistedCategories.AddRange(blacklistedCategories);

			OwlCards.Log("added");
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Pious";
		}
		protected override string GetDescription()
		{
			return "You get to pick a card among " +
				RarityToColorString(Rarities.Exotic) +
				" or " + RarityToColorString(Rarities.Rare) + " cards";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = false,
					stat = "Soul",
					amount = "-3",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_Pious");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return Rarities.Common;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.TechWhite;
		}
	}
}
