// For AC Framework Version 1.2, I've fixed a bug for Hold patterns in models -- previously, the first
// state in a sequence could be repeated -- JC

using System;
using System.Windows.Forms;

namespace ACFramework
{

    /*
    11/22/01	Giavinh	Pham Created. Added MD2 mesh model (animation) into the game.
    The MD2 mesh type was orginally created for Quake 2.
    4/14/2003  Rudy Rucker reworked so the textures are handled by cGraphics.
    CAUTION: Use only with skins that are rectangles whose edges are powers of two,
    like 64, 128, 256, etc.  This should be standard in any skins you'll download.
    6/26/2008  Jeffrey Childs converted to C#, added more model states, and REPEAT and HOLD
        states types for specific model frames that the programmer may want to show
    */

    class State // I put these in a class so someone can just type in "State." and see
    // the states that are available from the IntelliSense database -- JC
    {
        public const short Idle = 0;
        public const short Run = 1;
        public const short ShotButStillStanding = 2;
        public const short ShotInShoulder = 3;
        public const short Jump = 4;
        public const short ShotDown = 5;
        public const short Crouch = 6;
        public const short CrouchCrawl = 7;
        public const short CrouchWeapon = 8;
        public const short KneelDying = 9;
        public const short FallbackDie = 10;
        public const short FallForwardDie = 11;
        public const short Other = 12; // use Other type when, after the last frame
        // of a frame sequence, you want to Hold that frame, or Repeat
        // the frame sequence -- this is used as the first parameter in the
        // 4-parameter setstate function (see below) -- JC
    }

    class StateType
    {
        public const short Hold = 0; // these are used as the last parameter in the
        public const short Repeat = 1; // 4-parameter setstate function -- JC
    }

    class cSpriteQuake : cSprite
    {
        protected cMD2Model _pModel; 			// the character model 
        protected short _modelState; 	// model state 
        protected int _begframe; 				// beginning frame for OTHER modelState 
        protected int _endframe; 				// ending frame for OTHER modelState 
        protected short _stateType; 		// HOLD or REPEAT for OTHER modelState 
        protected short _lastModelState; // last model state 
        protected string _modelfilename; // the filepath of the model 
        protected string _skinfilename; // the filepath of the skin texture 
        protected cVector3 _correctionpercents; /* slide the image proportional to its bounding
											box radii along the x,y,z axes. */
        protected float _dt; 			//Save the frame dt from animate(dt) for use in render().
        protected float _framerate; 	/* Arbitrary number which is multiplied times _dt to set the rate of
							frames, default is 10, which seems to look good. */
        protected bool statelock = false;

        public cSpriteQuake(int modelIndex)
        {
            _modelfilename = Framework.models.getModelFileName(modelIndex);
            _skinfilename = Framework.models.getSkinFileName(modelIndex);
            _dt = 0.02f;
            _framerate = 3.5f;
            _correctionpercents = Framework.models.getCorrectionPercents(modelIndex);
            _pModel = new cMD2Model(); /* allocate memory for model objects and load it. */
            bool success = _pModel.Load(_modelfilename, _skinfilename);
            if (!success)
            {
                _pModel = null;
                MessageBox.Show("Unable to load one or both of: " + _modelfilename + " " +
                    _skinfilename);
                return;
            }
            _lastModelState = _modelState = State.Run; // set model state to RUN, Giavinh had IDLE.
            /* Normally our sprites of this type will be figurines that "stand" on the
            critter's central point. */
            Radius = _radius; /* Match the appearance to the _radius parameter, that is, 
			 Make _spriteattitude.scalefactor() * _pModel->boundingbox.radius() = _radius. 
			The _correctionpercents are used here as well. */
            fixResourceID();
        }

        /* Acts as no-arg consructor too.
                Note that you can may CString arguments instead of const char * arguments. 
                We translate the sprite by correctionpercents.x()*boundingbox()->xradius()
                etc.  Often we want to move it up in the z direction by half the height
                so it can walk on the "floor," that is on the xy plane, so we often
                put 0.5 in the third place, though it may be, say,  0.2 or 0.7 depending on
                the exact geometry. */
        //Special methods 

