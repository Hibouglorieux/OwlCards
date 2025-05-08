using ModdingUtils.Extensions;
using OwlCards.Extensions;
using Photon.Pun;
using RarityBundle;
using RarityLib.Utils;
using UnityEngine;

namespace OwlCards.Cards
{
	internal class AssertDominance : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			statModifiers.health = 1.4f;
			gun.damage = 1.4f;
			gun.knockback = 2.0f;
			gun.reloadTime = 0.75f;
			statModifiers.movementSpeed = 1.25f;
			statModifiers.sizeMultiplier = 1.15f;
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
			{
				int[] othersIDs = Utils.GetOpponentsPlayersIDs(player.playerID);
				float[] newSoulValues = new float[othersIDs.Length];
				for (int i = 0; i < othersIDs.Length; i++)
				{
					newSoulValues[i] = OwlCardsData.GetData(othersIDs[i]).Soul - 2;
				}
				OwlCardsData.UpdateSoul(othersIDs, newSoulValues);
			}
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
			{
				int[] othersIDs = Utils.GetOpponentsPlayersIDs(player.playerID);
				float[] newSoulValues = new float[othersIDs.Length];
				for (int i = 0; i < othersIDs.Length; i++)
				{
					newSoulValues[i] = OwlCardsData.GetData(othersIDs[i]).Soul + 2;
				}
				OwlCardsData.UpdateSoul(othersIDs, newSoulValues);
			}
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Assert Dominance";
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
					stat = "Knockback",
					amount = "+100%",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = true,
					stat = "Damage",
					amount = "+40%",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = true,
					stat = "Health",
					amount = "+40%",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = true,
					stat = "Move speed",
					amount = "+25%",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = true,
					stat = "Reload speed",
					amount = "-25%",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = true,
					stat = "Foes' Soul",
					amount = "-2",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = false,
					stat = "Size",
					amount = "+15%",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_AssertDominance");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return Rarities.Epic;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.NatureBrown;
		}
	}
}
