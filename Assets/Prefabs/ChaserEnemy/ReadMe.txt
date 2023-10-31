ReadMe

Steps to setup the enemy:

1.drag the 'enemy_chaser' prefab in to scene

2.drag the 'Pathfinding_environment' prefab in to scene

3.drag the 'Pathfinding_perciever' prefab in to scene

4.on the gameobject 'Pathfinding_perciever', find the component 'AIM steering perciever', assign the envrionment element to be 'Pathfinding_environment'

5.make sure the tag of the player is set to 'Enemy_chaser_follow'

6.set the layer of any gameobejct that the enemy needs to avoid to be 'Enemy_chaser_avoid'