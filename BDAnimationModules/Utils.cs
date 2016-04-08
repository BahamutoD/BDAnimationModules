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

		// Math by Minahito: http://sunday-lab.blogspot.nl/2008/04/get-pitch-yaw-roll-from-quaternion.html
		/// <summary>
		/// The pitch angle of the quaternion in radians.
		/// </summary>
		/// <param name="q">The quaternion.</param>
		/// <returns></returns>
		public static float Pitch(Quaternion q) {
			return Mathf.Atan2(2 * (q.y * q.z + q.w * q.x), q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z);
		}
		/// <summary>
		/// The yaw angle of the quaternion in radians.
		/// </summary>
		/// <param name="q">The quaternion.</param>
		/// <returns></returns>
		public static float Yaw(Quaternion q) {
			return Mathf.Asin(-2 * (q.x * q.z - q.w * q.y));
		}
		/// <summary>
		/// The roll angle of the quaternion in radians.
		/// </summary>
		/// <param name="q">The quaternion.</param>
		/// <returns></returns>
		public static float Roll(Quaternion q) {
			return Mathf.Atan2(2 * (q.x * q.y + q.w * q.z), q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z);
		}
	}

}

