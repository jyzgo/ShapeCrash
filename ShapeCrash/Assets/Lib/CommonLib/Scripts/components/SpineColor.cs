using UnityEngine;
using System.Collections;
using Spine;

#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace MTUnity {

	/**
	 * Change spine animation rendering color
	 * Can be used in Editor and runtime
	 */
	[ExecuteInEditMode]
	[RequireComponent (typeof(SkeletonAnimation))]
	public class SpineColor : MonoBehaviour {
		
		public Color color = Color.white;
		
		private Color _color = Color.white;
		
		void OnEnable() {
			_color = color;
			Apply(_color);
		}
		
		// Update is called once per frame
		void Update () {
			if (!color.Equals(_color)) {
				_color = color;
				Apply(color);
			}
		}
		
		void Apply(Color color) {
			Skeleton skeleton = GetComponent<SkeletonAnimation>().Skeleton;
			if (skeleton != null) {
				skeleton.SetColor(color);
			}
		}
		
		void OnDisable() {
			_color = Color.white;
			Apply(Color.white);
		}
	}
}
