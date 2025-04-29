using UnboundLib;
using TMPro;
using UnboundLib.Cards;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration; // load and save data
using UnityEngine;
using UnityEngine.Events;
using UnboundLib.Utils.UI;
using UnboundLib.GameModes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Photon.Compression;
using System;
using System.Linq;
using OwlCards.Cards;

namespace OwlCards
{
	// These are the mods required for our mod to work
	[BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
	// Declares our mod to Bepin
	[BepInPlugin(ModId, ModName, Version)]
	// The game our mod is associated with
	[BepInProcess("Rounds.exe")]
	public class OwlCards : BaseUnityPlugin
	{
		private const string ModId = "com.Hibou.Glorieux.OwlCards";
		public const string ModName = "OwlCards";
		public const string Version = "0.1.0"; // What version are we on (major.minor.patch)?

		public const string ModInitials = "OWL";
		private const string LogPrefix = ModName + ": ";

		public static OwlCards instance {get; private set;}

		public readonly AssetBundle Bundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("firstmodtest", typeof(OwlCards).Assembly);

		public ConfigEntry<float> soulOnGameStart;
		public ConfigEntry<float> soulGainedPerRound;
		public ConfigEntry<float> rerollPointsPerPointWon;

		public ConfigEntry<float> rerollSoulCost;
		public ConfigEntry<float> extraPickSoulCost;
		private List<int> pointWinnersID = new List<int>();

		void Awake()
        {
			instance = this;

			soulGainedPerRound = Config.Bind(ModName, nameof(soulGainedPerRound), 0.5f, "How much soul resource is earned passively each round");
			rerollPointsPerPointWon = Config.Bind(ModName, nameof(rerollPointsPerPointWon), 0.25f, "How much soul resource is earned passively each round");
			soulOnGameStart = Config.Bind(ModName, nameof(soulOnGameStart), 1.0f, "How much soul you have when a game starts");

			rerollSoulCost = Config.Bind(ModName, nameof(rerollSoulCost), 1.0f, "how much soul does it cost to reroll");
			extraPickSoulCost = Config.Bind(ModName, nameof(extraPickSoulCost), 3.0f, "how much soul does it cost to do an extra pick");

            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

        }
        void Start()
        {
			BuildCards();

			ModdingUtils.Utils.Cards.instance.AddCardValidationFunction(OwlCardValidation);
			// Spawns Menu
			OptionMenu.CreateMenu();

			// spawn specialized component
			gameObject.AddComponent<RerollButton>();
			gameObject.AddComponent<UI.Manager>();

			GameModeManager.AddHook(GameModeHooks.HookRoundEnd, UpdatePlayerResourcesRoundEnd);
			GameModeManager.AddHook(GameModeHooks.HookPointEnd, TrackPointWinners);
        }

		private IEnumerator TrackPointWinners(IGameModeHandler gm)
		{
			int[] newPointWinners = gm.GetPointWinners();
			foreach (int newPointWinner in newPointWinners)
				pointWinnersID.Add(newPointWinner);
			yield break;
		}

		private bool OwlCardValidation(Player player, CardInfo cardInfo)
		{
			if (cardInfo.categories.Contains(OwlCardCategory.modCategory))
				if (AOwlCard.conditions.ContainsKey(cardInfo.cardName))
				{
					bool bCardValidated = AOwlCard.conditions[cardInfo.cardName]
						(Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul);
					OwlCards.Log("Card: " + cardInfo.cardName + " isValidated: "  + bCardValidated);
					return bCardValidated;
				}
			return true;
		}
		private void BuildCards()
		{
			// TODO what to do actually...?
			//CustomCard.BuildCard<Cards.Blahaj>();
			//CustomCard.BuildCard<Cards.LetheRapide>();
			//CustomCard.BuildCard<Cards.Lethe>();

			// Debug/Test
			CustomCard.BuildCard<Cards.Soul>();
			CustomCard.BuildCard<Cards.Soulless>();

			// Should be ok
            CustomCard.BuildCard<Cards.SoulLeech>();
            CustomCard.BuildCard<Cards.FeedMe>();
            CustomCard.BuildCard<Cards.FunKiller>();
            CustomCard.BuildCard<Cards.LastHitter>();

			// TODO need to be tested
            CustomCard.BuildCard<Cards.SoulExhaustion>();
		}

		private IEnumerator UpdatePlayerResourcesRoundEnd(IGameModeHandler gm)
		{
			int[] winningPlayersID = gm.GetRoundWinners();

			// passive gain
			foreach (Player player in PlayerManager.instance.players.ToArray())
			{
				if (!winningPlayersID.Contains(player.playerID))
					Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul += soulGainedPerRound.Value;
			}

			// gain per point won
			float rerollEarnedWithPoints = 0;
			foreach (int pointWinner in pointWinnersID)
			{
				if (!winningPlayersID.Contains(pointWinner))
				{
					Player player = Utils.GetPlayerWithID(pointWinner);
					Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul += rerollPointsPerPointWon.Value;
					rerollEarnedWithPoints += rerollPointsPerPointWon.Value;
				}
			}
			pointWinnersID.Clear();
			Log("End of round total points earned with Points won: " + rerollEarnedWithPoints);
			yield break;
		}

		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookRoundEnd, UpdatePlayerResourcesRoundEnd);
			GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, TrackPointWinners);
		}

		static public void Log(string msg)
		{
			/*
			UnityEngine.Debug.Log(LogPrefix + msg);
			*/
		}
    }
}
