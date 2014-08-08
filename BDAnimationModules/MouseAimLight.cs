using System;
using UnityEngine;
using System.Collections.Generic;

namespace BDAnimationModules
{
	public class MouseAimLight : PartModule
	{
		public bool turretEnabled = false;  //user initiated
		public bool deployed = false;  //actual status
		public AnimationState[] deployStates;
		
		private Transform pitchTransform;
		private Transform yawTransform;
		private Vector3 yawAxis;
		private Vector3 pitchAxis;
		private bool turretZeroed = true;  //for knowing if the turret is ready to retract
		private float targetDistance = 0;
		private Vector3 targetPosition;
		private Light spotLight;
		private float spotlightIntensity;
		
		[KSPField(isPersistant = false)]
		public float minPitch = -5;
		[KSPField(isPersistant = false)]
		public float maxPitch = 80;
		[KSPField(isPersistant = false)]
		public float yawRange = -1;
		[KSPField(isPersistant = false)]
		public float rotationSpeed = 2;
		[KSPField(isPersistant = false)]
		public float maxTargetingRange = 2000;
		[KSPField(isPersistant = false)]
		public float resourceAmount = 0.02f;
		
		[KSPField(isPersistant = false)]
		public string deployAnimName = "";
		[KSPField(isPersistant = false)]
		public string yawTransformName = "aimRotate";
		[KSPField(isPersistant = false)]
		public string pitchTransformName = "aimPitch";
		
		
		public bool mouseTracking = true;
		
		[KSPAction("Toggle Position Lock", KSPActionGroup.Light)]
		public void AGToggleTracking(KSPActionParam param)
		{
			mouseTracking = !mouseTracking;
		}
		
		
		[KSPAction("Toggle Light", KSPActionGroup.Light)]
		public void AGToggle(KSPActionParam param)
		{
			toggle();
		}
		
		[KSPEvent(guiActive = true, guiName = "Toggle Light", active = true)]
		public void toggle()
		{
			if(!turretEnabled && part.RequestResource("ElectricCharge", resourceAmount*TimeWarp.fixedDeltaTime)>resourceAmount*TimeWarp.fixedDeltaTime)
			{
				turretEnabled = true;
				mouseTracking = true;
			}
			else
			{
				turretEnabled = false;
			}
		}
	
		
		
		public override void OnStart (PartModule.StartState state)
		{
			if(deployAnimName!="")
			{
				deployStates = SetUpAnimation(deployAnimName, this.part);
			}
			
			spotLight = part.FindModelTransform("light").GetComponent<Light>();
			spotlightIntensity = spotLight.intensity;
			spotLight.intensity = 4;
			
			pitchTransform = part.FindModelTransform(pitchTransformName);
			yawTransform = part.FindModelTransform(yawTransformName);
			yawAxis = new Vector3(0,0,1);
			pitchAxis = new Vector3(0,-1,0);
			
			
			part.force_activate();
		}
		
