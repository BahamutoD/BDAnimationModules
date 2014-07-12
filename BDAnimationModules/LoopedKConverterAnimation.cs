using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using UnityEngine;
//using System.Reflection;


namespace BDAnimationModules
{
	public class LoopedKConverterAnimation : PartModule
	{
		[KSPField(isPersistant = false)]
		public string DeployingAnimationName;
		[KSPField(isPersistant = false)]
		public string ConvertingAnimationName;
		
		private AnimationState[] deployingStates;
		private AnimationState[] convertingStates;
		
		private bool converting = false;
		private bool convIsEnabled = false;
		
		public Kethane.KethaneConverter thisConverter = null;
		
		
		
		
		public override void OnStart(PartModule.StartState state)
		{
			
			
			Type kethPlugin = Type.GetType ("Kethane.KethaneConverter, Kethane");
			if (kethPlugin != null)
			{
				deployingStates = SetUpAnimation (DeployingAnimationName, this.part);
				convertingStates = SetUpAnimation (ConvertingAnimationName, this.part);
				
				//Kethane.KethaneConverter thisConverter=null;
				foreach (var kc in this.part.FindModulesImplementing<Kethane.KethaneConverter>())
				{
					thisConverter = kc;
					Debug.Log("Found Converter on start");
					break;
				}
				
				if (thisConverter != null && thisConverter.IsEnabled)
				{
					Debug.Log ("Converter started enabled.");
					foreach (AnimationState anim in deployingStates)
					{
						anim.normalizedTime = 1;
						anim.enabled = false;
					}
					foreach (AnimationState anim in convertingStates)
					{
						anim.normalizedTime = 0;
						anim.speed = 1;
						anim.wrapMode = WrapMode.Loop;
					}
				}
				else
				{
					Debug.Log ("Converter started disabled.");
					foreach (AnimationState anim in deployingStates)
					{
						anim.speed = 0;
						anim.normalizedTime = 0;
					}
					foreach (AnimationState anim in convertingStates)
					{
						anim.normalizedTime = 0;
						anim.enabled = false;
					}
				}
			}
		}
		
		public override void OnUpdate()
		{
			Type kethPlugin = Type.GetType ("Kethane.KethaneConverter, Kethane");
			if (kethPlugin != null)
			{
				if(thisConverter == null)
				{
					foreach (var kc in this.part.FindModulesImplementing<Kethane.KethaneConverter>())
					{
						thisConverter = kc;
						Debug.Log("Found Converter in update");
						break;
					}
				}
				
				if (thisConverter!=null&&thisConverter.IsEnabled)
				{
					Debug.Log ("converterEnabled");
					
					foreach(AnimationState anim in deployingStates)
					{
						if (!convIsEnabled)
						{
							convIsEnabled = true;
							anim.enabled = true;
							anim.normalizedTime = 0;
						}
						if (anim.normalizedTime >= 0 && converting == false && anim.normalizedTime <1){anim.speed = 1;}
						else
						{
							anim.enabled = false;
							anim.speed = 0;
							converting = true;
							Debug.Log ("converting = true");
							foreach(AnimationState convAnim in convertingStates)
							{
								convAnim.enabled = true;
								if(convAnim.normalizedTime > 1){convAnim.normalizedTime = 0;}
								convAnim.wrapMode = WrapMode.Loop;
								convAnim.speed = 1;
							}
							
						}
					}
				}
				else
				{
					convIsEnabled = false;
					if (converting==true)
					{
						converting=false;
						foreach(AnimationState anim in convertingStates)
						{
							anim.speed = 0;
							anim.normalizedTime = 0;
							anim.enabled = false;
						}
						foreach(AnimationState anim in deployingStates)
						{
							anim.enabled = true;
							anim.normalizedTime = 1;
						}
		
					}
					
					foreach(AnimationState anim in deployingStates)
					{
						if(anim.normalizedTime <=0){anim.speed = 0;}
						else
						{
							anim.enabled = true;
							anim.speed = -1;
						}
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
                animationState.enabled = false;
                animationState.wrapMode = WrapMode.ClampForever;
                animation.Blend(animationName);
                states.Add(animationState);
				
            }
            return states.ToArray();
        }
		
		
		
	}
}

