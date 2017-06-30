using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Random = UnityEngine.Random;

[Serializable]
public class Generation {

    //GLOBAL VARIABLES TEMP
    public static int GlobalSpeciesIndex;


    public int Index { get; private set; }

    public List<Species> Pool = new List<Species>();

    public float FitnessSom;

    public float MaxFitness;

    private readonly GeneticAlgorithms _ga = new GeneticAlgorithms();

    /// <summary>
    /// Instantiates the Generation
    /// </summary>
    public void Instantiate(int index)
    {
        Index = index;
        //InnovationManager.Reset();
    }

    /// <summary>
    /// Fills the generation with random new Genomes
    /// </summary>
    public void FillGenerationStartGenomes()
    {
        //Create a new pool of Genomes
        Pool = new List<Species>();
        for (int i = 0; i < Parameters.PoolSize; i++)
        {
            Genome tempGenome = new Genome(i);
            tempGenome.CreateBasicNodes();
            tempGenome.CreateRandomLink();
            AddToSpecies(tempGenome);
        }
    }

    /// <summary>
    /// Adds a Genome to a Species or creates a new one
    /// </summary>
    public Species AddToSpecies(Genome genome)
    {
        //looking for matching species and adding if existant
        foreach (Species species in Pool)
        {
            if (species.SameSpecies(species.Genomes[0], genome))
            {
                species.Genomes.Add(genome);
                return species;
            }
        }

        //No matching species, creating new one
        Species temp = new Species(GlobalSpeciesIndex++);
        temp.Genomes.Add(genome);
        Pool.Add(temp);
        return null;
    }

    /// <summary>
    /// Sets the index for all Genomes in all Species
    /// </summary>
    internal void SetIndeces()
    {
        int index = 0;
        foreach (Species species in Pool)
        foreach (Genome genome in species.Genomes)
        {
            genome.Index = index++;
            genome.SpeciesIndex = species.Index;
        }
    }

    /// <summary>
    /// Sets the inputs for all Genomes in all Species
    /// </summary>
    public void SetInputs(List<float> inputs)
    {
        foreach (Species species in Pool)
            species.SetInputs(inputs);
    }

    //public List<float> GetOutputs(int genomeNum)
    //{
    //	return pool[genomeNum].GetOutputs();
    //}

    /// <summary>
    /// Returns the Genome with the fittest from all Species
    /// </summary>
    public Genome GetFittestGenome()
    {
        var fittest = new Genome(0) {Fitness = float.MinValue};
        foreach (var species in Pool) {
            var genome = species.GetFittestGenome();
            if (genome.Fitness > fittest.Fitness)
                fittest = genome;
        }
        return fittest;
    }

    /// <summary>
    /// Returns the Genome with the highest probability from all Species
    /// </summary>
    public Genome GetProbabiliestGenome()
    {
        var probabilitiest = new Genome(0) {Probability = 0.0f};
        foreach (var species in Pool)
        {
            var genome = species.GetProbabiliestGenome();
            if (genome.Probability > probabilitiest.Fitness)
                probabilitiest = genome;
        }
        return probabilitiest;
    }

    /// <summary>
    /// Calcultes the Outputs of all Genomes in all Species for the current inputs
    /// </summary>
    public void Calculate()
    {
        foreach (var species in Pool)
            species.Calculate();
    }

    /// <summary>
    /// Calculates the fitness for all Genomes in all Species
    /// </summary>
	public void CalcFitness()
    {
        foreach (var species in Pool)
            species.CalcFitness();
    }

    //Deviating from the probability aproach and going full random!
    //public void CalcProbablity()
    //{
    //    foreach (Genome genome in pool)
    //    {
    //        genome._probability = genome._fitness / _fitnessSom;
    //    }
    //}

    /// <summary>
    /// Ranks all Genomes per Species and then all Species by their fittest Genome
    /// </summary>
    public void RankGenomes()
    {
        foreach (Species species in Pool)
            species.RankGenomes();

        Pool.Sort((x, y) => x.Genomes[0].Fitness.CompareTo(y.Genomes[0].Fitness));
        Pool.Reverse();
    }