        public override void setstate(short newState, int begf, int endf, short type)
        { _modelState = newState; _begframe = begf; _endframe = endf; _stateType = type; }

        /* Scoot the icon up or down to make a walking figure
        seem to rest on the ground or make a jellyfish blob be centerd on the critter's pos.
        Mainly we tweak this to make the visual appearance match a sphere centered on 
        critter's position, can check by using View|Wireframe Polygons. */
        //Overloads 

        public override void fixResourceID()
        {
            _resourceID = 16000 + _pModel.SkinFileKey;
            /* I'm faking a resourceID number so that cBiota.Add can check if
        a critter's sprite uses the same texture as an existing sprite, so that
        we can render similarly textured critters one after the other.  Now
        in practice my bitmap resource IDs are in the 200s, and the skinfileKey
        values range through 0, 1, 2, and so on, using only small numbers.  So
        I can easily tell them apart I add 16,000 to the skinfilekey. */
        }

        /* This virtual helper method is needed
            because I plan to use _resourceID in cBiota::Add to see if
            two sprites use the same textures.  A cSpriteQuake will set a
            _resoruceID keyed to its skin file. */

        /* enabledisplaylist specifies whetehr or not you want to
            try and use a display list to show this kind of sprite. By default this is TRUE.  We 
            make it FALSE because the cSpriteIcon actually runs slower with displaylists, so we
            want to be able turn of display lists for them.  */

        public override void imagedraw(cGraphics pgraphics, int drawflags)
        {
            if (_pModel == null)
            {
                base.imagedraw(pgraphics, drawflags); //Use the baseclass draw (a circle) 
                return;
            }
            // set interpolation between keyframes 
            float percent;
            //percent = 0.2;  
            if (_dt > 0.00001f)
                //	percent = 0.001 * _framerate / _dt;  
                percent = _framerate * _dt;
            /* Want to get a number around 0.1
        to 0.3 range.  I stick in the 0.001 multiplier so that "_framerate"
        can be a reasonable kind of number like 2 or 3 */
            else
                percent = 0.2f;
            /*
               Frame#  Action
               ----------------
               0-39    idle
               40-46   running
               47-60   getting shot but not falling (back bending)
               61-66   getting shot in shoulder
               67-73   jumping
               74-95   idle
               96-112  getting shot and falling down
               113-122 idle
               123-135 idle
               136-154 crouch
               155-161 crouch crawl
               162-169 crouch adjust weapon (idle)
               170-177 kneeling dying
               178-185 falling back dying
               186-190 falling forward dying
               191-198 falling back slow dying
                */

            // set current model animation state 
            _pModel.ModelState = _modelState;
            if (_modelState != _lastModelState)
            {
                _lastModelState = _modelState;
                _pModel.AnimatePercent = 0.0f;
                statelock = false;
            }
            // perform animation based on model state 
            switch (_pModel.ModelState)
            {
                case State.Idle:
                    _pModel.Animate(pgraphics, 0, 39, percent);
                    break;
                case State.Run:
                    _pModel.Animate(pgraphics, 40, 46, percent);
                    break;
                case State.ShotButStillStanding:
                    _pModel.Animate(pgraphics, 47, 60, percent);
                    break;
                case State.ShotInShoulder:
                    _pModel.Animate(pgraphics, 61, 66, percent);
                    break;
                case State.Jump:
                    _pModel.Animate(pgraphics, 67, 73, percent);
                    break;
                case State.ShotDown:
                    _pModel.Animate(pgraphics, 96, 112, percent);
                    break;
                case State.Crouch:
                    _pModel.Animate(pgraphics, 136, 154, percent);
                    break;
                case State.CrouchCrawl:
                    _pModel.Animate(pgraphics, 155, 161, percent);
                    break;
                case State.CrouchWeapon:
                    _pModel.Animate(pgraphics, 162, 169, percent);
                    break;
                case State.KneelDying:
                    _pModel.Animate(pgraphics, 170, 171, percent);
                    break;
                case State.FallbackDie:
                    if ((_pModel.AnimatePercent < 1.0f ||
                        _pModel.CurrentFrame + 1 < _endframe) && !statelock)
                        _pModel.AnimateHold(pgraphics, 178, 185, percent);
                    else
                    {
                        _pModel.RenderFrame(pgraphics, 183);
                        statelock = true;
                    }
                    break;
                case State.FallForwardDie:
                    if ((_pModel.AnimatePercent < 1.0f ||
                        _pModel.CurrentFrame + 1 < _endframe) && !statelock)
                        _pModel.AnimateHold(pgraphics, 186, 190, percent);
                    else
                    {
                        _pModel.RenderFrame(pgraphics, 189);
                        statelock = true;
                    }
                    break;
                case State.Other:
                    if (_stateType == StateType.Hold)
                    {
                        if ((_pModel.AnimatePercent < 1.0f ||
                             _pModel.CurrentFrame + 1 < _endframe) && !statelock)
                            _pModel.AnimateHold(pgraphics, _begframe, _endframe, percent);
                        else
                        {
                            _pModel.RenderFrame(pgraphics, _endframe);
                            statelock = true;
                        }
                    }
                    else if (_stateType == StateType.Repeat)
                        _pModel.Animate(pgraphics, _begframe, _endframe, percent);
                    break;
                default:
                    break;
            }
        }

