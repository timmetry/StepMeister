using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StepFu
{
	public struct Options
	{
//        public bool footswitches; // if true, every double up arrow sequence will be treated as a footswitch

		public int drillChance;
		public int candleChance;
		public int doubleTapChance;
		public int crossoverChance;
		public int cross180Chance;

		// this is for an easier, slower difficulty where position doesn't matter that much
		public Options SetEasyMode()
		{
			drillChance = 15;
			candleChance = 50;
			doubleTapChance = 20;
			crossoverChance = 10;
			cross180Chance = 0;
			return this;
		}

		// this is for a normal expert track, to make everything flow smoothly without anything fancy
		public Options SetNormalMode()
		{
			drillChance = 15;
			candleChance = 50;
			doubleTapChance = 0;
			crossoverChance = 0;
			cross180Chance = 0;
			return this;
		}

		// this is for a normal expert track, to make everything flow smoothly without anything fancy
		public Options SetTrickyMode()
		{
			drillChance = 15;
			candleChance = 50;
			doubleTapChance = 0;
			crossoverChance = 20;
			cross180Chance = 0;
			return this;
		}

		// this is for a super expert level track where runs at insane speeds are common and long
		public Options SetIntenseMode()
		{
			drillChance = 30;
			candleChance = 25;
			doubleTapChance = 0;
			crossoverChance = 0;
			cross180Chance = 0;
			return this;
		}

		// this is for a part of a song that's insanely fast and requires zero movement
		public Options SetDrillMode()
		{
			drillChance = 100;
			candleChance = 0;
			doubleTapChance = 0;
			crossoverChance = 0;
			cross180Chance = 0;
			return this;
		}

		// this is for an intense part of a DDR-style song consisting of constant crossovers and candles
		public Options SetTwisterMode()
		{
			drillChance = 0;
			candleChance = 75;
			doubleTapChance = 0;
			crossoverChance = 75;
			cross180Chance = 0;
			return this;
		}
	}
}
