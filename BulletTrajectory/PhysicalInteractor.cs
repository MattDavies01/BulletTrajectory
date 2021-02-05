using UnityEngine;

[System.Serializable]
public class PhysicalInteractor {
	public HitObjectGroup defaultInteraction;
	public HitObjectGroup[] interactuableObjects;

	[System.Serializable]
	public class HitObjectGroup {
		public string tag;                          
		public float collisionMass = 5f;            
		public bool destroyOnHit = true;			
		public bool pullRigidbodiesOnBounce = true; 
		public GameObject hitEffectsEnd;			
		public GameObject hitEffectsBounce;        
		public string rigidReplacement;				
	}

	private BulletBehaviour bulletBehaviour;
	private BulletKinematics bulletKinematics;

	public void Initiate(BulletBehaviour bulletBehaviour, BulletKinematics bulletKinematics) {
		this.bulletBehaviour = bulletBehaviour;
		this.bulletKinematics = bulletKinematics;
	}

	public void OnBulletCollision(RaycastHit hit, bool canBounce) {

		GameObject hitEffects = null;
		HitObjectGroup hitObject = getHitObjectProperties(hit);
		if (canBounce) {
			hitEffects = hitObject.hitEffectsBounce;
		} else {
			hitEffects = hitObject.hitEffectsEnd;
		}
		if (hitObject.pullRigidbodiesOnBounce) {
			PullRigidbodies(hit, hitObject.collisionMass);
		}

		if (hitEffects != null) {
			bulletBehaviour.InstantiateEffects(hitEffects, hit);
		}

		// we are not bouncing more, this is the last interaction
		if (!canBounce || hitObject.destroyOnHit) {
			bulletBehaviour.RemoveBullet(hitObject.rigidReplacement);
		}
	}

	private HitObjectGroup getHitObjectProperties(RaycastHit hit) {
		foreach (HitObjectGroup hog in interactuableObjects) {
			if (hit.collider.CompareTag(hog.tag)) {
				return hog;
			}
		}

		return defaultInteraction;
	}

	private void PullRigidbodies(RaycastHit hit, float mass) {
		if (hit.rigidbody == null) {
	
			return;
		}

		float ec = 0.5f * mass * Mathf.Pow(bulletKinematics.VelocityAtTime(
			bulletBehaviour.GetTime()).magnitude, 2); // kinetic energy = 1/2*mass*v²
		hit.rigidbody.AddForceAtPosition(ec * bulletBehaviour.GetTransform().forward, hit.point);

	}
}