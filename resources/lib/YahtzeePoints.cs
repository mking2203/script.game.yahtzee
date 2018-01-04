#region Copyright (C) 2005-2010 Team MediaPortal

/* 
 *	Copyright (C) 2005-2010 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee
{
  public class YahtzeePoints
  {
    private static YahtzeePoints instance = new YahtzeePoints();

    private YahtzeePoints()
    {
    }
    public static YahtzeePoints GetInstance()
    {
      return instance;
    }

    public int GetCalc(int btnNo, int[] myDice)
    {
      int ret = 0;

      switch (btnNo)
      {
        case 1:
          ret = AddUpDice(1, myDice);
          break;
        case 2:
          ret = AddUpDice(2, myDice);
          break;
        case 3:
          ret = AddUpDice(3, myDice);
          break;
        case 4:
          ret = AddUpDice(4, myDice);
          break;
        case 5:
          ret = AddUpDice(5, myDice);
          break;
        case 6:
          ret = AddUpDice(6, myDice);
          break;
        //------------------------
        case 8:
          ret = CalculateThreeOfAKind(myDice);
          break;
        case 9:
          ret = CalculateFourOfAKind(myDice);
          break;
        case 10:
          ret = CalculateFullHouse(myDice);
          break;
        case 11:
          ret = CalculateSmallStraight(myDice);
          break;
        case 12:
          ret = CalculateLargeStraight(myDice);
          break;
        case 13:
          ret = CalculateYahtzee(myDice);
          break;
        case 14:
          ret = AddUpChance(myDice);
          break;
      }

      return ret;
    }

    ///
    /// Routine to calculate a dice 1-6 score
    /// 
    private int AddUpDice(int DiceNumber, int[] myDice)
    {
      int Sum = 0;

      for (int i = 0; i < 5; i++)
      {
        if (myDice[i] == DiceNumber)
        {
          Sum += DiceNumber;
        }
      }

      return Sum;
    } // 1-6
    ///
    /// Routine to calculate Three of a Kind Score
    ///
    private int CalculateThreeOfAKind(int[] myDice)
    {
      int Sum = 0;

      bool ThreeOfAKind = false;

      for (int i = 1; i <= 6; i++)
      {
        int Count = 0;
        for (int j = 0; j < 5; j++)
        {
          if (myDice[j] == i)
            Count++;

          if (Count > 2)
            ThreeOfAKind = true;
        }
      }

      if (ThreeOfAKind)
      {
        for (int k = 0; k < 5; k++)
        {
          Sum += myDice[k];
        }
      }

      return Sum;
    } // 8
    ///
    /// Routine to calculate Four of a Kind Score
    ///
    private int CalculateFourOfAKind(int[] myDice)
    {
      int Sum = 0;

      bool FourOfAKind = false;

      for (int i = 1; i <= 6; i++)
      {
        int Count = 0;
        for (int j = 0; j < 5; j++)
        {
          if (myDice[j] == i)
            Count++;

          if (Count > 3)
            FourOfAKind = true;
        }
      }

      if (FourOfAKind)
      {
        for (int k = 0; k < 5; k++)
        {
          Sum += myDice[k];
        }
      }

      return Sum;
    } // 9
    ///
    /// Routine to calculate Full House Score
    ///
    private int CalculateFullHouse(int[] myDice)
    {
      int Sum = 0;

      int[] i = new int[5];

      i[0] = myDice[0];
      i[1] = myDice[1];
      i[2] = myDice[2];
      i[3] = myDice[3];
      i[4] = myDice[4];

      Array.Sort(i);

      if ((((i[0] == i[1]) && (i[1] == i[2])) && // Three of a Kind
           (i[3] == i[4]) && // Two of a Kind
           (i[2] != i[3])) ||
          ((i[0] == i[1]) && // Two of a Kind
           ((i[2] == i[3]) && (i[3] == i[4])) && // Three of a Kind
           (i[1] != i[2])))
      {
        Sum = 25;
      }

      return Sum;
    } // 10
    ///
    /// Routine to calculate Small Straight Score
    ///
    private int CalculateSmallStraight(int[] myDice)
    {
      int Sum = 0;

      int[] i = new int[5];

      i[0] = myDice[0];
      i[1] = myDice[1];
      i[2] = myDice[2];
      i[3] = myDice[3];
      i[4] = myDice[4];

      Array.Sort(i);

      // Problem can arise hear, if there is more than one of the same number, so
      // we must move any doubles to the end
      for (int j = 0; j < 4; j++)
      {
        int temp = 0;
        if (i[j] == i[j + 1])
        {
          temp = i[j];

          for (int k = j; k < 4; k++)
          {
            i[k] = i[k + 1];
          }

          i[4] = temp;
        }
      }

      if (((i[0] == 1) && (i[1] == 2) && (i[2] == 3) && (i[3] == 4)) ||
          ((i[0] == 2) && (i[1] == 3) && (i[2] == 4) && (i[3] == 5)) ||
          ((i[0] == 3) && (i[1] == 4) && (i[2] == 5) && (i[3] == 6)) ||
          ((i[1] == 1) && (i[2] == 2) && (i[3] == 3) && (i[4] == 4)) ||
          ((i[1] == 2) && (i[2] == 3) && (i[3] == 4) && (i[4] == 5)) ||
          ((i[1] == 3) && (i[2] == 4) && (i[3] == 5) && (i[4] == 6)))
      {
        Sum = 30;
      }

      return Sum;
    } //11
    ///
    /// Routine to calculate Large Straight Score
    ///
    private int CalculateLargeStraight(int[] myDice)
    {
      int Sum = 0;

      int[] i = new int[5];

      i[0] = myDice[0];
      i[1] = myDice[1];
      i[2] = myDice[2];
      i[3] = myDice[3];
      i[4] = myDice[4];

      Array.Sort(i);

      if (((i[0] == 1) && (i[1] == 2) && (i[2] == 3) && (i[3] == 4) && (i[4] == 5)) ||
          ((i[0] == 2) && (i[1] == 3) && (i[2] == 4) && (i[3] == 5) && (i[4] == 6)))
      {
        Sum = 40;
      }

      return Sum;
    } // 12
    ///
    /// Routine to calculate Yahtzee Score
    ///
    private int CalculateYahtzee(int[] myDice)
    {
      int Sum = 0;

      for (int i = 1; i <= 6; i++)
      {
        int Count = 0;
        for (int j = 0; j < 5; j++)
        {
          if (myDice[j] == i)
            Count++;

          if (Count > 4)
            Sum = 50;
        }
      }

      return Sum;
    } // 13
    ///
    /// Routine to calculate Chance;
    ///
    private int AddUpChance(int[] myDice)
    {
      int Sum = 0;

      for (int i = 0; i < 5; i++)
      {
        Sum += myDice[i];
      }

      return Sum;
    } // 14
  }
}
