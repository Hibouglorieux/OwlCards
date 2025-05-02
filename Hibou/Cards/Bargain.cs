using RarityBundle;
using UnityEngine;
using Photon.Pun;
using OwlCards.Extensions;
using ModdingUtils.Extensions;
using RarityLib.Utils;
using System.Linq;

namespace OwlCards.Cards
{
	internal class Bargain : AOwlCard
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
			}

			CardInfo randomCard = ModdingUtils.Utils.Cards.instance.GetRandomCardWithCondition(player, gun, gunAmmo, data, health, gravity, block, characterStats,

				(cardInfo, player, gun, gunAmmo, data, health, gravity, block, characterStats) => {
					return RarityUtils.GetRarityData(cardInfo.rarity).calculatedRarity
					<= RarityUtils.GetRarityData(Rarities.Exotic).calculatedRarity;
				}
				);

			ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, randomCard, addToCardBar: true);
			ModdingUtils.Utils.CardBarUtils.instance.ShowAtEndOfPhase(player, randomCard);
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Bargain";
		}
		protected override string GetDescription()
		{
			return "You trade part of your soul for a random " + 
				RarityToColorString(Rarities.Exotic) + 
				" or higher card";
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
			return GetCardArt("C_Bargain");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return Rarities.Uncommon;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.TechWhite;
		}
	}
}
