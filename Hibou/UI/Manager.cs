using OwlCards.Cards;
using OwlCards.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnboundLib;
using UnboundLib.GameModes;
using UnityEngine;
using UnityEngine.UI;

namespace OwlCards.UI
{
	internal class Manager : MonoBehaviour
	{
		public static Manager instance;
		private GameObject canvas = null;
		private GameObject soulFill = null;
		private int playerSoulFillID = -1;
		private Dictionary<int, GameObject> soulCounters = new Dictionary<int, GameObject>();
		private List<Action<float>> handlers = new List<Action<float>>();

		void Awake()
		{
			instance = this;
		}

		void Start()
		{
			GameModeManager.AddHook(GameModeHooks.HookGameStart, BuildUI);
			GameModeManager.AddHook(GameModeHooks.HookGameEnd, ClearUI);

			GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd);
		}

		private IEnumerator OnPlayerPickEnd(IGameModeHandler gm)
		{
			playerSoulFillID = -1;
			soulFill.SetActive(false);
			yield break;
		}

		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd);

			GameModeManager.RemoveHook(GameModeHooks.HookGameStart, BuildUI);
			GameModeManager.RemoveHook(GameModeHooks.HookGameEnd, ClearUI);
		}

		private IEnumerator BuildUI(IGameModeHandler handler)
		{
			BuildCanvas();
			BuildFillUI();
			BuildSoulCounters();
			yield break;
		}
		private IEnumerator ClearUI(IGameModeHandler handler)
		{
			RemoveCanvas();
			yield break;
		}

		public void BuildCanvas()
		{
			GameObject assetCanvas = OwlCards.instance.Bundle.LoadAsset<GameObject>("OC_UI_Canvas");
			GameObject UI = GameObject.Find("UI_Game");

			canvas = Instantiate(assetCanvas, UI.transform);
		}

		public void UpdateFillUI(float soulValue)
		{
			if (!soulFill)
				return;

			//added UI is a Border image which itself has a Background child, which has two childs Image and text
			Transform background = soulFill.transform.GetChild(0);
			Image fillImage = background.GetChild(0).GetComponent<Image>();
			Text text = background.GetChild(1).GetComponent<Text>();

			text.text = String.Format(soulValue.ToString("G3") + " Soul");
			fillImage.fillAmount = (soulValue > 1 ? soulValue - (Mathf.Floor(soulValue)) : soulValue);

			Image backgroundImage = background.GetComponent<Image>();
			backgroundImage.color = Color.white;
			fillImage.fillOrigin = 0; // left
			if (soulValue < 0)
			{
				fillImage.fillOrigin = 1; // right
				fillImage.color = Color.red;
				fillImage.fillAmount = Mathf.Clamp(Mathf.Abs(soulValue), 0, 1);
			}
			if (soulValue > 0)
			{
				fillImage.color = new Color(0f, 1f, 0.38f, 1f);
			}
			if (soulValue > 1)
			{
				backgroundImage.color = fillImage.color;
				fillImage.color = new Color(0f, 1f, 0.82f, 1f);
			}
			if (soulValue > 2)
			{
				backgroundImage.color = fillImage.color;
				fillImage.color = new Color(0f, 0.27f, 1f, 1f);
			}
			if (soulValue > 3)
			{
				backgroundImage.color = fillImage.color;
				fillImage.color = new Color(0.63f, 0f, 1f, 1f);
			}
			if (soulValue > 4)
			{
				backgroundImage.color = fillImage.color;
				fillImage.color = new Color(1f, 0.58f, 0f, 1f);
			}

			Color inactiveColor = new Color(0.42f, 0.42f, 0.42f, 1f);
			Color activeBlockColor = new Color(0.22f, 0.91f, 0.91f, 1f);
			Color activeFireColor = new Color(1f, 0.51f, 0.33f, 1f);

			string blockDisplayedMsg = "Press <color=blue>Block</color> to reroll cards\n <i>(costs "+ OwlCards.instance.rerollSoulCost.Value.ToString("G3") +" soul)</i>";
			string fireDisplayMsg = "Press <color=red>Fire</color> to pick an extra card\n <i>(costs "+ OwlCards.instance.extraPickSoulCost.Value.ToString("G3") +" soul)</i>";

			soulFill.transform.GetChild(1).GetComponent<Image>().color = soulValue < OwlCards.instance.rerollSoulCost.Value ? inactiveColor : activeBlockColor;
			soulFill.transform.GetChild(2).GetComponent<Image>().color = soulValue < OwlCards.instance.extraPickSoulCost.Value ? inactiveColor : activeFireColor;

			soulFill.transform.GetChild(1).GetComponentInChildren<Text>().text = blockDisplayedMsg; 
			soulFill.transform.GetChild(2).GetComponentInChildren<Text>().text = fireDisplayMsg; 
		}

		public void BuildFillUI()
		{
			//already existant
			if (soulFill)
				return;

			playerSoulFillID = -1;
			GameObject assetFill = OwlCards.instance.Bundle.LoadAsset<GameObject>("OC_UI_SoulFill");
			if (!canvas)
				BuildCanvas();
			soulFill = Instantiate(assetFill, canvas.transform);
			soulFill.SetActive(false);
		}

		void Update()
		{
			// this 'should' be done at playerPickStart hook
			// however CardChoice.instance.pickrID isn't set yet
			if (soulFill && CardChoice.instance.pickrID != -1 && playerSoulFillID != CardChoice.instance.pickrID)
			{
				playerSoulFillID = CardChoice.instance.pickrID;
				soulFill.SetActive(true);
				UpdateFillUI(OwlCardsData.GetData(playerSoulFillID).Soul);
			}
		}

		public void BuildSoulCounters()
		{
			int i = 0;
			if (!canvas)
				BuildCanvas();
			foreach (Player player in PlayerManager.instance.players.ToArray())
			{
				//retrieve good Y position to align with cardBar

				GameObject CardViz = GameObject.Find("CardViz");
				Transform Bar = CardViz.transform.GetChild(3 + i); // skip all unused Bar (which is only 2, for some reason) + add 1 to skip Pointer gameobject)

				//spawn my UI
				GameObject soulCounter = OwlCards.instance.Bundle.LoadAsset<GameObject>("OC_UI_SoulCounter");
				GameObject addedUI = Instantiate(soulCounter, canvas.transform);

				//keep track for deletion
				int playerID = player.playerID;
				soulCounters[playerID] = addedUI;

				//keep track of soulChanged to update in its value in real time, store it to be able to delete it once the game is over
				Action<float> handler = (x) => OnSoulValueChanged(playerID, x);
				handlers.Add(handler);
				CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).soulChanged += handler;

				Text text = addedUI.GetComponent<Text>();
				text.text = String.Format(CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul.ToString("F2"));

				// set position
				Vector3 newPos = addedUI.transform.localPosition;
				Canvas gameCanvas = CardViz.transform.parent.GetComponent<Canvas>();
				newPos.y = Bar.localPosition.y * gameCanvas.scaleFactor / canvas.GetComponent<Canvas>().scaleFactor;
				addedUI.transform.localPosition = newPos;
				i++;
			}
		}

		public void OnSoulValueChanged(int playerID, float newValue)
		{
			Text text = soulCounters[playerID].GetComponent<Text>();
			text.text = String.Format(newValue.ToString("F2"));
			if (playerID == playerSoulFillID)
			{
				if (soulFill.activeSelf)
					UpdateFillUI(newValue);
			}
		}

		public void RemoveCounters()
		{
			foreach (int playerID in soulCounters.Keys.ToArray())
			{
				CharacterStatModifiersExtension.GetAdditionalData(Utils.GetPlayerWithID(playerID).data.stats).soulChanged -= handlers[playerID];
				Destroy(soulCounters[playerID]);
			}
			handlers.Clear();
			soulCounters.Clear();
		}
		public void RemoveCanvas()
		{
			RemoveFill();
			RemoveCounters();
			if (canvas)
				Destroy(canvas);
			canvas = null;
			soulCounters = new Dictionary<int, GameObject>();
		}

		public void RemoveFill()
		{
			if (!soulFill)
				return;

			playerSoulFillID = -1;
			Destroy(soulFill);
			soulFill = null;
		}
	}
}
