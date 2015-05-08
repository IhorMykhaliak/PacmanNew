﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pacman.GameEngine;

namespace Pacman.DesktopUI
{
    public partial class GameForm : Form
    {
        #region Fields

        private Game _game;
        private SoundHandler.SoundHandler _soundHandler;

        #endregion

        #region Initialization

        public GameForm()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }

        #endregion

        #region Event handlers

        private void GameLoad(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            _game = new Game();
            _soundHandler = new SoundHandler.SoundHandler();
            GameSubscribe();

            Paint += Draw;
            menu.Visible = true;
        }

        private void GameFormKeyDown(object sender, KeyEventArgs e)
        {
            _game.Player.PreviousDirection = _game.Player.Direction;
            _game.Player.PendingDirection = Direction.None;

            switch (e.KeyData)
            {
                case Keys.Up: _game.Player.Direction = Direction.Up;
                    break;
                case Keys.Down: _game.Player.Direction = Direction.Down;
                    break;
                case Keys.Left: _game.Player.Direction = Direction.Left;
                    break;
                case Keys.Right: _game.Player.Direction = Direction.Right;
                    break;
                case Keys.Space: _game.OnPauseGame();
                    break;
                case Keys.R: OnRestart(this, EventArgs.Empty);
                    break;
            }
        }

        private void OnRestart(object sender, EventArgs e)
        {
            Paint -= Draw;
            GameUnsubscribe();

            _game.Dispose();
            _game = new Game();
            
            _game.IsPaused = false;
            menu.Visible = false;
            _game.MainTimer.Enabled = true;
            
            Paint += Draw;
            GameSubscribe();


        }

        private void OnPause(object sender, EventArgs e)
         {
            menu.Visible = _game.IsPaused;
         }

        private void OnUpdate(object sender, EventArgs e)
        {
            countLabel.Text = _game.Score.ToString();
            Refresh();
        }

        private void OnWin(object sender, EventArgs e)
        {
            OnRestart(this, EventArgs.Empty);
            _game.OnPauseGame();
            MessageBox.Show("You won !");
        }

        private void OnDie(object sender, EventArgs e)
        {
            _game.OnPauseGame();
            OnRestart(this, EventArgs.Empty);
            _game.OnPauseGame();
            MessageBox.Show("You died !");
        }

        private void Draw(object sender, PaintEventArgs e)
        {
            Drawing.DrawGame(_game, sender, e);
        }

        #endregion

        private void GameSubscribe()
        {
            _game.Update += OnUpdate;
            _game.Pause += OnPause;
            _game.Win += OnWin;
            _game.Die += OnDie;
            _game.NewGame += _soundHandler.OnNewGame;
            _game.GhostDie += _soundHandler.OnGhostDie;
            _game.Player.EatItem += _soundHandler.OnEatItem;
        }

        private void GameUnsubscribe()
        {
            _game.Update -= OnUpdate;
            _game.Pause -= OnPause;
            _game.Win -= OnWin;
            _game.Die -= OnDie;
            _game.NewGame -= _soundHandler.OnNewGame;
            _game.GhostDie -= _soundHandler.OnGhostDie;
            _game.Player.EatItem -= _soundHandler.OnEatItem;
        }
    }
}
