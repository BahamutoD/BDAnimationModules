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
		private ModuleEnginesFX modEngFX;
		public bool isEngineFX = false;
		
		[KSPField(isPersistant = true, guiActive = true, guiName = "Gimbal Active", guiActiveEditor = true)]
		public bool gimbalStatus = true;
		
		[KSPAction("Toggle Gimbal")]
		public void AGToggleGimbal(KSPActionParam param)
		{
			foreach(ModuleGimbal mgg in this.part.FindModulesImplementing<ModuleGimbal>())
			{
				mgg.gimbalLock = !mgg.gimbalLock;	
				gimbalStatus = !mgg.gimbalLock;
			}
		}
		
		
		
		[KSPEvent(guiActive = false, guiName = "Toggle Gimbal", active = true)]
		public void GuiToggleGimbal()
		{
			foreach(ModuleGimbal mgg in this.part.FindModulesImplementing<ModuleGimbal>())
			{
				mgg.gimbalLock = !mgg.gimbalLock;	
				gimbalStatus = !mgg.gimbalLock;

			}
		}
		
		
		public override void OnStart(PartModule.StartState state)
		{
			animStates = Utils.SetUpAnimation(ThrustAnimationName, this.part);
			
			foreach(ModuleEngines me in this.part.FindModulesImplementing<ModuleEngines>())
			{
				modEng = me;
				isEngineFX = false;
				break;
			}
			
			foreach(ModuleEnginesFX me in this.part.FindModulesImplementing<ModuleEnginesFX>())
			{
				modEngFX = me;
				isEngineFX = true;
				break;
			}
			HideGimbalButtons();
			part.OnEditorAttach += new Callback(HideGimbalButtons);
			if(disableGimbalToggle)
			{
				Actions["AGToggleGimbal"].active = true;
				Events["GuiToggleGimbal"].guiActive = true;
				Events["GuiToggleGimbal"].guiActiveEditor = true;

				foreach(ModuleGimbal mgg in this.part.FindModulesImplementing<ModuleGimbal>())
				{
					gimbalStatus = !mgg.gimbalLock;
				}
			}
			else
			{
				Actions["AGToggleGimbal"].active = false;
			}
			
		}
		
		public override void OnUpdate()
		{
			if(!isEngineFX)
			{
				foreach(AnimationState anim in animStates)
				{
					anim.normalizedTime = modEng.finalThrust/modEng.maxThrust;
				}
				
			}
			else
			{
				foreach(AnimationState anim in animStates)
				{
					anim.normalizedTime = modEngFX.finalThrust/modEngFX.maxThrust;
				}
			}

			/*
			if(disableGimbalToggle)
			{
				foreach(ModuleGimbal mgg in this.part.FindModulesImplementing<ModuleGimbal>())
				{
					mgg.Events["LockGimbal"].active = false;
					mgg.Events["FreeGimbal"].active = false;
				}
			}
			*/
			
			
		}
		
		void HideGimbalButtons()
		{
			if(disableGimbalToggle)
			{
				foreach(ModuleGimbal mgg in this.part.FindModulesImplementing<ModuleGimbal>())
				{
					mgg.Actions["FreeAction"].active = false;	
					mgg.Actions["LockAction"].active = false;
					mgg.Actions["ToggleAction"].active = false;
					mgg.Fields["gimbalLock"].guiActive = false;
					mgg.Fields["gimbalLock"].guiActiveEditor = false;
					mgg.Fields["gimbalLimiter"].guiActive = false;
					mgg.Fields["gimbalLimiter"].guiActiveEditor = false;
					//mgg.Events["LockGimbal"].active = false;
					//mgg.Events["LockGimbal"].guiActiveEditor = false;
					//mgg.Events["FreeGimbal"].active = false;
					//mgg.Events["FreeGimbal"].guiActiveEditor = false;
				}
			}
			else
			{
				Actions["AGToggleGimbal"].active = false;
				Events["GuiToggleGimbal"].guiActive = false;
				Events["GuiToggleGimbal"].guiActiveEditor = false;
				Events["GuiToggleGimbal"].guiActiveUnfocused = false;
			}
		}
	}
}

