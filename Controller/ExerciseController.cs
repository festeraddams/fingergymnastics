﻿namespace HSA.FingerGymnastics.Controller
{
    using Exercises;
    using Game;
    using Manager;
    using Mhaze.Unity.DB.Models;
    using System.Linq;
    using UnityEngine;
    using View;
    using DB = DB.Models;

    public class ExerciseController : MonoBehaviour
    {
        public GameObject gameManagerPrefab;
        public GameObject musicPrefab;
        public GameObject handControllerPrefab;
        public GameObject viewManagerPrefab;

        public double timeOffset = 1d;

        private AudioSource music;

        private SceneManager sceneManager;
        private GameState gameState;
        private LeapHandController controller;
        private ViewManager viewManager;

        private GestureController gestureController;

        private DB.Song song;
        private bool hasGestures = true;

        private void InitializeGestures()
        {
            foreach (DB.Gesture gesture in song.Tracks.First().Value.Gestures.Values.OrderBy(g => g.StartTime))
            {
                gestureController.AddGesture(gesture, timeOffset);
            }
        }

        private void Start()
        {
            gameState = gameManagerPrefab.GetComponent<GameState>();
            controller = handControllerPrefab.GetComponent<LeapHandController>();
            viewManager = viewManagerPrefab.GetComponent<ViewManager>();
            music = musicPrefab.GetComponent<AudioSource>();
            sceneManager = GetComponentInParent<SceneManager>();

            song = Model.GetModel<DB.Song>(GameState.SelectedExercise);
            GameState.MaxScore = song.Tracks.First().Value.Gestures.Count;
            gestureController = new GestureController(viewManager);
            
            music.clip = song.File;
            viewManager.SetScoreText(0, song.Tracks.First().Value.Gestures.Count);

            InitializeGestures();
        }

        private void Update()
        {
            if (gestureController.RemovedGestureCount != gestureController.GestureCount)
            {
                if (controller.IsConnected && (controller.HasOneHand || GameState.Debug))
                {
                    if (music.isPlaying)
                    {
                        viewManager.UpdateIndicator(SongLength, CurrentTime);
                        gestureController.Proof(CurrentTime);
                    }
                    else
                    {
                        StartExercise();
                    }
                }
                else
                {
                    PauseExercise();
                }
            }
            else
            {
                StopErercise(true);
            }
        }

        public void StartExercise()
        {
            music.Play();
        }

        public void PauseExercise()
        {
            music.Pause();
        }

        public void StopErercise(bool finished)
        {
            music.Stop();

            if (finished)
                sceneManager.LoadScene("Finished");
        }

        public float CurrentTime
        {
            get
            {
                return music.time;
            }
        }

        public float SongLength
        {
            get
            {
                return music.clip.length;
            }
        }

        public DB.Track Track
        {
            get
            {
                return song.Tracks.First().Value;
            }
        }
    }
}
