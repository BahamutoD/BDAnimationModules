using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BDAnimationModules
{
	public class BDGenericAnimationState : PartModule
	{
		[KSPField(isPersistant = false)]
		public string AnimationName;
		
		[KSPField(isPersistant = false)]
		public string StateFrames = "0,1";
		
		[KSPField(isPersistant = false)]
		public string StateNames = "Closed,Open";
		
		private AnimationState[] animStates;
		private string[] framesArray;
		private string[] namesArray;
		private int[] intFramesArray;
		
		
		public override void OnStart(PartModule.StartState state)
		{
			animStates = Utils.SetUpAnimation(AnimationName, this.part);
			
			framesArray = StateFrames.Split(',');
			namesArray = StateNames.Split (',');
			
			intFramesArray = new int[framesArray.Length];
			int index = 0;
			foreach (string frame in framesArray)
			{
				intFramesArray[index] = Convert.ToInt32(frame);
				index++;
			}
			
			
		}
	}
}

