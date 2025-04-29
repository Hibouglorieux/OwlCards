using System;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace OwlCards.Extensions
{
	[Serializable]
	public class CharacterStatModifiersAdditionalData
	{
		private float _soul;
		public float Soul { get { return _soul; } set
			{
				soulChanged?.Invoke(_soul, value);
				_soul = value;
			}
		}

		public event Action<float, float> soulChanged;

		public CharacterStatModifiersAdditionalData()
		{
			Soul = OwlCards.instance.soulOnGameStart.Value;
		}
	}

		public static class CharacterStatModifiersExtension
		{
			public static readonly ConditionalWeakTable<CharacterStatModifiers, CharacterStatModifiersAdditionalData> data =
				new ConditionalWeakTable<CharacterStatModifiers, CharacterStatModifiersAdditionalData>();

			public static CharacterStatModifiersAdditionalData GetAdditionalData(this CharacterStatModifiers statModifiers)
			{
				return data.GetOrCreateValue(statModifiers);
			}

			public static void AddData(this CharacterStatModifiers statModifiers, CharacterStatModifiersAdditionalData value)
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
				__instance.GetAdditionalData().Soul = OwlCards.instance.soulOnGameStart.Value;
			}
		}
}
