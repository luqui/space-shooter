using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.Threading;
using System.IO;

namespace SpaceBattle
{
    class SoundID
    {
        public SoundID(Func<Vector2> pos) { this.pos = pos; }
        public Func<Vector2> pos;
    }

    class Sequencer
    {
        AudioEngine engine;
        WaveBank wavebank;
        SoundBank soundbank;
        Timer timer;
        AudioListener listener;

        Mutex mutex;

        struct Beat
        {
            public Beat(SoundID id_, string cue_) { id = id_; cue = cue_; }
            public SoundID id;
            public string cue;
        };

        List<List<Beat>> measure;
        int semidemi = 0;
        int measures = 0;
        int bpm = 100;
        bool played_this_measure = false;

        public Sequencer()
        {
            string prefix = Directory.Exists("Content\\Win") ? "Content\\Win" : "Content";
            engine = new AudioEngine(prefix + "\\Audio.xgs");
            wavebank = new WaveBank(engine, prefix + "\\Wave Bank.xwb");
            soundbank = new SoundBank(engine, prefix + "\\Sound Bank.xsb");
            listener = new AudioListener();
            listener.Position = new Vector3(0, 0, 16);
            listener.Forward = new Vector3(0, 0, -1);
            listener.Up = new Vector3(0, 1, 0);

            measure = new List<List<Beat>>();
            for (int i = 0; i < 16; i++)
            {
                measure.Add(new List<Beat>());
            }

            mutex = new Mutex();
        }

        public Cue StartCue(Vector2 pos, string name)
        {
            var emitter = new AudioEmitter();
            emitter.Position = new Vector3(pos, 0);
            Cue ret = soundbank.GetCue(name);
            ret.Apply3D(listener, emitter);
            ret.Play();
            return ret;
        }

        public void StopCue(Cue cue)
        {
            cue.Stop(AudioStopOptions.Immediate);
        }

        public void Start()
        {
            timer = new Timer(Fire, null, 0, 15000/bpm);
        }
        public void Stop()
        {
            timer.Dispose();
        }

        public void PlayOnce(Vector2 pos, string sound)
        {
            var emitter = new AudioEmitter();
            emitter.Position = new Vector3(pos,0);
            lock (mutex)
            {
                soundbank.PlayCue(sound);
            }
        }

        public SoundID Enqueue(Func<Vector2> pos, string sound)
        {
            SoundID id = new SoundID(pos);
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
                    var emitter = new AudioEmitter();
                    emitter.Position = new Vector3(i.id.pos(), 0);
                    soundbank.PlayCue(i.cue, listener, emitter);
                    played_this_measure = true;
                }

                semidemi++;
                if (semidemi == measure.Count)
                {
                    semidemi = 0;
                    // every 64 measures, change the meter, up the tempo
                    measures++;
                    if (measures == 64)
                    {
                        measures = 0;
                        int newsize = (Util.RANDOM.Next(2) + 3) * (Util.RANDOM.Next(3) + 3);
                        while (measure.Count < newsize) { measure.Add(new List<Beat>()); }
                        while (measure.Count > newsize) { measure.RemoveAt(0); }
                        bpm += Util.RANDOM.Next(15);
                        if (bpm > 140) bpm = 80;  // yikes that's fast
                        timer.Change(0, 15000 / bpm);
                    }
                    if (!played_this_measure)
                    {
                        // a measure of silence means we can change it up
                        bpm = Util.RANDOM.Next(30) + 80;
                        timer.Change(0, 15000 / bpm);
                    }
                    played_this_measure = false;
                }
                engine.Update();
            }
        }
    }

    static class Sounds
    {
        public static string[] Bongos = { "bongo1", "bongo2", "bongo3", "bongo4", "bongo5", "bongo6", "bongo7", "bongo8", "bongo9" };
        public static string[] Clave = { "clave1", "clave2" };
        public static string[] Conga = { "conga1", "conga2", "conga3", "conga4", "conga5", "conga6", "conga7", "conga8", "conga9", "conga10" };
        public static string[] Cowbell = { "cowbell1", "cowbell2", "cowbell3", "cowbell4", "cowbell5", "cowbell6", "cowbell7", "cowbell8", "cowbell9", "cowbell10" };
        public static string[] LightCym = { "cym1", "cym7", "cym9" };
        public static string[] HiHat = { "hihat1", "hihat2", "hihat3", "hihat4", "hihat5", "hihat6", "hihat7", "hihat8", "hihat9", "hihat10" };
        public static string[] Kettle = { "kett1", "kett2", "kett3", "kett4", "kett5" };
        public static string[] ShortRattle = { "rattle1", "rattle5", "rattle7" };
        public static string[] Tambourine = { "tambo1", "tambo2", "tambo3", "tambo4", "tambo5", "tambo6", "tambo7", "tambo8", "tambo9", "tambo10" };
        public static string[] Triangle = { "tri2", "tri3", "tri4", "tri5" };
        public static string[] Woodblock = { "woodblock1", "woodblock2", "woodpop1", "woodpop2" };
        public static string[] Crash = { "crash1", "crash2", "crash3", "crash4", "crash5", "crash6", "crash7", "crash8", "crash9", "crash10" };
        public static string[] Gong = { "gong1", "gong2", "gong3", "gong4" };
        public static string[] Tabla = { "tabla1", "tabla2", "tabla3", "tabla4", "tabla5", "tabla6", "tabla7", "tabla8", "tabla9", "tabla10", "tabla11", "tabla12", "tabla13", "tabla14" };
        public static string[] SingingBowl = { "singingbowl1", "singingbowl2", "singingbowl3" };

        public static T Select<T>(T[] data)
        {
            return data[Util.RANDOM.Next(data.Count())];
        }

        public static SoundID StartSound(Func<Vector2> pos, string[] selection)
        {
            return Util.Sequencer.Enqueue(pos, Select(selection));
        }
    }
}
