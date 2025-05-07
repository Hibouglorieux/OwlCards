using ModdingUtils.Extensions;
using RarityBundle;
using RarityLib.Utils;
using UnityEngine;
using System.Linq;
using UnboundLib.Utils;
using Photon.Pun;
using OwlCards.Extensions;
using OwlCards.Dependencies;

namespace OwlCards.Cards.Curses
{
	internal class WeakenedSoul : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			if (!cardInfo.categories.Contains(CurseHandler.CurseCategory))
				cardInfo.categories = cardInfo.categories.Append(CurseHandler.CurseCategory).ToArray();
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
				OwlCardsData.UpdateSoul(player.playerID, OwlCardsData.GetData(player.playerID).Soul - 1);
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
				OwlCardsData.UpdateSoul(player.playerID, OwlCardsData.GetData(player.playerID).Soul + 1);
			//Run when the card is removed from the player
		}
		public override string GetModName()
		{
			return "OWL-Curse";
		}

		protected override string GetTitle()
		{
			return "Weakened Soul";
		}
		protected override string GetDescription()
		{
			return "Your soul has been weakened";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = false,
					stat = "Soul",
					amount = "-1",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_WeakenedSoul");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return Rarities.Common;
		}
		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CurseHandler.CursedPink;
		}
	}
}
