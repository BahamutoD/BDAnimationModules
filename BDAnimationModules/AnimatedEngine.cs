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
		private bool userActivated = false;
		private bool engineIsFX = false;

		public override void OnStart(PartModule.StartState state)
		{
			
			try{
				engineStates = SetUpAnimation(EngineAnimationName, this.part);
			}catch(NullReferenceException e){Debug.Log ("NRE while setting up animation: "+e);}


			try{
				foreach (var me in this.part.FindModulesImplementing<ModuleEngines>())
				{
					engineIsOn = me.EngineIgnited;
					modEng = me;
					engineIsFX = false;
				}
			}catch(NullReferenceException e){Debug.Log ("NRE while finding ModuleEngines: "+e);}

			try{
				foreach (var me in this.part.FindModulesImplementing<ModuleEnginesFX>())
				{
					engineIsOn = me.EngineIgnited;
					modEngFX = me;
					engineIsFX = true;
				}
			}catch(NullReferenceException e){Debug.Log ("NRE while finding ModuleEnginesFX: "+e);}

			try{
				if (engineIsOn)
				{
					foreach (var anim in engineStates)
					{
						anim.normalizedTime = 1;
					}
				}
				else
				{
					foreach (var anim in engineStates)
					{
						anim.normalizedTime = 0;
					}
				}
			}catch(NullReferenceException e){Debug.Log ("NRE on anim initial state set: "+e);}


		}
		
		//Original OnUpdate (0.1)
		/**
		public override void OnUpdate()
		{
			bool engineWas = engineIsOn;
			engineIsOn = modEng.EngineIgnited;
			if(engineStates.All(s => s.normalizedTime>=1) || engineStates.All (s => s.normalizedTime<=0))
			{
				foreach(var anim in engineStates){anim.speed = 0;}
			}
			
			if (engineWas != engineIsOn)
			{
				if (engineIsOn)
				{
					foreach (var anim in engineStates)
					{
						anim.normalizedTime = 0;
						anim.speed = 1;
						//Debug.Log ("played forward");
					}
				}
				else
				{
					foreach (var anim in engineStates)
					{
						anim.normalizedTime = 1;
						anim.speed = -1;
						//Debug.Log ("played reverse");
					}
				}
			}
		}
		
		**/
		
		//New OnUpdate (waits till animation before activating Engine)
		
		public override void OnUpdate()
		{
			Debug.Log ("engineIsFX = "+engineIsFX);
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
			}


			
			if(engineIsOn)
			{
				userActivated = true;
				foreach(var anim in engineStates)
				{
					anim.speed = 1;
				}
			}
			foreach(var anim in engineStates)
			{
				if (anim.normalizedTime < WaitForAnimation)
				{
					if(!engineIsFX)
					{
						modEng.Shutdown();
					}
					else
					{
						modEngFX.Shutdown ();
					}
				}
				if (anim.normalizedTime >= WaitForAnimation && userActivated)
				{
					if(!engineIsFX)
					{
						modEng.Activate();
					}
					else
					{
						modEngFX.Activate();
					}
					userActivated = false;
				}
				if (anim.normalizedTime >= WaitForAnimation && !engineIsOn)
				{
					//userActivated = false;
					anim.speed = -1;
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

