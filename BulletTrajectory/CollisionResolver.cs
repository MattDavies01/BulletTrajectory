using UnityEngine;

[System.Serializable]
public class CollisionResolver {

	public LayerMask hitLayers = ~(1 << 8); 	
	public bool doRicochet = true;				
	public int maxBounces = 2;					
	[Range(0f, 1f)]
	public float ricochetFactor = 1f;			
	[Range(0f, 1f)]
	public float ricochetSpeedFactor = 0.5f;	
	[Range(0f, 180f)]
	public float maxRicochetAngle = 120f;		
	public float randomRicochetAngle = 0.1f;	

	private int bounces = 0;					
	private Vector3 previousPos;
	private Vector3 spawnpoint;
	private RaycastHit hit;

	private BulletBehaviour bulletBehaviour;
	private BulletKinematics bulletKinematics;

	public void Initiate(BulletBehaviour bulletBehaviour, Vector3 spawnpoint, BulletKinematics bulletKinematics) {
		this.bounces = 0;
		this.previousPos = spawnpoint;

		this.bulletBehaviour = bulletBehaviour;
		this.spawnpoint = spawnpoint;
		this.bulletKinematics = bulletKinematics;
	}
	public void UpdateCollisions() {

		if (previousPos == Vector3.zero) {
			previousPos = spawnpoint;
		}

		if (doLinecast() && (bulletBehaviour.GetTime() > 0.001f)) {
			if (hit.collider != bulletBehaviour.GetGameObject()) {

				bool maxBouncesNotReached = bounces < maxBounces;
				bool angleCondition = Vector3.Angle(bulletBehaviour.GetTransform().forward, hit.normal) <= maxRicochetAngle;
				bool ricochetFactorCondition = Random.Range(0f, 1f) < ricochetFactor;

				if (maxBouncesNotReached && doRicochet && (angleCondition && ricochetFactorCondition)) {
					
					doBounce();
				} else {
					
					bulletBehaviour.OnBulletCollision(hit, false);
				}
			}
		}

		previousPos = bulletBehaviour.GetTransform().position;
	}

	private void doBounce() {

		bounces++;

		
		bulletBehaviour.OnBulletCollision(hit, true);

		Vector3 reflectDirection = getReflectDirection();

		bulletBehaviour.GetTransform().forward = reflectDirection;
		bulletBehaviour.GetTransform().position = hit.point;

		
		bulletKinematics.velocity = bulletKinematics.VelocityAtTime(
			bulletBehaviour.GetTime()).magnitude * reflectDirection * ricochetSpeedFactor;
        
		bulletBehaviour.SetTime(0.0f);
		bulletKinematics.v0 = bulletKinematics.velocity;
		bulletKinematics.p0 = bulletBehaviour.GetTransform().position;

		bulletBehaviour.AdvanceTime(0.0f);

		spawnpoint = bulletBehaviour.GetTransform().position;
		previousPos = bulletBehaviour.GetTransform().position;
	}

	private bool doLinecast() {
		return Physics.Linecast(previousPos, bulletBehaviour.GetTransform().position, out hit, hitLayers);
	}

	private Vector3 getReflectDirection() {
		Vector3 reflectDirection = Vector3.Reflect(bulletBehaviour.GetTransform().forward, hit.normal);

		reflectDirection += new Vector3(
			Random.Range(0, randomRicochetAngle),
			Random.Range(-randomRicochetAngle, randomRicochetAngle),
			Random.Range(-randomRicochetAngle, randomRicochetAngle));

		return reflectDirection;
	}
}