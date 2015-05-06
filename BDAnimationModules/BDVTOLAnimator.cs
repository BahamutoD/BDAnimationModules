using System;
using UnityEngine;

namespace BDAnimationModules
{
	public class BDVTOLAnimator : PartModule
	{
		[KSPField(isPersistant = false)]
		public string animationName;
		
		[KSPField(isPersistant = false)]
		public float maxAngle = 90;
		
		[KSPField(isPersistant = true)]
		public float targetAngle = 90;
		
		[KSPField(isPersistant = true)]
		public float defaultAngle = 0;
		
		[KSPField(isPersistant = false)]
		public bool useAnimation = true;
		
		public float currentAngle = 0;
		
		public Vector3 thrustPosition = Vector3.zero;
		
		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "VTOL Mode"), 
			UI_Toggle(disabledText = "Off", enabledText = "On")]
		public bool vtolModeEnabled = false;
		
		public AnimationState[] deployStates;
		
		public override void OnStart (PartModule.StartState state)
		{
		
			if(useAnimation)
			{
				deployStates = Utils.SetUpAnimation(animationName, part);
				
				foreach(var anim in deployStates)
				{
					if(vtolModeEnabled)
					{
						anim.normalizedTime = targetAngle/maxAngle;	
					}
					
					currentAngle = anim.normalizedTime * maxAngle;
				}
			}
		}
		
		void FixedUpdate()
		{
			
			
			if(useAnimation)
			{
				foreach(var anim in deployStates)
				{
					currentAngle = anim.normalizedTime * maxAngle;
					
					if(vtolModeEnabled && currentAngle!=targetAngle) //vtol mode is on and engine is NOT at target angle
					{
						anim.speed = (targetAngle>currentAngle ? 1 : -1);
					}
					else if(vtolModeEnabled)  //vtol mode is on and engine is at target angle
					{
						anim.speed = 0;	
						anim.normalizedTime = targetAngle/maxAngle;
					}
					else if(currentAngle!=defaultAngle) //vtol mode is of and engine is NOT at default angle
					{
						anim.speed = (defaultAngle>currentAngle ? 1 : -1);
					}
					else //vtol mode is off and engine is at default angle
					{
						anim.speed = 0;
						anim.normalizedTime = 0;
					}
				}	
			}
			
			
			thrustPosition = part.FindModelTransform(part.FindModuleImplementing<ModuleEngines>().thrustVectorTransformName).position;
		}
	}
}

