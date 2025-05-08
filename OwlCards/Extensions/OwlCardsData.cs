using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnboundLib.Networking;
using UnboundLib;
using Photon.Pun;
using UnityEngine;

namespace OwlCards.Extensions
{
	[Serializable]
	public class OwlCardsData
	{
		private float _soul;
		public float Soul
		{
			get { return _soul; }
			private set
			{
				if (_soul != value)
				{
					_soul = value;
					soulChanged?.Invoke(value);
				}
			}
		}
		public float soulConsumptionFactor;
		public event Action<float> soulChanged;

		private int _rerolls;
		public int Rerolls { get => _rerolls; }

		[UnboundRPC]
		private static void UpdateSoul_RPC(int[] playersIDs, float[] newSoulValues)
		{
			for (int i = 0; i < playersIDs.Length; i++)
			{
				int playerID = playersIDs[i];
				float newSoulValue = newSoulValues[i];

				OwlCardsData data = GetData(playerID);
				data.Soul = newSoulValue;

				// update hand size if needed
				if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
				{
					OwlCards.Log("Checking if handsize must be updated");
					int oldDrawValue = DrawNCards.DrawNCards.GetPickerDraws(playerID);
					int newDrawValue = oldDrawValue;

					Action<int, int> updateHandSizeWithSoulValue = (int bound, int handSizeChange) =>
					{
						if (data.Soul < bound && newSoulValue >= bound)
							newDrawValue += handSizeChange;
						if (data.Soul >= bound && newSoulValue < bound)
							newDrawValue -= handSizeChange;
					};

					updateHandSizeWithSoulValue(0, 2);
					updateHandSizeWithSoulValue(2, 1);
					updateHandSizeWithSoulValue(4, 1);

					newDrawValue = Mathf.Clamp(newDrawValue, 1, 30);
					if (oldDrawValue != newDrawValue)
					{
						OwlCards.Log("updated hand size with old value being: " + oldDrawValue);
						DrawNCards.DrawNCards.RPCA_SetPickerDraws(playerID, newDrawValue);
						DrawNCards.DrawNCards.SetPickerDraws(playerID, (newDrawValue));
						OwlCards.Log("updated hand size with NEW value being: " + newDrawValue);
					}
				}
			}
		}

		public static void UpdateSoul(int[] playersIDs, float[] newSoulValues)
		{
			if (!(PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient))
			{
				OwlCards.Log("UpdateSoul called from client instead of masterClient !");
				return;
			}
			object[] obj = { playersIDs, newSoulValues };
			UpdateSoul_RPC(playersIDs, newSoulValues);
			if (!PhotonNetwork.OfflineMode)
				NetworkingManager.RPC_Others(typeof(OwlCardsData), nameof(UpdateSoul_RPC), obj);
		}

		// meant to be called by clients to request an update to the master client
		public static void RequestUpdateSoul(int[] playersIDs, float[] diff)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
				UpdateSoul(playersIDs, GetUpdatedSoulValueFromDiff(playersIDs, diff));
			else
				NetworkingManager.RPC_Others(typeof(OwlCardsData), nameof(RequestUpdateSoul_RPC), new object[] { playersIDs, diff });
		}

		private static float[] GetUpdatedSoulValueFromDiff(int[] playersIDs, float[] diff)
		{
			float[] newSoulValues = new float[playersIDs.Length];
			for (int i = 0; i < newSoulValues.Length; i++)
				newSoulValues[i] = GetData(playersIDs[i]).Soul + diff[i];
			return newSoulValues;
		}

		[UnboundRPC]
		private static void RequestUpdateSoul_RPC(int[] playersIDs, float[] diff)
		{
			if (!PhotonNetwork.IsMasterClient) return;
			UpdateSoul(playersIDs, GetUpdatedSoulValueFromDiff(playersIDs, diff));
		}

		public static void UpdateSoul(int playerID, float newSoulValue)
		{
			UpdateSoul(new int[] { playerID }, new float[] { newSoulValue });
		}
		public static OwlCardsData GetData(int playerID)
		{
			return GetData(Utils.GetPlayerWithID(playerID));
		}

		public static OwlCardsData GetData(Player player)
		{
			return CharacterStatModifiersExtension.GetAdditionalData(player.data.stats);
		}

		public OwlCardsData()
		{
			_soul = OwlCards.instance.soulOnGameStart.Value;
			_rerolls = 0;
			soulConsumptionFactor = 1.0f;
		}
	}

	public static class CharacterStatModifiersExtension
	{
		public static readonly ConditionalWeakTable<CharacterStatModifiers, OwlCardsData> data =
			new ConditionalWeakTable<CharacterStatModifiers, OwlCardsData>();

		public static OwlCardsData GetAdditionalData(this CharacterStatModifiers statModifiers)
		{
			return data.GetOrCreateValue(statModifiers);
		}

		public static void AddData(this CharacterStatModifiers statModifiers, OwlCardsData value)
		{
			try
			{
				data.Add(statModifiers, value);
			}
			catch (Exception) { }
		}
	}

	[HarmonyPatch(typeof(CharacterStatModifiers), "ResetStats")]
	class CharacterStatModifiersPatchResetStats
	{
		private static void Prefix(CharacterStatModifiers __instance)
		{
			OwlCardsData additionalData = __instance.GetAdditionalData();

			if (additionalData.Soul != OwlCards.instance.soulOnGameStart.Value)
			{
				var soulPropertySetter = AccessTools.PropertySetter(typeof(OwlCardsData), "Soul");
				soulPropertySetter.Invoke(additionalData, new object[] { OwlCards.instance.soulOnGameStart.Value });
			}

			var rerollsField = AccessTools.Field(typeof(OwlCardsData), "_rerolls");
			rerollsField.SetValue(additionalData, 0);

			additionalData.soulConsumptionFactor = 1.0f;
		}
	}
}
