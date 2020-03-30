using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using Verse.Sound;

namespace BlueprintTotalsTooltip.ChangeDetection
{
	[HarmonyPatch(typeof(WidgetRow))]
	[HarmonyPatch("ToggleableIcon")]
	class PlaySettingsChangeDetector
	{
		private static List<Action> registeredMethods = new List<Action>();

		public static void RegisterMethod(Action method) => registeredMethods.Add(method);

		public static void DeregisterMethod(Action method)
		{
			if (registeredMethods.Contains(method))
				registeredMethods.Remove(method);
		}

		// My first working transpiler patch :)
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var instructionsList = new List<CodeInstruction>(instructions);
			for (int i = 0; i < instructionsList.Count; i++)
			{
				CodeInstruction instruction = instructionsList[i];
				yield return instruction;
				if (instruction.opcode == OpCodes.Call &&
					instruction.operand == typeof(SoundStarter).GetMethod("PlayOneShotOnCamera", new Type[] { typeof(SoundDef), typeof(Map) }))
				{
					yield return new CodeInstruction(OpCodes.Call, typeof(PlaySettingsChangeDetector).GetMethod("NotifyAll"));
				}
			}
		}

		public static void NotifyAll()
		{
			foreach (Action method in registeredMethods)
				method();
		}
	}
}
