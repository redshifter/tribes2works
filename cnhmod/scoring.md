# Tribes 2: Capture and Hold modification - Scoring differences
Why was the scoring system changed anyway?

##### Normal Scoring (stock Tribes 2)
- The score limit is **1200 x n**, where **n** is the number of switches.
- Points are only awarded after your team holds a switch for 12 seconds. In a gametype like classic, this is a really, really long time.
- Your team gains 2 points per second, or 120 points per minute, for a switch being held uncontested.
- This normally works out to a maximum of 20 minutes for a balanced game and a maximum of 10 minutes for a curbstomp, until you factor in the fight over switches keeps them from always scoring.

##### Advanced Scoring
- The score limit is changed to **225 x n²**, where **n** is the number of switches.
- Once every switch is claimed, or after 2 minutes have passed since mission start, team points will start being awarded.
- Switches are always scoring. There's no wait time between touching a switch and when it starts scoring for your team.
- Each team gains **n²** points per second, where **n** is the number of switches their team has claimed.
- If a team holds all the switches, this works out to 3.75 minutes of hold time to win the game.
- If both teams hold an equal number of switches, the game will slowly move towards its end in 15 minutes, though cuts in this time will be made when one team has an advantage.

This makes for some really weird numbers, which is why chat messages are given for each 10% a team earns in points.

Why did I choose 225 as a number instead of a round number like 240? I don't really remember. It doesn't matter too much.


This scoring system was origially Auza's idea, with Red Shifter putting it into practice.