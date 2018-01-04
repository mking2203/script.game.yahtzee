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
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace Yahtzee
{
  [PluginIcons("Yahtzee.y.png", "Yahtzee.y_disabled.png")]
  public class Yahtzee : GUIWindow, ISetupForm
  {
    #region private declarations

    private int[,] values;   // tables for the points
    private int[] dice;      // values of the dices
    private bool[] hold;     // state of the buttons

    private int[] forecast;  // hold the forecast values

    Timer GameTick = new Timer();
    private static OnActionHandler ah;

    private string xPath = "";
    private bool startup = false;

    private int player = 2;
    private int computer = 1;

    private int actPly;
    private int actTry;
    private int actRnd;

    private bool diceOn;
    private int diceTime;
    private int waitTime;

    private bool sound = true;

    private bool boolDiced = false;

    private YahtzeeLogic yahtzee;

    #endregion

    #region SkinControl

    [SkinControlAttribute(2)]
    protected GUIButtonControl btnStart = null;
    [SkinControlAttribute(3)]
    protected GUIButtonControl btnMode = null;
    [SkinControlAttribute(4)]
    protected GUIButtonControl btnComputer = null;
    [SkinControlAttribute(5)]
    protected GUIToggleButtonControl btnSound = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl btnDice = null;
    [SkinControlAttribute(7)]
    protected GUIButtonControl btnHints = null;
    [SkinControlAttribute(8)]
    protected GUIButtonControl btnWhatsThis = null;

    [SkinControlAttribute(901)]
    protected GUIToggleButtonControl btnHold01 = null;
    [SkinControlAttribute(902)]
    protected GUIToggleButtonControl btnHold02 = null;
    [SkinControlAttribute(903)]
    protected GUIToggleButtonControl btnHold03 = null;
    [SkinControlAttribute(904)]
    protected GUIToggleButtonControl btnHold04 = null;
    [SkinControlAttribute(905)]
    protected GUIToggleButtonControl btnHold05 = null;
    #endregion

    #region ISetupForm Members

    // Returns the name of the plugin which is shown in the plugin menu 
    public string PluginName()
    {
      return "Yahtzee";
    }

    // Returns the description of the plugin is shown in the plugin menu 
    public string Description()
    {
      return "Yahtzee is a popular game with 5 dice.";
    }

    // Returns the author of the plugin which is shown in the plugin menu 
    public string Author()
    {
      return "Mark Koenig (kroko) 2009";
    }

    // show the setup dialog 
    public void ShowPlugin()
    {
      //MessageBox.Show("Nothing to configure, this is just an example");
    }

    // Indicates whether plugin can be enabled/disabled 
    public bool CanEnable()
    {
      return true;
    }

    // get ID of windowplugin belonging to this setup 
    public int GetWindowId()
    {
      return GetID;
    }

    // Indicates if plugin is enabled by default; 
    public bool DefaultEnabled()
    {
      return true;
    }

    // indicates if a plugin has its own setup screen 
    public bool HasSetup()
    {
      return false;
    }

    /// <summary> 
    /// If the plugin should have its own button on the main menu of MediaPortal then it 
    /// should return true to this method, otherwise if it should not be on home 
    /// it should return false 
    /// </summary> 
    /// <param name="strButtonText">text the button should have</param> 
    /// <param name="strButtonImage">image for the button, or empty for default</param> 
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param> 
    /// <param name="strPictureImage">subpicture for the button or empty for none</param> 
    /// <returns>true : plugin needs its own button on home 
    /// false : plugin does not need its own button on home</returns> 
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(0);
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = @"hover_my yahtzee.png";
      return true;
    }

    #endregion

    #region Overrides

    public override int GetID
    {
      get
      {
        return 22031923;
      }
      set
      {
        base.GetID = value;
      }
    }
    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\MyYahtzee.xml");
      GUILocalizeStrings.Load(GUILocalizeStrings.CurrentLanguage());
      if (ah == null) ah = new OnActionHandler(OnAction2);
      return result;
    }
    protected override void OnPageLoad()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xPath = xmlreader.GetValue("skin", "name");
        xPath = Config.GetFolder(MediaPortal.Configuration.Config.Dir.Skin) + "\\" + xPath + "\\Media\\Yahtzee\\";
      }

      GUIPropertyManager.SetProperty("#currentmodule", "Yahtzee");

      SetLanguage();

      if (!startup)
      {
        startup = true;
        NewGame();

        // load computer engine
        yahtzee = new YahtzeeLogic(Config.GetFolder(MediaPortal.Configuration.Config.Dir.Plugins) + "\\windows\\Yahtzee.dat");
        yahtzee.ReadRestgewinn();
      }

      GameTick.Tick -= new EventHandler(GameTick_Tick);
      GameTick.Tick += new EventHandler(GameTick_Tick);
      GameTick.Interval = 80;
      GameTick.Start();

      GUIGraphicsContext.OnNewAction -= ah;
      GUIGraphicsContext.OnNewAction += ah;

      UpdateButtonStates();
      DrawField();

      base.OnPageLoad();
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      GUIGraphicsContext.OnNewAction -= ah;
      GameTick.Tick -= new EventHandler(GameTick_Tick);
    }
    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnStart)
      {
        OnBtnNewGame();
      }
      if (control == btnMode)
      {
        OnBtnMode();
      }
      if (control == btnComputer)
      {
        OnBtnComputer();
      }
      if (control == btnSound)
      {
        OnBtnSound();
      }
      if (control == btnDice)
      {
        OnBtnDice();
      }
      if (control == btnHold01)
      {
        OnRenderSound("tick.wav");
        if (actTry > 1) hold[0] = !hold[0];
      }
      if (control == btnHold02)
      {
        OnRenderSound("tick.wav");
        if (actTry > 1) hold[1] = !hold[1];
      }
      if (control == btnHold03)
      {
        OnRenderSound("tick.wav");
        if (actTry > 1) hold[2] = !hold[2];
      }
      if (control == btnHold04)
      {
        OnRenderSound("tick.wav");
        if (actTry > 1) hold[3] = !hold[3];
      }
      if (control == btnHold05)
      {
        OnRenderSound("tick.wav");
        if (actTry > 1) hold[4] = !hold[4];
      }
      if ((controlId > 500) && (controlId < 520))
      {
        int x = controlId - 500;
        if ((actTry >= 2) || (actTry == 0) && (!diceOn))
        {
          if (values[(actPly - 1), x - 1] == -1)
          {
            YahtzeePoints points = YahtzeePoints.GetInstance();
            int p = points.GetCalc(x, dice);

            if (p == 0)
            {
              GUIDialogYesNo dlg = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
              dlg.SetHeading(GUILocalizeStrings.Get(42));
              dlg.SetLine(1, GUILocalizeStrings.Get(43));
              dlg.SetLine(2, GUILocalizeStrings.Get(44));
              dlg.SetLine(3, "");
              dlg.DoModal(GetID);

              if (dlg.IsConfirmed)
              {
                waitTime = 20;
                OnRenderSound("tick.wav");
                values[(actPly - 1), x - 1] = p;
                NextPlayer();
              }
            }
            else
            {
              waitTime = 20;
              OnRenderSound("tick.wav");
              values[(actPly - 1), x - 1] = p;
              NextPlayer();
            }
          }
        }
      }
      if (control == btnHints)
      {
        OnBtnHints();
      }
      if (control == btnWhatsThis)
      {
        OnBtnWhatsThis();
      }
    }

    protected override void OnShowContextMenu()
    {
      base.OnShowContextMenu();
    }

    #endregion

    #region Actions
    public override void OnAction(Action action)
    {
      base.OnAction(action);
    }
    public void OnAction2(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_KEY_PRESSED:
          switch (action.m_key.KeyChar)
          {
            case '1':
              if (actTry > 1) hold[0] = !hold[0];
              break;
            case '2':
              if (actTry > 1) hold[1] = !hold[1];
              break;
            case '3':
              if (actTry > 1) hold[2] = !hold[2];
              break;
            case '4':
              if (actTry > 1) hold[3] = !hold[3];
              break;
            case '5':
              if (actTry > 1) hold[4] = !hold[4];
              break;
            case '0':
              OnBtnDice();
              break;
          }
          break;
      }
      DrawField();
    }
    #endregion

    private void OnBtnNewGame()
    {
      bool dlgConfirmed = true;
      if (boolDiced)
      {
        GUIDialogYesNo dlg = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
        dlg.SetHeading(GUILocalizeStrings.Get(34));
        dlg.SetLine(1, GUILocalizeStrings.Get(35));
        dlg.SetLine(2, GUILocalizeStrings.Get(36));
        dlg.SetLine(3, "");
        dlg.DoModal(GetID);
        dlgConfirmed = dlg.IsConfirmed;

        if (dlgConfirmed)
        {
          NewGame();
        }
      }
    }
    private void OnBtnMode()
    {
      bool dlgConfirmed = true;
      if (boolDiced)
      {
        GUIDialogYesNo dlg = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
        dlg.SetHeading(GUILocalizeStrings.Get(30));
        dlg.SetLine(1, GUILocalizeStrings.Get(31));
        dlg.SetLine(2, GUILocalizeStrings.Get(32));
        dlg.SetLine(3, GUILocalizeStrings.Get(33));
        dlg.DoModal(GetID);
        dlgConfirmed = dlg.IsConfirmed;
      }

      if (dlgConfirmed)
      {
        player++;
        if (player > 4) player = 1;
        NewGame();
      }
    }
    private void OnBtnComputer()
    {
      bool dlgConfirmed = true;
      if (boolDiced)
      {
        GUIDialogYesNo dlg = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
        dlg.SetHeading(GUILocalizeStrings.Get(30));
        dlg.SetLine(1, GUILocalizeStrings.Get(31));
        dlg.SetLine(2, GUILocalizeStrings.Get(32));
        dlg.SetLine(3, GUILocalizeStrings.Get(33));
        dlg.DoModal(GetID);
        dlgConfirmed = dlg.IsConfirmed;
      }

      if (dlgConfirmed)
      {
        computer++;
        if (computer > player - 1) computer = 0;
        NewGame();
      }
    }
    private void OnBtnSound()
    {
      sound = !sound;
      UpdateButtonStates();
    }
    private void OnBtnHints()
    {
      string msg = string.Empty;

      int x = actTry;

      if (x == 0) x = 3;
      else
        x = x - 1;

      if (x > 0)
      {

        string[] hint = GetYahzeeMove(x);
        if (x < 3)
        {
          double d = double.Parse(hint[2]);

          msg += hint[0] + ". " + GUILocalizeStrings.Get(7) + " \n";
          msg += GUILocalizeStrings.Get(37) + " " + GUILocalizeStrings.Get(38) + " " + hint[1] + "\n";
          msg += GUILocalizeStrings.Get(39) + " " + Math.Round(d);
        }
        else
        {
          double d = double.Parse(hint[2]);

          msg += hint[0] + ". " + GUILocalizeStrings.Get(7) + " \n";
          msg += GUILocalizeStrings.Get(40) + " " + GetBestTypLanguage(hint[1]) + "\n";
          msg += GUILocalizeStrings.Get(39) + " " + Math.Round(d);
        }
      }
      else
      {
        msg += GUILocalizeStrings.Get(41);
      }

      GUIDialogText dlg = (GUIDialogText)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_TEXT);
      dlg.SetHeading(GUILocalizeStrings.Get(10));
      dlg.SetText(msg);
      dlg.DoModal(GetID);
    }
    private void OnBtnWhatsThis()
    {
      GUIDialogText dlg = (GUIDialogText)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_TEXT);
      dlg.SetHeading(GUILocalizeStrings.Get(11));
      dlg.SetText(GUILocalizeStrings.Get(50));
      dlg.DoModal(GetID);
    }
    private void OnBtnDice()
    {
      int comp = player - computer; // computer starts at ... 
      if (actPly <= comp)
      {
        if ((!diceOn) && (actTry > 0) && (actRnd != 0))
        {
          OnRenderSound("dice.snd");

          diceOn = true;
          diceTime = 12;
        }
      }
    }

    private void UpdateButtonStates()
    {
      btnSound.Selected = sound;

      btnMode.Label = GUILocalizeStrings.Get(2) + " " + player;
      btnComputer.Label = GUILocalizeStrings.Get(3) + " " + computer;

      if (actTry > 1)
      {
        btnHold01.Visible = true;
        btnHold02.Visible = true;
        btnHold03.Visible = true;
        btnHold04.Visible = true;
        btnHold05.Visible = true;
      }
      else
      {
        btnHold01.Visible = false;
        btnHold02.Visible = false;
        btnHold03.Visible = false;
        btnHold04.Visible = false;
        btnHold05.Visible = false;
      }
    }
    private void GameTick_Tick(object sender, EventArgs e)
    {
      int comp = player - computer; // computer starts at ... 

      // computer should dice
      if (waitTime > 0) waitTime--;

      if ((waitTime == 0) && (actPly > comp) && (actTry > 0))
      {
        if ((!diceOn) && (actRnd != 0))
        {
          OnRenderSound("dice.snd");

          diceOn = true;
          diceTime = 12;
        }
      }

      if (diceOn)
      {
        boolDiced = true;

        Random r = new Random();
        for (int i = 0; i < 5; i++)
        {
          if (
            ((i == 0) && (!btnHold01.Selected)) ||
            ((i == 1) && (!btnHold02.Selected)) ||
            ((i == 2) && (!btnHold03.Selected)) ||
            ((i == 3) && (!btnHold04.Selected)) ||
            ((i == 4) && (!btnHold05.Selected))
            )
            dice[i] = r.Next(6) + 1;
        }

        diceTime--;
        if (diceTime == 0)
        {
          waitTime = 20;
          diceOn = false;

          string[] hint = GetYahzeeMove(actTry);
          double d = Double.Parse(hint[2]);
          forecast[actPly - 1] = (int)Math.Round(d);

          actTry++;
          if (actTry == 4) actTry = 0;

          if (actPly > comp)
          {
            for (int i = 0; i < hold.Length; i++)
            {
              hold[i] = false; ;
            }

            if (actTry > 0)
            {
              if (hint[1] != "0")
              {
                for (int i = 0; i < hint[1].Length; i++)
                {
                  int s = Int32.Parse(Char.ToString(hint[1][i]));

                  for (int j = 0; j < hold.Length; j++)
                  {
                    if ((hold[j] == false) && (dice[j] == s))
                    {
                      hold[j] = true;
                      break;
                    }
                  }

                }
              }
            }
            else
            {
              // set field
              int s = Int32.Parse(hint[1]) + 1;
              if (s > 6) s++; // because we have one field space (6)

              YahtzeePoints points = YahtzeePoints.GetInstance();
              int p = points.GetCalc(s, dice);

              values[actPly - 1, s - 1] = p;


              NextPlayer();
            }
          }


          UpdateButtonStates();
        }
      }

      DrawField();

    }

    private void NewGame()
    {
      boolDiced = false;

      values = new int[4, 15];
      for (int p = 0; p < 4; p++)
      {
        for (int i = 0; i < 15; i++)
        {
          values[p, i] = -1;
        }
      }

      forecast = new int[4];
      for (int i = 0; i < 4; i++)
      {
        forecast[i] = 245;
      }

      dice = new int[5];
      dice[0] = 1; dice[1] = 2;
      dice[2] = 3; dice[3] = 4;
      dice[4] = 5;

      hold = new bool[5];

      actPly = 1;
      actTry = 1;
      actRnd = 1;

      DrawField();
      UpdateButtonStates();
    }
    private void NextPlayer()
    {
      actTry = 1;
      actPly++;
      if (actPly > player)
      {
        actPly = 1;
        actRnd++;

        if (actRnd == 14)
        {
          boolDiced = false;
          actRnd = 0; // gane over
          OnRenderSound("applaus.wav");
        }

      }
      UpdateButtonStates();
    }
    private void DrawField()
    {
      int x = 0;

      GUIPropertyManager.SetProperty("#Yahtzee_Player", GUILocalizeStrings.Get(6));
      GUIPropertyManager.SetProperty("#Yahtzee_Try", GUILocalizeStrings.Get(7) + " 1 " + GUILocalizeStrings.Get(8) + " 3");

      GUIPropertyManager.SetProperty("#Yahtzee_Player", GUILocalizeStrings.Get(6) + " " + actPly);

      if (actTry > 0)
        GUIPropertyManager.SetProperty("#Yahtzee_Try", GUILocalizeStrings.Get(7) + " " + actTry + " " + GUILocalizeStrings.Get(8) + " 3");
      else
        GUIPropertyManager.SetProperty("#Yahtzee_Try", GUILocalizeStrings.Get(29));

      if (actRnd == 0)
        GUIPropertyManager.SetProperty("#Yahtzee_Try", GUILocalizeStrings.Get(5));

      for (int i = 0; i < 5; i++)
      {
        if ((actTry == 0) || (actTry == 1)) hold[i] = false;
      }
      btnHold01.Selected = hold[0];
      btnHold02.Selected = hold[1];
      btnHold03.Selected = hold[2];
      btnHold04.Selected = hold[3];
      btnHold05.Selected = hold[4];

      for (int i = 0; i < 5; i++)
      {
        x = dice[i];
        GUIPropertyManager.SetProperty("#dice0" + (i + 1), "Yahtzee/w" + x + ".png");
      }

      for (int p = 0; p < 4; p++)
      {
        int p_top = 0;
        for (int i = 0; i < 6; i++)
        {
          x = 601 + (p * 20) + i;
          if (values[p, i] >= 0)
          {
            p_top += values[p, i];
            GUIPropertyManager.SetProperty("#P" + x, values[p, i].ToString());
          }
          else
            GUIPropertyManager.SetProperty("#P" + x, ".");
        }

        if (p_top >= 63) p_top += 35;

        values[p, 6] = p_top;
        x = 601 + (p * 20) + 6;
        GUIPropertyManager.SetProperty("#P" + x, p_top.ToString());

        int p_bottom = 0;
        for (int i = 7; i < 14; i++)
        {
          x = 601 + (p * 20) + i;
          if (values[p, i] >= 0)
          {
            p_bottom += values[p, i];
            GUIPropertyManager.SetProperty("#P" + x, values[p, i].ToString());
          }
          else
            GUIPropertyManager.SetProperty("#P" + x, ".");
        }
        values[p, 14] = p_top + p_bottom;
        x = 601 + (p * 20) + 14;
        GUIPropertyManager.SetProperty("#P" + x, (p_top + p_bottom).ToString());

        GUIPropertyManager.SetProperty("#P616", forecast[0].ToString());
        GUIPropertyManager.SetProperty("#P636", forecast[1].ToString());
        GUIPropertyManager.SetProperty("#P656", forecast[2].ToString());
        GUIPropertyManager.SetProperty("#P676", forecast[3].ToString());
      }
    }
    private void SetLanguage()
    {
      btnStart.Label = GUILocalizeStrings.Get(1);
      btnSound.Label = GUILocalizeStrings.Get(4);
      btnDice.Label = GUILocalizeStrings.Get(9);
      btnHints.Label = GUILocalizeStrings.Get(10);
      btnWhatsThis.Label = GUILocalizeStrings.Get(11);

      btnHold01.Label = GUILocalizeStrings.Get(12);
      btnHold02.Label = GUILocalizeStrings.Get(12);
      btnHold03.Label = GUILocalizeStrings.Get(12);
      btnHold04.Label = GUILocalizeStrings.Get(12);
      btnHold05.Label = GUILocalizeStrings.Get(12);

      GUIPropertyManager.SetProperty("#501", GUILocalizeStrings.Get(13));
      GUIPropertyManager.SetProperty("#502", GUILocalizeStrings.Get(14));
      GUIPropertyManager.SetProperty("#503", GUILocalizeStrings.Get(15));
      GUIPropertyManager.SetProperty("#504", GUILocalizeStrings.Get(16));
      GUIPropertyManager.SetProperty("#505", GUILocalizeStrings.Get(17));
      GUIPropertyManager.SetProperty("#506", GUILocalizeStrings.Get(18));

      GUIPropertyManager.SetProperty("#507", GUILocalizeStrings.Get(19));

      GUIPropertyManager.SetProperty("#508", GUILocalizeStrings.Get(20));
      GUIPropertyManager.SetProperty("#509", GUILocalizeStrings.Get(21));
      GUIPropertyManager.SetProperty("#510", GUILocalizeStrings.Get(22));
      GUIPropertyManager.SetProperty("#511", GUILocalizeStrings.Get(23));
      GUIPropertyManager.SetProperty("#512", GUILocalizeStrings.Get(24));
      GUIPropertyManager.SetProperty("#513", GUILocalizeStrings.Get(25));
      GUIPropertyManager.SetProperty("#514", GUILocalizeStrings.Get(26));

      GUIPropertyManager.SetProperty("#515", GUILocalizeStrings.Get(27));
      GUIPropertyManager.SetProperty("#516", GUILocalizeStrings.Get(28));

      GUIPropertyManager.SetProperty("#header_label", GUILocalizeStrings.Get(0));

      GUIPropertyManager.SetProperty("#Yahtzee_Player", GUILocalizeStrings.Get(6));
      GUIPropertyManager.SetProperty("#Yahtzee_Try", GUILocalizeStrings.Get(7) + " 1 " + GUILocalizeStrings.Get(8) + " 3");
    }
    private string GetBestTypLanguage(string Field)
    {
      string ret = "";

      switch (Field)
      {
        case "0":
          ret = GUILocalizeStrings.Get(13);
          break;
        case "1":
          ret = GUILocalizeStrings.Get(14);
          break;
        case "2":
          ret = GUILocalizeStrings.Get(15);
          break;
        case "3":
          ret = GUILocalizeStrings.Get(16);
          break;
        case "4":
          ret = GUILocalizeStrings.Get(17);
          break;
        case "5":
          ret = GUILocalizeStrings.Get(18);
          break;

        case "6":
          ret = GUILocalizeStrings.Get(20);
          break;
        case "7":
          ret = GUILocalizeStrings.Get(21);
          break;
        case "8":
          ret = GUILocalizeStrings.Get(22);
          break;
        case "9":
          ret = GUILocalizeStrings.Get(23);
          break;
        case "10":
          ret = GUILocalizeStrings.Get(24);
          break;
        case "11":
          ret = GUILocalizeStrings.Get(25);
          break;
        case "12":
          ret = GUILocalizeStrings.Get(26);
          break;
      }
      return ret;
    }

    private string[] GetYahzeeMove(int diceNo)
    {
      string[] args = new string[15];
      for (int i = 0; i < 6; i++)
      {
        if (values[actPly - 1, i] == -1) args[i] = "-";
        else args[i] = values[actPly - 1, i].ToString();
      }
      for (int i = 7; i < 14; i++)
      {
        if (values[actPly - 1, i] == -1) args[i - 1] = "-";
        else args[i - 1] = values[actPly - 1, i].ToString();
      }
      args[13] = diceNo.ToString();
      args[14] = dice[0].ToString() + dice[1].ToString() + dice[2].ToString() + dice[3].ToString() + dice[4].ToString();

      string[] ret = yahtzee.EinzelAbfrage(args);
      return ret;
    }

    private void OnRenderSound(string strFilePath)
    {
      if (sound)
      {
        MediaPortal.Util.Utils.PlaySound(strFilePath, false, true);
      }
    }

  }
}
