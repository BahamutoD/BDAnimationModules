using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BDAnimationModules
{
	public class BDEngineCase : PartModule
	{
		private string animName = "null";
		private AnimationState[] animStates;
		private List<ModuleEngines> engines = new List<ModuleEngines>();
		private bool editorToggle = false;
		
		public override void OnStart(PartModule.StartState state)
		{
			foreach (var animMod in this.part.FindModulesImplementing<ModuleAnimateGeneric>())
			{
				animName = animMod.animationName;
			}
			
			animStates = Utils.SetUpAnimation(animName, this.part);
			Debug.Log("setup animation");
			foreach(AnimationState anim in animStates)
			{
				if(!HighLogic.LoadedSceneIsEditor)
				{
					anim.wrapMode = WrapMode.Clamp;
				}
			}
			Debug.Log("animations set to clamp");
			
			if(this.part.children.Capacity>0){
				Debug.Log("part has children");
				foreach(Part node in this.part.children)
				{
					if(node!=null)
					{
						foreach(ModuleEngines me in node.FindModulesImplementing<ModuleEngines>())
						{	
							if (node.attachMode == AttachModes.STACK)
							{
								engines.Add (me);
								Debug.Log("added child ModuleEngine");
							}
						}
					}
				}
			}
			if(HighLogic.LoadedSceneIsEditor)
			{
				Events["EditorToggle"].active=true;
			}
			else
			{
				Events["EditorToggle"].active=false;
			}
			Debug.Log("OnStart complete==============================");
		}
		
		[KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Toggle")]
		public void EditorToggle()
		{
			foreach(AnimationState anim in animStates)
			{
				Debug.Log ("editor toggle = "+editorToggle);
				
				if(!editorToggle)
				{
					if(anim.normalizedTime<0){anim.normalizedTime=0;}
					Debug.Log ("editor: opening");
					anim.speed = 1;
					editorToggle = !editorToggle;
				}
				else
				{
					if(anim.normalizedTime>1){anim.normalizedTime=1;}
					Debug.Log ("editor: closing");
					anim.speed = -1;
					editorToggle = !editorToggle;
				}
				
			}
			
		}
		
		public override void OnUpdate()
		{
			foreach(AnimationState anim in animStates)
			{
				if (anim.normalizedTime >=1	)
				{
					
					if(engines!=null){
						foreach(ModuleEngines me in engines)
						{
							me.Activate();
						}
					}
					
				}
				else
				{
					
					if(engines!=null){
						foreach(ModuleEngines me in engines)
						{
							me.Shutdown();
						}
					}
				}
			}
		}
		
		
	}
}

