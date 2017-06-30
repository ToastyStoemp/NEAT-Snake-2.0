using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
[System.Diagnostics.DebuggerDisplay("ID = {_index}, topFitness = {_topfitness}")]
public class Species
{
    public int _index;

    public float _topfitness = float.MinValue;
    public float _staleness;
    public float _avgfitness;
    public List<Genome> Genomes = new List<Genome>();


    public float DeltaDisjoint = 2.0f;
    public float DeltaWeights = 0.4f;
    public float DeltaThreshold = 1.0f;

    public Species(int speciesIndex)
    {
        _index = speciesIndex;
    }


    /// <summary>
    /// returns if 2 genomes are similar enough to fit in the same species
    /// </summary>
    public bool SameSpecies(Genome genome1, Genome genome2)
    {
        float dd = DeltaDisjoint * genome1.DisJoint(genome2);
        float dw = DeltaWeights * genome1.WeightsDifference(genome2);

        return dd + dw < DeltaThreshold;
    }

    /// <summary>
    /// Sets the inputs for all Genomes in this species
    /// </summary>
    public void SetInputs(List<float> inputs)
    {
        foreach (Genome genome in Genomes)
            genome.SetInputs(inputs);
    }

    /// <summary>
    /// Returns the fittest Genome of this species
    /// </summary>
    public Genome GetFittestGenome()
    {
        Genome fittest = new Genome(0, 0, 0, 0);
        fittest._fitness = float.MinValue;
        foreach (Genome genome in Genomes)
            if (genome._fitness > fittest._fitness)
                fittest = genome;

        return fittest;
    }

    /// <summary>
    /// Returns the Genome with the highest probability of this species
    /// </summary>
    public Genome GetProbabiliestGenome()
    {
        Genome Probabilitiest = new Genome(0, 0, 0, 0);
        Probabilitiest._probability = 0.0f;
        foreach (Genome genome in Genomes)
            if (genome._probability > Probabilitiest._fitness)
                Probabilitiest = genome;

        return Probabilitiest;
    }

    /// <summary>
    /// Calculates the outputs for each genome in Genomes
    /// </summary>
    public void Calculate()
    {
        foreach (Genome genome in Genomes)
            genome.Calculate();
    }

    /// <summary>
    /// Calculates the fitness for each genome in Genomes
    /// </summary>
    public void CalcFitness(List<float> desired)
    {
        foreach (Genome genome in Genomes)
        {
            genome.FitnessCalc(desired);
            //_fitnessSom += Mathf.Clamp01(genome._fitness); //We will need the fitness som to calculate probability
        }
        //if (_fitnessSom == 0)
        //    _fitnessSom = 0.01f;
    }

    /// <summary>
    /// Ranks all the genomes within the species
    /// </summary>
    public void RankGenomes()
    {
        Genomes.Sort((x, y) => x._fitness.CompareTo(y._fitness));
        Genomes.Reverse();
        
        //We don't update the _topfitness yet, we will do this in the StallMethod,
        //so we can keep better track of which species are not making any progress
    }

    /// <summary>
    /// Mutates all the genomes within the species
    /// </summary>
    public void Mutate()
    {
        foreach (Genome genome in Genomes)
            genome.Mutate();
    }

    /// <summary>
    /// Returns a random Genome from the same species
    /// </summary>
    public Genome GetRandomGenome()
    {
        int randomIndex = (int)Mathf.Floor(Random.value * Genomes.Count);
        return Genomes[randomIndex];
    }

    /// <summary>
    /// Returns the largest Genome in structure
    /// </summary>
    public Genome GetLargestGenome()
    {
        Genome largest = new Genome(0, 0, 0, 0);
        foreach (Genome gen in Genomes)
            if (gen.NodeCollection.Count > largest.NodeCollection.Count)
                largest = gen;

        return largest;
    }

    public void Print(Vector3 pos, int offsetCounter, int speciesOffset, int width, int speciesIndex, int speciesCount)
    {
        float ypos = pos.y - speciesOffset * (3 * 1) - 3 * speciesOffset;
        Vector3 TopLeft = new Vector3(pos.x + offsetCounter - 1f, ypos + 2.5f, pos.z);
        Vector3 TopRight = TopLeft;
        TopRight.x += width - 0.1f;
        Vector3 BottomLeft = TopLeft;
        BottomLeft.y -= (6 - 0.5f) * speciesCount + 0.5f * speciesCount - 0.5f;
        Vector3 BottomRight = TopRight;
        BottomRight.y = BottomLeft.y;

        switch (speciesIndex % 10)
        {
            case 0:
                Gizmos.color = Color.blue;
                break;
            case 1:
                Gizmos.color = Color.black;
                break;
            case 2:
                Gizmos.color = Color.cyan;
                break;
            case 3:
                Gizmos.color = Color.green;
                break;
            case 4:
                Gizmos.color = Color.magenta;
                break;
            case 5:
                Gizmos.color = Color.red;
                break;
            case 6:
                Gizmos.color = Color.white;
                break;
            case 7:
                Gizmos.color = Color.yellow;
                break;
            case 8:
                Gizmos.color = Color.gray;
                break;
            case 9:
                Gizmos.color = new Color(188,51,178);
                break;
        }
        Gizmos.DrawLine(TopLeft, TopRight);
        Gizmos.DrawLine(BottomLeft, BottomRight);
        Gizmos.DrawLine(TopLeft, BottomLeft);
        Gizmos.DrawLine(TopRight, BottomRight);

        foreach (Genome genome in Genomes)
        {
            genome.Print(pos, offsetCounter, speciesIndex);
        }
    }
}