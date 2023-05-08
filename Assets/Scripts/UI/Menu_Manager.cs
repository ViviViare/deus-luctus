using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

namespace viviviare
{
    public class Menu_Manager : MonoBehaviour
    {
        public static Menu_Manager _i;
        private void Awake()
        {
            if (_i == null)
            {
                _i = this;
            }
            else
            {
                Destroy(this);
            }
        }

        [Header("Win Screen")]
        [SerializeField] private Canvas _winScreen;
        #region Win Screen Variables
        [SerializeField] private Button _winReplay;
        [SerializeField] private Button _winQuit;

        #endregion

        [Header("Lose Screen")]
        [SerializeField] private Canvas _loseScreen;
        #region Lose Screen Variables
        [SerializeField] private Button _loseRestart;
        [SerializeField] private Button _loseQuit;
        #endregion

        [Header("Pause Screen")]
        [SerializeField] private Canvas _pauseScreen;
        #region Pause Screen Variables
        [SerializeField] private Button _pauseRestart;
        [SerializeField] private Button _pauseQuit;
        [SerializeField] private Button _pauseResume;
        #endregion

        public bool _isMenuUp;
        private bool _isPaused;
        private bool _pressedESC;


        private void Start()
        {
            // Quit buttons setup
            _winQuit.onClick.AddListener(QuitGame);
            _loseQuit.onClick.AddListener(QuitGame);
            _pauseQuit.onClick.AddListener(QuitGame);

            // Restart / Replay buttons setup
            _winReplay.onClick.AddListener(Restart);
            _loseRestart.onClick.AddListener(Restart);
            _pauseRestart.onClick.AddListener(Restart);

            // Resume Button setup
            _pauseResume.onClick.AddListener(Resume);
        }


        public void AssignInput()
        {
            Input_Handler._i._playerActions.PlayerMenu.Pause.started += PressedPause;
        }

        private void PressedPause(InputAction.CallbackContext context)
        {
            if (_isPaused)
            {
                Resume();
            }
            else
            {
                PlayerHasPaused();
            }
        }

        private void Update()
        {
            // Guard Clause: Only run if the player has pressed Escape
            if (!_pressedESC) return;

            // Guard Clause: Only run if no menus are already up
            if (_isMenuUp) return;


            if (_isPaused)
            {
                Resume();
            }
            else
            {
                PlayerHasPaused();
            }

        }

        private void ChangeCursorState(bool turnOn)
        {
            if (turnOn)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void PlayerHasLost()
        {
            Time.timeScale = 0f;
            ChangeCursorState(false);
            _loseScreen.enabled = true;
            _isMenuUp = true;
        }

        public void PlayerHasWon()
        {
            Time.timeScale = 0f;
            ChangeCursorState(false);
            _winScreen.enabled = true;
            _isMenuUp = true;
        }

        public void PlayerHasPaused()
        {
            Time.timeScale = 0f;
            ChangeCursorState(false);
            _pauseScreen.enabled = true;
            _isPaused = true;
        }


        private void QuitGame()
        {
            Application.Quit();
        }

        private void Resume()
        {
            Time.timeScale = 1f;
            ChangeCursorState(true);
            _loseScreen.enabled = false;
            _winScreen.enabled = false;
            _pauseScreen.enabled = false;
            _isPaused = false;
        }

        // Reload the current scene
        private void Restart()
        {
            Object_Pooling.ClearPools();
            ChangeCursorState(true);
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
            Time.timeScale = 1f;
        }
    }
}
