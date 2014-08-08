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
		
		private float oMaxThrust;
		private float oMinThrust;

		public override void OnStart(PartModule.StartState state)
		{
			
			try{
				engineStates = SetUpAnimation(EngineAnimationName, this.part);
			}catch(NullReferenceException e){Debug.Log ("NRE while setting up animation: "+e);}


			
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
			
			if(modEng!=null)
			{
				oMaxThrust = modEng.maxThrust;
				oMinThrust = modEng.minThrust;
				modEng.maxThrust = 0;
				modEng.minThrust = 0;
			}
			if(modEngFX!=null)
			{
				oMaxThrust = modEngFX.maxThrust;
				oMinThrust = modEngFX.minThrust;
				modEngFX.maxThrust = 0;
				modEngFX.minThrust = 0;
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
				if(anim.normalizedTime>=1)
				{
					anim.speed = 0;
					anim.normalizedTime = 1;
				}
				if(anim.normalizedTime <=0)
				{
					anim.speed = 0;
					anim.normalizedTime = 0;
				}
			

				if(engineIsOn)
				{
					anim.speed = 1;
				}
				else
				{
					anim.speed = -1;
				}
				
			
				if (anim.normalizedTime < WaitForAnimation && anim.normalizedTime > 0)
				{
					if(!engineIsFX)
					{
						modEng.maxThrust = 0;
						modEng.minThrust = 0;
					}
					else
					{
						modEngFX.maxThrust = 0;
						modEng.minThrust = 0;
					}
				}
				if (anim.normalizedTime >= WaitForAnimation)
				{
					if(!engineIsFX)
					{
						modEng.maxThrust = oMaxThrust;
						modEng.minThrust = oMinThrust;
					}
					else
					{
						modEngFX.maxThrust = oMaxThrust;
						modEngFX.minThrust = oMinThrust;
					}
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

