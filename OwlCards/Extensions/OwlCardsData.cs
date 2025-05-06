using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnboundLib.Networking;
using UnboundLib;
using Photon.Pun;

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

				GetData(playerID).Soul = newSoulValue;
			}
		}

		public static void UpdateSoul(int[] playersIDs, float[] newSoulValues)
		{
			object[] obj = { playersIDs, newSoulValues };
			UpdateSoul_RPC(playersIDs, newSoulValues);
			if (!PhotonNetwork.OfflineMode)
				NetworkingManager.RPC_Others(typeof(OwlCardsData), nameof(UpdateSoul_RPC), obj);
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
				soulPropertySetter.Invoke(additionalData, new object[] {OwlCards.instance.soulOnGameStart.Value});
			}

			var rerollsField = AccessTools.Field(typeof(OwlCardsData), "_rerolls");
			rerollsField.SetValue(additionalData, 0);
		}
	}
}
