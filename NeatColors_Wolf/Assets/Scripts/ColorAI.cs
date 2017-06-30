using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class ColorAI : MonoBehaviour {

    private Material _mat;

    public NeatAi AI;
    public int PoolSize = 2;

    public GameObject targetColorObj;
    public Color targetColor;

    List<float> desired;

    List<float> input;

	public GameObject DebugLocation;

    public Text Generation;
    public Text Fitness;
    public Text Similarity;
    public Text ColorCount;

    float timer = 0.0f;
    public float nextUpdateTime = 0.1f;

    public bool finished = false;
    public float targetFitness = 0.98f;

    int colorCounter = 0;

    Genome lastFittest;

	// Use this for initialization
	void Start () {
        _mat = GetComponent<Renderer>().material;
        targetColorObj.GetComponent<Renderer>().material.color = targetColor;

        desired = new List<float>();
        desired.Add(targetColor.r); //Red
        desired.Add(targetColor.g); //Green
        desired.Add(targetColor.b); //Blue

        input = new List<float>();
        input.Add(1.0f); //Red
        input.Add(1.0f); //Green
        input.Add(1.0f); //Blue

        AI = new NeatAi();
        lastFittest = AI.Instantiate(desired, input, PoolSize);
        Debug.Log("Species count: " + AI.memory[AI.memory.Count - 1].Pool.Count);

        List<float> fittest = lastFittest.GetOutputs();
        Color newColor = new Color(fittest[0], fittest[1], fittest[2]);
        _mat.color = newColor;

	    AI.RankGenome();

        float diff = 1.0f - Vector3.Distance(new Vector3(fittest[0], fittest[1], fittest[2]), new Vector3(targetColor.r, targetColor.g, targetColor.b));
        Fitness.text = lastFittest._fitness.ToString();
        Similarity.text = diff.ToString();
        ColorCount.text = colorCounter.ToString();
    }
	
	// Update is called once per frame
	void Update () {
        if (!finished)
        {
            timer += Time.deltaTime;
            if (timer > nextUpdateTime)
            {
                //NewRandomColor();
                NextTick();
                timer = 0.0f;
            }
        }
    }

    public void NextTick()
    {
        List<float> outputs = new List<float>();
        outputs = lastFittest.GetOutputs();
        float diff = 0.0f;

        bool skipLastCheck = false;
        if (lastFittest._fitness != 0)
        {
            AI.UpdateSingle(lastFittest, desired, desired);
            outputs = lastFittest.GetOutputs();
            diff = 1.0f - Vector3.Distance(new Vector3(outputs[0], outputs[1], outputs[2]), new Vector3(targetColor.r, targetColor.g, targetColor.b));
            skipLastCheck = true;
        }
        if (diff >= targetFitness && !skipLastCheck)
        {
            Color newColor = new Color(outputs[0], outputs[1], outputs[2]);
            _mat.color = newColor;

            NewRandomColor();
            colorCounter++;
        }
        else
        {
            AI.Evolve();
            lastFittest = AI.Tick(desired);
            //Debug.Log("Species count: " + AI.memory[AI.memory.Count - 1].Pool.Count);
        }
        outputs = lastFittest.GetOutputs();
        if (outputs.Count == 3)
        {
            Color newColor = new Color(outputs[0], outputs[1], outputs[2]);
            _mat.color = newColor;

            diff = 1.0f - Vector3.Distance(new Vector3(outputs[0], outputs[1], outputs[2]), new Vector3(targetColor.r, targetColor.g, targetColor.b));

            Generation.text = AI.GetGenerationIndex().ToString();
            Fitness.text = lastFittest._fitness.ToString();
            Similarity.text = diff.ToString();
            ColorCount.text = colorCounter.ToString();

            if (diff >= targetFitness)
            {
                //finished = true;
                NewRandomColor();
                colorCounter++;
            }
        }
        else
        {
            int x = 5;
        }
	}

    public void NewRandomColor()
    {
        targetColor = new Color(Random.value, Random.value, Random.value);
        targetColorObj.GetComponent<Renderer>().material.color = targetColor;
        desired = new List<float>() { targetColor.r, targetColor.g, targetColor.b };
        //finished = false;
    }

    void OnDrawGizmos()
    {
        if (AI.memory.Count > 0)
        {
            AI.PrintAll(DebugLocation.transform.position);
            //AI.PrintBest(DebugLocation.transform.position);
        }
    }

}