		public override void OnFixedUpdate ()
		{
			if(!vessel.IsControllable)
			{
				turretEnabled = false;
			}
			
			bool hasPower;
			if(turretEnabled && part.RequestResource("ElectricCharge", resourceAmount * TimeWarp.fixedDeltaTime) >= resourceAmount * TimeWarp.fixedDeltaTime)
			{
				hasPower = true;	
			}
			else
			{
				hasPower = false;
				//turretEnabled = false;
			}
			
			
			if(deployAnimName!="")
			{
				foreach(AnimationState anim in deployStates)
				{
					//animation clamping
					if(anim.normalizedTime>1)
					{
						anim.speed = 0;
						anim.normalizedTime = 1;
					}
					if(anim.normalizedTime < 0)
					{
						anim.speed = 0;
						anim.normalizedTime = 0;
					}
					
					//deploying
					if(turretEnabled)
					{
						
						
						if(anim.normalizedTime<1 && anim.speed<1)
						{
							anim.speed = 1;	
							deployed = false;
						}
						if(anim.normalizedTime == 1)
						{
							deployed = true;
							anim.enabled = false;
						}
					}
					
					//retracting
					if(!turretEnabled)
					{
						spotLight.intensity = Mathf.MoveTowards(spotLight.intensity, 0, 3*spotlightIntensity * TimeWarp.fixedDeltaTime);
						
						
						deployed = false;
						ReturnTurret();
					
						if(turretZeroed)
						{
							anim.enabled = true;
							if(anim.normalizedTime > 0 && anim.speed > -1)
							{
								anim.speed = -1;	
							}
						}
					}
				}
			}
			else
			{
				deployed = turretEnabled;	
			}
			
			if(hasPower && deployed && (TimeWarp.WarpMode!=TimeWarp.Modes.HIGH || TimeWarp.CurrentRate == 1))
			{
				if(mouseTracking) Aim ();	
				spotLight.intensity = Mathf.MoveTowards(spotLight.intensity, spotlightIntensity, spotlightIntensity * TimeWarp.fixedDeltaTime);
			}
			if(!hasPower)
				spotLight.intensity = Mathf.MoveTowards(spotLight.intensity, 0, 3*spotlightIntensity * TimeWarp.fixedDeltaTime);
			//emissive
			float colorSet = spotLight.intensity/spotlightIntensity;
			part.FindModelTransform("lightPlane").renderer.material.SetColor("_EmissiveColor", new Color(colorSet,colorSet,colorSet,1));
			
		}
		
		
		
		private void Aim()
		{
			Vector3 target = Vector3.zero;
			Vector3 targetYawOffset;
			Vector3 targetPitchOffset;
			
			//auto target tracking
			
			Vessel targetVessel = null;
			if(vessel.targetObject!=null)
			{
				targetVessel = vessel.targetObject.GetVessel();
			}
			if(targetVessel!=null)
			{
				target = targetVessel.transform.position;
			}
			
			//
			
			if (target == Vector3.zero)  //if no target from vessel Target, use mouse aim
			{
				Vector3 mouseAim = new Vector3(Input.mousePosition.x/Screen.width, Input.mousePosition.y/Screen.height, 0);
				Ray ray = FlightCamera.fetch.mainCamera.ViewportPointToRay(mouseAim);
				
				RaycastHit hit;
				
				
				if(Physics.Raycast(ray, out hit, maxTargetingRange, 557057))
				{
					target = hit.point;
					try{
						Part p = Part.FromGO(hit.rigidbody.gameObject);
						if(p.vessel == this.vessel)
						{
							target = ray.direction * maxTargetingRange + FlightCamera.fetch.mainCamera.transform.position;		
						}
					}catch(NullReferenceException){}
					
				}else
				{
					target = ray.direction * maxTargetingRange + FlightCamera.fetch.mainCamera.transform.position;	
				}
			}
			targetDistance = (target - part.transform.position).magnitude;
			spotlightIntensity = Mathf.Clamp(0.005f * targetDistance, 1, 4.5f);
			targetYawOffset = yawTransform.position - target;
			targetYawOffset = Quaternion.Inverse(yawTransform.rotation) * targetYawOffset; //sets offset relative to the turret's rotation
			targetYawOffset = Quaternion.AngleAxis(90, yawAxis) * targetYawOffset; //fix difference in coordinate system.
			
			targetPitchOffset = pitchTransform.position - target;
			targetPitchOffset = Quaternion.Inverse(pitchTransform.rotation) * targetPitchOffset;
			targetPitchOffset = Quaternion.AngleAxis(180, pitchAxis) * targetPitchOffset;
			
			
			float rotationSpeedYaw = Mathf.Clamp (Mathf.Abs (10*targetYawOffset.x/Mathf.Clamp (targetDistance,10, maxTargetingRange)), 0, rotationSpeed);
			float rotationSpeedPitch = Mathf.Clamp (Mathf.Abs (10*targetPitchOffset.z/Mathf.Clamp (targetDistance,10, maxTargetingRange)), 0, rotationSpeed);
			
			targetPosition = target;
			
			if(TimeWarp.WarpMode!=TimeWarp.Modes.HIGH || TimeWarp.CurrentRate == 1)
			{
				//yaw movement
				if(yawRange >= 0)
				{
					
					float minYaw = -(yawRange/2);
					float maxYaw = yawRange/2;
					if(targetYawOffset.x > 0 && yawTransform.localRotation.Roll () < maxYaw*Mathf.Deg2Rad)
					{
						yawTransform.localRotation *= Quaternion.AngleAxis(rotationSpeedYaw, yawAxis);
					}
					else if(targetYawOffset.x < 0 && yawTransform.localRotation.Roll () > minYaw*Mathf.Deg2Rad)
					{
						yawTransform.localRotation *= Quaternion.AngleAxis(-rotationSpeedYaw, yawAxis);
					}
					
					
					
				}
				else{
					if(targetYawOffset.x > 0)
					{
						yawTransform.localRotation *= Quaternion.AngleAxis(rotationSpeedYaw, yawAxis);
					}
					if(targetYawOffset.x < 0)
					{
						yawTransform.localRotation *= Quaternion.AngleAxis(-rotationSpeedYaw, yawAxis);
					}
				}
				
				
			
			
				//pitch movement
				if(targetPitchOffset.z > 0 && pitchTransform.localRotation.Yaw ()>-maxPitch*Mathf.Deg2Rad)
				{
					pitchTransform.localRotation *= Quaternion.AngleAxis(rotationSpeedPitch, pitchAxis);
				}
				else if(targetPitchOffset.z < 0 && pitchTransform.localRotation.Yaw () < - minPitch*Mathf.Deg2Rad)
				{
					pitchTransform.localRotation *= Quaternion.AngleAxis(-rotationSpeedPitch, pitchAxis);
				}
				
			}
		}
		
