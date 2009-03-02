﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace SpaceBattle
{
    class Menu
    {
        class MenuItem
        {
            public string sText;
            public List<MenuItem> listSubItems;
            public Action fxnAction;
            public Func<bool> fxnIsActive;
        }
        Stack<MenuItem> stack;
        MenuItem menu;
        int ixSubItems;
        SpriteFont font;
        bool fDownPressed, fUpPressed, fLeftPressed, fRightPressed;
        Vector2 v2StickLast;
        readonly Vector2 v2StickThreshold = new Vector2(10.0f, 5.0f);



        public Menu(SpriteFont font)
        {
            ixSubItems = 0;
            this.font = font;
            stack = new Stack<MenuItem>();
            
            //Menu building helpers:
            Func<string, List<MenuItem>, Action, MenuItem> fxn = (s, list, action) => { return new MenuItem() { sText = s, listSubItems = list, fxnAction = action }; };
            Func<string, List<MenuItem>, Action, Func<bool>, MenuItem> fxnWithPredicate = (s, list, action, isactive) => { var item = fxn(s, list, action); item.fxnIsActive = isactive; return item; };
            Action fxnEnter = () => 
            {
                stack.Push(menu);
                menu = menu.listSubItems[ixSubItems];
                ixSubItems = 0;
            };

            Func<int,MenuItem> fxnParticleEffects = (count) => 
            {
                return fxnWithPredicate(count.ToString(),null,() => Explosion.CAP = count, () => {return Explosion.CAP == count;});
            };

            //Menu content:
            menu = fxn("Main Menu", new List<MenuItem>() 
            {
                fxn("One Player Game",null,() => Util.MODE = Util.Mode.OnePlayer),
                fxn("Two Player Game",null,() => Util.MODE = Util.Mode.TwoPlayer),
                fxn("Options",new List<MenuItem>()
                {
                   fxn("Particle Effects",new List<MenuItem>()
                   {
                       fxnParticleEffects(0),
                       fxnParticleEffects(250),
                       fxnParticleEffects(500),
                       fxnParticleEffects(1000),
                       fxnParticleEffects(1500),
                       fxnWithPredicate("Unlimited",null,() => Explosion.CAP = int.MaxValue, () => {return Explosion.CAP == int.MaxValue;})
                   },fxnEnter)
                }, fxnEnter),
                fxn("Exit",null,() => Util.MODE  = Util.Mode.Exit)
            }, null);

        }
        public void Update(GamePadState[] rgState)
        {
            //Stick Detection:
            Vector2 v2StickInput = Vector2.Zero;
            foreach (GamePadState state in rgState)
                v2StickInput += state.ThumbSticks.Left;

            //if changed direction, zero us out.
            if (v2StickLast.Y * v2StickInput.Y < 0)
                v2StickLast.Y = 0;
            v2StickLast.Y += v2StickInput.Y;
            if (v2StickLast.Y < -v2StickThreshold.Y)
            {
                ixSubItems = (ixSubItems + 1) % menu.listSubItems.Count;
                v2StickLast.Y += v2StickThreshold.Y;
            }
            else if (v2StickLast.Y > v2StickThreshold.Y)
            {
                ixSubItems = ixSubItems == 0 ? menu.listSubItems.Count - 1 : ixSubItems - 1;
                v2StickLast.Y -= v2StickThreshold.Y;
            }

            //horizontal:
            //only care if we can go left or right.
            //if changed direction, zero us out.
            if (v2StickLast.X * v2StickInput.X < 0)
                v2StickLast.X = 0;
            v2StickLast.X += v2StickInput.X;
            if (v2StickLast.X < -v2StickThreshold.X)
            {
                if (stack.Count != 0)
                {
                    ixSubItems = stack.Peek().listSubItems.IndexOf(menu);
                    menu = stack.Pop();
                }
                v2StickLast.X += v2StickThreshold.X;
            }
            else if (v2StickLast.X > v2StickThreshold.X)
            {
                menu.listSubItems[ixSubItems].fxnAction();
                v2StickLast.X -= v2StickThreshold.X;
            }

            
            //DPad detection -- depricated!
            if (rgState[0].DPad.Down == ButtonState.Pressed)
            {
                if (!fDownPressed)
                {
                    ixSubItems = (ixSubItems + 1) % menu.listSubItems.Count;
                    fDownPressed = true;
                }
            }
            else
                fDownPressed = false;
            if (rgState[0].DPad.Up == ButtonState.Pressed)
            {
                if (!fUpPressed)
                {
                    ixSubItems = ixSubItems == 0 ? menu.listSubItems.Count - 1 : ixSubItems - 1;
                    fUpPressed = true;
                }
            }
            else
                fUpPressed = false;
            if (rgState[0].DPad.Left == ButtonState.Pressed)
            {
                if (!fLeftPressed)
                {
                    if (stack.Count != 0)
                    {
                        ixSubItems = stack.Peek().listSubItems.IndexOf(menu);
                        menu = stack.Pop();
                    }
                    fLeftPressed = true;
                }
            }
            else
                fLeftPressed = false;
            if (rgState[0].DPad.Right == ButtonState.Pressed
                || rgState[0].Buttons.A == ButtonState.Pressed
                || rgState[0].Buttons.RightShoulder == ButtonState.Pressed)
            {
                if (!fRightPressed)
                {
                    menu.listSubItems[ixSubItems].fxnAction();
                    fRightPressed = true;
                }
            }
            else
                fRightPressed = false;
        }
        public void Draw(SpriteBatch sb, Rectangle rectViewport)
        {
            const float HEIGHT = 50.0f;
            const float TOP = 150.0f;
            Color COLOR_SELECTED = Color.DarkRed;
            Color COLOR_NORMAL = Color.White;
            Color COLOR_ACTIVE = Color.Gold;
            //title:
            sb.DrawString(font,menu.sText, new Vector2(rectViewport.Width/4,50),Color.Gray, 0,Vector2.Zero,2*Vector2.One,SpriteEffects.None,0);

            //contents:
            for (var ixItems = 0; ixItems < menu.listSubItems.Count; ixItems++)
            {
                var color = ixItems == ixSubItems ? COLOR_SELECTED : COLOR_NORMAL;
                if (color == COLOR_NORMAL && menu.listSubItems[ixItems].fxnIsActive != null)
                    if (menu.listSubItems[ixItems].fxnIsActive())
                        color = COLOR_ACTIVE;
                
                sb.DrawString(font, 
                    menu.listSubItems[ixItems].sText,
                    new Vector2(rectViewport.Width/4, TOP + ixItems * HEIGHT),
                    color);
            }

            //left menu:
            if (stack.Count > 0)
            {
                var ixParentHighlight = stack.Peek().listSubItems.IndexOf(menu);
                for (var ixItems = 0; ixItems < stack.Peek().listSubItems.Count; ixItems++)
                {
                    var color = ixItems == ixParentHighlight ? COLOR_SELECTED : COLOR_NORMAL;
                    sb.DrawString(font,
                        stack.Peek().listSubItems[ixItems].sText,
                        new Vector2(20, TOP + ixItems * HEIGHT/2),
                        color,
                        0.0f,
                        Vector2.Zero,
                        0.5f,
                        SpriteEffects.None,
                        0.0f);
                }
            }
            //right menu:
            if (menu.listSubItems[ixSubItems].listSubItems != null)
            {
                for (var ixItems = 0; ixItems < menu.listSubItems[ixSubItems].listSubItems.Count; ixItems++)
                {
                    var color = COLOR_NORMAL;
                    var fxn = menu.listSubItems[ixSubItems].listSubItems[ixItems].fxnIsActive;
                    if (fxn != null)
                        if (fxn())
                            color = COLOR_ACTIVE;

                    sb.DrawString(font,
                        menu.listSubItems[ixSubItems].listSubItems[ixItems].sText,
                        new Vector2(3 * rectViewport.Width / 5, TOP + ixItems * HEIGHT / 2),
                        color,
                        0.0f,
                        Vector2.Zero,
                        0.5f,
                        SpriteEffects.None,
                        0.0f);
                }
            }
        }
    }
}
