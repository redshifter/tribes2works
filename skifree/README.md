# Tribes 2: SkiFree
Free to ski. **This is a beta.**

Your goal is to ski through a bunch of gates, in order. The terrain, the gates, the spawn platform - everything is random. It's up to you to go for it and get through the eight gates as quickly as you can. Take discjumps and ski the terrain properly, turning where you can. Compete for the best run on the server!

This gametype is the official\* gametype of the 20th Anniversary of Tribes 2.

# INSTALLATION INSTRUCTIONS
- Simply drop the [SkiFreeGameType.vl2](SkiFreeGameType.vl2) file into the base folder of your Tribes 2 install, like a map pack.
- If you use an admin mod to customize the map rotation, make sure to add SkiFree (SkiFree) to the rotation.

# GAMEPLAY
You spawn on a platform in a random place on a randomly-selected terrain. Eight gates are generated in front of you. Ski through them in order, and do it as fast as you can!

Take discjumps when you can. Your armor has a safety feature that stops you from blasting yourself to death, and you'll regain a bit of health when you pass through each gate.

You should be watching the gate you're on and the next one. Plan out your turns across the terrain!

# THE DAILY CHALLENGE
If you open the Classic Solo & LAN shortcut, you can play on a randomly generated terrain, created by Tribes 2 at run time. Just select it under Training.

The terrain changes every day, and will be the same for each player, so you can compare scores against each other! Just be careful of time zone differences, as Tribes 2 doesn't have an implementation of UTC...

There will be a different terrain for each difficulty. Easy and Normal use the fBM Fractal implementation, which will generate some nice rolling hills. But Hard mode is mostly a sick joke.

There's also a Randomizer mode that generates a randomly generated terrain, whenever you want, in case the Daily Challenge isn't enough for you...

(Unfortunately, it's pretty much impossible to make this into a playable mode online.)

# OPTIONS
Admins can set options on the server:
- Phase Through Players: Run some magic and now players can go through each other. Good to keep people from running into each other doing a route.
- Scoring Mode: In Time Trial (default mode), go through eight gates. In Survival, go as far as you can in 60 seconds!

# HANDICAPS
If you leave the spawn platform after handicapping yourself, the game will acknowledge the challenge you've given yourself.
- Hard Mode: Go without your Energy Pack.
- Discless: Go without your Spinfusor, without discjumping from the spawn platform.
- Single Discjump: Throw away your Spinfusor as you discjump from the spawn platform.
- Glass: Reduce your health to a sliver before going (if you toss away your Repair Kit, it'll put you into this mode without having to hurt yourself). You cannot get any health back. You MUST finish the course without taking damage!

# ARTIFICAL INTELLIGENCE
You can add bots to the SkiFree map. They suck, but I did make them to the point that they kinda understand how to play...

They come in multiple difficulty levels and don't use any cheats (other than the classic mod energy buff that all bots get for some reason). I'm sure cheating bots would be more fun to play against, but kinda against the point...

# MAPPING SUPPORT
You can create your own custom maps. Why you would do that, I have no idea.

The first step is defining the terrain. A Terrain should have the name Terrain, like most missions should already have. (If you don't define the terrain, it chooses one at random. You may note that this is what SkiFree.mis already does. Therefore there's no point to releasing a map without the terrain.)

The next step is defining the Spawn Platform. You can add it by adding it from under shapes -> SkiFree Objects. You can only have one spawn platform. (If you skip this step, it'll throw the spawn platform somewhere on the terrain.)

After that, if you want to set down gates, use the **SkiFreeCustomGate** object, making sure it's ON THE GROUND. It has the following properties (underscores are required):
- **gateNum__:**: The gate number being created.
- **isFinish__**: Whether this should be the last gate of a Time Trial. Remember, Survival mode will ignore this and keep generating gates on the terrain!

This stuff is mostly untested, so make sure to let me know if it doesn't work.

# CREDITS
- Red Shifter: made gametype and a bunch of other crap
- DarkTiger: made the mempatch needed to phase through players.
- Rooster128: provided a cameo (until he finds out about it and makes me remove it)