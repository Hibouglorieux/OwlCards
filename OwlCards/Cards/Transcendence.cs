using ModdingUtils.Extensions;
using OwlCards.Extensions;
using Photon.Pun;
using RarityBundle;
using RarityLib.Utils;
using UnityEngine;

namespace OwlCards.Cards
{
	internal class Transcendence : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			cardInfo.GetAdditionalData().canBeReassigned = false;
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
				OwlCardsData.UpdateSoul(player.playerID, OwlCardsData.GetData(player.playerID).Soul + 20);
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
				OwlCardsData.UpdateSoul(player.playerID, OwlCardsData.GetData(player.playerID).Soul - 20);
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Transcendence";
		}
		protected override string GetDescription()
		{
			return "";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = true,
					stat = "Soul",
					amount = "+20",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_Transcendence");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return Rarities.Legendary;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.MagicPink;
		}
	}
}
