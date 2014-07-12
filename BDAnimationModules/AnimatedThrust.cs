using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using UnityEngine;

namespace BDAnimationModules
{
	public class AnimatedThrust : PartModule
	{
		
		[KSPField(isPersistant = false)]
		public string ThrustAnimationName;
		
		[KSPField(isPersistant = false)]
		public bool disableGimbalToggle = false;
		
		private AnimationState[] animStates;
		private ModuleEngines modEng;
		
		
		
		public override void OnStart(PartModule.StartState state)
		{
			animStates = Utils.SetUpAnimation(ThrustAnimationName, this.part);
			
			foreach(ModuleEngines me in this.part.FindModulesImplementing<ModuleEngines>())
			{
				modEng = me;
				break;
			}
			
			if(disableGimbalToggle)
			{
				foreach(ModuleGimbal mgg in this.part.FindModulesImplementing<ModuleGimbal>())
				{
					mgg.Actions["ToggleAction"].active = false;	
				}
			}
			
			
			
		}
		
		public override void OnUpdate()
		{
			
			foreach(AnimationState anim in animStates)
			{
				
				
				anim.normalizedTime = modEng.finalThrust/modEng.maxThrust;
				
			}
			if(disableGimbalToggle)
			{
				foreach(ModuleGimbal mgg in this.part.FindModulesImplementing<ModuleGimbal>())
				{
					mgg.Events["LockGimbal"].active = false;
					mgg.Events["FreeGimbal"].active = false;
				}
			}
		}
	}
}

