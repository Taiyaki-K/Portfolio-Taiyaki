using System;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.ContentSizeFitter;

namespace Taiyaki
{
    public class GameModeManager : MonoBehaviour
    {
        public static GameModeManager Instance { get; private set; }

        public Transform playerTarget3D;
        public Transform playerTarget2D;

        public bool Is2DMode { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        private void Start()
        {
            Is2DMode = false;
            EventBus.PublishModeChanged(Is2DMode);
        }
        private void OnEnable()
        {
            EventBus.OnSpacePressed += ChangeMode;
        }
        private void OnDisable()
        {
            EventBus.OnSpacePressed -= ChangeMode;
        }
        private void ChangeMode()
        {
            Is2DMode = !Is2DMode;
            EventBus.PublishModeChanged(Is2DMode);
        }
    }
}