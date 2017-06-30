# Graduation work
Neuroevolution of Augmented Topologies - Wolf Van Herreweghe

## Index
[TOC]

## 1. Introduction
   In this paper we will describe the process of constructing an AI that will play a 3D shooter game VS another AI instance. The AI will be constructed using NEAT - (Neural Evolution of Augmented Topologies)

## 2. Game
   * Description
     
      To represent the AI used we will construct a small demo project, that will showcase an example use case of the AI. Here we have chosen for a small shooter game. Two teams stand opposite of each other. They have to navigate around objects, find targets, eliminate targets, take cover, and make predictions about the enemy's behavior. Agents should make decisions between running away, engaging in fire, or taking cover. Agents will be awarded points for their actions. The team to first eliminate the other team will claim victory.
 
       Future additional content might be:
      * Walls that players can shoot through
      * Capture the flag gamemode
      * VIP gamemode (one agent in each team is a 'VIP', the team loses as soon as this agent dies, thus it has to be protected by its teammates)
      * An ammo system
      * Weapons/Medkit/Armour drops around the map
      * Shoot through light materials
      * ...
(It should be noted that any imaginable variety of additional gamemodes and conditions can be successfully introduced to the game. As long as the inputs are in accord with the tasks they have to perform, the agents will eventually learn how to master these tasks.)

   * Mechanics
      * Navigation
      * Shooting
      * Ducking for cover

	All the players (agents) in the game will be AI driven. Since there is a lot of possible game strategies and methods of playing we should see quite some diversity between the agents, and allow for some unexpected battle situations.

While working on the NEAT AI I will progressively make  different test cases to isolate issues and resolve those.
The first project will be a color matching AI, where the AI is given a target color as input, and has to output the exact same color as output.
Another test project will be a simplified version of Snake.

