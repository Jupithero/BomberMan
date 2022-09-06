﻿using System;
using ScoreSystem;
using UnityEngine;

namespace BomberMan {
    public class UIController : MonoBehaviour {
        public GameObject InGameHUD;
        public GameObject GameOverPopUp;
        public GameObject GameWonPopUp;
        
        private LeaderBoardController _leaderBoardController;
        
        private void Start() {
            _leaderBoardController = GameObject.FindGameObjectWithTag("HighScoreController").GetComponent<LeaderBoardController>();
            GameStateController.Instance.OnGameStateChanged += OnGameStateChanged;
            GameStateController.Instance.SetState(GameState.Playing);
        }

        private void OnGameStateChanged(GameState newGameState) {
            switch (newGameState) {
                case GameState.Playing:
                    DisplayUI(hud: true);
                    break;
                case GameState.Paused:
                case GameState.GameOver:
                    DisplayUI(gameOver: true, hud: true);
                    print("GameOver");
                    _leaderBoardController.DisplayLeaderBoard(ScoreController.Instance.GetScore());
                    break;
                case GameState.GameWon:
                    DisplayUI(gameWon: true, hud: true);
                    print("GameWon");
                    _leaderBoardController.DisplayLeaderBoard(ScoreController.Instance.GetScore());
                    break;
            }
        }

        private void DisplayUI(bool startingMenu = false, bool hud = false, bool gameOver = false, bool gameWon = false) {
            InGameHUD.SetActive(hud);
            GameOverPopUp.SetActive(gameOver);
            GameWonPopUp.SetActive(gameWon);
        }
            
        
        private void OnDestroy() {
            GameStateController.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}