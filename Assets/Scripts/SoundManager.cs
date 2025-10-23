using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SoundManager : MonoBehaviour
{
    public enum SoundType
    {
        BG_Music,
        BG_Title,
        BG_Fanfare,
        Pause,
        Unpause,
        Clock_Tick,
        Clock_Strike,
        Sab_Sticky,
        Sab_Swap,
        Sab_Ink,
        Sab_Slow,
        Frog,
        Crow,
        Spin,
        Potion,
        Pour,
        Magic,
        Stir
    }
 
    [System.Serializable]
    public class Sound
    {
        public SoundType Type;
        public AudioClip Clip;
 
        [Range(0f, 1f)]
        public float Volume = 1f;
 
        [HideInInspector]
        public AudioSource Source;
    }
 
    //Singleton
    public static SoundManager Instance;
 
    //All sounds and their associated type - Set these in the inspector
    private Sound[] AllSounds;
 
    //Runtime collections
    private Dictionary<SoundType, Sound> _soundDictionary = new Dictionary<SoundType, Sound>();
 
    // 'static' sounds
    private AudioSource _bgMusicSource;
    
    void Start()
    {
        //Assign singleton
        Instance = this;
        
        InitializeSounds();
 
        Debug.Log("RUNNING");
        
        //Set up sounds
        foreach(var s in AllSounds)
        {
            _soundDictionary[s.Type] = s;
            Debug.Log(s + " " + s.Type);
        }
    }

    private void InitializeSounds()
    {
        AllSounds = new Sound[18];
        
        Sound BG_MusicS = new Sound();
        BG_MusicS.Type = SoundType.BG_Music;
        BG_MusicS.Clip = Resources.Load<AudioClip>("SFX/OST/BG_Music");
        BG_MusicS.Volume = 1f;
        
        Sound BG_TitleS = new Sound();
        BG_TitleS.Type = SoundType.BG_Title;
        BG_TitleS.Clip = Resources.Load<AudioClip>("SFX/OST/BG_Title");
        BG_TitleS.Volume = 1f;
        
        Sound BG_FanfareS = new Sound();
        BG_FanfareS.Type = SoundType.BG_Fanfare;
        BG_FanfareS.Clip = Resources.Load<AudioClip>("SFX/OST/BG_Fanfare");
        BG_FanfareS.Volume = 1f;
        
        Sound PauseS = new Sound();
        PauseS.Type = SoundType.Pause;
        PauseS.Clip = Resources.Load<AudioClip>("SFX/Menu/Pause");
        PauseS.Volume = 0.5f;
        
        Sound UnpauseS = new Sound();
        UnpauseS.Type = SoundType.Unpause;
        UnpauseS.Clip = Resources.Load<AudioClip>("SFX/Menu/Unpause");
        UnpauseS.Volume = 0.5f;
        
        Sound Clock_TickS = new Sound();
        Clock_TickS.Type = SoundType.Clock_Tick;
        Clock_TickS.Clip = Resources.Load<AudioClip>("SFX/Menu/Clock_tick");
        Clock_TickS.Volume = 0.5f;
        
        Sound Clock_StrkeS = new Sound();
        Clock_StrkeS.Type = SoundType.Clock_Strike;
        Clock_StrkeS.Clip = Resources.Load<AudioClip>("SFX/Menu/Clock_strike");
        Clock_StrkeS.Volume = 0.5f;
        
        Sound Sab_StickyS = new Sound();
        Sab_StickyS.Type = SoundType.Sab_Sticky;
        Sab_StickyS.Clip = Resources.Load<AudioClip>("SFX/Sabotages/Sab_Sticky");
        Sab_StickyS.Volume = 0.5f;
        
        Sound Sab_SwapS = new Sound();
        Sab_SwapS.Type = SoundType.Sab_Swap;
        Sab_SwapS.Clip = Resources.Load<AudioClip>("SFX/Sabotages/Sab_Swap");
        Sab_SwapS.Volume = 0.5f;
        
        Sound Sab_InkS = new Sound();
        Sab_InkS.Type = SoundType.Sab_Ink;
        Sab_InkS.Clip = Resources.Load<AudioClip>("SFX/Sabotages/Sab_Ink");
        Sab_InkS.Volume = 0.25f;
        
        Sound Sab_SlowS = new Sound();
        Sab_SlowS.Type = SoundType.Sab_Slow;
        Sab_SlowS.Clip = Resources.Load<AudioClip>("SFX/Sabotages/Sab_Slow");
        Sab_SlowS.Volume = 0.25f;
        
        Sound FrogS = new Sound();
        FrogS.Type = SoundType.Frog;
        FrogS.Clip = Resources.Load<AudioClip>("SFX/Actions/Frog");
        FrogS.Volume = 0.5f;
        
        Sound CrowS = new Sound();
        CrowS.Type = SoundType.Crow;
        CrowS.Clip = Resources.Load<AudioClip>("SFX/Actions/Crow");
        CrowS.Volume = 0.5f;
        
        Sound SpinS = new Sound();
        SpinS.Type = SoundType.Spin;
        SpinS.Clip = Resources.Load<AudioClip>("SFX/Actions/Spin");
        SpinS.Volume = 0.5f;
        
        Sound PotionS = new Sound();
        PotionS.Type = SoundType.Potion;
        PotionS.Clip = Resources.Load<AudioClip>("SFX/Actions/Potion");
        PotionS.Volume = 0.5f;
        
        Sound PourS = new Sound();
        PourS.Type = SoundType.Pour;
        PourS.Clip = Resources.Load<AudioClip>("SFX/Actions/Pour");
        PourS.Volume = 0.5f;
        
        Sound MagicS = new Sound();
        MagicS.Type = SoundType.Magic;
        MagicS.Clip = Resources.Load<AudioClip>("SFX/Actions/Magic");
        MagicS.Volume = 0.5f;
        
        Sound StirS = new Sound();
        StirS.Type = SoundType.Stir;
        StirS.Clip = Resources.Load<AudioClip>("SFX/Actions/Stir");
        StirS.Volume = 1f;

        AllSounds[0]  = BG_MusicS;
        AllSounds[1]  = BG_TitleS;
        AllSounds[2]  = BG_FanfareS;
        AllSounds[3]  = PauseS;
        AllSounds[4]  = UnpauseS;
        AllSounds[5]  = Clock_TickS;
        AllSounds[6]  = Clock_StrkeS;
        AllSounds[7]  = Sab_StickyS;
        AllSounds[8]  = Sab_SwapS;
        AllSounds[9]  = Sab_InkS;
        AllSounds[10] = Sab_SlowS;
        AllSounds[11] = FrogS;
        AllSounds[12] = CrowS;
        AllSounds[13] = SpinS;
        AllSounds[14] = PotionS;
        AllSounds[15] = PourS;
        AllSounds[16] = MagicS;
        AllSounds[17] = StirS;
    }
 
    //Call this method to play a sound
    public void Play(SoundType type)
    {
        //Make sure there's a sound assigned to your specified type
        if (!_soundDictionary.TryGetValue(type, out Sound s))
        {
            Debug.LogWarning($"Sound type {type} not found!");
            return;
        }
 
        //Creates a new sound object
        var soundObj = new GameObject($"Sound_{type}");
        var audioSrc = soundObj.AddComponent<AudioSource>();
 
        //Assigns your sound properties
        audioSrc.clip = s.Clip;
        audioSrc.volume = s.Volume;
 
        //Play the sound
        audioSrc.Play();

        if (type == SoundType.BG_Music)
        {
            Debug.Log(type);
            _bgMusicSource = audioSrc;
        }
        else if (type == SoundType.BG_Title)
        {
            _bgMusicSource = audioSrc;
            _bgMusicSource.loop = true;
        }
        else
        {
            Destroy(soundObj, s.Clip.length);
        }
        
        Debug.Log(type);
    }
 
    //Call this method to change music tracks
    public void ChangeMusic(SoundType type)
    {
        if (!_soundDictionary.TryGetValue(type, out Sound track))
        {
            Debug.LogWarning($"Music track {type} not found!");
            return;
        }
 
        if (_bgMusicSource == null)
        {
            var container = new GameObject("SoundTrackObj");
            _bgMusicSource = container.AddComponent<AudioSource>();
            _bgMusicSource.loop = true;
        }
 
        _bgMusicSource.clip = track.Clip;
        _bgMusicSource.Play();
    }
    
    public void PauseMusic() { _bgMusicSource.Pause(); }
    public void ResumeMusic() { _bgMusicSource.Play(); }
    
    public float GetMusicTime() { return _bgMusicSource.time; }
    
    public void SetMusicVolume(float volume) { _bgMusicSource.volume = volume; }

    public void SetMusicTime(float time) { _bgMusicSource.time = time; } // FOR DEBUGGING ONLY
}
