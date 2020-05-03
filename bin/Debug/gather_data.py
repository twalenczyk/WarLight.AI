# a script to gather data from recorded games
import os
game_ids = [
    1360634434
]
game_turns = [
    14
]

for i in range(len(game_ids)):
    for turn in range(1,game_turns[i]):
        os.system('mono WarLight.AI.exe PlayExported Prod PlayBots ' + str(game_ids[i]) + ' 10 ' + str(turn))
