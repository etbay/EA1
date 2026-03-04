using System.Collections;
using System.Collections.Generic;
//using Tripolygon.UModelerX.Runtime;
using UnityEngine;

public class ParticlesController: MonoBehaviour{

    [SerializeField] ParticleSystem orb ;

    public Color paintColor;
    
    public float minRadius = 0.05f;
    public float maxRadius = 0.2f;
    public float strength = 1;
    public float hardness = 1;
    [Space]
    ParticleSystem part;
    List<ParticleCollisionEvent> collisionEvents;

    void Start(){
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
        //var pr = part.GetComponent<ParticleSystemRenderer>();
        //Color c = new Color(pr.material.color.r, pr.material.color.g, pr.material.color.b, .8f);
        //paintColor = c;
    }
    private void Update()
    {
        // Get player speed
        float playerSpeed = 0f;
        if (PlayerCharacter.instance != null)
        {
            playerSpeed = PlayerCharacter.instance.GetState().Velocity.magnitude;
        }

        // Adjust main particle (collision) inherit velocity
        var inherit = part.inheritVelocity;
        inherit.enabled = true;
        inherit.mode = ParticleSystemInheritVelocityMode.Current;
        //inherit.curveMultiplier = 1.1f;


        // Adjust orb (visual) particle inherit velocity
        if (orb != null)
        {
            var orbInherit = orb.inheritVelocity;
            orbInherit.enabled = true;
            orbInherit.mode = ParticleSystemInheritVelocityMode.Current;
            //orbInherit.curveMultiplier = 1.1f;
        }


    }


    void OnParticleCollision(GameObject other) {
        Debug.Log("Collided with " + other.name);
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        Paintable p = other.GetComponent<Paintable>();
        if(p != null){
            for  (int i = 0; i< numCollisionEvents; i++){
                Vector3 pos = collisionEvents[i].intersection;
                float radius = Random.Range(minRadius, maxRadius);
                Vector3 normal = collisionEvents[i].normal;

                Debug.DrawRay(pos, normal * 0.5f, Color.green, 2f);
                Debug.DrawLine(pos, pos + Vector3.up * 0.2f, Color.yellow, 2f);

                PaintManager.instance.paint(p, pos, radius, hardness, strength, paintColor);
            }
        }
    }
}