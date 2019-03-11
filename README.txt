# Spacerush
Made for CS 4455

Spacerush is an action game in which the player fights using a sword and guns against waves of enemies in a small arena.

# Controls
WASD - Move
Space - Jump
Mouse - Move Camera
Left Click - Attack/Fire Weapon
Right Click - Aim Weapon

Requirements/Notable Features for CS 4455:
- Player/Enemies move and attack with custom models, animations, and animation controllers tailored to gameplay.
- Player is moved by rigidbody forces, allowing them to bounce off of enemies when attacking or damaged.
- UI elements, such as an ingame overlay, a pause menu, a death screen, and a title screen.
- Enemies are controlled by unique AI:
	- The Brute (giant robot) slowly chases after the player and when nearing the player (or periodically), unleashes a large AOE.
	- The Buffer (twig with hole in middle) restores the health of other enemies in the arena (currently only Brutes).
- An item (either a health or ammo pickup) is presented between waves.
- Landmines are randomly generated each wave. The player has to avoid stepping on them or take damage.
- A score is tallied as the game progresses. The player is awarded:
	- 1 point for each enemy kill
	- 3 points for each wave complete
	- 3 points for ignoring the item given between waves
- Custom music for both the title screen and regular gameplay
	
Future plans:
- Adding in two more enemies (Flyer and Jumping Slime)
- Texture/Material work on all models
- Adding more/refinement of animations to achieve better gamefeel and less frustration
- Particle effects signaling when attacks/actions are about to happen, or have happened
- Sound effects on all animations
- Playtesting and reworking of enemy health, score awards, etc.
- More chances for score bonuses
- Methods to make subsequent waves more difficult (more enemies, larger/more landmines, shorter spawn times, etc.)
	- Maybe even randomizing between several different methods on a per wave basis