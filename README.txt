# Spacerush
Made for CS 4455

Spacerush is an action game in which the player fights using a sword and guns against waves of enemies in a small arena.
Defeat them and gain large numbers, or die alone.

# Controls:
WASD - Move
Space - Jump
Mouse - Move Camera
Left Click - Attack/Fire Weapon
Right Click - Aim Weapon

#Installation/Opening
Build folder is /builds
Scenes of interest are
- Arena.unity
- Title.unity

#Requirements/Notable Features for CS 4455:
- Player/Enemies move and attack with custom models, animations, and animation controllers tailored to gameplay.
- Player is moved by rigidbody forces, allowing them to bounce off of enemies when attacking or damaged.
- UI elements, such as an ingame overlay, a pause menu, a death screen, and a title screen.
- Enemies are controlled by unique AI:
	- The Brute (giant robot) slowly chases after the player and when nearing the player (or periodically), unleashes a large AOE.
	- The Buffer (twig with hole in middle) restores the health of other enemies in the arena
	- The Flyer (floating eyeball monster) chases after the player and shoots a number of projectiles
- An item (either a health or ammo pickup) is presented between waves.
	- Player has the option of ignoring it for more points
- Landmines are randomly generated each wave. The player has to avoid stepping on them or take damage.
- Waves get progressively harder, with more landmines and more enemies active at once that spawn faster
- Arena platforms are animated, enemy AI reacts intelligently to it
- A score is tallied as the game progresses. The player is awarded:
	- 1 point for each enemy kill
	- 3 points for each wave complete (increases with more waves)
	- 10 points for ignoring the item given between waves
- Custom music for both the title screen and regular gameplay

#Team:
Jacob Watson - jwatson64@gatech.edu - jwatson64	- Management, Player Animation, Gameplay
Jihan Ko - jko42@gatech.edu - jko42			- Music, SFX, Concept art
Jordan Shartar - jshartar6@gatech.edu - jshartar6		- AI, Testing, fill for basic tasks for rest
Daniela Avila Jaramillo - daniavilaj@gatech.edu - daniavilaj	- Modeling, Texturing, Animations, Level Design
Kelsey Johnson - kjohnson337@gatech.edu - kjohnson337	- Modeling, Texturing, Animations