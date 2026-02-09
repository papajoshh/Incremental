using System;
using TMPro;
using UnityEngine;

namespace Programental
{
    public class TaskbarClock : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI clockText;
        [SerializeField] private float timeScale = 10f;

        private DateTime _internalTime;
        private bool _running;

        public void StartClock()
        {
            _internalTime = DateTime.Now;
            _running = true;
            UpdateDisplay();
        }

        private void Update()
        {
            if (!_running) return;
            _internalTime = _internalTime.AddSeconds(Time.deltaTime * timeScale);
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            clockText.text = _internalTime.ToString("HH:mm");
        }
    }
}
