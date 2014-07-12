using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BDAnimationModules
{
	public class GenericLoopAnimation : PartModule
	{
		[KSPField(isPersistant = false)]
		public string deployName;
		[KSPField(isPersistant = false)]
		public string loopName;
		[KSPField(isPersistant = true)]
		public string StartGUIName="null";
		[KSPField(isPersistant = true)]
		public string EndGUIName="null";
		
		private AnimationState[] deployStates;
		private AnimationState[] loopStates;
		
		public override void OnStart(PartModule.StartState state)
		{
			deployStates = Utils.SetUpAnimation(deployName, this.part);
			loopStates = Utils.SetUpAnimation(loopName, this.part);
			foreach(AnimationState deploy in deployStates)
			{
				deploy.enabled = false;
			}
			foreach(AnimationState loop in loopStates)
			{
				loop.enabled = false;
			}
		}
		
		[KSPEvent(guiActive = true, guiName = "Activate")]
		public void startAnim()
		{
			Events["startAnim"].active = false;
			Events["endAnim"].active = true;
			foreach(AnimationState deploy in deployStates)
			{
				foreach(AnimationState loop in loopStates)
				{
					if(!loop.enabled)
					{
						deploy.enabled = true;
						deploy.speed = 1;
					}
				}
			}
		}
		
		[KSPEvent(guiActive = true, guiName = "Deactivate", active = false)]
		public void endAnim()
		{
			Events["startAnim"].active = true;
			Events["endAnim"].active = false;
			foreach(AnimationState deploy in deployStates)
			{
				foreach(AnimationState loop in loopStates)
				{
					if(!loop.enabled){deploy.speed = -1;}
					if(loop.enabled)
					{
						loop.speed = -1;
					}
				}
			}
		}
		
		[KSPAction("Toggle")]
		public void Toggle(KSPActionParam param)
		{
			if(Events["startAnim"].active == true)
			{
				startAnim ();
			}
			else
			{
				endAnim ();
			}
		}
		
		[KSPAction("Activate")]
		public void startAnimActionGroup(KSPActionParam param)
		{
			startAnim ();
		}
		
		[KSPAction("Deactivate")]
		public void endAnimActionGroup(KSPActionParam param)
		{
			endAnim ();
		}
		
		public override void OnUpdate()
		{
			foreach(AnimationState deploy in deployStates)
			{
				foreach(AnimationState loop in loopStates)
				{
					if(deploy.normalizedTime>=1 && deploy.speed == 1)
					{
						deploy.speed = 0;
						deploy.enabled = false;
						loop.enabled = true;
						loop.wrapMode=WrapMode.Loop;
						loop.speed = 1;
					}
					if(deploy.normalizedTime<=0 && deploy.speed == -1)
					{
						deploy.speed = 0;
						deploy.enabled = false;
					}
					if(loop.normalizedTime<=0 && loop.speed == -1)
					{
						loop.speed = 0;
						loop.enabled = false;
						deploy.enabled = true;
						deploy.normalizedTime = 1;
						deploy.speed = -1;
					}
				}
			}
		}
		
		
	}
}

