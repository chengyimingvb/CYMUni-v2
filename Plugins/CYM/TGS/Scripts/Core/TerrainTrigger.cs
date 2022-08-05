using UnityEngine;
using System.Collections;

namespace CYM.TGS
{
	public class TerrainTrigger : MonoBehaviour
	{
		TGS tgs;
		RaycastHit[] hits;
		public void Init<T> (TGS tgs) where T: Component
		{
			this.tgs = tgs;
			if (hits == null || hits.Length != 20) {
				hits = new RaycastHit[20];
			}
			if (GetComponent<T> () == null) {
				gameObject.AddComponent<T> ();
			}
		}
		void OnMouseEnter ()
		{
			if (tgs != null) {
				tgs.mouseIsOver = true;
			}
		}
		void OnMouseExit ()
		{
			if (tgs==null) return;
			// Make sure it's outside of grid
			Vector3 mousePos = Input.mousePosition;
			Camera cam = tgs.cameraMain;
			Ray ray = cam.ScreenPointToRay (mousePos);
			int hitCount = Physics.RaycastNonAlloc (cam.transform.position, ray.direction, hits);
			if (hitCount > 0) {
				for (int k = 0; k < hitCount; k++) {
					if (hits [k].collider.gameObject == gameObject)
						return; 
				}
			}
			tgs.mouseIsOver = false;
		}
	}
}