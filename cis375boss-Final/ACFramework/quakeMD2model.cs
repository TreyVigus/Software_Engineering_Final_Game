// For AC Framework Version 1.2, I've fixed a bug for Hold patterns in models -- previously, the first
// state in a sequence could be repeated.  I've also replaced a lot of bit conversion code
// with C#'s BitConverter functions; some of this was marked as unsafe, so I'm glad it no
// longer exists.
// Default parameters were added -- JC

using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace ACFramework
{

    /*
    The MD2 mesh type was orginally created for Quake 2.  You can create your own
    with Milkshape.
    11/22/01	Giavinh	Pham Created. Added MD2 mesh model (animation) into the game.
    4/14/2003  Rudy Rucker reworked so the textures are handled by cGraphics.
    6/13/08	   Jeffrey Childs reworked to add extra model states and convert to C# code
    */

    /* This file was among the most troublesome to convert to C#, and I spent about 5 days
     * on it before I could get everything right.  I am recording the rework that needed to
     * be done as part of the history of this file.  
     * The PCX and MD2 files are byte files, so it was very important to keep the types used
     * in C# consistent with the type sizes in MFC.  The palette and pdata objects were
     * really stored as strings in MFC, but I changed their types to bytes, because they
     * were so bulky.  While a character is one byte in MFC, it is 2 bytes in C# (unicode).
     * I couldn't, in good conscience, leave them as char types.
     * C# doesn't allow arrays to be given a size in the struct definition, so I ended up
     * making constructors for the structs to set the array sizes.  C# does not allow
     * parameterless constructors for structs, even though all I wanted to do was set the
     * array sizes to fixed lengths in most cases, so I pass in a dummy int variable into
     * the constructor that isn't used for anything other than satisfying C#'s requirement.
     * This made it much easier to initialize array sizes, rather than do it throughout
     * the code.
     * cMapFilenameToTextureInfo inherited from MFC's CMap class, which does not exist in C#.
     * However, I've noted that CMap really is not much more than a hash table.  There is
     * a Hashtable class in C#, so I had cMapFilenameToTextureInfo inherit from that instead,
     * which worked out well.
     * The original code was written to handle both BMP and PCX skin files.  However, out of
     * the hundreds of models that I have on hand, only one has a BMP skin file.  For that
     * reason, I couldn't justify the time to tranalate the BMP code into C#, and I ended
     * up cutting this code out.
     * C# and MFC have radically different ways of working with files.  The original designer
     * of these classes used the old C-style way of working with files, using fwrite, fread,
     * fseek, and the like.  I presume this is because MFC has the annoying characteristic
     * of disabling fstream.  I myself wrote a lot of data into files using fprintf, to
     * check the data against what I was getting with C#  (I'm showing my age).  In this
     * code, I ended up reading the entire PCX or MD2 file into a byte array, and then I 
     * worked with the byte array.
     * The models in the MFC code couldn't do much more than run.  I added quite
     * a few other states for the models (in MFC initially), and I made them public static readonly 
     * in the cMD2Model class, when it was eventually translated to C#.  I should have
     * recorded this as part of the history, as I had reworked this part about a couple of
     * years ago in MFC.
        -- JC   
     */

    // only partial pcx file header 
    struct PCXHEADER
    {
        //		public byte manufacturer; 
        //		public byte version; 
        //		public byte encoding; 
        //		public byte bits; 
        public uint xMin; //This and the following four lines were unsigned char 
        public uint yMin; //And this meant I couldn't store a correct size in them.
        public uint xMax; //If the size was greater than 256, so a PCX of 
        public uint yMax; //size 320 by 200 wouldn't load.
        public byte[] palette;
    }

    // a single vertex 
    struct vector_t
    {
        public float[] point;

        public vector_t(int i)
        {
            point = new float[3];
        }
    }


    /* 
    MD2 Model Helper Structures
    */

    // texture coordinate 
    struct texCoord_t
    {
        public float s;
        public float t;
    }

    // info for a single frame point 
    struct framePoint_t
    {
        public byte[] v;

        public framePoint_t(int i)
        {
            v = new byte[3];
        }
    }

    // information for a single frame 
    struct frame_t
    {
        public float[] scale;
        public float[] translate;
        public char[] name;
        public framePoint_t[] fp;

        public frame_t(int i)
        {
            scale = new float[3];
            translate = new float[3];
            name = new char[16];
            fp = null;
        }
    }

    // data for a single triangle 
    struct mesh_t
    {
        public ushort[] meshIndex; // vertex indices 
        public ushort[] stIndex; // texture coordinate indices 

        public mesh_t(int i)
        {
            meshIndex = new ushort[3];
            stIndex = new ushort[3];
        }
    }

    struct modelHeader_t
    {
        public int ident; // identifies as MD2 file "IDP2" 
        public int version; // mine is 8 
        public int skinwidth; // width of texture 
        public int skinheight; // height of texture 
        public int framesize; // number of bytes per frame 
        public int numSkins; // number of textures 
        public int numXYZ; // number of points 
        public int numST; // number of texture 
        public int numTris; // number of triangles 
        public int numGLcmds;
        public int numFrames; // total number of frames 
        public int offsetSkins; // offset to skin names (64 bytes each) 
        public int offsetST; // offset of texture s-t values 
        public int offsetTris; // offset of triangle mesh 
        public int offsetFrames; // offset of frame data (points) 
        public int offsetGLcmds; // type of OpenGL commands to use 
        public int offsetEnd; // end of file 
    }

    //===================Class for turning strings into integer keys 

    class cStringPool : LinkedList<string>
    {
        public cStringPool()
            : base(delegate(out string str1, string str2)
            {
                str1 = str2;
            }
            )
        { }

        public int lookupKey(string str)
        {
            int i = 0;
            foreach (string str2 in this)
            {
                if (str == str2)
                    return i;
                else
                    i++;
            }
            Add(str);
            return Size - 1;
        }

    }

    class cMapFilenameToTextureInfo : Hashtable
    {
        public static readonly cStringPool _skinFilenamePool = new cStringPool();

        public bool Lookup(string skinfilename, out cTextureInfo ptextureinfo)
        {
            bool success;
            int skinfileKey = _skinFilenamePool.lookupKey(skinfilename);

            ptextureinfo = (cTextureInfo)this[skinfileKey];
            if (ptextureinfo != null) //We've already read in and saved the _pdata before.
                return true;
            //Otherwise create and register a new cTextureInfo* object that's loaded with the skinfile.
            ptextureinfo = new cTextureInfo();
            success = ptextureinfo.LoadPCXTexture(skinfilename);
            if (success) //Save the ptextureinfo 
                this[skinfileKey] = ptextureinfo;

            return success;
        }


        public int lookupKey(string skinfilename) { return _skinFilenamePool.lookupKey(skinfilename); }
        /* I need cMapFilenameToTextureInfo::lookupKey for the cTextureInfo::skinfileKey
        method whichs is used by the cGraphicsOpenGL::selectSkinTexture. */

    }

    class cTextureInfo
    {
        //static readonly 
        public static readonly cMapFilenameToTextureInfo _mapFilenameToTextureInfo =
            new cMapFilenameToTextureInfo();
        //members									 
        public int _width; 					// width of texture 
        public int _height; 					// height of texture 
        public byte[] _pdata; 		// the texture data 
        public byte[] _ppalette;
        //TextureInfo loading methods 

        public byte[] LoadPCXFile(string skinfilename, ref PCXHEADER pcxHeader)
        {
            int idx = 0; // counter index 
            int c; // used to retrieve a char from the file 
            int i, p; // counter index 
            int numRepeat;
            int width; // pcx width 
            int height; // pcx height 
            byte[] pixelData; // pcx image data 
            byte[] filedata;

            filedata = File.ReadAllBytes(skinfilename);

            // open and read PCX file 
            filedata = File.ReadAllBytes(skinfilename);
            // retrieve first character; should be equal to 10 
            if (filedata[0] != 10)
                return null;
            // retrieve next character; should be equal to 5 
            if (filedata[1] != 5)
                return null;

            c = 4;
            int xminlo = filedata[c++]; // loword 
            int xminhi = filedata[c++]; // hiword 
            xminhi <<= 8;
            int xmin = xminlo | xminhi;
            pcxHeader.xMin = (uint)xmin;

            // retrieve bottom-most y value of PCX 
            pcxHeader.yMin = filedata[c++]; // loword 
            pcxHeader.yMin |= (((uint)filedata[c++]) << 8); // hiword 
            // retrieve rightmost x value of PCX 
            pcxHeader.xMax = filedata[c++]; // loword 
            pcxHeader.xMax |= (((uint)filedata[c++]) << 8); // hiword 
            // retrieve topmost y value of PCX 
            pcxHeader.yMax = filedata[c++]; // loword 
            pcxHeader.yMax |= (((uint)filedata[c++]) << 8); // hiword 
            // calculate the width and height of the PCX 
            width = (int)(pcxHeader.xMax - pcxHeader.xMin) + 1;
            height = (int)(pcxHeader.yMax - pcxHeader.yMin) + 1;

            pixelData = new byte[width * height];
            // set c to 128th byte of file, where the PCX image data starts 
            c = 128;
            p = 0;
            // decode the pixel data and store 
            while (idx < (width * height))
            {
                if (filedata[c++] > 0xbf)
                {
                    numRepeat = 0x3f & filedata[c - 1];
                    for (i = 0; i < numRepeat; i++)
                    {
                        pixelData[p++] += filedata[c];
                        idx++;
                    }
                    c++;
                }
                else
                {
                    pixelData[p++] += filedata[c - 1];
                    idx++;
                }
            }

            // palette is the last 769 bytes of the PCX file
            c = filedata.Length - 769;
            // verify palette; first character should be 12 
            if (filedata[c++] != 12)
                return null;
            // allocate memory for the PCX image palette 
            pcxHeader.palette = new byte[768];
            // read and store all of palette
            for (i = 0; i < 768; i++)
                pcxHeader.palette[i] = filedata[c++];
            // return the pixel image data 
            return pixelData;
        }

        public bool LoadPCXTexture(string skinfilename)
        {
            PCXHEADER headerinfo; // header of texture 
            byte[] unscaledData; 			// used to calculate pcx 
            int i; // index counter 
            int j; // index counter 

            // load the PCX file into the texture struct
            headerinfo = default(PCXHEADER);
            _pdata = LoadPCXFile(skinfilename, ref headerinfo);
            if (_pdata == null)
                return false;
            // store the texture information 
            _ppalette = headerinfo.palette;
            _width = (int)(headerinfo.xMax - headerinfo.xMin) + 1;
            _height = (int)(headerinfo.yMax - headerinfo.yMin) + 1;
            unscaledData = new byte[_width * _height * 4];
            // store the unscaled rearranged data via the _ppalette 
            for (j = 0; j < _height; j++)
            {
                for (i = 0; i < _width; i++)
                {
                    unscaledData[4 * (j * _width + i) + 0] = _ppalette[
                        3 * _pdata[j * _width + i] + 0];
                    unscaledData[4 * (j * _width + i) + 1] = _ppalette[
                        3 * _pdata[j * _width + i] + 1];
                    unscaledData[4 * (j * _width + i) + 2] = _ppalette[
                        3 * _pdata[j * _width + i] + 2];
                    unscaledData[4 * (j * _width + i) + 3] = (byte)255;
                }
            }

            // clear the texture data and reallocate new memory for _pdata 
            _pdata = new byte[_width * _height * 4];
            // copy the rearranged unscaled data 
            for (i = 0; i < _width * _height * 4; i++)
                _pdata[i] = unscaledData[i];
            //Done
            return true;
        }


        public cTextureInfo() { }

        public void resetData(int width, int height, byte[] pdatanew)
        {
            _pdata = new byte[pdatanew.Length];
            for (int i = 0; i < pdatanew.Length; i++)
                _pdata[i] = pdatanew[i];
            _width = width;
            _height = height;
        }

        public virtual byte[] Data
        {
            get
                { return _pdata; }
        }


    }

    //MD2Model Stuff========================================== 
    class cMapFilenameToMD2Info : Hashtable
    {
        public static readonly cStringPool _modelFilenamePool = new cStringPool();

        public bool Lookup(string modelfilename, string skinfilename, out cMD2Info pmd2info)
        {
            bool success;
            int modelfileKey = _modelFilenamePool.lookupKey(modelfilename);

            if (Contains(modelfileKey))
            {
                pmd2info = (cMD2Info)this[modelfileKey];
                return true; //We've already read in and saved the _pdata before.
            }
            //Otherwise create and register a new cMD2Info* object that's loaded with the modelfile.
            pmd2info = new cMD2Info();
            success = pmd2info.Load(modelfilename, skinfilename);
            if (success) //Save the ptextureinfo 
                this[modelfileKey] = pmd2info;
            return success;
        }

    }

    struct cMD2Info
    {
        public int numFrames; 				// number of model frames 
        public int numVertices; 			// number of vertices per frame 
        public int _numTriangles; 			// number of triangles per frame 
        public int numST; 					// number of skins.  Not used now, we have only one skin.
        public int frameSize; 				// size of each frame in bytes 
        public mesh_t[] _triIndex; 			// triangle list 
        public texCoord_t[] st; 			// texture coordinate list 
        public vector_t[] vertexList; 		// vertex list 


        public bool Load(string modelfilename, string skinfilename)
        {
            //First load the modelfilename================== 
            byte[] buffer; // file buffer 

            modelHeader_t modelHeader; // model header 

            frame_t frame; // frame data 
            int vertexListPos; // index variable 
            int i, j; // index variables 

            // open the model file and read entire file into buffer
            buffer = File.ReadAllBytes(modelfilename);


            // extract model file header from buffer
            int c = 0;
            modelHeader.ident = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.version = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.skinwidth = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.skinheight = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.framesize = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.numSkins = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.numXYZ = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.numST = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.numTris = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.numGLcmds = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.numFrames = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.offsetSkins = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.offsetST = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.offsetTris = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.offsetFrames = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.offsetGLcmds = BitConverter.ToInt32(buffer, c);
            c += 4;
            modelHeader.offsetEnd = BitConverter.ToInt32(buffer, c);
            c += 4;

            vertexList = new vector_t[modelHeader.numXYZ * modelHeader.numFrames];
            for (i = 0; i < modelHeader.numXYZ * modelHeader.numFrames; i++)
                vertexList[i] = new vector_t(0);

            numVertices = modelHeader.numXYZ; //Vertices per frame 
            numFrames = modelHeader.numFrames;
            frameSize = modelHeader.framesize; //Size of a frame in bytes 

            for (j = 0; j < numFrames; j++)
            {
                c = modelHeader.offsetFrames + frameSize * j;
                frame = new frame_t(0);
                frame.fp = new framePoint_t[numVertices];
                for (int vernum = 0; vernum < numVertices; vernum++)
                    frame.fp[vernum] = new framePoint_t(0);
                for (int scalenum = 0; scalenum < 3; scalenum++)
                {
                    frame.scale[scalenum] = BitConverter.ToSingle(buffer, c);
                    c += 4;
                }
                for (int translatenum = 0; translatenum < 3; translatenum++)
                {
                    frame.translate[translatenum] = BitConverter.ToSingle(buffer, c);
                    c += 4;
                }
                for (int namenum = 0; namenum < 16; namenum++)
                    frame.name[namenum] = (char)buffer[c++];
                for (int vernum = 0; vernum < numVertices; vernum++)
                {
                    for (int vnum = 0; vnum < 3; vnum++)
                        frame.fp[vernum].v[vnum] = buffer[c++];
                    c++;    // I had to put this in to compensate for boundary alignment -- JC
                }
                vertexListPos = numVertices * j;
                for (i = 0; i < numVertices; i++)
                {
                    vertexList[vertexListPos + i].point[0] = frame.scale[0] * frame.fp[i].v[0] + frame.translate[0];
                    vertexList[vertexListPos + i].point[1] = frame.scale[1] * frame.fp[i].v[1] + frame.translate[1];
                    vertexList[vertexListPos + i].point[2] = frame.scale[2] * frame.fp[i].v[2] + frame.translate[2];
                }
            }

            _numTriangles = modelHeader.numTris;
            _triIndex = new mesh_t[_numTriangles];
            for (int n = 0; n < _numTriangles; n++)
                _triIndex[n] = new mesh_t(0);

            // set c to triangle indexes in buffer 
            c = modelHeader.offsetTris;

            for (i = 0; i < _numTriangles; i++)
            {
                _triIndex[i].meshIndex[0] = BitConverter.ToUInt16(buffer, c);
                c += 2;
                _triIndex[i].meshIndex[1] = BitConverter.ToUInt16(buffer, c);
                c += 2;
                _triIndex[i].meshIndex[2] = BitConverter.ToUInt16(buffer, c);
                c += 2;
                _triIndex[i].stIndex[0] = BitConverter.ToUInt16(buffer, c);
                c += 2;
                _triIndex[i].stIndex[1] = BitConverter.ToUInt16(buffer, c);
                c += 2;
                _triIndex[i].stIndex[2] = BitConverter.ToUInt16(buffer, c);
                c += 2;
            }

            /* Now get the cTextureInfo* object, which will have the effect of registering
        the filename and creating and retgistering a cTextureInfo*, if this is the first time
        the skin file name is used. */
            cTextureInfo ptextureinfo = null;
            bool success = cTextureInfo._mapFilenameToTextureInfo.Lookup(skinfilename, out ptextureinfo);
            if (!success || ptextureinfo == null)
                return false;

            //Use the model info and skin info together to initialize texture coords st 
            int imagewidth = ptextureinfo._width;

            int imageheight = ptextureinfo._height;

            /* The stPtr[i].s and stPtr[i].s are actually pixel count
            numbers, as in "m-th pixel over and n-th pixel down in my skin file".
            We convert them into proportions between 0.0 and 1.0 relative to the
            size of the original skin image that was saved with the MD2 model
            and save these values in the st[i].s and st[i].t so the OpenGL
            texture can use them.
                Even if we plan to rescale the image that won't in fact affect the
            proportional locations of the texture points since we will still think
            of the map as a swatch in the plane from corner (0.0,0.0) to corner
            (1.0, 1.0).  So there seems to be no need to compute here the rescaled
            image size with cTexture::makePowersOfTwo(imagewidth, imageheight);
            and then use it in any fashion. */
            numST = modelHeader.numST;
            st = new texCoord_t[numST];
            c = modelHeader.offsetST;

            for (i = 0; i < numST; i++)
            {
                float s = BitConverter.ToInt16(buffer, c);
                c += 2;
                float t = BitConverter.ToInt16(buffer, c);
                c += 2;
                st[i].s = s / (float)imagewidth;
                st[i].t = t / (float)imageheight;
            }

            return true;
        }

        public cRealBox3 boundingbox(int framenumber = 0)
        {
            /*We return the bounding box for the critter as it appears in the first frame.
            We have numVertices vertices per frame, so the first numVertices all belong to
            the first frame. */
            float lox, hix, loy, hiy, loz, hiz, val;

            int baseindex = numVertices * framenumber;

            lox = hix = vertexList[0].point[0];
            loy = hiy = vertexList[0].point[1];
            loz = hiz = vertexList[0].point[2];

            for (int i = 1; i < numVertices; i++)
            {
                val = vertexList[baseindex + i].point[0];
                if (val < lox) { lox = val; }
                if (val > hix) { hix = val; }
                val = vertexList[baseindex + i].point[1];
                if (val < loy) { loy = val; }
                if (val > hiy) { hiy = val; }
                val = vertexList[baseindex + i].point[2];
                if (val < loz) { loz = val; }
                if (val > hiz) { hiz = val; }
            }
            return new cRealBox3(new cVector3(lox, loy, loz), new cVector3(hix, hiy, hiz));
        }

    }

    class cMD2Model
    {
        public static readonly cMapFilenameToMD2Info _mapFilenameToMD2Info =
            new cMapFilenameToMD2Info();
        protected string _skinfilename; //Use this to get cTextureInfo 
        //Store the bulky cMD2Info _pdata arrays in a shared static readonly cMD2Info object 
        protected string _modelfilename; //Use with _mapFilenameToMD2Info to find a cMD2Info object.
        //Individual instance data 
        protected int _currentFrame; 			// current frame # in animation 
        protected int _nextFrame; 				// next frame # in animation 
        protected float _interpol; 			// percent through current frame 
        protected short _modelState; 	// current model animation state 

        public cMD2Model()
        {
            _currentFrame = 0; // current keyframe  
            _nextFrame = 1; // next keyframe 
            _interpol = 0.0f; // interpolation percent 
            _skinfilename = ""; // skinfilename 
            _modelfilename = ""; // modelfilename 
            _modelState = State.Idle;
        }

        // constructor 
        //Shared Static cMD2Info data Methods	 

        public bool Load(string modelfilename, string skinfilename)
        {
            _modelfilename = modelfilename;
            _skinfilename = skinfilename;
            cMD2Info pdummy;
            return _mapFilenameToMD2Info.Lookup(_modelfilename, _skinfilename, out pdummy);
        }


        // If necessary, make a new cMD2Info object and load model, also load skin.
        //Accessors 
        /* triIndex, numTriangles, textureCoords are
            used by cGraphicsOpenGL::interpolateAndRender */

        public cRealBox3 boundingbox(int framenumber) { return MD2Info.boundingbox(framenumber); } //Used by cSpriteQuake constructor and setRadius.

        /* Used by cGraphicsOpenGL::selectSkinTexture in
            the cGraphicsOpenGL::interpolateAndRender method. */

        /* Used by cGraphicsOpenGL::selectSkinTexture. */
        //Individual Instance Methods 

        public int Animate(cGraphics pgraphics, int startFrame, int endFrame, float percent)
        {
            if ((startFrame > _currentFrame))
                _currentFrame = startFrame;
            if ((startFrame < 0) || (endFrame < 0))
                return -1;
            if ((startFrame >= NumFrames) || (endFrame >= NumFrames))
                return -1;
            if (_interpol >= 1.0f)
            {
                _interpol = 0.0f;
                _currentFrame++;
                if (_currentFrame >= endFrame)
                    _currentFrame = startFrame;
                _nextFrame = _currentFrame + 1;
                if (_nextFrame >= endFrame)
                    _nextFrame = startFrame;
            }

            cMD2Info pinfo = MD2Info;
            int numvertsperframe = pinfo.numVertices;
            int startframe, endframe;
            startframe = numvertsperframe * _currentFrame;
            endframe = numvertsperframe * _nextFrame;
            pgraphics.interpolateAndRender(this, pinfo.vertexList, startframe, endframe, _interpol);

            _interpol += percent; // increase percentage of interpolation between frames 

            return 0;
        }

        // This function was created to fix a bug in a hold pattern -- 
        // It makes sure a beginning frame is not repeated -- JC
        public int AnimateHold(cGraphics pgraphics, int startFrame, int endFrame, float percent)
        {
            if ((startFrame > _currentFrame))
                _currentFrame = startFrame;
            if ((startFrame < 0) || (endFrame < 0))
                return -1;
            if ((startFrame >= NumFrames) || (endFrame >= NumFrames))
                return -1;
            if (_interpol >= 1.0f)
            {
                _interpol = 0.0f;
                _currentFrame++;
                if (_currentFrame >= endFrame)
                    _currentFrame = startFrame;
                _nextFrame = _currentFrame + 1;
            }

            cMD2Info pinfo = MD2Info;
            int numvertsperframe = pinfo.numVertices;
            int startframe, endframe;
            startframe = numvertsperframe * _currentFrame;
            endframe = numvertsperframe * _nextFrame;
            pgraphics.interpolateAndRender(this, pinfo.vertexList, startframe, endframe, _interpol);

            _interpol += percent; // increase percentage of interpolation between frames 

            return 0;
        }

        // render model with interpolation to get animation 

        public int RenderFrame(cGraphics pgraphics, int keyFrame)
        {
            return Animate(pgraphics, keyFrame, keyFrame, 0.0f);
        }

        // render a single frame 
        //Mutators 

        // set animation state of model 

        // set the current interpolatio percent 
        //Accessors 

        // retrieve animation state of model 

        // get the current interpolation percent 

        public virtual cMD2Info MD2Info
        {
            get
            {
                cMD2Info pinfo;
                bool success;

                success = _mapFilenameToMD2Info.Lookup(_modelfilename, _skinfilename, out pinfo);
                return pinfo; //If failure.
            }
        }

        public virtual mesh_t[] TriIndex
        {
            get
                { return MD2Info._triIndex; }
        }

        public virtual int NumTriangles
        {
            get
                { return MD2Info._numTriangles; }
        }

        public virtual int NumFrames
        {
            get
                { return MD2Info.numFrames; }
        }

        public virtual vector_t[] VertexList
        {
            get
                { return MD2Info.vertexList; }
        }

        public virtual texCoord_t[] TextureCoords
        {
            get
                { return MD2Info.st; }
        }

        public virtual cRealBox3 BoundingBox
        {
            get
                { return MD2Info.boundingbox(0); }
        }

        public virtual cTextureInfo TextureInfo
        {
            get
            {
                cTextureInfo pinfo = null;
                bool success;

                success = cTextureInfo._mapFilenameToTextureInfo.Lookup(_skinfilename, out pinfo);
                return pinfo; //If failure.
            }
        }

        public virtual int SkinFileKey
        {
            get
                { return cTextureInfo._mapFilenameToTextureInfo.lookupKey(_skinfilename); }
        }

        public virtual short ModelState
        {
            get
            {
                return _modelState;
            }
            set
            {
                _modelState = value;
            }
        }

        public virtual float AnimatePercent
        {
            get
            {
                return _interpol;
            }
            set
            {
                _interpol = value;
            }
        }

        public virtual int CurrentFrame
        {
            get
            {
                return _currentFrame;
            }
        }


    }
}
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     