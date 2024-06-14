README FISH SWARM

The SwarmManager prefab should be placed in the scene. 

	- a prefab for the individual fish must be specified
	- A goal game object must be specified. This defines the target position of the fish swarm

	- Swim Limits: the fish are randomly distributed within the specified area around the SwarmManager at the start
	- Num Fish: the number of fish in the swarm 
	- Obstacles: obstacles where the fish are blocked and cannot get through
	- Rejections: Obstacles that the school of fish recognizes early and can swim around
			- detection value: the distance value from which an obstacle is detected
			- rejection min&max speed: Rotation speed for turning away. A value in this range is calculated based on 
				calculated based on the distance to the obstacle 
	- Min & Max Speed: the fish are given a random speed in this range 
	- Neighbor Distance: the fish calculate rotation and direction based on their neighbors. This value determines which fish 
          possible neighbors based on their distance
	- Avoid Value: determines how much distance the fish keep to each other 

! many values influence each other. The Jobs and Burst Package is also required.
	:) 
