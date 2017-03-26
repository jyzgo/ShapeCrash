using UnityEngine;
using System.Collections;

namespace MTUnity {

	/**
	 * Play one animation on start, destroy the game object when finished
	 */
	public class SpineDestroyer : MonoBehaviour {
		
		public string animationName;
		
		private SkeletonAnimation _anim;
		
		// Use this for initialization
		void Start () {
			_anim = GetComponent<SkeletonAnimation>();
			if (animationName != null && animationName != "") {
				_anim.state.ClearTracks();
				_anim.state.AddAnimation(0, animationName, false, 0);
			}
			_anim.state.End += OnEnd;
		}
		
		void OnEnd(Spine.AnimationState state, int trackIndex) {
			Destroy(gameObject);
		}
	}
}
