using Cysharp.Threading.Tasks;
using UnityEngine;


/// <summary>
///  Available from RadioFurniture, licensed under GNU General Public License.
///  Source: https://github.com/legoandmars/RadioFurniture/tree/master/RadioFurniture
/// </summary>
namespace CruiserXL.ClipLoading
{
    public class UnityAudioStream : MonoBehaviour
    {
        private AudioSource _audioSource = null!;
        private MP3Stream? _stream;
        
        public void Awake()
        {
            var audioSourceObject = new GameObject("AudioSource");
            DontDestroyOnLoad(audioSourceObject);
            audioSourceObject.hideFlags = HideFlags.HideAndDontSave;
            _audioSource = audioSourceObject.AddComponent<AudioSource>();
        }

        public void PlayAudioFromStream(string uri)
        {
            if (_stream == null) _stream = new MP3Stream();
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
                _audioSource.clip = AudioClip.Create("mp3_Stream", int.MaxValue,
                    _stream.bufferedWaveProvider.WaveFormat.Channels,
                    _stream.bufferedWaveProvider.WaveFormat.SampleRate,
                    true, new AudioClip.PCMReaderCallback(_stream.ReadData));

                _stream.decomp = false; // do not create shitload of audioclips
            }
        }

        public void Stop()
        {
            _stream?.StopPlayback();
            _stream = null;

            if (_audioSource != null )
            {
                _audioSource.Stop();
                _audioSource.time = 0;
                _audioSource.clip = null;
            }
            // _stream?.Dispose();
        }
    }
}
