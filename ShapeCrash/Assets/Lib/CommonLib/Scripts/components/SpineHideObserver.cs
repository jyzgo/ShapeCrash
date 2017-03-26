using UnityEngine;
using System.Collections;

namespace MTUnity {

	public class SpineHideObserver : MonoBehaviour {
		public string animationName;

		private SkeletonAnimation _anim = null;

		public void Awake() {
			_anim = GetComponent<SkeletonAnimation>();
			_anim.state.End += OnEnd;
		}

		public void ShowAnimation (string name) {
			if (_anim != null && name != null && name != "") {
				animationName = name;
				_anim.state.ClearTracks();
				_anim.state.AddAnimation(0, animationName, false, 0);
				this.gameObject.SetActive (true);
			}
		}

		void OnEnd(Spine.AnimationState state, int trackIndex) {
			if (animationName != null && animationName != "") {
//				Debug.Log ("MTUnity: SpineHideObserver: OnEnd animationName = " + animationName);
				Spine.TrackEntry te = state.GetCurrent (trackIndex);
				if (te != null && te.animation.name.CompareTo(animationName) == 0) {
					this.gameObject.SetActive (false);
				}
			}
		}
	}
}
