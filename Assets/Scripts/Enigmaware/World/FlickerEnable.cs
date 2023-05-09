#region

using UnityEngine;

#endregion

namespace Enigmaware.World
{
    public class FlickerEnable : MonoBehaviour
    {
        public float FlickerTime = 0.1f;
        public float FlickerDelay = 0.1f;
        public float FlickerDuration = 0.1f;
        public float FlickerInterval = 0.1f;
        public bool FlickerOnStart;
        public GameObject Flickerer;

        private float _flickerTimer;
        private float _flickerDelayTimer;
        private float _flickerDurationTimer;
        private float _flickerIntervalTimer;
        private bool _flickerEnabled;


        private void Start()
        {
            if (FlickerOnStart)
            {
                _flickerEnabled = true;
            }
        }

        private void Update()
        {
            if (_flickerEnabled)
            {
                _flickerTimer += Time.deltaTime;
                if (_flickerTimer >= FlickerTime)
                {
                    _flickerTimer = 0f;
                    _flickerDelayTimer = 0f;
                    _flickerDurationTimer = 0f;
                    _flickerIntervalTimer = 0f;
                    Flickerer.SetActive(false);
                }
                else
                {
                    _flickerDelayTimer += Time.deltaTime;
                    if (_flickerDelayTimer >= FlickerDelay)
                    {
                        _flickerDelayTimer = 0f;
                        _flickerDurationTimer = 0f;
                        _flickerIntervalTimer = 0f;
                        Flickerer.SetActive(true);
                    }
                    else
                    {
                        _flickerDurationTimer += Time.deltaTime;
                        if (_flickerDurationTimer >= FlickerDuration)
                        {
                            _flickerDurationTimer = 0f;
                            _flickerIntervalTimer = 0f;
                            Flickerer.SetActive(false);
                        }
                        else
                        {
                            _flickerIntervalTimer += Time.deltaTime;
                            if (_flickerIntervalTimer >= FlickerInterval)
                            {
                                _flickerIntervalTimer = 0f;
                                Flickerer.SetActive(!Flickerer.activeSelf);
                            }
                        }
                    }
                }
            }
        }
    }
}