using System;
using System.Drawing;

namespace ACFramework
{

    class cSpriteCircle : cPolygon
    {
        public static readonly int CIRCLESLICES = 16; //Approximate a circle by a poly of CIRCLESLICES edges.

        public override cSprite copy()
        {
            cSpriteCircle s = new cSpriteCircle();
            s.copy(this);
            return s;
        }

        public override bool IsKindOf(string str)
        {
            return str == "cSpriteCircle" || base.IsKindOf(str);
        }

        public cSpriteCircle() : base(cSpriteCircle.CIRCLESLICES) { }

    }

    class cSpriteBubble : cSpriteComposite //Basic bubble has a circular polygon and a rect on top.
    {
        public static readonly float ACCENTRELIEF = 0.1f; //Raise the accent poly this much 
        //Constructors 

        public cSpriteBubble()
        {
            cPolygon pcircle = new cPolygon(cSpriteCircle.CIRCLESLICES);
            add(pcircle);
            setAccentPoly();
        }

        public override cSprite copy()
        {
            cSpriteBubble s = new cSpriteBubble();
            s.copy(this);
            return s;
        }

        public override bool IsKindOf(string str)
        {
            return str == "cSpriteBubble" || base.IsKindOf(str);
        }

        public virtual void setAccentPoly()
        {
            cPolygon p = AccentPoly;
            if (p != null)
                _childspriteptr.RemoveAt(1);
            float side = 0.33f * (CirclePoly.Radius);
            cVector3[] pverts = new cVector3[] { new cVector3( 0.0f, 0.0f, 0.0f ), new cVector3( 2 * side, 0.0f, 0.0f ), 
				new cVector3( 2 * side, side, 0.0f ), new cVector3( 0.0f, side, 0.0f )};
            cPolygon prectpoly = new cPolygon(4, pverts);
            prectpoly.SpriteAttitude = cMatrix3.translation(new cVector3(side, 0.5f * side, cSpriteBubble.ACCENTRELIEF));
            add(prectpoly); //Decoration rectangle.
            FillColor = CirclePoly.FillColor; //Make the accent color match the circle.
        }


        //Accessors.

        //Overload 

        public override void mutate(int mutationflags, float mutationstrength)
        {
            CirclePoly.mutate(mutationflags & ~cPolygon.MF_VERTCOUNT, mutationstrength);
            setAccentPoly();
            int red, green, blue;
            //Pick bright colors that I can add 64 to and still be in range.
            red = Framework.randomOb.random(64, 255 - 64);
            green = Framework.randomOb.random(64, 255 - 64);
            blue = Framework.randomOb.random(64, 255 - 64);
            FillColor = Color.FromArgb(red, green, blue);
        }


        //Just look at the pcirclepoly.
        //Colorstyle mutators 

        /* We copy the fillcolor,
            and	then we set _accentcolor to be a brighter color of the same hue. This
            is virtual as cSpriteBubbleGrayscale must keep bubblecolor white. */
        public virtual cPolygon CirclePoly
        {
            get
            {
                if (_childspriteptr.Size > 0)
                    return (cPolygon)(_childspriteptr[0]);
                else
                    return null;
            }
        }

        public virtual cPolygon AccentPoly
        {
            get
            {
                if (_childspriteptr.Size > 1)
                    return (cPolygon)(_childspriteptr[1]);
                else
                    return null;
            }
        }

        public override float Radius
        {
            get
            {
                return _spriteattitude.ScaleFactor * CirclePoly.Radius;
            }
        }

        public override Color FillColor
        {
            set
            { /* We set the _accentcolor to be _brighter than the value, in about same hue.
		    To build _accentcolor, we use GetRValue which is a Windows macro to get the 
		    "red" byte out of the 32 bit COLORREF.  We cast it into an int so we
		    can add 64 to it without it wrapping around to 0 if it becomes greater
		    than 256.  Then we use the CLAMP macro from realnumber.h.  Do same for green
		    and blue. */
                int red, green, blue;
                CirclePoly.FillColor = value;
                red = 64 + (int)value.R;
                if (red > 255)
                    red = 255;
                green = 64 + (int)value.G;
                if (green > 255)
                    green = 255;
                blue = 64 + (int)value.B;
                if (blue > 255)
                    blue = 255;
                Color accentcolor = Color.FromArgb(red, green, blue);
                //	setLineColor(accentcolor); 
                cPolygon p = AccentPoly;
                if (p != null)
                    AccentPoly.FillColor = accentcolor;
            }
        }
    }
}