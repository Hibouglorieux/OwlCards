using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using UnboundLib;
using UnboundLib.Utils.UI;
using UnityEngine;
using UnityEngine.UI;

namespace OwlCards
{
	internal static class OptionMenu
	{

		public static void CreateMenu()
		{
			Unbound.RegisterMenu(OwlCards.ModName, () => { }, CreateMenuUI, null, false);
		}

		private static void CreateMenuUI(GameObject menu)
		{
			MenuHandler.CreateText(OwlCards.ModName + " Options", menu, out TextMeshProUGUI _);
			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);

			MenuHandler.CreateSlider("Soul points passively earned per round (not applied to round winner)", menu, 30, 0, 2, OwlCards.instance.soulGainedPerRound.Value,
				(float newValue) => { OwlCards.instance.soulGainedPerRound.Value = newValue; }, out UnityEngine.UI.Slider _);

			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateSlider("Extra soul gained per point won (not applied to round winner)", menu, 30, 0, 1,
				OwlCards.instance.soulGainedPerPointWon.Value,
				(float newValue) => { OwlCards.instance.soulGainedPerPointWon.Value = newValue; }, out UnityEngine.UI.Slider _);

			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateSlider("Starting Soul", menu, 30, 0, 5,
				OwlCards.instance.soulOnGameStart.Value,
				(float newValue) => { OwlCards.instance.soulOnGameStart.Value = newValue; }, out UnityEngine.UI.Slider _);

			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateSlider("Cost of reroll (block button)", menu, 30, 0.5f, 3.0f,
				OwlCards.instance.rerollSoulCost.Value,
				(float newValue) => { OwlCards.instance.rerollSoulCost.Value = newValue; }, out UnityEngine.UI.Slider _);

			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateSlider("Cost of extra pick (fire button)", menu, 30, 1, 10,
				OwlCards.instance.extraPickSoulCost.Value,
				(float newValue) => { OwlCards.instance.extraPickSoulCost.Value = newValue; }, out UnityEngine.UI.Slider _, true);
		}
	}
}
