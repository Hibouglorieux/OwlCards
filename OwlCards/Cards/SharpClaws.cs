using ModdingUtils.Extensions;
using RarityBundle;
using RarityLib.Utils;
using UnityEngine;
using Photon.Pun;
using OwlCards.Extensions;

namespace OwlCards.Cards
{
	internal class SharpClaws : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			gun.damage = 1.25f;
			gun.bursts = 1;
			gun.ammo = -1;
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
				OwlCardsData.UpdateSoul(player.playerID, OwlCardsData.GetData(player.playerID).Soul + 0.5f);
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
				OwlCardsData.UpdateSoul(player.playerID, OwlCardsData.GetData(player.playerID).Soul - 0.5f);
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "SharpClaws";
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
					stat = "Damage",
					amount = "25%",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = true,
					stat = "+1",
					amount = "Bullet",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = false,
					stat = "Ammo",
					amount = "-1",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = true,
					stat = "Soul",
					amount = "+0.5",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_SharpClaws");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return Rarities.Common;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.DestructiveRed;
		}
	}
}
