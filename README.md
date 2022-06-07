# SuToRyKo project
## Synopsis
SuToRyKo is a classic RTS game, in which you need to destroy the enemy base.
Using the different units at your disposal, you will need to capture control points, acquire more ressources, and build new units and their buildings.
Outwit the enemy AI, destroy their buildings, steal their control points, and restaure peace to the battlefield!
## Authors

- William Nardone
- Ryan Sandrin
- Alex Figueiredo
- Hugo Chemouny
## Deployment

To deploy the game on Windows, simply launch the executable from within the build file you downloaded.

## Inputs

Left click: Select a unit/building, hold to select multiple. Hold down left Ctrl with a click to *add* to the current selection. Also used to interact with the UI.

Middle click / Keyboard arrows: Hold to move around the map.  
Mouse wheel: Zoom in/out.  

Right click: Move units to location of click. Automatically initates attack stances if an enemy unit/building is clicked.  

Ctrl + A: Select all of your units.

C: Cancel factory placement, and the fabrication of new units.

A: Selected units go in attack stances.  
S: Selected units go in idle.  
Delete: Selected units are destroyed.
Numpad 1\~2\~3: Selected units go into a formation.
+: Selected units' group is saved.
## Features

### AI Planner and layers:

The enemy AI is separated in three layers.  
At the top of the hierachy is the Strategist, who controls buildings and creates/gives orders to the second layer: Tactitians.  
Tactitians are groups of units. Players have them generated automatically for selected groups of units.
Units receive and carry out orders from Tactitians.

While the units and the Tactitians have simple AI states, meant to execute actions without being dependent of another state, 
the AI Strategist has a planner, with dynamic task influence/cost analysis and flexible execution: It both orders its potential tasks in an advantaging order, 
and executes them in order, skipping some if it does not have enough ressources to fuffil the cost.

### Interactive formations:

With every selection (especially for the player), a new group of units is formed.  
This group can either be moved as usual, or you can apply formations using UI buttons, or the numpad keys.

- 1 or [Line] For a straight line perpendicular to their direction.  
- 2 or [Curve] For a curve facing towards their direction.  
- 3 or [Spike] For your units to form a V, pointing towards their direction.  

Additionally, using the slider, it is possible to give the group a custom more-or-less pointy formation that can even go backwards.

Locked group mode: Pressing "+" on the keypad will lock your current group. The associated Tactitian will as a result not despawn.  
After clicking away from your group, if it is locked, clicking on any units that was a member of it will re-select your entire formation.  
You can even select temporary units to join a locked group!

### Fog of war:

There is two layers of fog: The permanent and the non-visibility layers.  
In places that were not explored, the area is shrouded in *dark grey*, and nothing inside is visible apart from a hint of the terrain.  
In places that were previously explored but not currently watched by your units, the area is shrouded in *light grey*, and no enemy units are visible inside.
Any area currently supervised by of your units, a neutral building, or one of your buildings, will be lit up and enemies can be seen inside.

### Bug known:

- Sometimes the navMeshAgent of the unit is locked and can't do his task. Due to is limited of running task, the strategist won't make others units until the current task is destroy. You can kill the units to make the AI run again.