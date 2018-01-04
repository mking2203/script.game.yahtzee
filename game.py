#!/usr/bin/python
# -*- coding: utf-8 -*-
#
#     Copyright (C) 2018 Mark König (mark.koenig@kleiner-schelm.de)
#
#    This program is free software: you can redistribute it and/or modify
#    it under the terms of the GNU General Public License as published by
#    the Free Software Foundation, either version 3 of the License, or
#    (at your option) any later version.
#
#    This program is distributed in the hope that it will be useful,
#    but WITHOUT ANY WARRANTY; without even the implied warranty of
#    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#    GNU General Public License for more details.
#
#    You should have received a copy of the GNU General Public License
#    along with this program. If not, see <http://www.gnu.org/licenses/>.
#

import os
import random
from random import randint
import time
import thread
import string
import sys
from decimal import Decimal

import xbmc
import xbmcaddon
import xbmcgui

addon = xbmcaddon.Addon()

ADDON_NAME = addon.getAddonInfo('name')
ADDON_PATH = addon.getAddonInfo('path').decode('utf-8')
MEDIA_PATH = os.path.join(
    xbmc.translatePath(ADDON_PATH),
    'resources',
    'skins',
    'default',
    'media'
)
LIB_PATH = os.path.join(
    xbmc.translatePath(ADDON_PATH),
    'resources',
    'lib'
)

ACTION_PREVIOUS_MENU = 10
ACTION_SELECT_ITEM = 7

