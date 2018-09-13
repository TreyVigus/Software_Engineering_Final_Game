/*  This class replaces the cController class that was used in the Pop framework.  It is
 * more efficient because it does not process every key of the keyboard, it only 
 * processes the keys that the programmer selects for use in the game.  The disadvantage
 * of this class compared to the Pop framework is that the programmer has to specifically
 * select the keys that need to be used for the game, and place these in the resource.cs
 * file, but I think the tradeoff is worth it. -- JC  */


using System;
using OpenTK.Input;


namespace ACFramework
{
    class cKeyInfo
    {
	    private	int pressed;
	    private	float age;
        private float _dt;
        private bool[] keyinfo;

	    public cKeyInfo()
		{
            keyinfo = new bool[vk.KeyList.Length];
            age = 0.0f;

		}

        // returns how long the key k has been pressed down -- can be useful if
        // events should change depending on how long a key is pressed
		public float keystateage(int k)
		{
			if ( !this[k] )
			{
				if ( pressed == k ) 
					age = 0.0f;
				return 0.0f;
			}

            if (k != pressed)
            {
                pressed = k;
                age = 0.0f;
            }

			return age;
		}

		public void update(float dt)
		{
			age += dt;
            _dt = dt;
        }

        public float dt()
        {
            return _dt;
        }

        public void setkey(int i)
        {
            keyinfo[i] = true;
        }

        public void resetkey(int i)
        {
            keyinfo[i] = false;
        }

        public bool this[int i]
        {
            get
            {
                return keyinfo[i];
            }
        }
    }
}
