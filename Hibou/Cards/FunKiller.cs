using ModdingUtils.Extensions;
using OwlCards.Extensions;
using Photon.Pun;
using System.Linq;

namespace OwlCards.Cards
{
	internal class FunKiller : AOwlCard
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
				int[] playersIDs = new int[PlayerManager.instance.players.Count];
				float[] souls = new float[PlayerManager.instance.players.Count];
				for (int i = 0; i < playersIDs.Length; i++)
				{
					int playerID = PlayerManager.instance.players[i].playerID;
					float soul = OwlCardsData.GetData(playerID).Soul;
					if (playerID == player.playerID)
						soul -= 2f;
					else
						soul -= 0.5f;

					playersIDs[i] = playerID;
					souls[i] = soul;
				}
				OwlCardsData.UpdateSoul(playersIDs, souls);

			RerollButton.instance.Add1Reroll(player.playerID);
			}
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Fun Killer";
		}
		protected override string GetDescription()
		{
			return "Reroll your cards and drain everyone's soul.";
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
				},
				new CardInfoStat()
				{
					positive = true,
					stat = "Others Soul",
					amount = "-0.5",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
			};
		}

		/*
			protected override GameObject GetCardArt()
			{
				return GetCardArt("C_CARD_NAME");
			}
		*/
		protected override CardInfo.Rarity GetRarity()
		{
			return CardInfo.Rarity.Common;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.TechWhite;
		}
	}
}
