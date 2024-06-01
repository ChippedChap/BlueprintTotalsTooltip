using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace BlueprintTotalsTooltip.FrameChangeNotifiers
{
	[HarmonyPatch]
	static class FrameWorkedOnDetector
	{
		public static List<Action<Frame>> registeredMethods = new List<Action<Frame>>();

		static MethodBase TargetMethod()
		{
			// This is intended to point to the delegate build.tickAction is set to in MakeNewToils()
			Type innerClass = AccessTools.Inner(typeof(JobDriver_ConstructFinishFrame), "<>c__DisplayClass6_0");
			return AccessTools.Method(innerClass, "<MakeNewToils>b__1");
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instrList = new List<CodeInstruction>(instructions);
			for (int i = 0; i < instrList.Count; i++)
			{
				if (instrList[i].opcode == OpCodes.Stfld && (FieldInfo)instrList[i].operand == typeof(Frame).GetField("workDone") &&
					instrList[i-1].opcode == OpCodes.Add)
				{
					yield return new CodeInstruction(OpCodes.Ldloc_1);
					yield return new CodeInstruction(OpCodes.Call, typeof(FrameWorkedOnDetector).GetMethod("NotifyFrameBeingWorkedOn"));
				}
				yield return instrList[i];
			}
		}

		public static void RegisterMethod(Action<Frame> method) => registeredMethods.Add(method);

		public static void DeregisterMethod(Action<Frame> method)
		{
			if (registeredMethods.Contains(method)) registeredMethods.Remove(method);
		}

		public static void NotifyFrameBeingWorkedOn(Frame frameBeingBuilt)
		{
			foreach (Action<Frame> registeredMethod in registeredMethods) registeredMethod(frameBeingBuilt);
		}
	}
}
