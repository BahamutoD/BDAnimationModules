using System;
using UnityEngine;

namespace BDAnimationModules
{
	public class SurfaceWalker : PartModule
	{
		[KSPField(isPersistant = true, guiActive = true, guiName = "Enabled")]
		public bool walkerEnabled = true;
		
		[KSPField(isPersistant = false)]
		public float moveForce = 30;
		
		[KSPField(isPersistant = false)]
		public float slowForce = 10;
		
		[KSPField(isPersistant = false)]
		public float rotateTorque = 25;
		
		[KSPField(isPersistant = false)]
		public float slowTorque = 10;
		
		[KSPField(isPersistant = false)]
		public float moveAccelFactor = 1;
		
		public bool stickyFeet = false;
		
		Vector3 forwardForce = Vector3.zero;
		Vector3 strafeForce = Vector3.zero;
		float turnTorque = 0;
		
		//check if feet are touching ground with raycast
		bool feetAreDown = true;
		
		bool hasInitialized = false;
		
		bool groundHit = false;
		float debugWheelSuspDist = 0;
		
		public override void OnStart (PartModule.StartState state)
		{
			part.force_activate();
			
			
			
		}
		
		public void Update()
		{
			if(Input.GetKeyDown(KeyCode.Keypad0))
			{
				foreach(var wheel in part.FindModelComponents<WheelCollider>())
				{
					Debug.Log ("Wheel susp: "+wheel.suspensionDistance	);
				}
			}
		}
		
		public void FixedUpdate()
		{
			if(HighLogic.LoadedSceneIsFlight && vessel!=null && vessel.IsControllable)
			{
				if(!hasInitialized)
				{
					hasInitialized = true;
					part.FindModelTransform("bounds").GetComponent<Collider>().enabled = false;
				}
			}
			else return;
			
			Ray feetDownRay = new Ray(transform.position, -transform.up);
			if(Physics.Raycast(feetDownRay, 5, 1<<15))
			{
				feetAreDown = true;
			}
			else
			{
				feetAreDown = false;	
			}
			
			if(walkerEnabled && vessel!=null && vessel.loaded)
			{
                var rigidbody = vessel.GetComponent<Rigidbody>();
				if(vessel.checkLanded() && !rigidbody.isKinematic && feetAreDown)
				{
					
					//stickyFeet
					if(stickyFeet) rigidbody.AddForce(10 * -part.transform.up);
					
					
					forwardForce = Vector3.MoveTowards(forwardForce, -vessel.ctrlState.pitch * moveForce * Vector3.forward, moveForce * moveAccelFactor * Time.fixedDeltaTime);
					strafeForce = Vector3.MoveTowards(strafeForce, vessel.ctrlState.yaw * moveForce * Vector3.right, moveForce * moveAccelFactor * Time.fixedDeltaTime);
					turnTorque = Mathf.MoveTowards(turnTorque, rotateTorque * vessel.ctrlState.roll, rotateTorque * moveAccelFactor*2 * Time.fixedDeltaTime);
					
					Vector3 force = Vector3.ClampMagnitude(forwardForce+strafeForce, moveForce);
					
					rigidbody.AddRelativeForce(force);	
					rigidbody.AddRelativeTorque(Vector3.up * turnTorque);
					
					//drag
					rigidbody.AddTorque(-slowForce * rigidbody.angularVelocity);
					rigidbody.AddForce(-slowForce * rigidbody.velocity);
						
					foreach(var wheel in part.FindModelTransforms("wheelCollider"))
					{
						var wheelCollider = wheel.GetComponent<WheelCollider>();
						debugWheelSuspDist = wheelCollider.suspensionDistance;
						WheelHit hit;
						Transform colEnhancer = wheel.FindChild("collisionEnhancer");
						
						if(wheelCollider.GetGroundHit(out hit))
						{
							groundHit = true;
							colEnhancer.localPosition = new Vector3(0,(hit.point - wheel.position).magnitude, 0);
						}
						else
						{
							groundHit = false;	
						}
						
					}
					
				}
				
			}
		}
		
		//just debug stuff
		void OnGUI()
		{
			GUI.Label(new Rect(10,10,200,200), "Groundhit: " + groundHit);
			GUI.Label(new Rect(10,30,200,200), "Wheel susp dist: " + debugWheelSuspDist);
		}
		
		
		
		
	}
}

