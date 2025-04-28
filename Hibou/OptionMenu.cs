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
			MenuHandler.CreateSlider("reroll points passively earned per round", menu, 30, 0, 2, OwlCards.instance.rerollPointsPerRound.Value,
				(float newValue) => { OwlCards.instance.rerollPointsPerRound.Value = newValue; }, out UnityEngine.UI.Slider _);

			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateSlider("Extra reroll earned per point won (not applied to round winner)(should it ?)", menu, 30, 0, 1,
				OwlCards.instance.rerollPointsPerPointWon.Value,
				(float newValue) => { OwlCards.instance.rerollPointsPerPointWon.Value = newValue; }, out UnityEngine.UI.Slider _);

			MenuHandler.CreateText("", menu, out TextMeshProUGUI _);
			MenuHandler.CreateSlider("starting rerolls", menu, 30, 0, 5,
				OwlCards.instance.startingRerolls.Value,
				(float newValue) => { OwlCards.instance.startingRerolls.Value = newValue; }, out UnityEngine.UI.Slider _);
		}
	}
}