class Game(xbmcgui.WindowXML):

    pointsP1 = []
    pointsP2 = []
    pointsP3 = []
    pointsP4 = []
    
    hold = []
    dice = []
    
    soundOn = True

    round = 0
    actualTry = 1
    actualPlayer = 0

    player = 2
    computer = 0
    gameOn = False
    
    diceOn = False
    diceCnt = 0
    diceFinished = 0

    CONTROL_ID_GRID = 3001
    CONTROL_ID_RESTART = 3002
    CONTROL_ID_MOVES_COUNT = 3003
    CONTROL_ID_TARGET_COUNT = 3004
    CONTROL_ID_TIME = 3005
    CONTROL_ID_EXIT = 3006
    CONTROL_ID_GAME_ID = 3007

    def onInit(self):
        # init vars
                
        # init points
        for i in range(15):
            self.pointsP1.append(0)
            self.pointsP2.append(0)
            self.pointsP3.append(0)
            self.pointsP4.append(0)
            
        for i in range(5):
            self.hold.append(False)
            self.dice.append(6)
        
        # set points
        for i in range(15):
            self.p0x = self.getControl(5400+ i)
            self.p0x.setLabel(str(self.pointsP1[i]))
            self.p0x = self.getControl(5500+ i)
            self.p0x.setLabel(str(self.pointsP2[i]))
            self.p0x = self.getControl(5600+ i)
            self.p0x.setLabel(str(self.pointsP3[i]))
            self.p0x = self.getControl(5700+ i)
            self.p0x.setLabel(str(self.pointsP4[i]))
            
        # draw dices
        for i in range(5):
            self.p0x = self.getControl(5100+ i)
            self.p0x.setImage('w%s.png' % (i+1))

        # get controls

        # init the grid

        # start the timer thread
        thread.start_new_thread(self.timer_thread, ())
        # start the game
        
        y = YahtzeeLogic()
        #y.ReadRestgewinn()

        self.updateToggleButtons(0)

    def onAction(self, action):
        action_id = action.getId()
        focus_id = self.getFocusId()

        if action_id == ACTION_PREVIOUS_MENU:
            self.close()

    def onFocus(self, control_id):
        self.updateToggleButtons(control_id)

    def onClick(self, control_id):

        self.log('OnClick ' + str(control_id))
        
        if(control_id == 5300):
            #calc = YahtzeePoint()
            #dice = [2,3,5,4,1]
            #dialog = xbmcgui.Dialog()
            #confirmed = dialog.yesno('Value', str(calc.GetCalc(14, dice)))
            
            #y = YahtzeeLogic()
            #y.ReadRestgewinn()
            pass

        # set player
        if(control_id == 5003):
            if(not self.gameOn):
                if(self.player<4):
                    self.player = self.player + 1
                else:
                    self.player = 1
                
                if(self.player + self.computer) > 4:
                   self.computer = 4 - self.player
                self.updateToggleButtons(0)

        # set computer
        if(control_id == 5004):
            if(not self.gameOn):
                if self.computer < (4 - self.player):
                    self.computer = self.computer + 1
                else:
                    self.computer = 0
                self.updateToggleButtons(0)

        # sound on/off
        if(control_id == 5005):
            self.soundOn = not self.soundOn
            self.updateToggleButtons(control_id)

        # dice
        if(control_id == 5008):
            if(not self.gameOn):
                self.gameOn = True
                self.actualPlayer = 1
                actualTry = 1
                self.round = 1
                self.diceOn = True
                for i in range(5):
                    self.hold[i] = False
                self.updateToggleButtons(control_id)
                self.diceOn = True
                xbmc.playSFX(MEDIA_PATH + '\\dice.snd')


        if(self.actualTry > 1) and self.gameOn:
            if(control_id == 5200):
                self.hold[0] = not self.hold[0]
                self.updateToggleButtons(control_id)
            if(control_id == 5201):
                self.hold[1] = not self.hold[1]
                self.updateToggleButtons(control_id)
            if(control_id == 5202):
                self.hold[2] = not self.hold[2]
                self.updateToggleButtons(control_id)
            if(control_id == 5203):
                self.hold[3] = not self.hold[3]
                self.updateToggleButtons(control_id)
            if(control_id == 5204):
                self.hold[4] = not self.hold[4]
                self.updateToggleButtons(control_id)

        if control_id == self.CONTROL_ID_RESTART:
            self.start_game()
        elif control_id == self.CONTROL_ID_EXIT:
            self.exit()

    def timer_thread(self):
        while not xbmc.abortRequested:
            if(self.diceOn):
                self.diceCnt = self.diceCnt +1
                if(self.diceCnt < 10):
                    for i in range(5):
                        x =randint(1, 6)
                        self.p0x = self.getControl(5100+ i)
                        self.p0x.setImage('w%s.png' % x)
                else:
                    self.diceOn = False
                    self.diceFinished = True
            xbmc.sleep(200)

    def updateToggleButtons(self, control_id):

        # update player settings
        self.p0x = self.getControl(5003)
        self.p0x.setLabel('Player: ' + str(self.player))
        self.p0x = self.getControl(5004)
        self.p0x.setLabel('Computer: ' + str(self.computer))

        # update actual player
        self.p0x = self.getControl(5006)
        if(self.gameOn):
            self.p0x.setLabel('PLAYER ' + str(self.actualPlayer))
        else:
            self.p0x.setLabel('GAME OVER')

        # protect for startup
        self.p0x = self.getControl(5007)
        self.p0x.setLabel('ROUND ' + str(self.round))

        if len(self.hold) < 4:
            return

        # set fcous / set for the hold button
        for i in range(5):
            self.p0x = self.getControl(5900 + i)

            if(self.hold[i] == False):
                self.p0x.setImage('t_button.png')
            else:
                self.p0x.setImage('t_button_sel.png')

            if(control_id == (5200 + i)):
                if(self.hold[i] == False):
                    self.p0x.setImage('t_button_active.png')
                else:
                    self.p0x.setImage('t_button_active_sel.png')

        # set fcous / set for the sound button
        self.p0x = self.getControl(6005)
        if(self.soundOn == False):
            self.p0x.setImage('t_button.png')
        else:
            self.p0x.setImage('t_button_sel.png')
        if(control_id == (5005)):
            if(self.soundOn == False):
                self.p0x.setImage('t_button_active.png')
            else:
                self.p0x.setImage('t_button_active_sel.png')

    def log(self, msg):
        xbmc.log('[ADDON][%s] %s' % ('TEST', msg.encode('utf-8')),
                 level=xbmc.LOGNOTICE)

# ----------------------------------------------------------------------------------------------------

    # http://www.holderied.de/kniffel/
    # Autor: Felix Holderied 

    # ported to C# by Mark König 2009, http://www.team-mediaportal.com
    # ported to Python by Mark König 2018, http://www.kodinerds.net (partly)

class YahtzeeLogic(object):

     restgewinn = []  # Einzulesende Erwartungswerte
     
     def ReadRestgewinn(self):
     
         pDialog = xbmcgui.DialogProgress()
         pDialog.create('Yahtzee', 'Loading values, please wait!')
     
         fobj = open(ADDON_PATH + '\\Yahtzee.dat', "r")
         data = fobj.read()
         fobj.close()
         
         sp = data.split(';')
         
         for i in range(len(sp) - 1):
             self.restgewinn.append(Decimal(sp[i].replace(',','.')))
             p =  i * 100 / 524288
             pDialog.update(p)
         pDialog.close()


# ----------------------------------------------------------------------------------------------------

