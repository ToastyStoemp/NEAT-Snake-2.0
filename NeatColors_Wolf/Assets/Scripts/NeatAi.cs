using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;

[System.Serializable]
public class NeatAi {

    public int _poolSize,
               _inputCount,
               _outputCount;

    public bool _initialized = false;

    public int generationCount = 0;
    int genomeCount = 0;

    public List<Generation> memory;

    List<float> input;

    public Genome Instantiate (List<float> Desired, List<float> Input, int poolSize) {
        memory = new List<Generation>();

        _poolSize = poolSize;
        _inputCount = Input.Count;
        _outputCount = Desired.Count;

        input = Input;

        Generation tempGeneration = new Generation();
        tempGeneration.Instantiate(generationCount, _poolSize, _inputCount, _outputCount);
        tempGeneration.FillGenerationStartGenomes();
        tempGeneration.SetInputs(Desired);
        tempGeneration.Calculate();
        tempGeneration.CalcFitness(Desired);
        tempGeneration.RankGenomesGlobal();
        tempGeneration.CalculateAverageFitness();
        tempGeneration.SetIndeces();
        tempGeneration.RankGenomesGlobal();
        memory.Add(tempGeneration);

        return tempGeneration.GetFittestGenome();
    }
	
	public Genome Tick(List<float> desired)
    {
        memory[generationCount].SetInputs(desired);
        memory[generationCount].Calculate();
		memory[generationCount].CalcFitness(desired);
        //memory[generationCount].CalcProbablity();
        memory[generationCount].RankGenomes();
        //int fittestGenomeIndex = memory[generationCount].GetFittestGenome()._index;
        return memory[generationCount].GetFittestGenome(); //returns fittest genome
    }

    public void RankGenome(int generationIndex = 0)
    {
        memory[generationIndex].RankGenomes();
    }

    public void Evolve()
    {
        Generation tempGeneration = ObjectCopier.Clone<Generation>(memory[generationCount]);    
        tempGeneration.CullSpecies();
        tempGeneration.RankGenomesGlobal();
        tempGeneration.RemoveStallSpecies();
        tempGeneration.RankGenomesGlobal();
        tempGeneration.CalculateAverageFitness();
        tempGeneration.RemoveWeakSpecies();
        tempGeneration.FillGeneration();
        tempGeneration.RankGenomesGlobal();
        tempGeneration.SetIndeces();


        //memory[generationCount].Instantiate(generationCount, _poolSize, _inputCount, _outputCount);
        //memory[generationCount].SetGeneration(tempPool);
        //memory[generationCount].Mutate();
        memory.Add(tempGeneration);
        generationCount++;
    }

    public void UpdateSingle(Genome genome, List<float> inputs, List<float> desired)
    {
        genome.SetInputs(inputs);
        genome.Calculate();
        genome.FitnessCalc(desired);
    }

    public void PrintAll(Vector3 pos)
    {
        if (Application.isPlaying)
        {
            int offsetCounter = 0;
            for (int i = 0; i < memory.Count; i++)
            {
                int speciesOffset = 0;
                int width = 0;
                for (int k = 0; k < memory[i].Pool.Count; k++)
                {
                    Species tempSpecies = memory[i].Pool[k];
                    width = tempSpecies.GetLargestGenome().NodeCollection.Count - _inputCount - _outputCount + 3;

                    tempSpecies.Print(pos, offsetCounter, speciesOffset, width, tempSpecies._index, tempSpecies.Genomes.Count);
                    speciesOffset += tempSpecies.Genomes.Count;
                }
                offsetCounter += memory[i].GetLargestGenome().NodeCollection.Count - _inputCount - _outputCount + 3;
            }
        }
    }

    public void PrintBest(Vector3 pos)
    {
        if (Application.isPlaying)
        {
            int offsetCounter = 0;
            for (int i = 0; i < memory.Count; i++)
            {
                Genome fittest = memory[i].GetFittestGenome();
                fittest.Print(pos, offsetCounter, fittest._genIndex, true); //TODO: REPLACE fittest._genIndex with Species index
                offsetCounter += fittest.NodeCollection.Count - _inputCount - _outputCount + 3;
            }
        }
    }

    public int GetGenerationIndex()
    {
        return generationCount;
    }

    public Genome GetBestFitness()
    {
        return memory[generationCount].GetFittestGenome();
    }

    public Genome GetBestProbability()
    {
        return memory[generationCount].GetProbabiliestGenome();
    }
}

//first iteration -> test fitness
//Check fitness ranking
//Check diversity ranking ~
//combine ranks ~
//Decide probablity
//Crossover
//Mutation
//Copy to new generation
