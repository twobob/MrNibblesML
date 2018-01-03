﻿using System.Collections.Generic;
using System.Linq;
using MrNibblesML;
using UnityEngine;

namespace MrNibbles
{
    public class MrNibblesAgent : Agent
    {
        public enum State
        {
            Playing,
            Transitioning
        }

        public const int None = 0;
        public const int MoveLeft = 1;
        public const int MoveRight = 2;
        public const int Jump = 3;

        private PlayerPlatformerController _player;
        private GameController _game;
        private GameObject _currentLevel;
        private IEnumerable<TilesController.TileInfo> _tiles;
        private ExitLevelTrigger _exitPoint;
        private State _state;

        void Awake()
        {
            _player = GetComponent<PlayerPlatformerController>();
            _game = FindObjectOfType<GameController>();
            _state = State.Transitioning;
        }

        public override List<float> CollectState()
        {
            if (_currentLevel != _game.CurrentLevel)
                UpdateStateConstants();

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
            state.Add(_player.enabled ? 1 : 0);

            return state;
        }

        private void UpdateStateConstants()
        {
            _tiles = _game.CurrentLevel.GetComponentInChildren<TilesController>()
                .GetTiles(new BoundsInt(-25,-25,0,50,50,1));

            _exitPoint = _game.CurrentLevel.GetComponentInChildren<ExitLevelTrigger>();

            _currentLevel = _game.CurrentLevel;
        }

        public override void AgentStep(float[] actions)
        {
            if (_state == State.Playing)
            {
                if (!_player.enabled)
                {
                    done = true;
                    reward = 1;
                    _state = State.Transitioning;
                }
                else
                {
                    var nothing = (int)actions[None] == 1;
                    var moveLeft = (int)actions[MoveLeft] == 1;
                    var moveRight = (int)actions[MoveRight] == 1;
                    var jump = (int)actions[Jump] == 1;

                    var isJumping = false;
                    var hozMove = 0f;

                    if (moveLeft)
                        hozMove = -1f;
                    if (moveRight)
                        hozMove = 1f;
                    if (jump)
                        isJumping = true;

                    _player.Tick(hozMove, isJumping);

                    reward -= 0.01f;
                    done = false;
                }
            }
            else if (_state == State.Transitioning)
            {
                done = false;
                if (_player.enabled)
                {
                    _state = State.Playing;
                }
            }
        }

        public override void AgentReset()
        {
        }
    }
}