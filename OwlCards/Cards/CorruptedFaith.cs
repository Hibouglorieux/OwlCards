using ModdingUtils.Extensions;
using OwlCards.Extensions;
using Photon.Pun;
using RarityBundle;
using UnityEngine;
using System.Linq;

namespace OwlCards.Cards
{
	internal class CorruptedFaith : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			conditions[GetTitle()] = (float soul) => { return soul >= 2; };
			if (!cardInfo.categories.Contains(OwlCardCategory.soulCondition))
				cardInfo.categories = cardInfo.categories.Append(OwlCardCategory.soulCondition).ToArray();
			cardInfo.allowMultiple = false;
			cardInfo.GetAdditionalData().canBeReassigned = false;
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
			{
				OwlCardsData.UpdateSoul(player.playerID, OwlCardsData.GetData(player.playerID).Soul - 2);
				Reroll.instance.Add1Reroll(player.playerID);
			}
			foreach (int otherPLayerID in Utils.GetOpponentsPlayersIDs(player.playerID))
				DrawNCards.DrawNCards.SetPickerDraws(otherPLayerID, DrawNCards.DrawNCards.GetPickerDraws(otherPLayerID) - 1);

			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			foreach (int otherPLayerID in Utils.GetOpponentsPlayersIDs(player.playerID))
				DrawNCards.DrawNCards.SetPickerDraws(otherPLayerID, DrawNCards.DrawNCards.GetPickerDraws(player.playerID) + 1);
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Corrupted Faith";
		}
		protected override string GetDescription()
		{
			return "Reroll your hand and cripple your foes";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = true,
					stat = "Foes' hand size",
					amount = "-1",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = false,
					stat = "Soul",
					amount = "-2",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_CorruptedFaith");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return Rarities.Exotic;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.EvilPurple;
		}
	}
}
