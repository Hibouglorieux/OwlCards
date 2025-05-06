using OwlCards.Extensions;
using Photon.Pun;
using RarityBundle;
using UnityEngine;
using ModdingUtils.Extensions;

namespace OwlCards.Cards
{
	internal class Dedication : AOwlCard
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
				OwlCardsData.UpdateSoul(player.playerID, OwlCardsData.GetData(player).Soul + 1);
			}

			CardInfo randomCard = ModdingUtils.Utils.Cards.instance.GetRandomCardWithCondition(player, gun, gunAmmo, data, health, gravity, block, characterStats,

				(cardInfo, player, gun, gunAmmo, data, health, gravity, block, characterStats) => {
					return cardInfo.rarity == Rarities.Common;
				}
				);

			ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, randomCard, addToCardBar: true);
			StartCoroutine(ModdingUtils.Utils.CardBarUtils.instance.ShowImmediate(player, randomCard));
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Dedication";
		}
		protected override string GetDescription()
		{
			return "Gives you a random " +
				RarityToColorString(Rarities.Common) + 
				" card";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = true,
					stat = "Soul",
					amount = "+1",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_Dedication");
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
