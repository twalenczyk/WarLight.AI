# a script to gather data from recorded games
import os
game_ids = [
            1360634434
        ]
game_turns = [
            14
        ]

for i in range(game_ids):
    for turn in range(1,game_turns[i]):
        os.system('WarLight.AI.exe Prod PlayBots ' + game_ids[i].ToString() + ' 10 ' + turn.ToString() 
