using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using UnityEngine;
//AnimatedEngine allows an animation to be played while activating/deactivating an engine.

namespace BDAnimationModules
{
	public class AnimatedEngine : PartModule
	{
		[KSPField(isPersistant = false)]
		public string EngineAnimationName;
		
		[KSPField(isPersistant = false)]
		public float WaitForAnimation = 0;
		
		private bool engineIsOn = false;
		private ModuleEngines modEng = null;
		private ModuleEnginesFX modEngFX = null;
		private AnimationState[]  engineStates;
		private bool engineIsFX = false;
		

		public override void OnStart(PartModule.StartState state)
		{
			
			engineStates = SetUpAnimation(EngineAnimationName, this.part);
		
			foreach (var me in this.part.FindModulesImplementing<ModuleEngines>())
			{
				engineIsOn = me.EngineIgnited;
				modEng = me;
				engineIsFX = false;
			}
		
			foreach (var me in this.part.FindModulesImplementing<ModuleEnginesFX>())
			{
				engineIsOn = me.EngineIgnited;
				modEngFX = me;
				engineIsFX = true;
			}
			

			foreach(var anim in engineStates)
			{
				if (engineIsOn)
				{
						anim.normalizedTime = 1;
				}
				else
				{
						anim.normalizedTime = 0;
				}
			}
			
		}
		
		
		
		public override void OnUpdate()
		{
			if(!engineIsFX)
			{
				engineIsOn = modEng.EngineIgnited;
			}
			else
			{
				engineIsOn = modEngFX.EngineIgnited;
			}
			

			foreach(var anim in engineStates)
			{
				
				
				if(engineIsOn && anim.normalizedTime < WaitForAnimation)
				{
					anim.speed = 1;
					if(engineIsFX) modEngFX.Shutdown();
					else modEng.Shutdown();
					
				}
				
				
				if(anim.normalizedTime >= WaitForAnimation && anim.speed > 0)
				{
					if(engineIsFX) modEngFX.Activate();
					else modEng.Activate();	
				}
				
				if(anim.normalizedTime>=1)
				{
					anim.speed = 0;
					anim.normalizedTime = 1;
				}
				
				if(anim.normalizedTime >=1 && !engineIsOn)
				{
					anim.speed = -1;
					
				}
				
				if(anim.normalizedTime <0)
				{
					anim.speed = 0;
					anim.normalizedTime = 0;
				}
				
				
			}
			
		}
		
		
		
		
		
		public static AnimationState[] SetUpAnimation(string animationName, Part part)  //Thanks Majiir!
        {
            var states = new List<AnimationState>();
            foreach (var animation in part.FindModelAnimators(animationName))
            {
                var animationState = animation[animationName];
                animationState.speed = 0;
                animationState.enabled = true;
                animationState.wrapMode = WrapMode.ClampForever;
                animation.Blend(animationName);
                states.Add(animationState);
				
            }
            return states.ToArray();
        }
	}
}

