using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpaceBattle
{
    class Ring : Actor
    {
        public Ring(Vector2 pos) { position = pos; } 

        const float DURATION = 30;
        const float ENV = 2;
        const float STRENGTH = 10;
        
        float timer = 0;
        Microsoft.Xna.Framework.Audio.Cue cue;

        public float Strength
        {
            get
            {
                if (timer < ENV) { return STRENGTH * timer / ENV; }
                else if (timer < DURATION - ENV) { return STRENGTH; }
                else if (timer < DURATION) { return STRENGTH * (DURATION - timer) / ENV; }
                else { return 0; }
            }
        }

        public override void Start()
        {
            Util.Actors.AddRing(this);
            cue = Util.Sequencer.StartCue(position, Sounds.Select(Sounds.SingingBowl));
        }

        public override void Finish() {
            Util.Sequencer.StopCue(cue);
            Util.Actors.RemoveRing(this); 
        }

        public override bool Dead { get { return timer >= DURATION; } }
        public override float Radius { get { return 0.5f; } }

        Vector2 position;
        public override Vector2 Position { get { return position; } }

        public override void Update(float dt)
        {
            timer += dt;
        }

        public override void Draw()
        {
            Util.DrawSprite(Textures.RingEnemy, position, 0, Strength);
        }

        public override void Die()
        {
            timer = DURATION;
        }
    }
}
