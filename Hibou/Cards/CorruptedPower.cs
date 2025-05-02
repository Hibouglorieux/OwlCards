using ModdingUtils.Extensions;
using UnityEngine;
using Photon.Pun;
using RarityBundle;
using RarityLib.Utils;

namespace OwlCards.Cards
{
    internal class CorruptedPower : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			//active only if at least one epic card is available
			conditions[GetTitle()] = (float _) => {
				foreach (CardInfo info in ModdingUtils.Utils.Cards.active)
				{
					if (info.rarity == Rarities.Epic)
						return true;
				}
				return false;
				};
			cardInfo.GetAdditionalData().canBeReassigned = false;
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
			{
				int[] othersIDs = Utils.GetOtherPlayersIDs(player.playerID);
				Reroll.instance.AddReroll(othersIDs);
			}

			CardInfo randomCard = ModdingUtils.Utils.Cards.instance.GetRandomCardWithCondition(player, gun, gunAmmo, data, health, gravity, block, characterStats,
				(cardInfo, player, gun, gunAmmo, data, health, gravity, block, characterStats) => {
					return RarityUtils.GetRarityData(cardInfo.rarity).calculatedRarity
					<= RarityUtils.GetRarityData(Rarities.Epic).calculatedRarity;
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
			return "Corrupted Power";
		}
		protected override string GetDescription()
		{
			return "Acquire a random " +
				RarityToColorString(Rarities.Epic) +
				" or rarer card";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = false,
					stat = "Pick for everyone else",
					amount = "+1",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_CorruptedPower");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return Rarities.Rare;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.TechWhite;
		}
	}
}
