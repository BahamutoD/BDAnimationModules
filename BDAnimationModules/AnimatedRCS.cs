using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using UnityEngine;

namespace BDAnimationModules
{
	public class AnimatedRCS : PartModule
	{
		[KSPField(isPersistant = false)]
		public string AnimationName;
		
		private AnimationState[] rcsStates;
		private bool rcsIsOn;
		private bool rcsPartActive;
		
		public override void OnStart(PartModule.StartState state)
		{
			rcsStates = Utils.SetUpAnimation (AnimationName, this.part);
		}
		
		public override void OnUpdate()
		{
			rcsIsOn = this.vessel.ActionGroups.groups[3];
			foreach(ModuleRCS rcs in part.FindModulesImplementing<ModuleRCS>())
			{
				rcsPartActive = rcs.isEnabled;
			}
			
			
			foreach (AnimationState anim in rcsStates)
			{
				if(rcsIsOn && rcsPartActive && anim.normalizedTime<1){anim.speed = 1;}
				if(rcsIsOn && rcsPartActive && anim.normalizedTime>=1)
				{
					anim.speed = 0;
					anim.normalizedTime = 1;
				}
				if((!rcsIsOn||!rcsPartActive) && anim.normalizedTime>0){anim.speed = -1;}
				if((!rcsIsOn||!rcsPartActive) && anim.normalizedTime<=0)
				{
					anim.speed = 0;
					anim.normalizedTime = 0;
				}
			}
				
			
			
		}
		
		
	}
}

