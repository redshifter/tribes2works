# Tribes 2: Capture and Hold modification
Capture and Hold. A gametype most people ignored the first time around. Probably because there was only one good map (Jacob's Ladder) and most of the rest of the maps are afterthoughts tacked onto CTF.

When team I'm on it! came to the pond asking to run Capture and Hold pickups, we were skeptical, but we went along with it anyway. After all, it wasn't some Capture the Flag thing. It became very obvious that there were a lot of things wrong with the gametype, including bad maps and odd design decisions that didn't make much sense in classic mod.

So we made changes. This download is a result of those changes.

# INSTALLATION INSTRUCTIONS
- Simply drop the [CaptureAndHoldMod.vl2](CaptureAndHoldMod.vl2) file into the base folder of your Tribes 2 install, like a map pack.
- If you use an admin mod to customize the map rotation, make sure to add the new maps. Also remove crap like Lakefront and Shock Ridge because those maps suck.

# MODIFICATION FEATURES
- A few bug fixes (like not being able to control a turret after it's changed hands).
- You can no longer buy turret packs. This is to stop people from turning everything into Mortar Turrets. If you want to farm up a switch, you'll need to spend some time putting down equipment.
- Personal scoring better favors playing the switches and you no longer lose points for death/suicide.
- Games go faster due to an Advanced Scoring mode (toggleable). This mode drops messages into chat when both teams cross a 10% scoring threshold.
- You spawn with Energy Pack (toggleable).
- A few additional settings have been added, such as Secure Switches.

## Advanced Scoring (default ON)
Normal CnH takes a very, very long time and the gametype generally overstays its welcome. So a new scoring system was implemented.

In layman's terms, when Advanced Scoring is on, the game will end more quickly as one team is performing better than the other. Full curbstomps go by very quickly (around 4 minutes), and fully balanced games will take longer (around 15 minutes at worst).

<details>
<summary>Click here for a technical explanation of Advanced Scoring and how it differs from the normal Tribes 2 CnH scoring system.</summary>

##### Normal Scoring
- 1200 points per switch to win
- Points awarded after holding a switch for 12 seconds. In a gametype like classic, this is a really long time.
- 120 points awarded per switch per minute
- This normally works out to a maximum of 20 minutes, until you factor in the fight over switches requiring 12 seconds of hold time before you score. You can see how this generally overstays its welcome.

##### Advanced Scoring
- The score limit is changed to **225 x n²**, where **n** is the number of switches.
- Once every switch is claimed, or after 2 minutes have passed since mission start, team points will start being awarded.
- Each team gains **n²** points per second, where **n** is the number of switches their team has claimed.
- If a team holds all the switches, this works out to roughly 3.75 minutes of hold time to win the game.
- If both teams hold an equal number of switches, the game will slowly move towards its end in 15 minutes, though cuts will be made when teams hold the enemy switch.

Why did I choose 225 as a number instead of 240? I dunno. It doesn't really matter though.
</details>

## Spawn w/Energy Pack (default ON)
Speed up the game a little. You can turn this on and off for some reason.

## Secured Switches (default OFF)
When I see games like Star Wars Battlefront that base their life on Capture and Hold, it became obvious that being able to grab any switch at any time is kind of a mess. Though it's a very Tribes 2 thing to have.

Turning on Secured Switches will make it so standing next to the switch makes it so the other team can't claim it. The switch waypoint changes color to note that it's being defended, and you'll get a slight blue aura on your vision as a visual cue that you're within range.

The range is pretty low (around 5 meters) but you don't need to be touching it.

## Slow Respawn (default OFF)
Respawn time changes to 5 seconds. I forget why I put this in the game. Maybe I was drunk? Maybe I thought it would stop people from clustering a secured switch? I forget. But I didn't feel like removing it.

# CREDITS
- Red Shifter: Gametype redesign, lead scripter
- Celios: l33t Hax0r
- Special thanks to a tiny fishie for running the pond server
- Special thanks to Auza for coordinating pickups of CnH and coming up with the scoring system