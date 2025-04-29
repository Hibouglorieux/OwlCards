using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;


namespace OwlCards.Cards
{
	internal abstract class AOwlCard : CustomCard
	{
		public static Dictionary<string, Func<float, bool>> conditions = new Dictionary<string, Func<float, bool>>();
		public abstract void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block);

		public sealed override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			cardInfo.categories = new CardCategory[] { OwlCardCategory.modCategory};
			SetupCard_child(cardInfo, gun, cardStats, statModifiers, block);
		}
		protected GameObject GetCardArt(string name)
		{
			return OwlCards.instance.Bundle.LoadAsset<GameObject>(name);
		}

		protected override GameObject GetCardArt()
		{
			return null;
		}
		public override string GetModName()
		{
			return OwlCards.ModInitials;
		}

		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
		}
	}

	static internal class OwlCardCategory
	{
		public static CardCategory modCategory = CustomCardCategories.instance.CardCategory(OwlCards.ModName);
		public static CardCategory soulCondition = CustomCardCategories.instance.CardCategory(OwlCards.ModName);
	}
}
