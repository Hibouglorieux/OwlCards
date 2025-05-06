using RarityBundle;
using UnityEngine;
using Photon.Pun;
using OwlCards.Extensions;
using ModdingUtils.Extensions;
using RarityLib.Utils;

namespace OwlCards.Cards
{
	internal class OpenBar : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			cardInfo.GetAdditionalData().canBeReassigned = false;
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
			{
				int[] othersIDs = Utils.GetOtherPlayersIDs(player.playerID);
				float[] newSoulValues = new float[othersIDs.Length];
				for (int i = 0; i < othersIDs.Length; i++)
				{
					newSoulValues[i] = OwlCardsData.GetData(othersIDs[i]).Soul + 1;
				}
				OwlCardsData.UpdateSoul(othersIDs, newSoulValues);
			}

			CardInfo randomCard = ModdingUtils.Utils.Cards.instance.GetRandomCardWithCondition(player, gun, gunAmmo, data, health, gravity, block, characterStats,

				(cardInfo, player, gun, gunAmmo, data, health, gravity, block, characterStats) => {
					return RarityUtils.GetRarityData(cardInfo.rarity).calculatedRarity
					<= RarityUtils.GetRarityData(Rarities.Rare).calculatedRarity;
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
			return "OpenBar";
		}
		protected override string GetDescription()
		{
			return "You get a random " +
				RarityToColorString(Rarities.Rare) +
				" or rarer card";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = false,
					stat = "Others Soul",
					amount = "+1",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_OpenBar");
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
