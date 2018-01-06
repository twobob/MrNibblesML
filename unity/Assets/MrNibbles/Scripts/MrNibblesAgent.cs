﻿using System;
using System.Collections.Generic;
using System.Linq;
using MrNibblesML;
using UnityEngine;

namespace MrNibbles
{
    public class MrNibblesAgent : Agent
    {
        public const int None = 0;
        public const int MoveLeft = 1;
        public const int MoveRight = 2;
        public const int Jump = 3;

        public BoundsInt tileBoundsToIncludeInState = new BoundsInt(-25, -1, 0, 40, 1, 1);

        private PlayerPlatformerController _player;
        private GameController _game;
        private IEnumerable<TilesController.TileInfo> _tiles;
        private ExitLevelTrigger _exitPoint;
        private SpiderMap _spiders;

        public override List<float> CollectState()
        {
            var state = new List<float>();
            foreach (var tile in _tiles)
            {
                state.Add(tile.position.x);
                state.Add(tile.position.y);
                state.Add(tile.type);
            }

            state.Add(_exitPoint.transform.position.x);
            state.Add(_exitPoint.transform.position.y);

            state.Add(_player.transform.position.x);
            state.Add(_player.transform.position.y);
            state.Add(_player.IsGrounded ? 1 : 0);

            return state;
        }

       public override void AgentStep(float[] actions)
        {
            PerformActions(actions);
            HandleStateConditions();
        }

        private void PerformActions(float[] actions)
        {
            var nothing = (int) actions[None] == 1;
            var moveLeft = (int) actions[MoveLeft] == 1;
            var moveRight = (int) actions[MoveRight] == 1;
            var jump = (int) actions[Jump] == 1;

            var isJumping = false;
            var hozMove = 0f;

            if (moveLeft)
                hozMove = -1f;
            if (moveRight)
                hozMove = 1f;
            if (jump)
                isJumping = true;

            _player.Tick(hozMove, isJumping);
        }

        private void HandleStateConditions()
        {
            if (_exitPoint.IsTriggered)
            {
                Wins++;
                done = true;
                reward = 1;
            }
            else if (_spiders.IsTriggered)
            {
                Deaths++;
                done = true;
                reward = -1;
            }
            else
            {
                reward = -0.01f;
            }
        }

        public override void AgentReset()
        {
            if (IsFirstRun()==false)
                _game.ChangeToNextLevel();

            _player = GetComponent<PlayerPlatformerController>();
            _game = FindObjectOfType<GameController>();
            _tiles = _game.CurrentLevel.GetComponentInChildren<TilesController>().GetTiles(tileBoundsToIncludeInState);
            _exitPoint = _game.CurrentLevel.GetComponentInChildren<ExitLevelTrigger>();
            _spiders = FindObjectOfType<SpiderMap>();
        }

        private bool IsFirstRun()
        {
            return _game==null;
        }

        public int Deaths { get; set; }
        public int Wins { get; set; }
    }
}