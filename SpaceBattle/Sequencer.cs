using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.Threading;
using System.IO;

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
        int measures = 0;

        public Sequencer()
        {
            string prefix = Directory.Exists("Content\\Win") ? "Content\\Win" : "Content";
            engine = new AudioEngine(prefix + "\\Audio.xgs");
            wavebank = new WaveBank(engine, prefix + "\\Wave Bank.xwb");
            soundbank = new SoundBank(engine, prefix + "\\Sound Bank.xsb");

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

        public void PlayOnce(string sound)
        {
            lock (mutex)
            {
                soundbank.PlayCue(sound);
            }
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
                if (semidemi == measure.Count)
                {
                    semidemi = 0;
                    // every 64 measures, change the meter.
                    measures++;
                    if (measures == 64)
                    {
                        measures = 0;
                        int newsize = (Util.RANDOM.Next(2) + 3) * (Util.RANDOM.Next(3) + 3);
                        while (measure.Count < newsize) { measure.Add(new List<Beat>()); }
                        while (measure.Count > newsize) { measure.RemoveAt(0); }
                    }
                }
                semidemi %= measure.Count;
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

        public static T Select<T>(T[] data)
        {
            return data[Util.RANDOM.Next(data.Count())];
        }

        public static SoundID StartSound(string[] selection)
        {
            return Util.Sequencer.Enqueue(Select(selection));
        }
    }
}
