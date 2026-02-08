using System;
using System.Collections.Generic;
using UnityEngine;

namespace Programental
{
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Programental/SoundLibrary")]
    public class SoundLibrary : ScriptableObject
    {
        [Serializable]
        public struct SoundEntry
        {
            public string key;
            public AudioClip[] clips;
            [Range(0f, 1f)] public float volume;

            public AudioClip RandomClip => clips[UnityEngine.Random.Range(0, clips.Length)];
        }

        [SerializeField] private SoundEntry[] sounds;

        private Dictionary<string, SoundEntry> _lookup;

        public SoundEntry Get(string key)
        {
            BuildLookup();
            return _lookup[key];
        }

        private void BuildLookup()
        {
            if (_lookup != null) return;
            _lookup = new Dictionary<string, SoundEntry>(sounds.Length);
            foreach (var entry in sounds)
                _lookup[entry.key] = entry;
        }
    }
}