    /// <summary>
    /// Ranks all Genomes globally
    /// </summary>
    public void RankGenomesGlobal()
    {
        var globalGenomes = Pool.SelectMany(species => species.Genomes).ToList();

        globalGenomes.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));

        for (int i = 0; i < globalGenomes.Count; i++)
            globalGenomes[i].GlobalRank = i;

        globalGenomes.Reverse();
    }

    public void SetGeneration(List<Species> newPool)
    {
        Pool = newPool;
    }

    /// <summary>
    /// Mutates all Genomes in all Species
    /// </summary>
    public void Mutate()
    {
        foreach (Species species in Pool)
            species.Mutate();
    }

    /// <summary>
    /// Returns the largest Genome in all Species
    /// </summary>
    public Genome GetLargestGenome()
    {
        Genome largest = new Genome(0);
        foreach (Species species in Pool)
        {
            Genome temp = species.GetLargestGenome();
            if (temp.NodeCollection.Count > largest.NodeCollection.Count)
                largest = temp;
        }

        return largest;
    }


    /// <summary>
    /// Compares 2 Genomes to see if they should be in the same Species
    /// </summary>
    public bool SameSpecies(Genome genome1, Genome genome2)
    {
        var diversity = Parameters.DeltaDisjoint * genome1.DisJoint(genome2);
        var weightDiversity = Parameters.DeltaWeights * genome1.WeightsDifference(genome2);
        return diversity + weightDiversity < Parameters.DeltaThreshold;
    }

    /// <summary>
    /// Removes half or all but one Genomes from all Species
    /// </summary>
    public void CullSpecies(bool cullToOne = false)
    {
        foreach (var species in Pool)
        {
            species.RankGenomes();
            var keepCount = cullToOne ? 1 : (int)Mathf.Ceil(species.Genomes.Count / 2.0f);
            species.Genomes.RemoveRange(keepCount, species.Genomes.Count - keepCount);
        }
    }

    /// <summary>
    /// Removes stale Species or will increase the stale counter
    /// </summary>
    public void RemoveStallSpecies()
    {
        if (Pool.Count == 1) return;

        List<Species> surviving = new List<Species>();
        foreach (Species species in Pool)
        {
            if (species.Genomes[0].Fitness > species.Topfitness)
                species.Topfitness = species.Genomes[0].Fitness;
            else
                species.Staleness++;

            if (species.Staleness < Parameters.StallNessFactor || species.Topfitness >= MaxFitness)
                surviving.Add(species);
        }

        Pool = surviving;
    }

    /// <summary>
    /// Removes weak Species compared to all other species
    /// </summary>
    public void RemoveWeakSpecies()
    {
        if (Pool.Count == 1) return;

        List<Species> surviving = new List<Species>();
        
        float totalAverageFitness = CalculateTotalAverageFitness();
        foreach (Species species in Pool)
        {
            if (species.Avgfitness / totalAverageFitness * Pool.Count >= Parameters.SurvivalRate )
            {
                surviving.Add(species);
            }
        }

        if (surviving.Count == 0)
        {
            surviving.Add(Pool[1]);
            //surviving.Add(Pool[2]);
        }

        Pool = surviving;
    }

    /// <summary>
    /// Calculates the _avgfitnees for each Species
    /// </summary>
    public void CalculateAverageFitness()
    {
        foreach (var species in Pool)
        {
            float speciesTotal = 0;
            foreach (var genome in species.Genomes)
            {
                speciesTotal += genome.GlobalRank;
                if (genome.Fitness > species.Topfitness) //Find the top fitness for this species
                    species.Topfitness = genome.Fitness;
                if (genome.Fitness > MaxFitness) //Find the overall best fitness
                    MaxFitness = genome.Fitness;
            }
            species.Avgfitness = speciesTotal / species.Genomes.Count;
        }
    }

    /// <summary>
    /// Calculates the total average fitness for all Species
    /// </summary>
    public float CalculateTotalAverageFitness()
    {
        return Pool.Sum(species => species.Avgfitness);
    }

    public void FillGeneration()
    {
        float totalAvgFitness = CalculateTotalAverageFitness();
        List<Genome> childeren = new List<Genome>();
        //List<Genome> current = GetUnsorted();

        foreach (Species species in Pool)
        {
            int breed = (int)Mathf.Floor((species.Avgfitness / totalAvgFitness) * Parameters.PoolSize);
            if (breed <= 0) break;
            for (int i = 0; i < breed - 1; i++)
                childeren.Add(BreedChild(species));
        }
        CullSpecies(true);
        while (childeren.Count + Pool.Count < Parameters.PoolSize)
            childeren.Add(BreedChild(GetRandomSpecies()));

        foreach (Genome child in childeren)
            AddToSpecies(child);
    }

    public Genome BreedChild(Species species)
    {
        Genome tempChild;
        if (Random.value < Parameters.CrossoverRate)
        {
            Genome parent1 = ObjectCopier.Clone(species.GetRandomGenome());
            Genome parent2 = ObjectCopier.Clone(species.GetRandomGenome());
            tempChild = _ga.CrossOver(parent1, parent2);
        }
        else
            tempChild = ObjectCopier.Clone(species.GetRandomGenome());

        tempChild.Mutate();

        return tempChild;
    }

    public Species GetRandomSpecies()
    {
        return Pool[Random.Range(0, Pool.Count)];
    }

    /// <summary>
    /// Returns a list of all genomes, without keeping their species in consideration
    /// </summary>
    public List<Genome> GetUnsorted()
    {
        return Pool.SelectMany(species => species.Genomes).ToList();
    }
}