## 3. AI
The AI that will be used for the agents is called NEAT (Neural Evolution of Augmented Topologies).

 * ### Basic NEAT
	NEAT exists of two big sub systems
	* #### Neural Networks
		'Neural Networks' can be best described like an real brain, with nodes and neurons (connections)

		![Basic Neural Network](http://i.imgur.com/uDgf5v9.png)

		We have an input and output, every input is connected to every output.
		Each connection has a certain 'weight' to it, this is how much influence the value will have on the end of the connection.
		We can also introduce a hidden layer to a network. See picture below
		
		![Hidden Neural Network](http://i.imgur.com/3JgqfHW.png)
		Once again, every input is connected to every node on the hidden layer, and every hidden node is connected to every out going connection.
		Of course we can add even more hidden nodes, and create a complex network, but that is beyond the scope of the explanation here.
		
	* #### Genetic Algorithms
	     'Genetic Algorithms' is a method we use to combine two neural networks in this scenario.

		Two preexisting points of data (the 'parents') are combined to produce two new points of data (the 'offspring') as explained in the figuren below.
		
		![Simple Crossover](http://i.imgur.com/Ap0XvbD.png)

		In the figure above you can see two parents (Right) that produce a set of offspring (Left). A random part from from parent #1 and parent #2 are passed to each point of data of the next generation. Note that offspring #2 will always be the exact opposite of offspring #1, meaning that all the parts that offspring #1 did not get will be given to offspring #2 and vice versa.

		The application of this process will be further explained in the topic on Network Crossover & Global innovation number
		
 * ### NEAT

	#### Basic life cycle
	![Basic Evolution](http://i.imgur.com/UUIPIR2.png)
	1. #### Populate
		All the agents are given their basic input nodes, output nodes, and possible hidden nodes.
		The previous (last generation), or a random weight is assigned to the connections.

	2. #### Mutate
		* Chance for a weight modification.
		* Chance for a new connection, with random weight value.
		* Chance for a new hidden node, on an existing connection.

		The new in-going connection from the new node will have a weight value of 1.
		The new outgoing connection from the new hidden node will have the same weight as the original connection.
		The original connection is disabled.

	3. #### Calculate
		In this step all the inputs will be transferred over to the network and the final result will be calculated.

	4. #### Fitness
		Calculate how well the AI handled the current task.

	5. #### Filter
		Remove outperforming networks.

	6. #### Probability
		Calculate how likly networks are to be selected for crossover

	7. #### Crossover
		Crossover 2 networks to create 1 or more offspring(s)
		More on this in the topic Network Crossover & Global innovation number
           
	8. #### Populate
		Fill the pool with parent networks to be adapted to the next generation

 * ### Genotype vs Phenotype
	* Genotype
		The way the data is represented in memory
	* Phenotype
		The way the data is represented to the end user (actions the agents take)
          
 * ### Network Crossover & Global innovation number
	Earlier in this document we described the basics of crossover in a conventional method, however to apply this same concept on neural networks we need to make some small modifications.
	The problem lays in the fact that comparing two networks for which connections they have would be virtually impossible, there is no construct way to know if a connection is connecting to the same nodes in the same way the first network did. Even if they are connected to the same nodes with the same index, this does not mean that the connection is the same.
	The network structures are unable to be compared to one another.
	This is where we introduce the Global innovation number. This is a global counter that will count up with a value of one, each time a new connection is being made. This number can then be used to track connections through time, and reveal how similar two networks are in structure.

	Now we have a starting point to apply crossover.

	![Complex Crossover](http://i.imgur.com/DVBiU6R.png)

	The two parent networks are compared in structure (global innovation number).
	Connections that are present in Partner #1 but not in Partner #2 are called disjoint.
	Connections that are not present in Partner #1 but are in Partner #2 and are have a smaller global innovation number than the highest global innovation number in Partner #1 are also called disjoint. Other connections are called excess. This will be important to compare the similarities within networks, read more on this in the Species topic.

	Connections are randomly chosen between Partner #1 and Partner #2, any connection that is disabled stays disabled. Not only are the connections transferred over, also the weight of the connections are maintained.

 * ### FSNEAT
       FS (automatic Feature Selection) is an method to build a better network for the desired task in a more direct way.  
       Normal Neural Networks have connections from all inputs to all outputs, also in NEAT we start with connections from all inputs to all outputs. However some of the inputs are less important, and might have data that has no need to be influencing the system.  
       Thus we let the network select it's own features by starting from a blank slate, where not a single connection is present. In the first generation when the very first connection is made we do force it to be a connection from input to output node.  
       Automatic Feature Selection limits the time the network needs to scale the important input weights versus the redundant connection weights.

 * ### rtNEAT
       rtNEAT (real time NEAT) is an implementation of neat where we update the agents in real time. 
       Every few game ticks we analyze the active systems. And update the neural network through genetic algorithms just like we are used to.  
       However, if we were to update all agents at once there would have a noticeable behavior change for all agents across the field. This would influence the player's experience.  
       To solve this issue we switch out agents dynamically, every few game ticks the weaker agents are replaced with an offspring agent from better performing agents. Thus we can dynamically see the improvement of the AI over time. This does mean that we will need more agents in the field, to blend transitions.  
       All agents are placed in the scene at the same time and update each other. This method also allows us to train the AI more specifically. Starting out with simple navigation, we can add more obstacles to the map to train the agents for their current task at hands.
		


 * ### Protection
	When a new node enters the system, the complexity of the system will increase,see the topic on minimification. However this additional node can have a huge benefit when given the time to mature.
	Two main ways to protect the system

	* Weight transfer
		When adding a new node on an existing connection we can transfer the existing weight of this connection, and thus the network should result almost the same fitness.
		The input connection weight is set to a value of 1.
		The outgoing connection weight is set to a value of the original connection.
	*  Protection through species
		Species will be explained more thoroughly in the next topic. Since the current network is part of a species, it will be protected by the neighboring systems.
		This will give it more chance to live through the next selection process.

 * ### Species
	Species is a method of grouping similar AI's together and sharing their strengths and weaknesses.
	All agents within the species their fitness will be accumulated for, and result in a average fitness factor for this species.
	The average fitness factor is used as influence whether their species have to be discontinued or not.
	The average fitness is calculated based on the Global rank of all the agents. This means that having one or more strong players within the species can contribute enough to allow it to stay in existence.
	Weaker agents will be protected by these stronger agents, and will have time to mature their networks to surpass the stronger agents.
	![Species Evolution](http://i.imgur.com/E99sDvL.png)
	
	The Evolution steps in a Species based NEAT AI are similar to the basic version of NEAT.
	Networks are grouped by the species.
	Grouping of networks is done by comparing how similar these 2 networks are, with the following formula:
		
	$$\delta = \frac{c_1E}{N} + \frac{c_2D}{N} + c_3\bar{W}\\
	E = \text{The amount of Excess} \\
	D = \text{The amount of Disjoint} \\
	\bar{W} = \text{Average weight difference}$$

	If a new network is not compatible with any other species, a new species is created.

	When all the agents have calculated their fitness, the average fitness is calculated for each species.
	First we have to rank all the networks on a global rank.
	```markdown
	global = []
	for species in pool
		for genome in species
			add genome to global 
			
	sort global by fitness
	counter = 0
	for genome in global 
		genome.globalRank = counter++
	```
	Calculating the average fitness for one species, the average fitness is based on the global ranks of every genome ( structure ) part of that species
	```markdown
	total = 0
	for genome in species
	    total += genome.globalRank
	species.averageFitness = total / species.genomes.Count
	```
	The average fitness is further used to discontinue species that are being out preformed.
	```markdown
	survived = [] 
    for species in pool
		rand = math.floor(species.averageFitness / totalAverageFitness * poolSize)
        if rand >= 1
	        add species to survived
	```
	We also track the staleness of a species. If a species didn't gain fitness in X-amount of generations, it will be discontinued. 
	```markdown
	survived = []
    for species in pool
		sort genomes by fitness               
	    if genomes[0].fitness > topFitness
	       topFitness = genomes[0].fitness
           staleness = 0
        else
		    staleness++
        if staleness < maxStaleCount or topFitness >= pool.topFitness
		    add species to survived 
	```
	
 * ### Activation
	Outputs of a network go through an activation function, this activation function will decide which action is applied or not.
	Just like physical buttons there is no way to 'half'-do a certain action.
	
	For the activation function we will use the sigmoid function:
	$$f_{(x)} = \frac{1}{1 + e^{-x}}$$
	
    ![Sigmoid Graph](http://i.imgur.com/3dJb4Ve.png)
	Shifting the function left and right will allow us to set a 'sensitivity'

 * ### Diversity
	The ultimate selection of agents is a variation of both a good fitness, but also a good amount of variation
	Some challenges found throughout the life span of an agent can only be solved by backtracking, even just a little.
	Thus we support variation throughout generations.
	Once again this is handled mostly by the use of species. Weaker species that can survive.
	![Diversity Graph](http://i.imgur.com/M9oeN3N.png)

 * ### Minimification
	Networks that grow to large in complexity are unwanted, these can only lead to situations where the AI spends a long time finding a good weight balance, and are thus frowned upon.
		
	While calculating the fitness, we apply an influence based on the size of the network. With the exception of the input and output nodes.

	Every other connection and or hidden node, will result in a small penalty for the current system. Even if systems obtain a good fitness factor, smaller, less complex networks, will be put forward and result in a higher probability to be kept in future generations.
		
 * ### Pool Selection vs Rejection Sampling
	#### Pool Selection
	All the actors are placed a pool for the amount of times they are likely to be picked.
	For example; Agents A, B, and C have a probability of 8, 2, and 1 respectively.
	Agent A would be placed in the pool 8 times,
	Agent B would be placed in the pool 2 times,
	Agent C would be placed in the pool 1 time.   

	Now we can randomly select agents, and maintain the same probability for each agent to be selected.
		
	![Pool Selection](http://i.imgur.com/Lfb58hr.png)

	#### Rejection Sampling
	A random agent is selected.
	If the random agents fitness is higher than a random number, (scaled to be between 0 - 1) then the agent is used as selection.
	If the agent does not meet the required fitness we select a new random agents

	![Rejection Sampling](http://i.imgur.com/EvKdlEN.png)

	The advantages of Rejection Sampling over Pool Selection is that less memory is consumed by a large pool of duplicate agents. The processing power required for Rejection Sampling is much lower, and thus will execute much faster compared to Pool Selection.

 * ### Inputs
	Picking a good set of inputs for the agents can make a noticeable different in the time required to generate a good AI structure.

	In this scenario we have divided to give agents a view cone, just like a human.
	The AI will be able to see enemies that are visible.
	On top of that the AI will be given a top down view of the map, with regions where the agents can take cover from enemy fire.
	The map will also show all friendly units, even if they can't actively see their allies.

	Agents are also given the amount of bullets left in their current clip, and their Health Points.
	So that they can make decisions based on their current situations.

	Future additional parameters might be:
		* Sound from nearby enemies
		* Position of explosive barrels (damage to area of effect)

 * ### Outputs
      The outputs of the AI will be the link to the players actual actions (phenotype)  
       
       List of outputs:
       * Walk forward
       * Rotate left/right
       * Shoot
       * Crouch
	
	More possible outputs might be added depending on test results.