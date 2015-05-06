using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using UnityEngine;

namespace BDAnimationModules
{
	public class Utils
	{
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

		public static float RoundToMultiple(float f, float multiple)
		{
			float factor = 1/multiple;
			f *= factor;
			f = Mathf.Round(f);
			f /= factor;
			return f;
		}

		public static float SignedAngle(Vector3 fromDirection, Vector3 toDirection, Vector3 referenceRight)
		{
			float angle = Vector3.Angle(fromDirection, toDirection);
			float sign = Mathf.Sign(Vector3.Dot(toDirection, referenceRight));
			float finalAngle = sign * angle;
			return finalAngle;
		}
	}

}

