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

The chat will be sent a message for every 10 percent that a team gains, since the score numbers are otherwise pretty weird.

Click [here](scoring) for a technical explanation of Advanced Scoring and how it differs from the stock Tribes 2 CnH scoring system.

## Spawn w/Energy Pack (default ON)
Speed up the game a little. You can turn this on and off for some reason.

## Secured Switches (default OFF)
When I see games like Star Wars Battlefront that base their life on Capture and Hold, it became obvious that being able to grab any switch at any time is kind of a mess. Though it's a very Tribes 2 thing to have.

Turning on Secured Switches will make it so standing next to the switch makes it so the other team can't claim it. The switch waypoint changes color to note that it's being defended, and you'll get a slight blue aura on your vision as a visual cue that you're within range.

The range is pretty low (around 5 meters) but you don't need to be touching it.

## Slow Respawn (default OFF)
Respawn time changes to 5 seconds. I forget why I put this in the game. Maybe I was drunk? Maybe I thought it would stop people from clustering a secured switch? I forget. But I don't feel like removing it.

A better option would be disabling a spawnsphere if the enemy is attacking a place, but I'm too lazy to do that.

# MAPPING CONSIDERATIONS
Just use normal CnH mapping conventions.

Of course, you need to think about what makes a good CnH map.

Here's some hints for CnH map design:
- Don't just take a CTF map and tack CnH onto it by putting objectives in the center. Please don't.
- Teams should generally spawn on opposite sides of the map. Don't do weird things like spawning everyone together in the middle. They don't need more than a couple stations and repair pack.
- Your map should have at least 3 objectives. More is better if you want a bigger map, but more than 5 or 6 is probably too much.
- Some objectives should be closer to your base than to the enemy base. It makes it easier for teams to coordinate together on the fly if they're given a suggestion of where to go first, and where they're eventually working towards. Just dropping a few objectives an equal distance from both teams just feels like you're trying to hold a switch rather than holding an objective.
- Objectives don't always need a spawnsphere, but any objective with a spawnsphere should have inventory and a repair pack, as well as not being in a remote corner of the map. People should want to spawn there. Make sure to think about the sphereWeight too.
- A central objective should be easy to attack and take back. You don't want one team to be able to farm it up and use its power to dominate the game without a lot of effort.

# CREDITS
- Red Shifter: Gametype redesign, lead scripter
- Celios: l33t Hax0r
- Special thanks to a tiny fishie for running the pond server
- Special thanks to Auza for coordinating pickups of CnH and coming up with the scoring system
