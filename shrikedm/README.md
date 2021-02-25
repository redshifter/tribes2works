# Tribes 2: Shrike Deathmatch
The Shrike. In some circles, the Shrike IS Tribes 2.

That is not any circle that plays classic.

Except ZP.

He's weird.

Also too good with a Shrike.

Scoring is mainly based on tagging enemies with shots.
Killing enemies won't give you the full value of points,
so playing a kill stealing game probably won't win you the game!


# INSTALLATION INSTRUCTIONS
- Simply drop the [ShrikeDMGameType.vl2](ShrikeDMGameType.vl2) file into the base folder of your Tribes 2 install, like a map pack.
- If you use an admin mod to customize the map rotation, make sure to add the ShrikeDM files to the rotation.

# SHRIKE DEATHMATCH MAPS
## Equinox
- A Capture and Hold map nobody plays now has a use - Shrike Deathmatch!
- This map has a lot of small mountains and a lot of canyons, making it a really cool map for vehicles.

## Ocean Battle
- Nothing but some extremely lethal water as far as the eye can see.
- There may be Equinox terrain under the water, but you'll never get down there alive.

# SCORING
You gain points by hitting or killing your opponents.

- Every kill will be awarded with 8 points.
- Every hit you score will be awarded 1 point, even if it's against the vehicle's shield.
- Dying will reduce your score by 2 points.

Also try not to die.

- Every time you die, by your own hand or not, you will lose 2 points.
- Blowing up your own shrike will reduce your score by 5 points. Though kill credit will be given if possible.

# MAPPING CONSIDERATIONS
## Spawning
Spawngraphs are fickle. Also problematic for Shrikes. So this gametype doesn't use them at all.

First, you should set your Mission Area boundaries. That's your first step in a map.

Then, you can create spawn points. You can automatically build the spawn points for a map by typing in:

```GenerateShrikeSpawns(height);```

This will drop a bunch of them into a new SimGroup called **ShrikeSpawns**. They will be generated around the OOB grid.

You should delete spawnspheres that are under terrain or send the player on a crash course with a hill.

## Advanced Spawning
There's a more advanced spawn generator if you want more fine-tuning:

```ShrikeDM_GenerateShrikeSpawns(height, spread, edgeSize);```
- Height is the height that you want the spawns to be at, like the "basic" command.
- Spread is the distance between spawns. The default value is **120**.
- EdgeSize is the distance from the mission area. The default value is **20**.

## Manually Placing Spawns
Have fun with that.

Spawns should be OOB.