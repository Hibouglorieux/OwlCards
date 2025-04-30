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
			Unbound.RegisterMenu(OwlCards.ModName, () => { }, CreateMenuUI, null, true);
		}

		private static void CreateMenuUI(GameObject menu)
		{
			MenuHandler.CreateText(OwlCards.ModName + " Options", menu, out TextMeshProUGUI _);
			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);

			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateSlider("reroll points passively earned per round", menu, 30, 0, 2, OwlCards.instance.soulGainedPerRound.Value,
				(float newValue) => { OwlCards.instance.soulGainedPerRound.Value = newValue; }, out UnityEngine.UI.Slider _);

			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateSlider("Extra reroll earned per point won (not applied to round winner)(should it ?)", menu, 30, 0, 1,
				OwlCards.instance.rerollPointsPerPointWon.Value,
				(float newValue) => { OwlCards.instance.rerollPointsPerPointWon.Value = newValue; }, out UnityEngine.UI.Slider _);

			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateSlider("Starting rerolls", menu, 30, 0, 5,
				OwlCards.instance.soulOnGameStart.Value,
				(float newValue) => { OwlCards.instance.soulOnGameStart.Value = newValue; }, out UnityEngine.UI.Slider _);

			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateSlider("Cost of reroll (block button)", menu, 30, 0.5f, 3.0f,
				OwlCards.instance.rerollSoulCost.Value,
				(float newValue) => { OwlCards.instance.rerollSoulCost.Value = newValue; }, out UnityEngine.UI.Slider _);

			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateSlider("Cost of pick + reroll (fire button)", menu, 30, 1, 10,
				OwlCards.instance.extraPickSoulCost.Value,
				(float newValue) => { OwlCards.instance.extraPickSoulCost.Value = newValue; }, out UnityEngine.UI.Slider _, true);
		}
	}
}
