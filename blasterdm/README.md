# Tribes 2: Blaster Deathmatch
This is exactly what it says on the tin. BlasterDM for short, BDM for even shorter, D for the grade on DSEZ's maps.

Scoring is mainly based on tagging enemies with shots. Killing enemies won't give you that many extra points. So trying to steal kills and avoiding the fray isn't 
going to win you the game! Get out there and blast some fools!

WARNING: This summary is mostly unedited from the original readme last updated 10 years ago.

# INSTALLATION INSTRUCTIONS
- Simply drop the [BlasterDMGameType.vl2](BlasterDMGameType.vl2) file into the base folder of your Tribes 2 install, like a map pack.
- If you use an admin mod to customize the map rotation, make sure to add the BDM files to the rotation.

- Optionally add the [BlasterDM_Client.vl2](BlasterDM_Client.vl2) file to your local install to get detailed information on your stats and the stats of the leader. 
(These will be the same information as seen on the Score Menu, so it's not strictly necessary.)

# GAMETYPE FEATURES
## Blaster Ownage
Ownage by blaster is the name of the game.  In the frantic arena, players must act calmly under extreme circumstances, or, more likely, pray and spray.

## Scoring
You gain points by hitting or killing your opponents.  Throw away your energy pack, and you can gain even more points.

- Every hit will be awarded with 3 points.  If not wearing an energy pack, you gain 4 points (1 bonus point).
- Every kill will be awarded with 2 points.  If not wearing an energy pack, you gain 5 points (3 bonus points).
- A clean kill while wearing an energy pack will give you 14 to 17 points.
- A clean kill while NOT wearing energy pack will give you 21 to 25 points.

That's not the whole story.  You can lose points as well.

- Every time you hit yourself, you will lose a point.
- Every time you die, by your own hand or not, you will lose a point.

## Self-Healing through the powers of Vampirism
You may notice a lack of stations, and a lack of pickups.  Fear not, for you can still gain healing through the powers of Vampirism! Every time you make a kill, 
you gain back about 30% of your health.  This will protect you from a single blaster shot.  If you are already at full health, you will gain a Repair Kit.

## ACCURACY, or the lack thereof
How bad is your aim?  Your accuracy is measured during the game.  Go to the score screen and see if your aim is really as bad as you think it is.

## FLARES
Throw a few flares around.  VGW VGW VGW!

Every time you get hit, you will spontaneously throw a flare.  This flare doesn't come from your inventory, so get hit as often as you can!  Or not...

# KNOW YOUR TERRAIN
## ALCATRAZ
A fortified island base has become the site of a dangerous free-for-all.  This map combines
outdoor and indoor areas into a chaotic battlefield.  This darker, more menacing version of
Alcatraz was specifically designed for Deathmatch.  Stay away from the lethal water, or you
will get killed.  Use the stations inside to power up, but watch your back.
- Map by Dynamix Staff.  Adaptation by Red Shifter.

## BLASTED BATTLEFIELD
Play outdoors for a change of pace.  This map will require accuracy, energy conservation,
and amazing dueling skills.  If you truly wish to prove your mastery of the blaster, you
will do it on this battlefield.  The hilly terrain is makes it very difficult to duel with
a blaster, and is your major obstacle.
- Map by Red Shifter.

## FRANTIC
Holy crap!  There is little room to move, little room to jet, and to top it all off, there
are no hiding places in this map of pure insanity.  A very simple design gives way to the
twitch gameplay that one would look for in a map named "Frantic".  Strafe like crazy, shoot
like crazy, and hope that you survive.
- Map by Red Shifter.  Inspired by a TK fest on the pond.

## GORGON
A large base which seems specifically designed for Blaster Deathmatch.  There are no real
chokepoints, only some frantic battles around the level.  Keep moving, hunt your enemies
down, but remember - there is no way out of this maze.  Also, you may want to be careful of
sloped walls - your blaster will reflect in strange ways...
- Map by Nefilim.  Adaptation by Red Shifter.

## SCARABRAE
- Map by Nefilim/Dynamix.  Adaptation by Red Shifter.  Thoughts by Gogo0.
A classical showdown of the masters of the blasters.  Undertake in an epic blaster duel that
spans outdoors and indoors!  Move around the massive base, or sit around outdoors.  Players
can use stations to heal themselves, or just duel their ways to pure ownage.

## SUN DRIED
Standard Deathmatch fare.  Mostly flat ground and large open spaces make it very difficult
to use the terrain to your advantage.  You need energy conservation, great accuracy, and
decent dueling skills to win on this map.
- Map by Dynamix Staff.  Adaptation by Red Shifter.

## THE BASEMENT
Forcefields haunt you at every turn.  They can appear and disappear as they please.  These
evil forcefields like to spring up on you when you're fighting and throw you off your game.
Don't be foiled!  Beware of ledges in a few rooms, for enemies can hide and snipe at you.
- Map by Red Shifter.

THE GREAT HALL
A simple little room.  What's the worst that could happen?  ALL HELL BREAKS LOOSE on this
decievingly calm map.  The lower part of the room is where people duel it out for the right
to own, while the upper part of the room is used by players who like to play it safe and
try to snipe their enemies.  With over 10 people, this map can become a frantic hell within
five seconds.
- Map by Red Shifter.  Concept by DSEZ.

ULTIMA THULE
A huge base to explore.  Be careful and cautious, or you may run into an ambush!  This map
was built for a lot of players.  Hang out in the complicated outer hallways, or undertake
in a massive free-for-all in the great hall!  Stations will heal the weary warrior, while
forcefields will give you a simple message - there is no escape from this cold maze.
- Map by Dynamix Staff.  Adaptation by Red Shifter.

# MAPPING CONSIDERATIONS
## Flare Throwing
When a player takes a hit, a flare is spontaneously thrown.  This is just a special
effect I thought up.  However, if your map has a lot of players in a single area, the FPS
can really drop.  Therefore, I included a mechanism to shut down this feature for your map.

Blaster DM maps should have the setting "NoFlare".  0 will activate auto-flare, while 1
will deactivate it.

See TheGreatHall_BDM.mis for an example.  You can't miss it.

## Inventory Stations
When a player dies, their score is decreased by 1 point, and the killer gains 5/8
points as opposed to 3/4 points for a regular hit.  Therefore, humping the station is not
as cheap a strategy as in normal Deathmatch.  Self-healing makes this strategy even more
worthless.

You should place stations into massive maps, but leave them out of the smaller
maps. It gets frantic in there, and players will hate falling into a station at the most
critical of times.  However, this can be used as a feature instead of an annoyance...

# SPECIAL THANKS
- **Myself**: Owning with the blaster.  I am the true Master of the Blaster.
- **Gogo0**: Formerly owning with the blaster.  How the mighty have fallen.
- **Kamikaze Samurai**: Annoying me with SL to the point that I made Blaster Deathmatch.
- **DSEZ**: I forgot who helped me with Great Hall and just released it anonymously, but it was FRIGGIN DSEZ!
- **a tiny fishie**: Helping me with some script issues and hosting this on his site, as well as making SLDM which was very helpful as a template for this gametype.
- **Dr Pimento**: Helping me get my gametype playtested in the Tribes2Maps.com Pickups.  Without him, I wouldn't realize how bad the Great Hall truly was as a map.
- **Auza**: Taking my title of Blaster Master long enough for me to train in the ways of my old weapon and then finally defeat him to regain the title that is rightfully mine.
- **Golarth The Bold**: Haxing spawngraphs and possibly figuring out the solution to my spawning problem.
- **Multiple players**: Playtesting the gametype and making me realize I need to increase FPS.
- **Countless others**: Shooting down my idea for Blaster CTF (why not?  it'd be funny!)
