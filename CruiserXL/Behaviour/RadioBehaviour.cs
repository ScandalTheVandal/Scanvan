using Cysharp.Threading.Tasks;
using CruiserXL.ClipLoading;
using CruiserXL.Events;
using CruiserXL.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
///  Available from RadioFurniture, licensed under GNU General Public License.
///  Source: https://github.com/legoandmars/RadioFurniture/tree/master/RadioFurniture
/// </summary>
namespace CruiserXL.Behaviour
{
    public class RadioBehaviour : NetworkBehaviour
    {
        public bool _radioOn = false;

        public CruiserXLController _controller = null!;

        [SerializeField]
        public AudioSource _audioSource = default!;

        [SerializeField]
        public AudioSource _staticAudioSource = default!;

        [SerializeField]
        public List<AudioClip> _channelSeekClips = new();

        [SerializeField]
        public AudioClip _static = null!;

        [SerializeField]
        public Transform _volumeKnob = null!;

        public MP3Stream? _stream;
        public Guid? _lastStationId;
        public bool _hasClientRequestedChange;
        public bool _stationChangeRequested;
        public float _radioCheckInterval;
        public float _timeSinceChangingVol;
        public bool _playingStatic = false;
        public bool _currentlyStorming = false;
        public string _currentFrequency = null!;
        public float _volume;

        public void Awake()
        {
            _staticAudioSource.clip = _static;
            _currentFrequency = "FM";
            WeatherEvents.OnStormStarted += OnStormStarted;
            WeatherEvents.OnStormEnded += OnStormEnded;
            SetVolumeOnLocalClient(0.4f);
        }

        public new void OnDestroy()
        {
            WeatherEvents.OnStormStarted -= OnStormStarted;
            WeatherEvents.OnStormEnded -= OnStormEnded;
        }

        private void OnStormStarted()
        {
            _currentlyStorming = true;
            if (_stream != null)
            {
                _stream.Distorted = true;
            }
        }

        private void OnStormEnded()
        {
            _currentlyStorming = false;
            if (_stream != null)
            {
                _stream.Distorted = false;
            }
        }

        public void TogglePowerLocalClient()
        {
            if (RadioManager._stations.Count == 0)
            {
                HUDManager.Instance.DisplayTip("Radio failed to preload!",
                    $"Retry attempting to preload stations.. " + $"You may need to reboot your game!",
                    isWarning: true);
                RadioManager.GetRadioStations().Forget();
                return;
            }
            if (_controller.batteryCharge <= _controller.dischargedBattery) return;
            if (_radioOn)
            {
                TurnOffRadioRpc();
                return;
            }
            TurnOnRadioServerRpc();
            // set a random frequency when the radio is *first* turned on
            if (_currentFrequency == "FM")
            {
                string initialFrequency = GetRandomFrequency();
                ChangeAndSyncFrequencyRpc(initialFrequency);
            }
        }

        public void SetVolumeOnLocalClient(float setVolume)
        {
            _timeSinceChangingVol = Time.realtimeSinceStartup;
            setVolume = Mathf.Clamp01(setVolume);
            setVolume = Mathf.Round(setVolume / 0.1f) * 0.1f;
            _volume = setVolume;
            _audioSource.volume = setVolume;
            _staticAudioSource.volume = setVolume != 0f ? Mathf.Clamp01(setVolume + 0.1f) : 0f;
        }

        public void RaiseVolume()
        {
            float radioVol = _volume + 0.1f;
            SetRadioVolumeOnClientsRpc(radioVol);
        }

