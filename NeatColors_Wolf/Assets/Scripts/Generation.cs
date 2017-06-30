using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Generation {

    //GLOBAL VARIABLES TEMP
    public static int globalSpeciesIndex = 0;

    float DeltaDisjoint = 2.0f;
    float DeltaWeights = 0.4f;
    float DeltaThreshold = 1.0f;

    int StallNessFactor = 15;
    float CrossoverChance = 0.75f;

    int _index;
    int _poolSize;
    int _inputCount;
    int _outputCount;

    public List<Species> Pool = new List<Species>();

    float _fitnessSom;

    float _maxFitness;

    /// <summary>
    /// Instantiates the Generation
    /// </summary>
    public Generation Instantiate(int index, int poolSize, int inputCount, int outputCount)
    {
        _index = index;
        _poolSize = poolSize;
        _inputCount = inputCount;
        _outputCount = outputCount;
        return this;
    }

    /// <summary>
    /// Fills the generation with random new Genomes
    /// </summary>
    public void FillGenerationStartGenomes()
    {
        //Create a new pool of Genomes
        Pool = new List<Species>();
        for (int i = 0; i < _poolSize; i++)
        {
            Genome tempGenome = new Genome(_index, i, _inputCount, _outputCount);
            tempGenome.CreateBasicNodes();
            tempGenome.CreateRandomConnection();
            AddToSpecies(tempGenome);
        }
    }

    /// <summary>
    /// Adds a Genome to a Species or creates a new one
    /// </summary>
    public void AddToSpecies(Genome genome)
    {
        //looking for matching species and adding if existant
        foreach (Species species in Pool)
        {
            if (species.SameSpecies(species.Genomes[0], genome))
            {
                species.Genomes.Add(genome);
                return;
            }
        }

        //No matching species, creating new one
        Species temp = new Species(globalSpeciesIndex++);
        temp.Genomes.Add(genome);
        Pool.Add(temp);
    }

    /// <summary>
    /// Sets the index for all Genomes in all Species
    /// </summary>
    internal void SetIndeces()
    {
        int index = 0;
        foreach (Species species in Pool)
            foreach (Genome genome in species.Genomes)
                genome._index = index++;
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
        Genome fittest = new Genome(0, 0, 0, 0);
        fittest._fitness = float.MinValue;
        foreach (Species species in Pool) {
            Genome genome = species.GetFittestGenome();
            if (genome._fitness > fittest._fitness)
                fittest = genome;
        }
        return fittest;
    }

    /// <summary>
    /// Returns the Genome with the highest probability from all Species
    /// </summary>
    public Genome GetProbabiliestGenome()
    {
        Genome Probabilitiest = new Genome(0, 0, 0, 0);
        Probabilitiest._probability = 0.0f;
        foreach (Species species in Pool)
        {
            Genome genome = species.GetProbabiliestGenome();
            if (genome._probability > Probabilitiest._fitness)
                Probabilitiest = genome;
        }
        return Probabilitiest;
    }

    /// <summary>
    /// Calcultes the Outputs of all Genomes in all Species for the current inputs
    /// </summary>
    public void Calculate()
    {
        foreach (Species species in Pool)
            species.Calculate();
    }

    /// <summary>
    /// Calculates the fitness for all Genomes in all Species
    /// </summary>
	public void CalcFitness(List<float> desired)
    {
        foreach (Species species in Pool)
            species.CalcFitness(desired);
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

        Pool.Sort((x, y) => x.Genomes[0]._fitness.CompareTo(y.Genomes[0]._fitness));
        Pool.Reverse();
    }

    /// <summary>
    /// Ranks all Genomes globally
    /// </summary>
    public void RankGenomesGlobal()
    {
        List<Genome> globalGenomes = new List<Genome>();
        foreach (Species species in Pool)
            foreach (Genome genome in species.Genomes)
                globalGenomes.Add(genome);

        globalGenomes.Sort((x, y) => x._fitness.CompareTo(y._fitness));
        //globalGenomes.Reverse();

        for (int i = 0; i < globalGenomes.Count; i++)
            globalGenomes[i]._globalRank = i;
    }

    //TODO REVISION of this segment!
    //public List<Genome> Selection()
    //{
    //    if (Pool.Count != _poolSize)
    //    {
    //        int k = 5;
    //    }

    //    int safety = 0;
    //    List<Genome> result = new List<Genome>();

    //    bool usePoolSelection = false;
    //    if (usePoolSelection)
    //    {
    //        //Pool Selection
    //        result.Add(Pool[0]);
    //        while (result.Count < _poolSize / 2 && safety < 10000)
    //        {
    //            int randomIndex = (int)Mathf.Floor(Random.value * _poolSize);
    //            if (Random.Range(0.0f, 1.0f / _poolSize) < Pool[randomIndex]._probability)
    //            {
    //                if (!result.Contains(Pool[randomIndex]))
    //                {
    //                    result.Add(Pool[randomIndex]);
    //                }
    //            }
    //            safety++;
    //        }
    //        if (safety >= 10000) Debug.LogError("infinit loop: Pool selection, result.count: " + result.Count);
    //    }
    //    else
    //    {
    //        //Rejection Sampling
    //        while (result.Count < _poolSize / 2 && safety < 50000)
    //        {
    //            int randomIndex = (int)Mathf.Floor(Random.value * Pool.Count);
    //            if (randomIndex >= Pool.Count)
    //                Debug.Log("Too big " + randomIndex + " pool size: " + _poolSize + " " + Pool.Count);
    //            else if (Random.value < Pool[randomIndex]._probability)
    //            {
    //                if (!result.Contains(Pool[randomIndex]))
    //                {
    //                    result.Add(Pool[randomIndex]);
    //                }
    //            }
    //            safety++;
    //        }
    //        if (safety >= 50000) Debug.Log("infinit loop: Rejection Sampling");
    //    }
    //    while (result.Count < _poolSize / 2)
    //    {
    //        int res = (int)Mathf.Floor(Random.value * result.Count);
    //        if (res >= result.Count)
    //            Debug.Log("Why won't this work! GRRRR");
    //        result.Add(result[res]);
    //    }
    //    if (result.Count != _poolSize / 2f)
    //    {
    //        Debug.Log("OH OH!");
    //    }
    //    return result;
    //}

    ////TODO update to species based generation filling
    //public List<Genome> FillNewGeneration(List<Genome> oldPool)
    //{
    //    List<Genome> result = ObjectCopier.Clone<List<Genome>>(oldPool);

    //    for (int i = 0; i < result.Count; i++)
    //    {
    //        result[i]._index = i;
    //    }
    //    for (int i = 0; i < oldPool.Count; i += 2)
    //    {
    //        Genome Offspring1 = new Genome(_index, i + oldPool.Count, oldPool[i]._inputCount, oldPool[i]._outputCount);
    //        Genome Offspring2 = new Genome(_index, i + 1 + oldPool.Count, oldPool[i]._inputCount, oldPool[i]._outputCount);
    //        //oldPool[i].Mate(oldPool[i+1], Offspring1, Offspring2); //TODO: Pick random friend! not just next friend! (Rejection sampling)
    //        //oldPool[i].Mate(GetRandomDiffGenome(oldPool, oldPool[i]), Offspring1, Offspring2);
    //        Offspring1.CreateBasicNodes();
    //        Offspring1.CreateExtendedNodes();
    //        Offspring2.CreateBasicNodes();
    //        Offspring2.CreateExtendedNodes();
    //        result.Add(Offspring1);
    //        result.Add(Offspring2);
    //    }
    //    return result;
    //}


    //TODO Revise if needed
    public void SetGeneration(List<Species> newPool)
    {
        Pool = newPool;
        //foreach (Genome genome in pool)
        //{
        //    genome._genIndex = _index;
        //}
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
        Genome largest = new Genome(0, 0, 0, 0);
        foreach (Species species in Pool)
        {
            Genome temp = species.GetLargestGenome();
            if (temp.NodeCollection.Count > largest.NodeCollection.Count)
                largest = temp;
        }

        return largest;
    }

    //TODO to be revised if still needed
    //public Genome GetRandomDiffGenome(List<Genome> selectionPool, Genome original)
    //{
    //    Genome selected = new Genome(0, 0, 0, 0);
    //    Genome empty = new Genome(0, 0, 0, 0);
    //    while (selected != original && selected != empty)
    //    {
    //        int randomIndex = (int)Mathf.Floor(Random.value * selectionPool.Count);
    //        if (Random.value < pool[randomIndex]._probability)
    //            selected = pool[randomIndex];
    //    }
    //    return selected;
    //}

    /// <summary>
    /// Compares 2 Genomes to see if they should be in the same Species
    /// </summary>
    public bool SameSpecies(Genome Genome1, Genome Genome2)
    {
        float diversity = DeltaDisjoint * Genome1.DisJoint(Genome2);
        float weightDiversity = DeltaWeights * Genome1.WeightsDifference(Genome2);
        return diversity + weightDiversity < DeltaThreshold;
    }

    /// <summary>
    /// Removes half or all but one Genomes from all Species
    /// </summary>
    public void CullSpecies(bool CullToOne = false)
    {
        foreach (Species species in Pool)
        {
            species.RankGenomes();
            int keepCount = CullToOne ? 1 : (int)Mathf.Ceil(species.Genomes.Count / 2.0f);
            species.Genomes.RemoveRange(keepCount, species.Genomes.Count - keepCount);
        }
    }

    /// <summary>
    /// Removes stale Species or will increase the stale counter
    /// </summary>
    public void RemoveStallSpecies()
    {
        List<Species> surviving = new List<Species>();
        foreach (Species species in Pool)
        {
            if (species.Genomes[0]._fitness > species._topfitness)
                species._topfitness = species.Genomes[0]._fitness;
            else
                species._staleness++;

            if (species._staleness < StallNessFactor || species._topfitness >= _maxFitness)
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
            if (Mathf.Floor(species._avgfitness / totalAverageFitness * _poolSize) >= 1.0f )
            {
                surviving.Add(species);
            }
        }

        if (surviving.Count < 2)
        {
            surviving.Add(Pool[1]);
        }

        Pool = surviving;
    }

    /// <summary>
    /// Calculates the _avgfitnees for each Species
    /// </summary>
    public void CalculateAverageFitness()
    {
        foreach (Species species in Pool)
        {
            float speciesTotal = 0;
            foreach (Genome genome in species.Genomes)
            {
                speciesTotal += genome._globalRank;
                if (genome._fitness > species._topfitness) //Find the top fitness for this species
                    species._topfitness = genome._fitness;
                if (genome._fitness > _maxFitness) //Find the overall best fitness
                    _maxFitness = genome._fitness;
            }
            species._avgfitness = speciesTotal / species.Genomes.Count;
        }
    }

    /// <summary>
    /// Calculates the total average fitness for all Species
    /// </summary>
    public float CalculateTotalAverageFitness()
    {
        float total = 0;
        foreach (Species species in Pool)
        {
            total += species._avgfitness;
        }
        return total;// / Pool.Count;
    }

    public void FillGeneration()
    {
        float totalAvgFitness = CalculateTotalAverageFitness();
        List<Genome> childeren = new List<Genome>();

        foreach (Species species in Pool)
        {
            int breed = (int)Mathf.Floor(species._avgfitness / totalAvgFitness * _poolSize);
            if (breed <= 0) break;
            for (int i = 0; i < breed - 1; i++)
                childeren.Add(BreedChild(species));
        }
        CullSpecies(true);
        while (childeren.Count + Pool.Count < _poolSize)
            childeren.Add(BreedChild(GetRandomSpecies()));

        foreach (Genome child in childeren)
            AddToSpecies(child);
    }

    public Genome BreedChild(Species species)
    {
        Genome tempChild = new Genome(0, 0, 0, 0);
        if (UnityEngine.Random.value < CrossoverChance)
        {
            Genome parrent1 = ObjectCopier.Clone<Genome>(species.GetRandomGenome());
            Genome parrent2 = ObjectCopier.Clone<Genome>(species.GetRandomGenome());
            tempChild = parrent1.Crossover(parrent2);
        }
        else
            tempChild = ObjectCopier.Clone<Genome>(species.GetRandomGenome());

        tempChild.Mutate();

        return tempChild;
    }

    public Species GetRandomSpecies()
    {
        int randIndex = (int)UnityEngine.Random.value * Pool.Count;
        return Pool[randIndex];
    }
}