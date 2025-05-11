using ModdingUtils.Extensions;
using RarityBundle;
using RarityLib.Utils;
using UnityEngine;
using System.Linq;
using OwlCards.Dependencies;
using Photon.Pun;
using OwlCards.Extensions;
using UnboundLib;

namespace OwlCards.Cards.Curses
{
	internal class Giveaway : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			conditions[GetTitle()] = (float soul) => { return soul >= 2; };
			if (!cardInfo.categories.Contains(CurseHandler.CurseCategory))
				cardInfo.categories = cardInfo.categories.Append(CurseHandler.CurseSpawnerCategory).ToArray();
			if (!cardInfo.categories.Contains(OwlCardCategory.soulCondition))
				cardInfo.categories = cardInfo.categories.Append(OwlCardCategory.soulCondition).ToArray();
			cardInfo.GetAdditionalData().canBeReassigned = false;
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
				OwlCardsData.UpdateSoul(player.playerID, OwlCardsData.GetData(player.playerID).Soul - 2);
			if (CurseHandler.bCurseAvailable)
			{
				OwlCards.instance.ExecuteAfterFrames(20, () =>
				{
					foreach (int otherPlayerID in Utils.GetOpponentsPlayersIDs(player.playerID))
					{
						OwlCurse.GiveCurse(Utils.GetPlayerWithID(otherPlayerID), 1);
					}
				});
			}
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Giveaway";
		}
		protected override string GetDescription()
		{
			return "Reroll your hand and curse your foes";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
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
			return GetCardArt("C_Giveaway");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return Rarities.Scarce;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.EvilPurple;
		}
	}
}
