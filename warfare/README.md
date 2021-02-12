# Tribes 2: Warfare Gametype
This is a two-team Siege where both teams must play both offense and defense. Blow up generators to lower forcefields and touch the enemy switch, without letting the enemy team do the same!

The game is played in 10 minute rounds. Best 3 out of 5 rounds. If neither side caps, just skip that game and keep going.

It says "switching sides" between rounds but it doesn't actually switch sides. Don't worry about that.

# INSTALLATION INSTRUCTIONS
- Simply drop the [WarfareGameType.vl2](WarfareGameType.vl2) file into the base folder of your Tribes 2 install, like a map pack.
- If you use an admin mod to customize the map rotation, make sure to add Blackout/Phalanx/IronFist as part of Warfare gametype. (You can add Katazatic too, but that map sucks.)

# WARFARE MAPS
## Blackout
"That one played surprisingly well.  I was shocked." -Celios
- Two Sanctuary-looking bases sit a few hundred meters from each other. Blow up the generators within to capture the switch directly.
- There's one power switch for your equipment. Don't forget to play the objective or you soon find yourself unable to suit up!
- If you're feeling spiteful, repair the generator under the power switch and they won't be able to retake the objective before destroying it!

## Phalanx
- Based on Broadside. Blow up a bunch of generators in order so you can attack the Main generators at the top and grab the the final switch.
- With the exception of the Main generators, none of the generators can be repaired. Stand clear, since they will automatically overload and explode when they no longer provide power!
- <s>Of course there's no easter eggs. Why would you even ask a question like that?</s>

## Iron Fist
- Based on Gauntlet. Blow up the forcefield generator to assault the main base for the switch. Really simple.
- After five minutes, the backdoor opens, allowing you to bypass the forcefield generator. You'll get warnings in the chat. Beware!

## Katazatic
"This map sucks."
- Based on Katabatic. This map was mostly made and included as a joke, since there weren't enough maps and it wouldn't take much work.
- First, destroy the Forcefield Generator in the side base (which is unrepairable, so don't bother camping).
- Then you need to hit the Containment Generator to remove the other forcefields, so that you can go after the Main Generators.
- The Main Generators power the switch.
- There's a path that can skip the Containment Generator, so be careful!

# MAPPING CONSIDERATIONS

## Standard Mapping Practice
As in Siege, objectives should have needsObjectiveWaypoint = 1 set on them. Switches belonging directly to the team should also have needsObjectiveWaypoint set on them. Use nameTag on all of your objectives to name the waypoint that'll show up.

Any neutral switch that should get equipment should NOT have needsObjectiveWaypoint. You should use a waypoint object on these.

Any forcefield with quickPass = 1 will generate without a physical zone, meaning you can just slip through it without stopping if it belongs to you. This was often the preferred behavior on the pond.

Any equipment with notRepairable = 1 is not repairable. Disabling an object in the Warfare gametype will delete its waypoint, no scripts required. The object will also explode, so don't stand too close!

## Advanced Mapping Practice
If you want to add a script to fire at the beginning of the round, right before players are dropped into the map, overload **WarfareGame::initScript** in your map package. See Blackout.mis for an example.

If you want to add a script to fire as the round begins, when the countdown hits 0, overload **WarfareGame::startMatch** in your map package. See IronFist.mis for an example.

# CREDITS
- Gametype concept and design by Red Shifter
- Blackout, Phalanx by Red Shifter
- Iron Fist by Celios
- Katazatic by [uncredited]