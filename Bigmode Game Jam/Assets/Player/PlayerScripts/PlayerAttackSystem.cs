using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using EZCameraShake;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Debug = UnityEngine.Debug;

public class PlayerAttackSystem : MonoBehaviour
{
    struct TracerDetails
    {
        public BulletTracer tracer;
        public Vector3 spawn;
        public Vector3 end;
        public float width;
        public float decay;
        public bool isHit;
        public RaycastHit hit;
        public TracerDetails(BulletTracer bulletTracer, Vector3 position, Vector3 point, float tracerWidth, float tracerDecay, bool v, RaycastHit hit) : this()
        {
            this.tracer = bulletTracer;
            this.spawn = position;
            this.end = point;
            this.width = tracerWidth;
            this.decay = tracerDecay;
            this.isHit = v;
            this.hit = hit;
        }
    }

    [SerializeField] private Camera playerCamera;
    [SerializeField] private WeaponSFXBank sfxBank;
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private ParticleSystem impact;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Light muzzleFlashLight;
    [SerializeField] private BulletTracer tracerPrefab;
    [SerializeField] private BulletTracer timeSlowTracerPrefab;
    [SerializeField] private Animator animator;

    [SerializeField] private float tracerDecay = 0.6f;
    [SerializeField] private float tracerWidth = 0.7f;
    [SerializeField] private float shotDelay = 0.05f;
    [SerializeField] private float lightTimer = 0.08f;
    [SerializeField] private float lightIntensity = 5f;

    [Header("Camera Shake")]
    [SerializeField] private float shakeMagnitude;
    [SerializeField] private float shakeRoughness;
    [SerializeField] private float shakeDuration;

    private bool _reqestedAttack = false;
    // private PlayerInputActions _inputActions;
    private int targetsShotInSlow = 0;
    private float delayTimer;
    private Queue<TracerDetails> tracerTracker = new Queue<TracerDetails>();
    
    // Object Pools for reusing effects
    ObjectPool tracerPool;
    ObjectPool timeSlowTracerPool;
    ObjectPool impactParticles;
    ObjectPool hitLights;

    private void Start()
    {
        tracerPool = gameObject.AddComponent<ObjectPool>();
        tracerPool.GeneratePool(15, tracerPrefab.gameObject);

        timeSlowTracerPool = gameObject.AddComponent<ObjectPool>();
        timeSlowTracerPool.GeneratePool(15, timeSlowTracerPrefab.gameObject);

        impactParticles = gameObject.AddComponent<ObjectPool>();
        impactParticles.GeneratePool(15, impact.gameObject);

        hitLights = gameObject.AddComponent<ObjectPool>();
        hitLights.GeneratePool(15, muzzleFlashLight.gameObject);
        muzzleFlashLight.enabled = false;
    }

    public void updateInput(CharacterInput input)
    {
        _reqestedAttack = input.Attack;
    }

    private void Update()
    {
        if (delayTimer <= shotDelay)
        {
            delayTimer += Timeslow.IsSlowed ? Time.deltaTime / Timeslow.slowFactor : Time.deltaTime;
        }
        
        if (_reqestedAttack && delayTimer >= shotDelay)
        {
            Shoot();
        }
        else if (_reqestedAttack && delayTimer + (shotDelay / 4) >= shotDelay)
        {
            StartCoroutine(BufferShoot());
        }
    }

    private void Shoot()
    {
        delayTimer = 0;

        animator.Play("firing", -1, 0f);
        muzzleFlash.Play();

        CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRoughness, 0.1f, shakeDuration);

