[System.Serializable]
public static class Parameters
{
    public static int PoolSize = 200;

    public static int NumInputs = 5;
    public static int NumOutputs = 3;
    public static float Bias = -1;

    public static int NumAddLinkAttempts = 5;
    public static int NumTrysToFindLoopedLink = 5;
    public static int NumTrysToFindOldLink = 5;

    public static int YoungBonusAgeThreshhold = 10;
    public static float YoungFitnessBonus = 1.3f;
    public static int OldAgeThreshold = 50;
    public static float OldAgePenalty = 0.7f;

    public static float SurvivalRate = 0.2f;

    public static int NumGensAllowedNoImprovement = 15;
    public static int MaxPermittedLinks = 100;

    public static float ChanceAddLink = 0.07f;
    public static float ChanceAddNode = 0.04f;
    //public static float ChanceAddRecurrentLink = 0.05f;
    public static float ChanceWeightChange = 0.2f;
    public static float ChanceWeightReplaced = 0.1f;
    public static float MaxWeightPerturbatation = 0.5f;

    public static float ChanceDissableLink = 0.04f;
    public static float ChanceEnableLink = 0.2f;
    public static float MaxWeightPerturbation = 0.5f;

    public static float NodeSizePenalty = 0.01f;
    public static float LinkSizePenalty = 0.00f;

    public static float ActivationMutationRate = 0.1f;
    public static float MaxActivationPerturbation = 0.1f;

    public static float CompatibilityThreshold = 0.26f;

    public static int MaxNumberOfSpecies = 0;
    public static int StallNessFactor = 15;
    public static float CrossoverRate = 0.7f;

    public static float DeltaDisjoint = 2.0f;
    public static float DeltaWeights = 0.4f;
    public static float DeltaThreshold = 1.0f;
}
