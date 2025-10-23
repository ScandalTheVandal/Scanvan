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
            if (_radioOn)
            {
                TurnOffRadioServerRpc();
                return;
            }
            TurnOnRadioServerRpc();
            // set a random frequency when the radio is *first* turned on
            if (_currentFrequency == "FM")
                ChangeAndSyncFrequencyServerRpc();
        }

        public void SetVolumeOnLocalClient(float setVolume)
        {
            _timeSinceChangingVol = Time.realtimeSinceStartup;
            setVolume = Mathf.Clamp01(setVolume);
            setVolume = Mathf.Round(setVolume / 0.1f) * 0.1f;
            _volume = setVolume;
            _audioSource.volume = setVolume;
            _staticAudioSource.volume = setVolume != 0f ? Mathf.Clamp01(setVolume + 0.1f) : 0f;
            Plugin.Logger.LogMessage($"Local client: setting radio volume to: {setVolume}");
        }

        public void RaiseVolume()
        {
            float radioVol = _volume + 0.1f;
            //SetVolumeOnLocalClient(radioVol);
            SetRadioVolumeOnClientsRpc(radioVol);
        }

        public void LowerVolume()
        {
            float radioVol = _volume - 0.1f;
            //SetVolumeOnLocalClient(radioVol);
            SetRadioVolumeOnClientsRpc(radioVol);
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void SetRadioVolumeOnClientsRpc(float setVolume)
        {
            _timeSinceChangingVol = Time.realtimeSinceStartup;
            setVolume = Mathf.Clamp01(setVolume);
            setVolume = Mathf.Round(setVolume / 0.1f) * 0.1f;
            _volume = setVolume;
            _audioSource.volume = setVolume;
            _staticAudioSource.volume = setVolume != 0f ? Mathf.Clamp01(setVolume + 0.1f) : 0f;
            Plugin.Logger.LogMessage($"Syncing radio volume to: {setVolume}");
        }

        //[ServerRpc(RequireOwnership = false)]
        //public void SetRadioVolumeServerRpc(float setVolume)
        //{
        //    SetRadioVolumeClientRpc(setVolume);
        //}

        //[ClientRpc]
        //public void SetRadioVolumeClientRpc(float setVolume)
        //{
        //    _timeSinceChangingVol = Time.realtimeSinceStartup;
        //    setVolume = Mathf.Clamp01(setVolume);
        //    setVolume = Mathf.Round(setVolume / 0.1f) * 0.1f;
        //    _volume = setVolume;
        //    _audioSource.volume = setVolume;
        //    _staticAudioSource.volume = setVolume != 0f ? Mathf.Clamp01(setVolume + 0.1f) : 0f;
        //    Plugin.Logger.LogMessage($"Syncing radio volume to: {setVolume}");
        //}

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
                ChangeAndSyncFrequencyServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeAndSyncFrequencyServerRpc()
        {
            string radioFrequency = GetRandomFrequency();
            ChangeAndSyncFrequencyClientRpc(radioFrequency);
        }

        [ClientRpc]
        public void ChangeAndSyncFrequencyClientRpc(string frequency)
        {
            _currentFrequency = frequency;
        }

        [ServerRpc(RequireOwnership = false)]
        public void TurnOnRadioServerRpc()
        {
            if (_lastStationId == null)
            {
                _lastStationId = GetRandomRadioGuid();
                TurnOnAndSyncRadioClientRpc(_lastStationId!.Value.ToString());
            }
            TurnOnRadioClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TurnOffRadioServerRpc()
        {
            TurnOffRadioClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeStationServerRpc()
        {
            if (!_radioOn) return;
            _lastStationId = GetRandomRadioGuid();
            TurnOnAndSyncRadioClientRpc(_lastStationId!.Value.ToString());
        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncRadioServerRpc()
        {
            SyncRadioClientRpc(_lastStationId.ToString(), _radioOn, _currentlyStorming);
        }

        [ClientRpc]
        public void SyncRadioClientRpc(string guidString, bool radioOn, bool currentlyStorming)
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

        [ClientRpc]
        public void TurnOnRadioClientRpc()
        {
            TurnRadioOnOff(true);
        }

        [ClientRpc]
        public void TurnOnAndSyncRadioClientRpc(string guidString)
        {
            // SYNC 
            if (Guid.TryParse(guidString, out Guid guid))
            {
                _lastStationId = guid;
            }
            TurnRadioOnOff(true);
        }

        [ClientRpc]
        public void TurnOffRadioClientRpc()
        {
            TurnRadioOnOff(false);
        }

        private void TurnRadioOnOff(bool state)
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
            if (_stream != null && _stream.decomp)
            {
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