		//returns turret to default position when turned off
		private void ReturnTurret()
		{
			float returnSpeed = Mathf.Clamp (rotationSpeed, 0.1f, 6f);
			bool yawReturned = false;
			bool pitchReturned = false;
			turretZeroed = false;
			float yaw = yawTransform.localRotation.Roll() * Mathf.Rad2Deg;
			float pitch = pitchTransform.localRotation.Yaw () * Mathf.Rad2Deg;
			
			//Debug.Log ("Pitch: "+pitch*Mathf.Rad2Deg+", Yaw: "+yaw*Mathf.Rad2Deg);
			
			if(yaw > 1 || yaw < -1)
			{
				if(yaw > 0)
				{
					yawTransform.localRotation *= Quaternion.AngleAxis(-Mathf.Clamp(Mathf.Abs (yaw)/2, 0.01f, returnSpeed), yawAxis);
				}
				if(yaw < 0)
				{
					yawTransform.localRotation *= Quaternion.AngleAxis(Mathf.Clamp(Mathf.Abs (yaw)/2, 0.01f, returnSpeed), yawAxis);
				}
			}
			else
			{
				yawReturned = true;
			}
			
			if(pitch > 1 || pitch < -1)
			{
				if(pitch > 0)
				{
					pitchTransform.localRotation *= Quaternion.AngleAxis(Mathf.Clamp(Mathf.Abs (pitch)/2, 0.01f, returnSpeed), pitchAxis);
				}
				if(pitch < 0)
				{
					pitchTransform.localRotation *= Quaternion.AngleAxis(-Mathf.Clamp(Mathf.Abs (pitch)/2, 0.01f, returnSpeed), pitchAxis);
				}
			}
			else
			{
				pitchReturned = true;
			}
			if(yawReturned && pitchReturned)
			{
				yawTransform.localRotation = Quaternion.Euler(0,0,0);
				pitchTransform.localRotation = Quaternion.Euler(0,0,0);
				turretZeroed = true;
			}
			
		}
		
		
		//Animation Setup
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

