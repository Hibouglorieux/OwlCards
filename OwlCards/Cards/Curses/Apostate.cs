using ModdingUtils.Extensions;
using RarityBundle;
using RarityLib.Utils;
using UnityEngine;
using OwlCards.Dependencies;
using System.Linq;
using Photon.Pun;
using OwlCards.Extensions;

namespace OwlCards.Cards.Curses
{
	internal class Apostate : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			conditions[GetTitle()] = (float soul) => { return soul < OwlCards.instance.extraPickSoulCost.Value && soul >= (OwlCards.instance.extraPickSoulCost.Value / 2); };
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
			{
				OwlCardsData.UpdateSoul(player.playerID, OwlCardsData.GetData(player.playerID).Soul - 15);
				Reroll.instance.AddCustomDraw(player.playerID, 0, OwlCardCategory.GetRarityCategoryAndHigher(Rarities.Epic, true));
				Reroll.instance.Add1Reroll(player.playerID);
			}
			OwlCurse.GiveCurse(player);

			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Apostate";
		}
		protected override string GetDescription()
		{
			return "You sell your soul to chose an " + 
				RarityToColorString(Rarities.Epic)
				+" or rarer card";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = false,
					stat = "Curse",
					amount = "+1",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = false,
					stat = "Soul",
					amount = "-15",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}

			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_Apostate");
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
