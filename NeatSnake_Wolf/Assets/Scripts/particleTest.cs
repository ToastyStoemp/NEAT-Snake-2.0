using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleTest : MonoBehaviour
{
    private float counter = 0;
    private float timer = 5.0f;

    public ParticleSystem particles;

	// Use this for initialization
	void Start () {
		particles.Play();
	}
	
	// Update is called once per frame
	void Update ()
	{
	    counter += Time.deltaTime;
	    if (counter >= timer)
	    {
	        particles.Stop();
	    }

        Destroy(particles);
	}

    void OnTriggerEnter(Collider other)
    {
        particles.Stop();
    }
}