        public void LowerVolume()
        {
            float radioVol = _volume - 0.1f;
            SetRadioVolumeOnClientsRpc(radioVol);
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void SetRadioVolumeOnClientsRpc(float setVolume)
        {
            SetVolumeOnLocalClient(setVolume);
        }

        private Guid GetRandomRadioGuid()
        {
            var randomStation = RadioManager.GetRandomRadioStation();
            if (randomStation == null) return Guid.Empty;

            return randomStation.StationUuid;
        }

        public void ToggleStationLocalClient()
        {
            if (_radioOn)
            {
                ChangeStationServerRpc();
                string radioFrequency = GetRandomFrequency();
                ChangeAndSyncFrequencyRpc(radioFrequency);
            }
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void ChangeAndSyncFrequencyRpc(string frequency)
        {
            _currentFrequency = frequency;
        }

        //[ServerRpc(RequireOwnership = false)]
        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void TurnOnRadioServerRpc()
        {
            if (_lastStationId == null)
            {
                _lastStationId = GetRandomRadioGuid();
                TurnOnAndSyncRadioRpc(_lastStationId!.Value.ToString());
            }
            TurnOnRadioRpc();
        }

        //[ClientRpc]
        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void TurnOnAndSyncRadioRpc(string guidString)
        {
            _stationChangeRequested = false;
            _hasClientRequestedChange = false;
            _radioCheckInterval = 0f;
            // SYNC 
            if (Guid.TryParse(guidString, out Guid guid))
            {
                _lastStationId = guid;
            }
            TurnRadioOnOff(true);
        }

        //[ClientRpc]
        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void TurnOnRadioRpc()
        {
            TurnRadioOnOff(true);
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void TurnOffRadioRpc()
        {
            TurnRadioOnOff(false);
        }

        //[ServerRpc(RequireOwnership = false)]
        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void ChangeStationServerRpc()
        {
            if (!_radioOn) return;
            if (_stationChangeRequested)
                return;
            _stationChangeRequested = true;
            _lastStationId = GetRandomRadioGuid();
            TurnOnAndSyncRadioRpc(_lastStationId!.Value.ToString());
        }

        //[ServerRpc(RequireOwnership = false)]
        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void SyncRadioServerRpc()
        {
            SyncRadioRpc(_lastStationId.ToString(), _radioOn, _currentlyStorming);
        }

        //[ClientRpc]
        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void SyncRadioRpc(string guidString, bool radioOn, bool currentlyStorming)
        {
            if (Guid.TryParse(guidString, out Guid guid) &&
                guid == _lastStationId &&
                radioOn == _radioOn &&
                currentlyStorming == _currentlyStorming)
                return;
            _currentlyStorming = currentlyStorming;
            _lastStationId = guid;
            TurnRadioOnOff(radioOn);
        }

        public void TurnRadioOnOff(bool state)
        {
            Plugin.Logger.LogMessage("Changing radio state!");
            Plugin.Logger.LogMessage(state);

            StopStaticIfPlaying();

            if (state && _lastStationId != null)
            {
                Plugin.Logger.LogMessage("Changing radio station...");
                if (_stream != null)
                {
                    Stop();
                }
                PlayTransitionSound();
                PlayStatic();

                var station = RadioManager.GetRadioStationByGuid(_lastStationId.Value);
                if (station != null)
                {
                    PlayAudioFromStream(station.UrlResolved.ToString());
                }
            }
            else if (!state && _stream != null)
            {
                Stop();
                _hasClientRequestedChange = false;
                _stationChangeRequested = false;
                _radioCheckInterval = 0f;
                PlayTransitionSound();
            }
            _radioOn = state;
        }

        public string GetRandomFrequency()
        {
            float randomFreq = UnityEngine.Random.Range(
                _controller.minFrequency,
                _controller.maxFrequency);
            string formattedFreq = randomFreq.ToString("0.##");
            _currentFrequency = $"FM {formattedFreq}";
            return _currentFrequency;
        }

        private void PlayTransitionSound()
        {
            var seekClip = _channelSeekClips[UnityEngine.Random.Range(0, _channelSeekClips.Count)];
            _staticAudioSource.PlayOneShot(seekClip);
        }

        private void PlayStatic()
        {
            _playingStatic = true;
            _staticAudioSource.Play();
        }

        public void PlayAudioFromStream(string uri)
        {
            if (_stream == null) _stream = new MP3Stream();
            if (_currentlyStorming) _stream.Distorted = true;
            StartMP3Stream(uri).Forget();
        }

        private async UniTask StartMP3Stream(string uri)
        {
            await UniTask.SwitchToThreadPool();
            _stream?.PlayStream(uri, _audioSource);
        }

        public void FixedUpdate()
        {
            _stream?.UpdateLoop();
        }

        public void Update()
        {
            //HUDManager.Instance.SetDebugText($"" +
            //    $"stream: {_stream}" +
            //    $"\nstreamDecomp{_stream?.decomp}" +
            //    $"\nradioInterval{_radioCheckInterval}" +
            //    $"\nbufferInf{_stream?.buffer_info}");

            if (RadioManager._stations.Count == 0) return;

            if (_stream != null &&
                string.IsNullOrEmpty(_stream.buffer_info) && 
                !_stream.decomp)
            {
                _radioCheckInterval += Time.deltaTime;
                if (_radioCheckInterval > 8.5f &&
                    _radioOn && !_hasClientRequestedChange)
                {
                    //HUDManager.Instance.DisplayTip("Radio Timeout", 
                    //    "Requesting server to change station!");
                    _radioCheckInterval = 0f;
                    _hasClientRequestedChange = true;
                    ChangeStationServerRpc();
                }
            }

            if (_stream != null && _stream.decomp)
            {
                _radioCheckInterval = 0f;
                _hasClientRequestedChange = false;
                _stationChangeRequested = false;
                if (!_currentlyStorming)
                {
                    StopStaticIfPlaying();
                }
                else if (_currentlyStorming && !_playingStatic)
                {
                    PlayStatic();
                }

                _audioSource.clip = AudioClip.Create("mp3_Stream", int.MaxValue,
                    _stream.bufferedWaveProvider.WaveFormat.Channels,
                    _stream.bufferedWaveProvider.WaveFormat.SampleRate,
                    true,
                    new AudioClip.PCMReaderCallback(_stream.ReadData));

                _stream.decomp = false; // do not create shitload of audioclips
            }
        }

        private void StopStaticIfPlaying()
        {
            if (_playingStatic)
            {
                _staticAudioSource.Stop();
                _playingStatic = false;
            }
        }
        
        public void Stop()
        {
            _stream?.StopPlayback();
            _stream = null;

            if (_audioSource != null)
            {
                _audioSource.Stop();
                _audioSource.time = 0;
                _audioSource.clip = null;
            }
        }
    }
}