        public override bool IsKindOf(string str)
        {
            return str == "cSpriteQuake" || base.IsKindOf(str);
        }

        public override void animate(float dt, cCritter powner) { _dt = dt; }

        /* Set _radius to newradius, and then match
            the appearance to the _radius parameter, that is, make
            _spriteattitude.scalefactor() * _pModel->boundingbox.radius() = _radius. */

        public override short ModelState
        {
            get
                { return _modelState; }
            set
                { _modelState = value; }
        }

        public virtual cVector3 CorrectionPercents
        {
            set
            {
                /* It may be that you don't want a figurine that stands on the critter origin. Maybe 
                you want a jellyfish centered around the critter origin. */

                _correctionpercents.copy(value);
                float scalefactor = _spriteattitude.ScaleFactor;
                cRealBox3 box = _pModel.boundingbox(0); //Could get frame 40 size, but its about the same.
                _spriteattitude = cMatrix3.scale(scalefactor).mult(
                    cMatrix3.translation(new cVector3(_correctionpercents.X * box.XSize,
                        _correctionpercents.Y * box.YSize, _correctionpercents.Z * box.ZSize)));
                //	_spriteattitude =  cMatrix::translation(cVector(0.0, 0.0, _zElevation)) *  
                //		cMatrix::scale(scalefactor);  
            }
        }

        public override bool UsesTexture
        {
            get
                { return true; }
        }

        public override bool EnabledDisplayList
        {
            get
                { return false; }
        }

        public override float Radius
        {
            get
                { return _radius; }
            set
            {
                if (_pModel == null)
                {
                    base.Radius = value;
                    return;
                }
                /*   Match the appearance to the _radius parameter, that is, 
                    make _spriteattitude.scalefactor() * _pModel->boundingbox.radius() = _radius. */
                cRealBox3 box = _pModel.boundingbox(0); //Could get frame 40 size, but its about the same.
                float boxradius = box.Radius;
                float scalefactor;

                _radius = value;
                /* The visual size of the sprite is about 0.85*scalefactor*box.averageradius.  
            The 0.85 is an eyeball impression I have that the "sphere" around a running
            figure isn't really the extent of the figure, it's tighter in. Match to _radius. */
                if (box.AverageRadius > 0.00001f)
                    scalefactor = _radius / (0.85f * box.AverageRadius);
                else
                    scalefactor = 1.0f; //punt if there's no box radius.
                /* A matrix of the form S * T, the T acts first and then the S.  So we slide
                the figurine up to put its base at critter origin if _zElevation wants this,
                and then we scale it, which will leave base at origin. */
                _spriteattitude = cMatrix3.scale(scalefactor).mult(
                    cMatrix3.translation(new cVector3(_correctionpercents.X * box.XSize,
                        _correctionpercents.Y * box.YSize, _correctionpercents.Z * box.ZSize)));
                NewGeometryFlag = true; //This doesn't matter here, but stay consistent with cSprite 
            }
        }

        public virtual bool IsLoaded
        {
            get
                { return _pModel != null; }
        }


    }
}

//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 