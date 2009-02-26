using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.Threading;

namespace SpaceBattle
{
    class SoundID
    { }

    class Sequencer
    {
        AudioEngine engine;
        WaveBank wavebank;
        SoundBank soundbank;
        Timer timer;

        Mutex mutex;

        struct Beat
        {
            public Beat(SoundID id_, string cue_) { id = id_; cue = cue_; }
            public SoundID id;
            public string cue;
        };

        List<List<Beat>> measure;
        int semidemi = 0;

        public Sequencer()
        {
            engine = new AudioEngine("..\\..\\..\\Content\\Win\\Audio.xgs");
            wavebank = new WaveBank(engine, "..\\..\\..\\Content\\Win\\Wave Bank.xwb");
            soundbank = new SoundBank(engine, "..\\..\\..\\Content\\Win\\Sound Bank.xsb");

            measure = new List<List<Beat>>();
            for (int i = 0; i < 16; i++)
            {
                measure.Add(new List<Beat>());
            }

            mutex = new Mutex();
        }

        public void Start()
        {
            timer = new Timer(Fire, null, 0, 150);  // 100BPM (400 Hz)
        }
        public void Stop()
        {
            timer.Dispose();
        }

        public SoundID Enqueue(string sound)
        {
            SoundID id = new SoundID();
            lock (mutex)
            {
                measure[semidemi].Add(new Beat(id, sound));
            }
            return id;
        }

        public void Dequeue(SoundID id)
        {
            lock (mutex)
            {
                foreach (var i in measure)
                {
                    i.RemoveAll(p => p.id == id);
                }
            }
        }

        void Fire(object state)
        {
            lock (mutex)
            {
                foreach (var i in measure[semidemi])
                {
                    soundbank.PlayCue(i.cue);
                }
                semidemi++;
                semidemi %= measure.Count;
                engine.Update();
            }
        }

        
    }
}
