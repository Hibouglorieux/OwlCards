using ModdingUtils.Extensions;
using UnityEngine;
using Photon.Pun;
using RarityBundle;

namespace OwlCards.Cards
{
	internal class CorruptedPower : AOwlCard
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
				int[] IDsWithReroll = new int[PlayerManager.instance.players.Count - 1];

				int i = 0;
				foreach (Player otherPlayer in PlayerManager.instance.players.ToArray())
				{
					if (otherPlayer.playerID != player.playerID)
					{
						IDsWithReroll[i++] = otherPlayer.playerID;
					}
				}
				RerollButton.instance.AddReroll(IDsWithReroll);
			}

			CardInfo randomCard = ModdingUtils.Utils.Cards.instance.GetRandomCardWithCondition(player, gun, gunAmmo, data, health, gravity, block, characterStats,

				(cardInfo, player, gun, gunAmmo, data, health, gravity, block, characterStats) => {
					return RarityLib.Utils.RarityUtils.GetRarityData(cardInfo.rarity).calculatedRarity
					<= RarityLib.Utils.RarityUtils.GetRarityData(Rarities.Epic).calculatedRarity;
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
			return "Acquire random Epic or rarer card";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = false,
					stat = "Pick to everyone else",
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
