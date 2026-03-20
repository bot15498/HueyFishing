using UnityEngine;

public class ParticleRingPull : MonoBehaviour
{
    public Transform target;
    public float pullSpeed = 8f;
    public float hitDistance = 0.2f;
    public TargetWhiteFlash flashTarget;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        int count = ps.particleCount;
        if (count == 0) return;

        if (particles == null || particles.Length < count)
            particles = new ParticleSystem.Particle[count];

        int alive = ps.GetParticles(particles);

        Vector3 targetPos = target.position;
        bool anyHit = false;

        for (int i = 0; i < alive; i++)
        {
            Vector3 worldPos;

            if (ps.main.simulationSpace == ParticleSystemSimulationSpace.World)
                worldPos = particles[i].position;
            else
                worldPos = transform.TransformPoint(particles[i].position);

            Vector3 dir = (targetPos - worldPos);
            float dist = dir.magnitude;

            if (dist > 0.001f)
            {
                float speedMultiplier = Mathf.Lerp(1f, 2.5f, 1f - Mathf.Clamp01(dist / 2f));
                Vector3 move = dir.normalized * pullSpeed * speedMultiplier * Time.deltaTime;
                if (move.magnitude > dist)
                    move = dir;

                worldPos += move;

                if (ps.main.simulationSpace == ParticleSystemSimulationSpace.World)
                    particles[i].position = worldPos;
                else
                    particles[i].position = transform.InverseTransformPoint(worldPos);
            }

            if (dist <= hitDistance)
            {
                particles[i].remainingLifetime = 0f;
                anyHit = true;
            }
        }

        ps.SetParticles(particles, alive);

        if (anyHit && flashTarget != null)
            flashTarget.Flash();
    }

    public void PlayRing(float loopRadius)
    {
        ps.Clear();
        var shape = ps.shape;
        shape.radius = loopRadius;
        pullSpeed = loopRadius;
        Debug.Log(loopRadius);
        ps.Play();

        Debug.Log("playparticles");
    }
}
