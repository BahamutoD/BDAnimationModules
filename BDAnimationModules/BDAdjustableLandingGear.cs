using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BDAnimationModules
{
	public enum GearStates{Deployed, Deploying, Retracted, Retracting}
	
	public class BDAdjustableLandingGear : PartModule
	{
		
		//tweakables
		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Spring"),
        	UI_FloatRange(minValue = 1f, maxValue = 100f, stepIncrement = 1f, scene = UI_Scene.Editor)]
		public float suspensionSpring = 35;
		
		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Damper"),
        	UI_FloatRange(minValue = 0f, maxValue = 20f, stepIncrement = 0.5f, scene = UI_Scene.Editor)]
		public float suspensionDamper = 10;
		
		/*
		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Brake Torque"),
        	UI_FloatRange(minValue = 0f, maxValue = 300f, stepIncrement = 1f, scene = UI_Scene.Editor)]
		public float brakeTorque = 50;
		*/
		
		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Wheel Height"),
        	UI_FloatRange(minValue = 0f, maxValue = 120f, stepIncrement = 0.01f, scene = UI_Scene.Editor)]
		float wheelHeight = 0;
		
		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Wheel Angle"),
        	UI_FloatRange(minValue = 0f, maxValue = 60f, stepIncrement = 1f, scene = UI_Scene.Editor)]
		public float wheelAngle = 0;
		
		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Leg Angle"),
        	UI_FloatRange(minValue = 0f, maxValue = 60f, stepIncrement = 1f, scene = UI_Scene.Editor)]
		public float legAngle = 0;
		
		JointSpring jointSpring = new JointSpring();
		
		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Adjustment: "), 
			UI_Toggle(disabledText = "Show", enabledText = "Hide")]
		bool showSettings = false;
		bool showSettingsPrev = false;
		
		
		//transforms
		[KSPField(isPersistant = false)]
		public string boundsCollider;
			
		[KSPField(isPersistant = false)]
		public float doorSpeed = 20;
		
		[KSPField(isPersistant = false)]
		public string doors1;
		public List<Transform> doors1Transforms;
		
		[KSPField(isPersistant = false)]
		public string doors2;
		public List<Transform> doors2Transforms;
		
		[KSPField(isPersistant = false)]
		public string transformsToMirror;
		public List<Transform> transformsToMirrorList;
		
		[KSPField(isPersistant = false)]
		public string tiltTransformName;
		
		[KSPField(isPersistant = false)]
		public string tiltTargetTransformName;
		
		[KSPField(isPersistant = true)]
		public string tiltRetractTargetTransformName;
		
		[KSPField(isPersistant = false)]
		public string deployTransformName;
		
		[KSPField(isPersistant = false)]
		public string deployTargetTransformName;
		
		[KSPField(isPersistant = false)]
		public string retractTargetTransformName;
		
		[KSPField(isPersistant = false)]
		public string wheelHingeTransformName;
		
		[KSPField(isPersistant = false)]
		public string retractedWheelTargetTransformName;
		
		[KSPField(isPersistant = false)]
		public string deployedWheelTargetTransformName;
		
		[KSPField(isPersistant = false)]
		public string wheelColliderTargetName;
	
		public float deployTime;
		
		[KSPField(isPersistant = false)]
		public float maxTilt;
		
		[KSPField(isPersistant = false)]
		public float minHeight;
		
		[KSPField(isPersistant = false)]
		public float maxHeight;
		
		[KSPField(isPersistant = false)]
		public float stageNormTime;
		
		[KSPField(isPersistant = false)]
		public string animationName;
		
		[KSPField(isPersistant = false)]
		public float maxWheelAngle;
		
		[KSPField(isPersistant = false)]
		public float wheelRadius;
		
		[KSPField(isPersistant = false)]
		public bool mirrorRetractedWheel = false;
		
		AnimationState anim;
		Animation emptyAnimation;
		
		
		//persistent fields
		[KSPField(isPersistant = true)]
		public GearStates CurrentState = GearStates.Deploying;
		
		[KSPField(isPersistant = true)]
		public GearStates TargetState = GearStates.Deployed;
		
		[KSPField(isPersistant = true, guiActiveEditor = false)]
		public bool isMirrored = false;
		
		[KSPField(isPersistant = true)]
		Quaternion tiltTargetRotationP;
		
		[KSPField(isPersistant = true)]
		Quaternion wheelTargetRotationP;
		
		[KSPField(isPersistant = true)]
		Vector3 wheelTargetPositionP;
		
		[KSPField(isPersistant = true)]
		Vector3 wheelColliderTargetPositionP;
		
		[KSPField(isPersistant = true)]
		bool posAndRotSaved = false;
		
		[KSPField(isPersistant = true)]
		float suspensionDistance = 5;
		
		
		
		[KSPField(isPersistant = true)]
		float tiltAngleP = 0;
		
		[KSPField(isPersistant = true)]
		float animNormTimeP = 0;
		
		[KSPField(isPersistant = true)]
		float animSpeedP = 0;
		
		
		
		public Transform tiltTransform;
		public Transform tiltTargetTransform;
		public Transform tiltRetractTargetTransform;
		public Transform deployTransform;
		public Transform deployTargetTransform;
		public Transform retractTargetTransform;
		public Transform wheelHingeTransform;
		public Transform retractedWheelTargetTransform;
		public Transform deployedWheelTargetTransform;
		
		public Transform wheelColliderHolderTransform;
		public Transform wheelColliderTarget;
		
		
		float pistonTransAdjustSpeed = 0.4f;
		float initialHeight;
		float tiltAdjustSpeed = 60;
		float currentNormTime = 0;
		
		bool hasLoaded = false;
		
		bool doorsAreOpen = true;
		
		float doorsNormTime = 0;
		
		
		
		
		[KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Flip Side", active = true)]
		public void FlipSideEvent()
		{
			FlipSide();
			foreach(var pSym in part.symmetryCounterparts)
			{
				if(pSym != part)
				{
					pSym.FindModuleImplementing<BDAdjustableLandingGear>().FlipSide();	
				}
			}
		}
		
		public void FlipSide()
		{
			isMirrored = !isMirrored;
			
			float mirrorMult = isMirrored ? -1 : 1;
			
			float tiltAngle = Quaternion.Angle(tiltRetractTargetTransform.localRotation, tiltTargetTransform.localRotation);
			
			tiltTargetTransform.localRotation *= Quaternion.AngleAxis(mirrorMult * 2 * tiltAngle, Vector3.up);
			deployedWheelTargetTransform.localRotation = deployedWheelTargetTransform.localRotation * Quaternion.AngleAxis(180, Vector3.forward);
			deployedWheelTargetTransform.localRotation = deployedWheelTargetTransform.localRotation * Quaternion.AngleAxis(-2 * tiltAngle, Vector3.up);
			
			if(CurrentState == GearStates.Deployed || CurrentState == GearStates.Deploying)
			{
				wheelHingeTransform.localRotation = deployedWheelTargetTransform.localRotation;
				tiltTransform.localRotation = tiltTargetTransform.localRotation;
			}
			
		}
		
		
		[KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Auto-Align Wheel", active = true)]
		public void PointWheelToGroundEvent()
		{
			PointWheelToGround();
			foreach(Part pSym in part.symmetryCounterparts)
			{
				if(pSym!=part)
				{
					pSym.FindModuleImplementing<BDAdjustableLandingGear>().PointWheelToGround();	
				}
			}
		}
		
		public void PointWheelToGround()
		{
			float angle = Mathf.Abs(Mathf.DeltaAngle(270, tiltTargetTransform.rotation.eulerAngles.x));
			wheelAngle = angle;
		}
		
		
		
		public override void OnStart (PartModule.StartState state)
		{
			
			
			emptyAnimation = part.FindModelAnimators(animationName)[0];
			anim = emptyAnimation[animationName];
			
			//set max and min for wheel height
			var wheelHeightTweakable = (UI_FloatRange) Fields["wheelHeight"].uiControlEditor;
			wheelHeightTweakable.maxValue = maxHeight;
			wheelHeightTweakable.minValue = minHeight;
			
			//set max wheelAngle tweakable
			var wheelAngleTweakable = (UI_FloatRange) Fields["wheelAngle"].uiControlEditor;
			wheelAngleTweakable.maxValue = maxWheelAngle;
			wheelAngleTweakable.minValue = 0;
			
			//set max leg tilt tweakable
			var tiltAngleTweakable = (UI_FloatRange) Fields["legAngle"].uiControlEditor;
			tiltAngleTweakable.maxValue = maxTilt;
			tiltAngleTweakable.minValue = 0;
			
			
			
			//tiltTransforms
			tiltTransform = part.FindModelTransform(tiltTransformName);
			tiltTargetTransform = part.FindModelTransform(tiltTargetTransformName);
			tiltRetractTargetTransform = part.FindModelTransform(tiltRetractTargetTransformName);
			
			//deployTransforms
			deployTransform = part.FindModelTransform(deployTransformName);
			deployTargetTransform = part.FindModelTransform (deployTargetTransformName);
			retractTargetTransform = part.FindModelTransform(retractTargetTransformName);
			
			//wheelTransforms
			wheelHingeTransform = part.FindModelTransform (wheelHingeTransformName);
			retractedWheelTargetTransform = part.FindModelTransform (retractedWheelTargetTransformName);
			deployedWheelTargetTransform = part.FindModelTransform (deployedWheelTargetTransformName);
			
			wheelColliderTarget = part.FindModelTransform(wheelColliderTargetName);
			wheelColliderHolderTransform = part.FindModelTransform("wheelColliderHolder");
		
			initialHeight = Vector3.Distance(deployedWheelTargetTransform.localPosition, retractedWheelTargetTransform.localPosition);
			
			SetUpDoors();
			
			if(animSpeedP > 0)
			{
				animNormTimeP = 1;	
			}
			else if(animSpeedP < 0)
			{
				animNormTimeP = 0;	
			}
		
			
			anim.normalizedTime = animNormTimeP;
			//Debug.LogWarning("anim norm time: "+anim.normalizedTime);
			
			float animNormTime = 1-animNormTimeP;	
			animNormTime = Mathf.Clamp01(animNormTime);
			
			if(animNormTime >= 1)
			{
				//emptyAnimation.Stop();
				//anim.normalizedTime = 0;
				CurrentState = GearStates.Deployed;
			}
			else if(animNormTime <= 0)
			{
				//emptyAnimation.Stop();
				//anim.normalizedTime = 1;
				CurrentState = GearStates.Retracted;
			}	
			else if(anim.speed < 0)
			{
				CurrentState = GearStates.Deploying;	
			}
			else
			{
				CurrentState = GearStates.Retracting;	
			}
		
			
			//Debug.LogWarning("gear current state: "+CurrentState);
			
			
			if(posAndRotSaved) LoadPosAndRot();	
				
			if(CurrentState == GearStates.Deployed || CurrentState == GearStates.Deploying)	
			{
				wheelHingeTransform.localPosition = deployedWheelTargetTransform.localPosition;
				wheelHingeTransform.localRotation = deployedWheelTargetTransform.localRotation;
				tiltTransform.localRotation = tiltTargetTransform.localRotation;
				deployTransform.localRotation = deployTargetTransform.localRotation;
			}
			else
			{
				wheelHingeTransform.localPosition = retractedWheelTargetTransform.localPosition;
				wheelHingeTransform.localRotation = retractedWheelTargetTransform.localRotation;
				tiltTransform.localRotation = tiltRetractTargetTransform.localRotation;
				deployTransform.localRotation = retractTargetTransform.localRotation;
			}
			
			part.OnEditorAttach += new Callback(OnEditorAttach);
		}
		
		public void Update()
		{
		}
		
		public void FixedUpdate()
		{
			if(isMirrored)
			{
				foreach(var t in transformsToMirrorList)	
				{
					t.localRotation = Quaternion.Euler(t.localRotation.eulerAngles.x, 180, t.localRotation.eulerAngles.z);	
				}
			}
			else
			{
				foreach(var t in transformsToMirrorList)	
				{
					t.localRotation = Quaternion.Euler(t.localRotation.eulerAngles.x, 0, t.localRotation.eulerAngles.z);	
				}
			}
			
			if(mirrorRetractedWheel && isMirrored)
			{
				retractedWheelTargetTransform.localRotation = Quaternion.Euler(retractedWheelTargetTransform.localRotation.x, retractedWheelTargetTransform.localRotation.y, 180);	
			}
			
			
			wheelColliderHolderTransform.localRotation = wheelHingeTransform.localRotation;
			
			//scene specific stuff
			if(HighLogic.LoadedSceneIsEditor)
			{
				RefreshTweakables();
				
				tiltAngleP = Vector3.Angle(deployedWheelTargetTransform.forward, tiltTargetTransform.forward); 
				
				foreach(var wheelCollider in part.FindModelComponents<WheelCollider>())
				{
					suspensionDistance = Mathf.Cos(tiltAngleP*Mathf.Deg2Rad) * wheelHeight;	
					wheelCollider.suspensionDistance = suspensionDistance;
				}
				
				wheelHeight = Mathf.Clamp(wheelHeight, minHeight, maxHeight);
				wheelColliderHolderTransform.localPosition = new Vector3(0, 0, -wheelHeight);
				deployedWheelTargetTransform.localPosition = new Vector3(0, 0, -wheelHeight);
				
				
				
				//wheel angle target stuff
				float wlocalYrot = isMirrored ? wheelAngle : -wheelAngle;
				float wlocalZrot = isMirrored ? 180 : 0;
				deployedWheelTargetTransform.localRotation = Quaternion.Euler(0, wlocalYrot, wlocalZrot);
				
				//leg angle target stuff
				float legLocalXrot = 270+legAngle;
				float legLocalYrot = 0;
				float legLocalZrot = 0;
				if(legAngle!=0)
				{
					legLocalYrot = isMirrored ? 270 : 90;
					legLocalZrot = isMirrored ? 90 : 270;
				}
				
				tiltTargetTransform.localRotation = Quaternion.Euler(legLocalXrot, legLocalYrot, legLocalZrot);
				
				part.FindModelTransform(boundsCollider).GetComponent<Collider>().enabled = false;
			}
			else
			{
				if(vessel.loaded)
				{
					part.FindModelTransform(boundsCollider).GetComponent<Collider>().enabled = false;	
				}
				else
				{
					part.FindModelTransform(boundsCollider).GetComponent<Collider>().enabled = true;	
				}
				
				if(CurrentState == GearStates.Deployed)
				{
					tiltAngleP = Vector3.Angle(deployedWheelTargetTransform.forward, tiltTargetTransform.forward);
					
					foreach(WheelCollider wheelCollider in part.FindModelComponents<WheelCollider>())
					{
						suspensionDistance = Mathf.Cos(tiltAngleP*Mathf.Deg2Rad) * wheelHeight;
						wheelCollider.suspensionDistance = suspensionDistance;
						jointSpring.spring = suspensionSpring;
						jointSpring.damper = suspensionDamper;
						jointSpring.targetPosition = wheelCollider.suspensionSpring.targetPosition;
						wheelCollider.suspensionSpring = jointSpring;
						//wheelCollider.brakeTorque = brakeTorque;
						
					}
					
					float mirrorMult = isMirrored ? 1 : -1;
					
					float m = wheelHeight * Mathf.Sin(tiltAngleP*Mathf.Deg2Rad);
					
					float localX = m * Mathf.Cos (tiltAngleP * Mathf.Deg2Rad) * mirrorMult;
					
					float localZ = -(m * Mathf.Sin (tiltAngleP * Mathf.Deg2Rad));
					
					wheelColliderHolderTransform.localPosition = new Vector3(localX, 0, localZ);
				
				}
				else
				{
					foreach(var wheelCollider in part.FindModelComponents<WheelCollider>())
					{
						wheelCollider.suspensionDistance = 0;
					}
					
					wheelColliderHolderTransform.localPosition = Mathf.Clamp01((currentNormTime-stageNormTime)/(1-stageNormTime)) * new Vector3(0,0,-wheelHeight);
			
				}
			}
		
			
			
			
			//get animation normalized time and current state
			float animNormTime = 0;
		
			if(!hasLoaded)
			{
				//emptyAnimation.Stop();
				anim.normalizedTime = animNormTimeP;	
				hasLoaded = true;
			}
			else
			{
				animNormTimeP = anim.normalizedTime;
				animSpeedP = anim.speed;
			}
			
			deployTime = anim.length;
			anim.normalizedTime = Mathf.Clamp01(anim.normalizedTime);
			if(emptyAnimation.isPlaying) animNormTime = 1-anim.normalizedTime;	
			
			foreach(WheelCollider wc in part.FindModelComponents<WheelCollider>())
			{
				if(animNormTime < 0.2f && CurrentState != GearStates.Deployed)	
				{
					wc.radius = 0.0001f;	
				}
				else
				{
					wc.radius = wheelRadius;	
				}
			}
			
			
			
			
		
			if(animNormTime == 0 && !emptyAnimation.isPlaying)
			{
				CurrentState = TargetState;
			}	
			else if(anim.speed < 0)
			{
				TargetState = GearStates.Deployed;
				CurrentState = GearStates.Deploying;	
			}
			else if(anim.speed > 0)
			{
				TargetState = GearStates.Retracted;
				CurrentState = GearStates.Retracting;	
			}
			
			
			
			
			
			UpdateDoors();
			
			doorsNormTime = (90/doorSpeed)/deployTime;
			
			deployTime -= 90/doorSpeed;
			
			
			UpdateGear();
			
			
			SavePosAndRot();
		}
		
		void UpdateGear()
		{
			//speeds
			float deployAngularSpeed;
			float tiltAngularSpeed;
			float wheelAngularSpeed;
			float pistonTransSpeed;
			
			//set speed to adjustment speed if wheels are deployed
			if(CurrentState == GearStates.Deployed)
			{
				deployAngularSpeed = tiltAdjustSpeed;
				tiltAngularSpeed = tiltAdjustSpeed;
				wheelAngularSpeed = tiltAdjustSpeed;
				pistonTransSpeed = pistonTransAdjustSpeed;
			}
			else //otherwise, set speed based on time it takes to deploy
			{
				deployAngularSpeed = Quaternion.Angle(deployTargetTransform.localRotation, retractTargetTransform.localRotation)/deployTime;
				tiltAngularSpeed = Quaternion.Angle(tiltTargetTransform.localRotation, tiltRetractTargetTransform.localRotation)/(deployTime-(stageNormTime*deployTime));
				wheelAngularSpeed = Quaternion.Angle(deployedWheelTargetTransform.localRotation, retractedWheelTargetTransform.localRotation)/(deployTime-(stageNormTime*deployTime));
				pistonTransSpeed = Vector3.Distance(deployedWheelTargetTransform.localPosition, retractedWheelTargetTransform.localPosition)/(deployTime-(stageNormTime*deployTime));
			}
			
			currentNormTime = 1 - (Quaternion.Angle(deployTransform.localRotation, deployTargetTransform.localRotation)/Quaternion.Angle(deployTargetTransform.localRotation, retractTargetTransform.localRotation));
			
			
			float deployAngleAmount = deployAngularSpeed * TimeWarp.fixedDeltaTime;
			float wheelAngleAmount = wheelAngularSpeed * TimeWarp.fixedDeltaTime;
			float tiltAngleAmount = tiltAngularSpeed * TimeWarp.fixedDeltaTime;
			float pistonTransAmount = pistonTransSpeed * TimeWarp.fixedDeltaTime;
			
			//moving stuff w/anim
		
			if((CurrentState == GearStates.Deploying && doorsAreOpen) || CurrentState == GearStates.Deployed)
			{
				deployTransform.localRotation = Quaternion.RotateTowards(deployTransform.localRotation, deployTargetTransform.localRotation, deployAngleAmount);
				
				if(currentNormTime > stageNormTime)
				{
					wheelHingeTransform.localRotation = Quaternion.RotateTowards(wheelHingeTransform.localRotation, deployedWheelTargetTransform.localRotation, wheelAngleAmount);
					tiltTransform.localRotation = Quaternion.RotateTowards(tiltTransform.localRotation, tiltTargetTransform.localRotation, tiltAngleAmount);
					wheelHingeTransform.localPosition = Vector3.MoveTowards(wheelHingeTransform.localPosition, deployedWheelTargetTransform.localPosition, pistonTransAmount);
				}
			}
			else if((CurrentState == GearStates.Retracting && doorsAreOpen) || CurrentState == GearStates.Retracted)
			{
				deployTransform.localRotation = Quaternion.RotateTowards(deployTransform.localRotation, retractTargetTransform.localRotation, deployAngleAmount);
				
				if(currentNormTime > stageNormTime)
				{
					wheelHingeTransform.localRotation = Quaternion.RotateTowards(wheelHingeTransform.localRotation, retractedWheelTargetTransform.localRotation, wheelAngleAmount);
					tiltTransform.localRotation = Quaternion.RotateTowards(tiltTransform.localRotation, tiltRetractTargetTransform.localRotation, tiltAngleAmount);
					wheelHingeTransform.localPosition = Vector3.MoveTowards(wheelHingeTransform.localPosition, retractedWheelTargetTransform.localPosition, pistonTransAmount);
				}	
			}
			
	
		}
		
		void SavePosAndRot()
		{
			wheelTargetPositionP = deployedWheelTargetTransform.localPosition;
			wheelTargetRotationP = deployedWheelTargetTransform.localRotation;
			wheelColliderTargetPositionP = wheelColliderTarget.localPosition;
			
			tiltTargetRotationP = tiltTargetTransform.localRotation;
			
			posAndRotSaved = true;
		}
		
		void LoadPosAndRot()
		{
			deployedWheelTargetTransform.localPosition = wheelTargetPositionP;
			deployedWheelTargetTransform.localRotation = wheelTargetRotationP;
			wheelColliderTarget.localPosition = wheelColliderTargetPositionP;
			
			tiltTargetTransform.localRotation = tiltTargetRotationP;
		}
		
		public void OnEditorAttach()
		{
			foreach(Part pSym in part.symmetryCounterparts)
			{
				if(pSym!=part && isMirrored == false)
				{
					pSym.FindModuleImplementing<BDAdjustableLandingGear>().isMirrored = true;
				}
				else if(pSym != part && isMirrored == true)
				{
					BDAdjustableLandingGear symGear = pSym.FindModuleImplementing<BDAdjustableLandingGear>();
					symGear.isMirrored = false;
					float tiltAngle = Quaternion.Angle(symGear.tiltRetractTargetTransform.localRotation, symGear.tiltTargetTransform.localRotation);
					
					tiltTargetTransform.localRotation = tiltRetractTargetTransform.localRotation * Quaternion.AngleAxis(-tiltAngle, Vector3.up);
					
					///wheel angle target stuff
					float localYrot = isMirrored ? wheelAngle : -wheelAngle;
					float localZrot = isMirrored ? 180 : 0;
					deployedWheelTargetTransform.localRotation = Quaternion.Euler(0, localYrot, localZrot);
					
					if(wheelHeight < symGear.wheelHeight)
					{
						wheelHeight = symGear.wheelHeight;
					}
					else
					{
						symGear.wheelHeight = wheelHeight;	
					}
				}
				
			}
			if(CurrentState == GearStates.Deployed || CurrentState == GearStates.Deploying)
			{
				wheelHingeTransform.localRotation = deployedWheelTargetTransform.localRotation;
				tiltTransform.localRotation = tiltTargetTransform.localRotation;
			}
			
			
		}
		
		void SetUpDoors()
		{
			string[] doors1Arr = doors1.Split(',');	
			string[] doors2Arr = doors2.Split(',');
			string[] transformsToMirrorArr = transformsToMirror.Split(',');
			
			doors1Transforms = new List<Transform>();
			foreach(string door in doors1Arr)
			{
				doors1Transforms.Add(part.FindModelTransform(door));	
			}
			
			doors2Transforms = new List<Transform>();
			foreach(string door in doors2Arr)
			{
				doors2Transforms.Add(part.FindModelTransform(door));	
			}
			
			transformsToMirrorList = new List<Transform>();
			foreach(string t in transformsToMirrorArr)
			{
				transformsToMirrorList.Add(part.FindModelTransform(t));	
			}
		}
		
		void UpdateDoors()
		{
			if(CurrentState == GearStates.Retracted)	
			{
				Quaternion targetRotation = Quaternion.Euler(0,0,0);
				
				foreach(var t in doors1Transforms)
				{
					t.localRotation = Quaternion.RotateTowards(t.localRotation, targetRotation, doorSpeed * TimeWarp.fixedDeltaTime);	
				}
				
				foreach(var t in doors2Transforms)
				{
					t.localRotation = Quaternion.RotateTowards(t.localRotation, targetRotation, doorSpeed * TimeWarp.fixedDeltaTime);	
				}
			}
			else if(CurrentState == GearStates.Retracting)
			{
				Quaternion targetRotation = Quaternion.Euler(0,-90,0);
				
				foreach(var t in doors1Transforms)
				{
					t.localRotation = Quaternion.RotateTowards(t.localRotation, targetRotation, doorSpeed * TimeWarp.fixedDeltaTime);	
					
					if(Quaternion.Angle(t.localRotation, targetRotation) < 0.5f)
					{
						doorsAreOpen = true;	
					}
					else
					{
						doorsAreOpen = false;	
					}
				}	
				
				
			}
			else if(CurrentState == GearStates.Deployed)
			{
				Quaternion targetRotation = Quaternion.Euler(0,0,0);
				
				foreach(var t in doors1Transforms)
				{
					t.localRotation = Quaternion.RotateTowards(t.localRotation, targetRotation, doorSpeed * TimeWarp.fixedDeltaTime);	
				}
			}
			else if(CurrentState == GearStates.Deploying)
			{	
				Quaternion targetRotation = Quaternion.Euler(0,-90,0);
				
				foreach(var t in doors1Transforms)
				{
					t.localRotation = Quaternion.RotateTowards(t.localRotation, targetRotation, doorSpeed * TimeWarp.fixedDeltaTime);	
					if(Quaternion.Angle(t.localRotation, targetRotation) < 0.5f)
					{
						doorsAreOpen = true;	
					}
					else
					{
						doorsAreOpen = false;	
					}
				}
				
				foreach(var t in doors2Transforms)
				{
					t.localRotation = Quaternion.RotateTowards(t.localRotation, targetRotation, doorSpeed * TimeWarp.fixedDeltaTime);	
				}
				
			}
			
			
		}
		
		public void OnGUI()
		{
			/*
			float angleToGround = Vector3.Angle(deployedWheelTargetTransform.forward, Vector3.up);
			
			GUI.Label(new Rect(50,50,200,200), 
				"anim is playing: "+ emptyAnimation.isPlaying
				+ "\n anim norm time: "+anim.normalizedTime.ToString("0.00")
				+ "\n gear state: "+CurrentState
				);
				
			*/
				
		}
		
		
		
		//Thanks FlowerChild
		//refreshes part action window
		void RefreshTweakables()
        {
			if(showSettings!=showSettingsPrev || Fields["legAngle"].guiActiveEditor!=showSettings)
			{
				showSettingsPrev = showSettings;	
				
				
				Fields["legAngle"].guiActiveEditor = showSettings;	
				Fields["wheelAngle"].guiActiveEditor = showSettings;
				Fields["suspensionSpring"].guiActiveEditor = showSettings;
				Fields["suspensionDamper"].guiActiveEditor = showSettings;
				Fields["wheelHeight"].guiActiveEditor = showSettings;
				//Fields["brakeTorque"].guiActiveEditor = showSettings;
				
			
				foreach ( UIPartActionWindow window in FindObjectsOfType( typeof( UIPartActionWindow ) ) ) 
	            {
					if ( window.part == part )
	                {
	                    window.displayDirty = true;
	                }
	            }
			}
        }
		
	}
}

