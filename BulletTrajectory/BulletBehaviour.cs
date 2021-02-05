using UnityEngine;

public class BulletBehaviour : MonoBehaviour {

	public string POOLTAG = "bullet";

	public bool debugTrajectory = true;

	
	public float lifeTime = 5f;

	[System.Serializable]
	public class KinematicProperties 
    {
		public Vector3 gravity = new Vector3(0, -9.81f, 0);

		
		public Vector3 windVelocity = Vector3.zero;
             
		public float terminalVelocity = 35;

		public bool updateRotation = true;

		
		public float rotationStiffness = 3.0f;

		public float rotationDamping = 0.3f;
	}
	public KinematicProperties kinematicProperties;
	public CollisionResolver collisionResolver;
	public PhysicalInteractor physicalInteractor;

	BulletKinematics bulletKinematics;
	private float _time;                // time active

	public void Awake() {
		bulletKinematics = new BulletKinematics();	
	}

	public void StartTrajectory(Vector3 initialVelocity) {
		_time = 0f;
		collisionResolver.Initiate(this, transform.position, bulletKinematics);

		physicalInteractor.Initiate(this, bulletKinematics);

		bulletKinematics.Initiate(
			transform.position,
			kinematicProperties.gravity,
			kinematicProperties.windVelocity, 
			initialVelocity,
			kinematicProperties.terminalVelocity,
			kinematicProperties.rotationStiffness,
			kinematicProperties.rotationDamping);
	}
	
	public void OnBulletCollision(RaycastHit hit, bool canBounce) {
		physicalInteractor.OnBulletCollision(hit, canBounce);
	}

	void FixedUpdate() {
		AdvanceTime (Time.deltaTime > 1f ? 0.01f : Time.deltaTime);
	}

	public void SetTime(float newTime) {
		_time = newTime;
	}

	public float GetTime() {
		return _time;
	}

	public Transform GetTransform() {
		return transform;
	}

	public GameObject GetGameObject() {
		return gameObject;
	}
	public void AdvanceTime(float deltaTime) {
		_time += deltaTime;

		
		if (_time > lifeTime) {
			RemoveBullet(null);
		}
		transform.position = bulletKinematics.PositionAtTime(_time);

		bulletKinematics.UpdateVelocity(_time);

		if (kinematicProperties.updateRotation) {
			Quaternion newRotation = bulletKinematics.UpdateRotation(deltaTime, transform);
			transform.rotation = newRotation;
        }
		collisionResolver.UpdateCollisions();

		if (debugTrajectory)
			bulletKinematics.DebugDrawLastTrajectory(_time, lifeTime);

	}

	public void RemoveBullet(string replacementHash) {
		if (replacementHash != null && !"".Equals(replacementHash)) {
			GameObject go = PoolManager.getInstance().getObject(replacementHash);
			go.transform.position = bulletKinematics.PositionAtTime(_time - Time.deltaTime);
			go.transform.rotation = transform.rotation;
			go.GetComponent<Rigidbody>().velocity = bulletKinematics.VelocityAtTime(_time);
		}

		PoolManager.getInstance().recycleObject(POOLTAG, gameObject);
	}

	public void InstantiateEffects(GameObject hitEffects, RaycastHit hit) {
		Instantiate(hitEffects, hit.point, Quaternion.FromToRotation(hit.transform.up, hit.normal));
	}
}