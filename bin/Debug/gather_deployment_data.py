# Runs bot games on a map over and over again to accumulate data on the number
# of armies deployed to border regions in the context of either
# attack or defense. Data is published to the respective foler with Raw/
import os
import time

num_games = 100
bots = [ 'Prod' 'Prod' ] # keep it simple

os.system('mono WarLight.AI.exe PlayBots Prod Prod')
