using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//AnimatedContainer allows an animation to correspond with the percentage of a particular resource in a container.

namespace BDAnimationModules
{
	public class AnimatedContainer : PartModule
	{
		[KSPField(isPersistant = false)]
		public string ContainerAnimationName;
		
		[KSPField(isPersistant = false)]
		public string ResourceType;
		
		private AnimationState[]  containerStates;
		private PartResource animatedResource;
		private double resAmount;
		private double resMax;
		private double normalizedRes;
		
		
	
		
		public override void OnStart(PartModule.StartState state)
		{
		
			containerStates = SetUpAnimation(ContainerAnimationName, this.part);
			
			foreach(PartResource pr in part.Resources)  //finds which resource has been specified
			{
				if (pr.resourceName.Equals (ResourceType))
				{	
					animatedResource = pr;
					break;
				}		
			}
			
			
			resMax = animatedResource.maxAmount;
			resAmount = animatedResource.amount;
			normalizedRes = resAmount/resMax;
			foreach (var cs in containerStates)
			{
				cs.normalizedTime = (float)normalizedRes;
			}
			
			
			
		}
		
		public override void OnUpdate()
		{
			
			resAmount = animatedResource.amount;
			resMax = animatedResource.maxAmount;
			normalizedRes = resAmount/resMax;
			
			foreach (var cs in containerStates)
			{
				cs.normalizedTime = (float)normalizedRes;
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
	
	[KSPAddon(KSPAddon.Startup.EditorAny, false)]
	public class AnimContainerEditorUpdate : MonoBehaviour
	{
		void Update()
		{
			if(HighLogic.LoadedSceneIsEditor)
			{
				foreach(Part p in EditorLogic.fetch.ship.Parts)
				{
					foreach(AnimatedContainer ac in p.FindModulesImplementing<AnimatedContainer>())
					{
						ac.OnUpdate();	
					}
				}
			}
		}
	}
}