class YahtzeePoint(object):

    def GetCalc(self, button, dice):
        val = 0

        if(button == 1):
            val = self.AddUpDice(1, dice)
        elif(button == 2):
            val = self.AddUpDice(2, dice)
        elif(button == 3):
            val = self.AddUpDice(3, dice)
        elif(button == 4):
            val = self.AddUpDice(4, dice)
        elif(button == 5):
            val = self.AddUpDice(5, dice)
        elif(button == 6):
            val = self.AddUpDice(6, dice)
        elif(button == 8):
            val = self.CalculateThreeOfAKind(dice)
        elif(button == 9):
            val = self.CalculateFourOfAKind(dice)
        elif(button == 10):
            val = self.CalculateFullHouse(dice)
        elif(button == 11):
            val = self.CalculateSmallStraight(dice)
        elif(button == 12):
            val = self.CalculateLargeStraight(dice)
        elif(button == 13):
            val = self.CalculateYahtzee(dice)
        elif(button == 14):
            val = self.AddUpChance(dice)

        return val

    def AddUpDice(self, no, dice):
        val = 0

        for i in range(5):
            if(dice[i] == no):
                val = val + no;

        return val

    def CalculateThreeOfAKind(self, dice):
        val = 0
        ThreeOfAKind = False

        for x in range(6):
            cnt = 0
            for i in range(5):
                if (dice[i] == (x + 1)):
                    cnt = cnt + 1
                if (cnt > 2):
                    ThreeOfAKind = True
                    break

        if (ThreeOfAKind):
            for i in range(5):
                val = val + dice[i]

        return val

    def CalculateFourOfAKind(self, dice):
        val = 0
        FourOfAKind = False

        for x in range(6):
            cnt = 0
            for i in range(5):
                if (dice[i] == (x + 1)):
                    cnt = cnt + 1
                if (cnt > 3):
                    FourOfAKind = True
                    break

        if (FourOfAKind):
            for i in range(5):
                val = val + dice[i]

        return val

    def CalculateFullHouse(self, dice):
        val = 0
        i = sorted(dice)

        if ((((i[0] == i[1]) and (i[1] == i[2])) and # Three of a Kind
           (i[3] == i[4]) and # Two of a Kind
           (i[2] != i[3])) or
           ((i[0] == i[1]) and # Two of a Kind
           ((i[2] == i[3]) and (i[3] == i[4])) and # Three of a Kind
           (i[1] != i[2]))):
            val = 25;

        return val

    def CalculateSmallStraight(self, dice):
        val = 0
        i = sorted(dice)

        # Problem can arise hear, if there is more than one of the same number, so
        # we must move any doubles to the end

        for j in range(4):
            if (i[j] == i[j + 1]):
                temp = i[j]
                for k in range(j, 4):
                    i[k] = i[k + 1]
                    i[4] = temp

        if(((i[0] == 1) and (i[1] == 2) and (i[2] == 3) and (i[3] == 4)) or
            ((i[0] == 2) and (i[1] == 3) and (i[2] == 4) and (i[3] == 5)) or
            ((i[0] == 3) and (i[1] == 4) and (i[2] == 5) and (i[3] == 6)) or
            ((i[1] == 1) and (i[2] == 2) and (i[3] == 3) and (i[4] == 4)) or
            ((i[1] == 2) and (i[2] == 3) and (i[3] == 4) and (i[4] == 5)) or
            ((i[1] == 3) and (i[2] == 4) and (i[3] == 5) and (i[4] == 6))):
            val = 30

        return val

    def CalculateLargeStraight(self, dice):
        val = 0
        i = sorted(dice)

        if (((i[0] == 1) and (i[1] == 2) and (i[2] == 3) and (i[3] == 4) and (i[4] == 5)) or
            ((i[0] == 2) and (i[1] == 3) and (i[2] == 4) and (i[3] == 5) and (i[4] == 6))):

            val = 40

        return val

    def CalculateYahtzee(self, dice):

        for x in range(6):
            cnt = 0
            for i in range(5):
                if (dice[i] == (x + 1)):
                    cnt = cnt + 1
                if (cnt > 4):
                    return 50
        return 0

    def AddUpChance(self,dice):
        val = 0

        for i in range(5):
            val = val + dice[i]

        return val

# ----------------------------------------------------------------------------------------------------

if __name__ == '__main__':
    game = Game(
        'script-%s-main.xml' % ADDON_NAME,
        ADDON_PATH,
        'default',
        '720p'
    )
    game.doModal()
    del game

sys.modules.clear()
