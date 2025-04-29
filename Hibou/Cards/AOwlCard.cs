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
}
