using System;
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
        public void Update(GamePadState state)
        {
            if (state.DPad.Down == ButtonState.Pressed)
            {
                if (!fDownPressed)
                {
                    ixSubItems = (ixSubItems + 1) % menu.listSubItems.Count;
                    fDownPressed = true;
                }
            }
            else
                fDownPressed = false;
            if (state.DPad.Up == ButtonState.Pressed)
            {
                if (!fUpPressed)
                {
                    ixSubItems = ixSubItems == 0 ? menu.listSubItems.Count - 1 : ixSubItems - 1;
                    fUpPressed = true;
                }
            }
            else
                fUpPressed = false;
            if (state.DPad.Left == ButtonState.Pressed)
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
            if (state.DPad.Right == ButtonState.Pressed
                || state.Buttons.A == ButtonState.Pressed
                || state.Buttons.RightShoulder == ButtonState.Pressed)
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
