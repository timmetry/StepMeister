using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StepFu
{
	public enum Pos
	{
		Unknown = -1,
		Left = 0,
		Down = 1,
		Up = 2,
		Right = 3,
		Max = 4,
	}

	public enum Foot
	{
		Ambiguous = -1,
		None = 0,
		Left = 1,
		Right = 2,
		Both = 3,
	}

	public enum Flag
	{
		Invalid=-2,	    // an uninitialized Flag
		Trail=-1,	    // a trail is ending but the player is not moving
		Normal,		    // a random type of single step based on the options
		Jump,		    // a random type of jump step based on the options
		Drill,		    // the next foot steps with the same as last time, meaning zero movement
		Candle,		    // a step moving a foot from up arrow to down arrow, reaching across the pad
		DoubleTap,	    // a step where the same arrow as last time was hit again with the same foot
		Crossover,	    // an advanced step where the right foot is on the left arrow or vice versa
		Cross180,	    // an extended crossover with right on left and left on right AT THE SAME TIME
        FootswitchPrep, // the arrow right before a footswitch (must either be down or up)
        Footswitch,     // a footswitch step (press the same arrow with the opposite foot)
	}
	
	public enum Note
	{
		None,	// empty space in the chart, denoted by '0' in the .sm file
		Step,	// a regular arrow with no special properties, denoted by '1' in the .sm file
		Hold,	// the start of a hold arrow with a smooth trail that must be held down ('2' in the .sm file)
		Trail,	// the end of a hold or roll trail, denoted by '3' in the .sm file
		Roll,	// the start of a roll arrow with a jagged trail that must be tapped repeatedly ('4' in the .sm file)
		Mine,	// a mine or bomb that must be avoided, denoted by 'M' in the .sm file
		Max,	// not a valid note... denotes the number of types of notes
	}
}
