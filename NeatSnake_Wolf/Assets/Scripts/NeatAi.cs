using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class NeatAi {

    public bool Initialized = false;

    public int GenerationCount;

    //public List<Generation> memory;
    public Generation CurrentGeneration;

    public Genome LastFittest;

    public void Initialize ()
    {

        Generation tempGeneration = new Generation();
        tempGeneration.Instantiate(GenerationCount);
        tempGeneration.FillGenerationStartGenomes();
        tempGeneration.SetIndeces();

        Debug.Log("Species Count: " + tempGeneration.Pool.Count);

        CurrentGeneration = tempGeneration;
        //memory.Add(tempGeneration);
    }
	
	public Genome Tick(List<float> inputs)
    { 

        CurrentGeneration.SetInputs(inputs);
        CurrentGeneration.Calculate();
        CurrentGeneration.CalcFitness();
        CurrentGeneration.RankGenomes();
        LastFittest = CurrentGeneration.GetFittestGenome();

        return LastFittest;
    }

    public void RankGenome(int generationIndex = 0)
    {
        CurrentGeneration.RankGenomes();
    }

    public void Evolve()
    {
        if (CurrentGeneration.Pool.Count == 1 )
        {
            int x = 5;
        }
        //Generation tempGeneration = ObjectCopier.Clone<Generation>(memory[generationCount]);
        Generation tempGeneration = ObjectCopier.Clone(CurrentGeneration);
        tempGeneration.RankGenomesGlobal();
        tempGeneration.CullSpecies();
        tempGeneration.RankGenomesGlobal();
        tempGeneration.RemoveStallSpecies();
        tempGeneration.RankGenomesGlobal();
        tempGeneration.CalculateAverageFitness();
        tempGeneration.RemoveWeakSpecies();
        tempGeneration.FillGeneration();
        tempGeneration.RankGenomesGlobal();
        tempGeneration.SetIndeces();

        Debug.Log("Species Count: " + tempGeneration.Pool.Count);

        CurrentGeneration = tempGeneration;
        //memory.Add(tempGeneration);
        GenerationCount++;
    }

    //TODO:REMOVE THIS OR UPDTE
    public void UpdateSingle(Genome genome, List<float> inputs, List<float> desired)
    {
        genome.SetInputs(inputs);
        genome.Calculate();
        genome.FitnessCalc(0);
    }

    //public void PrintAll(Vector3 pos)
    //{
    //    if (Application.isPlaying)
    //    {
    //        int offsetCounter = 0;
    //        for (int i = 0; i < memory.Count; i++)
    //        {
    //            int speciesOffset = 0;
    //            int width = 0;
    //            for (int k = 0; k < memory[i].Pool.Count; k++)
    //            {
    //                Species tempSpecies = memory[i].Pool[k];
    //                width = tempSpecies.GetLargestGenome().NodeCollection.Count - _inputCount - _outputCount + 3;

    //                tempSpecies.Print(pos, offsetCounter, speciesOffset, width, tempSpecies._index, tempSpecies.Genomes.Count);
    //                speciesOffset += tempSpecies.Genomes.Count;
    //            }
    //            offsetCounter += memory[i].GetLargestGenome().NodeCollection.Count - _inputCount - _outputCount + 3;
    //        }
    //    }
    //}

    //public void PrintBest(Vector3 pos)
    //{
    //    if (Application.isPlaying)
    //    {
    //        int offsetCounter = 0;
    //        for (int i = 0; i < memory.Count; i++)
    //        {
    //            Genome fittest = memory[i].GetFittestGenome();
    //            fittest.Print(pos, offsetCounter, fittest._genIndex, true); //TODO: REPLACE fittest._genIndex with Species index
    //            offsetCounter += fittest.NodeCollection.Count - _inputCount - _outputCount + 3;
    //        }
    //    }
    //}

    public int GetGenerationIndex()
    {
        return GenerationCount;
    }

    public Genome GetBestFitness()
    {
        //return memory[generationCount].GetFittestGenome();
        return CurrentGeneration.GetFittestGenome();
    }

    public Genome GetBestProbability()
    {
        //return memory[generationCount].GetProbabiliestGenome();
        return CurrentGeneration.GetProbabiliestGenome();
    }
}