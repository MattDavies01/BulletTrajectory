using UnityEngine;

public class BulletKinematics {

	public Vector3 velocity;          

	private float k;                    
	private Vector3 vInfinity;          
	public Vector3 v0;                  
	public Vector3 p0;                  

	private Vector3 _angularVelocity;  
	private Vector3 _windVelocity;    
	private float _headingFrequency;    
	private float _headingDamping;      

	public void Initiate(Vector3 currentPosition, Vector3 gravity, Vector3 windVelocity, Vector3 initialVelocity, 
		float terminalVelocity, float rotationStiffness, float rotationDamping) {

		float gravityLength = gravity.magnitude;
		k = 0.5f * gravityLength / terminalVelocity;

		if (gravity == Vector3.zero) { 
			vInfinity = Vector3.zero;
		} else {
			vInfinity = gravity * (terminalVelocity / gravityLength) + windVelocity;
		}

		velocity = initialVelocity;
		v0 = velocity;
		p0 = currentPosition;

		_headingFrequency = Mathf.Sqrt(k) * rotationStiffness;
		_headingDamping = rotationDamping;
		_windVelocity = windVelocity;
	}

	public void UpdateVelocity(float t) {
		velocity = VelocityAtTime(t);
	}

	public Vector3 PositionAtTime(float t) {
		float kt = k * t;
		return (v0 + vInfinity * kt) * t / (1 + kt) + p0;
	}
	public Vector3 VelocityAtTime(float t) {
		float kt = k * t;
		float h = 1 + kt;
		return (v0 + kt * (2 + kt) * vInfinity) / (h * h);
	}

	
	public Quaternion UpdateRotation(float deltaTime, Transform rotationTransform) {
		Vector3 forward = rotationTransform.forward;
		Vector3 relativeVelocity = velocity - _windVelocity;
		Vector3 sin = Vector3.Cross(relativeVelocity, forward);
		float cos = Vector3.Dot(relativeVelocity, forward);

		Vector3 angularError = 3 * sin / (2 * relativeVelocity.magnitude + cos + Mathf.Epsilon);

		float wt = _headingFrequency * deltaTime;
		_angularVelocity = (_angularVelocity - _headingFrequency * wt * angularError) /
						   (1 + wt * (2 * _headingDamping + wt));
		Vector3 angularDelta = _angularVelocity * deltaTime;

		Quaternion q = new Quaternion(angularDelta.x, angularDelta.y, angularDelta.z,
									  2.0f - 0.125f * angularDelta.sqrMagnitude);

		return q * rotationTransform.rotation;
	}

	public void DebugDrawLastTrajectory(float _time, float timeToTarget) {
		int numSegments = 20;
		int numSamples = numSegments * 2 + 4;
		float dt = timeToTarget / (numSamples - 4);
		float t = Time.timeSinceLevelLoad / (dt * 4);
		t = (t - Mathf.Floor(t)) * dt * 4 - dt * 4;
		Color black = new Color(0, 0, 0, 0.75f), white = new Color(1, 1, 1, 0.75f);

		for (int i = 0; i < numSamples; ++i) {
			float fromTime = Mathf.Clamp(t, 0.0f, timeToTarget);
			float toTime = Mathf.Clamp(t + dt, 0.0f, timeToTarget);
			if (fromTime > _time)
				Debug.DrawLine(PositionAtTime(fromTime), PositionAtTime(toTime), i % 4 < 2 ? black : white);
			else
				Debug.DrawLine(PositionAtTime(fromTime), PositionAtTime(toTime), Color.red);

			t += dt;
		}
	}
}