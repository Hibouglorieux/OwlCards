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
		private GameObject canvas;
		private GameObject soulFill;
		private int playerSoulFillID;
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
			//GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, BuildUI);
		}

		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookGameStart, BuildUI);
			GameModeManager.RemoveHook(GameModeHooks.HookGameEnd, ClearUI);
		}

		private IEnumerator BuildUI(IGameModeHandler handler)
		{
			BuildCanvas();
			BuildSoulCounters();
			yield break;
		}
		private IEnumerator ClearUI(IGameModeHandler handler)
		{
			RemoveCounters();
			RemoveFill();
			RemoveCanvas();
			yield break;
		}

		public void BuildCanvas()
		{

			//canvas = GameObject.Find("UI_Game").transform.GetChild(0).gameObject;
			GameObject assetCanvas = OwlCards.instance.Bundle.LoadAsset<GameObject>("OC_UI_Canvas");
			GameObject UI = GameObject.Find("UI_Game");

			canvas = Instantiate(assetCanvas, UI.transform);

		}

		public void UpdateFillUI(float soulValue)
		{
			if (!soulFill)
				return;
			Transform background = soulFill.transform.GetChild(0);
			Image fillImage = background.GetChild(0).GetComponent<Image>();
			Text text = background.GetChild(1).GetComponent<Text>();

			text.text = String.Format(soulValue.ToString("F2") + " Soul");
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
		}

		public void BuildFillUI(Player player)
		{
			//already existant
			if (soulFill)
				return;
			GameObject assetFill = OwlCards.instance.Bundle.LoadAsset<GameObject>("OC_UI_SoulFill");
			if (!canvas)
				BuildCanvas();
			soulFill = Instantiate(assetFill, canvas.transform);
			playerSoulFillID = player.playerID;
			//added UI is a Border image which itself has a Background child, which has two childs Image and text
			UpdateFillUI(CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul);
			CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).soulChanged += UpdateFillUI;
		}

		void Update()
		{
			if (soulFill && CardChoice.instance.pickrID != -1)
			{
				if (playerSoulFillID != CardChoice.instance.pickrID)
				{
					CharacterStatModifiersExtension.GetAdditionalData(Utils.GetPlayerWithID(playerSoulFillID).data.stats).soulChanged -= UpdateFillUI;
					playerSoulFillID = CardChoice.instance.pickrID;
					CharacterStatModifiers stat = Utils.GetPlayerWithID(playerSoulFillID).data.stats;
					UpdateFillUI(CharacterStatModifiersExtension.GetAdditionalData(stat).Soul);
					CharacterStatModifiersExtension.GetAdditionalData(Utils.GetPlayerWithID(playerSoulFillID).data.stats).soulChanged += UpdateFillUI;
				}
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
				Action<float> handler = (x) => UpdateSoulCounterValue(playerID, x);
				handlers.Add(handler);
				CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).soulChanged += handler;

				Text text = addedUI.GetComponent<Text>();
				text.text = String.Format(CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul.ToString("F2"));

				// set position
				// /!\ this is currently bugged for other resolutions than 2k for some reason i don't get
				Vector3 newPos = addedUI.transform.localPosition;
				Canvas gameCanvas = CardViz.transform.parent.GetComponent<Canvas>();
				newPos.y = Bar.localPosition.y * gameCanvas.scaleFactor / canvas.GetComponent<Canvas>().scaleFactor;
				addedUI.transform.localPosition = newPos;
				i++;
			}
		}

		public void UpdateSoulCounterValue(int playerID, float newValue)
		{
			Text text = soulCounters[playerID].GetComponent<Text>();
			text.text = String.Format(newValue.ToString("F2"));
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
			soulFill = null;
			soulCounters = new Dictionary<int, GameObject>();
		}

		public void RemoveFill()
		{
			if (!soulFill)
				return;

			Player player = Utils.GetPlayerWithID(playerSoulFillID);
			if (player)
				CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).soulChanged -= UpdateFillUI;
			Destroy(soulFill);
			soulFill = null;
		}
	}
}