        if (Timeslow.IsSlowed)
        {
            targetsShotInSlow += 1;
        }
        StartCoroutine(MuzzleFlash(muzzleFlashLight, lightTimer, lightIntensity));
        RaycastHit hit;
        // Adding playercamera.transform.forward fixes a bug where the player could hit their own collider if moving backwards and shooting
            if (Physics.Raycast(playerCamera.transform.position + (playerCamera.transform.forward * .55f), playerCamera.transform.forward, out hit))
            {
                var target = hit.collider.gameObject;
                if (target.GetComponent<Destructible>() != null)
                {
                    AudioManager.instance.PlayOmnicientSoundClip(sfxBank.HitDing(), 1f, false, true);
                    if (Timeslow.IsSlowed)
                    {
                        target.GetComponent<Destructible>().Kill(targetsShotInSlow);
                    }
                    else
                    {
                        target.GetComponent<Destructible>().Kill(0);
                    }
                }
                if (Timeslow.IsSlowed)
                {
                    // Shoot out a timeslow indicator
                    var tracer = timeSlowTracerPool.RequestAndReturnToPool();
                    tracer.GetComponent<BulletTracer>().FireTracer(bulletSpawn.position, hit.point, tracerWidth);

                    // Wait to display most effects if time is slowed
                    // Add real tracers to the tracker pool
                    tracer = tracerPool.RequestAndReturnToPool();
                    tracerTracker.Enqueue(new TracerDetails(tracer.GetComponent<BulletTracer>(), bulletSpawn.position, hit.point, tracerWidth, tracerDecay, true, hit));
                }
                else
                {
                    var tracer = tracerPool.RequestAndReturnToPool();
                    tracer.GetComponent<BulletTracer>().FireTracer(bulletSpawn.position, hit.point, tracerWidth, tracerDecay);
                    AudioManager.instance.PlaySoundClipFromList(sfxBank.HitSounds(), hit.point, 1f, true, true);
                    ActivateImpactParticles(hit);
                }

            }
            else
            {
                Vector3 maxDistance = playerCamera.transform.position + playerCamera.transform.forward * 100f;
                if (Timeslow.IsSlowed)
                {
                    // Shoot out a timeslow indicator
                    var tracer = timeSlowTracerPool.RequestAndReturnToPool();
                    tracer.GetComponent<BulletTracer>().FireTracer(bulletSpawn.position, maxDistance, tracerWidth);

                    // Wait to display most effects if time is slowed
                    tracer = tracerPool.RequestAndReturnToPool();
                    tracerTracker.Enqueue(new TracerDetails(tracer.GetComponent<BulletTracer>(), bulletSpawn.position, maxDistance, tracerWidth, tracerDecay, false, new RaycastHit()));
                }
                else
                {
                    var tracer = tracerPool.RequestAndReturnToPool();
                    tracer.GetComponent<BulletTracer>().FireTracer(bulletSpawn.position, maxDistance, tracerWidth, tracerDecay);
                }
            }
            AudioManager.instance.PlayOmnicientSoundClip(sfxBank.GunshotSound(), 1f, true, true);
    }

    public IEnumerator FireTrackedTracers()
    {
        while (Timeslow.IsSlowed)
        {
            yield return null;
        }
        while (tracerTracker.Count > 0)
        {
            yield return new WaitForSeconds(1/15f);
            targetsShotInSlow = 0;
            var tracer = tracerTracker.Dequeue();
            tracer.tracer.FireTracer(tracer.spawn, tracer.end, tracer.width, tracer.decay);
            // tuple is: bullettracer, its recorded spawn, its recorded end point, its recordedwidth, and its recorded decayrate
            AudioManager.instance.PlaySoundClipFromList(sfxBank.TracerSounds(), tracer.spawn, 1f, true, true);
            AudioManager.instance.PlaySoundClipFromList(sfxBank.HitSounds(), tracer.end, 1f, true, true);
            if (tracer.isHit)
            {
                ActivateImpactParticles(tracer.hit);
            }
        }
    }
    
    private IEnumerator BufferShoot()
    {
        while (delayTimer < shotDelay)
        {
            yield return null;
        }
        Shoot();
    }
    private IEnumerator MuzzleFlash(Light light, float time, float intensity)
    {
        light.enabled = true;
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            light.intensity = Mathf.Lerp(intensity, 0f, elapsedTime);
            yield return null;
        }
        light.enabled = false;
    }
    private void ActivateImpactParticles(RaycastHit hit)
    {
        ParticleSystem impactFX = impactParticles.RequestAndReturnToPool().GetComponent<ParticleSystem>();
        impactFX.gameObject.transform.position = hit.point;
        impactFX.gameObject.transform.forward = hit.normal;
        impactFX.Play();
        
        Light hitLight = hitLights.RequestAndReturnToPool().GetComponent<Light>();
        hitLight.transform.position = hit.point + hit.normal * 0.2f; // spawn a point light where it hits
        hitLight.range *= 3;
        StartCoroutine(MuzzleFlash(hitLight, lightTimer, lightIntensity / 2));
    }


    private IEnumerator BulletDecay(LineRenderer line, Vector3 start, Vector3 end, float time)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;

            // Move the start position towards the end position, creating a trailing effect
            line.SetPosition(0, Vector3.Lerp(start, end, t));
            line.SetPosition(1, end);

            yield return null;
        }

        line.gameObject.SetActive(false);
    }
}
