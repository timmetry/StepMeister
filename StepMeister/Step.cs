using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StepFu
{
	public class Step
	{
		protected Step first;
		protected Step prev;
		protected Step next;
		protected Pos leftFoot;
		protected Pos rightFoot;
		protected Foot footMoved = Foot.None;
		protected Foot footHeld = Foot.None;
		protected Flag flag = Flag.Invalid;
		protected Note[] beats = new Note[4];

		public Pos LeftFoot { get { return leftFoot; } }
		public Pos RightFoot { get { return rightFoot; } }
		public Foot FootMoved { get { return footMoved; } }
		public Flag StepFlag { get { return flag; } }

		// default constructor starts a new chart
		public Step()
		{
			first = this;
			leftFoot = Pos.Left;
			rightFoot = Pos.Right;
		}

		private Step(Step prevStep)
		{
			first = prevStep.first;
			prev = prevStep;
			leftFoot = prevStep.leftFoot;
			rightFoot = prevStep.rightFoot;
			footHeld = prevStep.footHeld;
		}

		public void Validate()
		{
			if (first != this && flag == Flag.Invalid)
				throw new Exception("ERROR: Arrow for Step not yet Initialized.");
		}

		public override string ToString()
		{
			Validate();
			string s = "";
			foreach (Note n in beats)
			{
				switch (n)
				{
					case Note.None:
						s += '0';
						break;
					case Note.Step:
						s += '1';
						break;
					case Note.Hold:
						s += '2';
						break;
					case Note.Trail:
						s += '3';
						break;
					case Note.Roll:
						s += '4';
						break;
					case Note.Mine:
						s += 'M';
						break;
					default:
						// this should never happen
						throw new Exception();
				}
			}
			return s;
		}

		private Foot GetLastFootMoved()
		{
            if (footMoved != Foot.None && (footHeld == Foot.Left || footHeld == Foot.Right))
                return footHeld;
//            else if (prev != null && ((prev.footHeld == Foot.Left && footMoved == Foot.Right) || (prev.footHeld == Foot.Right && footMoved == Foot.Left)))
//                return footMoved;
            else if (footMoved == Foot.Left || footMoved == Foot.Right)
                return footMoved;
            else if (prev == null)
                return Foot.None;
            else
                return prev.GetLastFootMoved();
		}

		private void EndTrails(int numTrails)
		{
			if (numTrails > 0 && flag == Flag.Invalid)
				flag = Flag.Trail;

			if (footHeld == Foot.Left && numTrails > 0)
			{
				// end the trail on the left foot
				beats[(int)leftFoot] = Note.Trail;
				footHeld = Foot.None;
			}
			else if (footHeld == Foot.Right && numTrails > 0)
			{
				// end the trail on the right foot
				beats[(int)rightFoot] = Note.Trail;
				footHeld = Foot.None;
			}
			else if (footHeld == Foot.Both && numTrails > 1)
			{
				// end the trails on both feet
				beats[(int)leftFoot] = Note.Trail;
				beats[(int)rightFoot] = Note.Trail;
				footHeld = Foot.None;
			}
			else if (footHeld == Foot.Both && numTrails == 1)
			{
				// for now, simply find the longer trail and end that
				Step temp = prev;
				while (temp.footHeld == Foot.Both)
					temp = prev.prev;

				if (temp.footHeld == Foot.Left)
					footHeld = Foot.Right;
				else if (temp.footHeld == Foot.Right)
					footHeld = Foot.Left;
				else if (temp.footMoved == Foot.Left)
					footHeld = Foot.Right;
				else if (temp.footMoved == Foot.Right)
					footHeld = Foot.Left;
				else switch (temp.GetLastFootMoved())
				{
					case Foot.Left:
						footHeld = Foot.Right;
						break;
					case Foot.Right:
						footHeld = Foot.Left;
						break;
					default:
						// always start with the left foot
						footHeld = Foot.Right;
						break;
				}
				
				// now, which trail are we ending?
				if (footHeld == Foot.Left)
					beats[(int)rightFoot] = Note.Trail;
				else
					beats[(int)leftFoot] = Note.Trail;
			}
		}

		public Step NextStep(Options opt, Random rand, int[] notes)
		{
			if (next != null) throw new Exception("Error: Next Step already created!");
			next = new Step(this);

            // end any trails, even if we didn't move the player
            next.EndTrails(notes[(int)Note.Trail]);

			// now, see if we're adding a special type of step
			switch (notes[(int)Note.Step] + notes[(int)Note.Hold] + notes[(int)Note.Roll])
			{
				case 0:
					// we don't need to move the player
					break;

				// we're just adding a random kind of step based on the Options
				case 1:
					Foot lastFoot = GetLastFootMoved();
					// are we in a 180 crossover position?
					if (leftFoot == Pos.Right && rightFoot == Pos.Left)
						//TODO 180s not supported yet
						throw new Exception();
					// always follow a crossover with a drill, unless we're doing a 180
					else if ((lastFoot == Foot.Left && leftFoot == Pos.Right)
							|| (lastFoot == Foot.Right && rightFoot == Pos.Left))
					{
						if (rand.Next() % 100 < opt.cross180Chance)
							next.Move(Flag.Cross180, rand, notes);
						else
							next.Move(Flag.Drill, rand, notes);
					}
					// always follow up a jump with a double tap
					else if (flag == Flag.Jump)
						next.Move(Flag.DoubleTap, rand, notes);
					// any arrow can be a drill (same as arrow before last) or double tap (same as last)
					else if (rand.Next() % 100 < opt.drillChance)
						next.Move(Flag.Drill, rand, notes);
					// make sure we never have more than one double-tap in a row though
					else if (flag != Flag.DoubleTap && rand.Next() % 100 < opt.doubleTapChance)
						next.Move(Flag.DoubleTap, rand, notes);
					// if we're in the right position, we can try for a crossover
					else if (((lastFoot == Foot.Left && (leftFoot == Pos.Down || leftFoot == Pos.Up) && rightFoot != Pos.Left)
							|| (lastFoot == Foot.Right && (rightFoot == Pos.Down || rightFoot == Pos.Up) && leftFoot != Pos.Right))
							&& rand.Next() % 100 < opt.crossoverChance)
						next.Move(Flag.Crossover, rand, notes);
					// if we're in the right position, we can try for a candle
					else if (((lastFoot == Foot.Left && leftFoot == Pos.Left 
								&& (rightFoot == Pos.Down || rightFoot == Pos.Up))
							|| (lastFoot == Foot.Right && rightFoot == Pos.Right 
								&& (leftFoot == Pos.Down || leftFoot == Pos.Up)))
							&& rand.Next() % 100 < opt.candleChance)
						next.Move(Flag.Candle, rand, notes);
					// only if no other option was picked do we want to create a normal arrow
					else
						next.Move(Flag.Normal, rand, notes);
					break;
				// we're adding a Jump step (two arrows at once)
				default:
					// create a random jump step
					next.Move(Flag.Jump, rand, notes);
					break;
			}

			// make sure the step is properly created before sending it off
			next.Validate();
			return next;
		}

		protected virtual void Move(Flag stepFlag, Random rand, int[] notes)
		{
			// make sure the previous step is valid, because it must!
			if (prev == null) throw new Exception();
			prev.Validate();
			// also make sure this step hasn't already been initialized yet
			if (flag != Flag.Invalid && flag != Flag.Trail && flag != Flag.Jump) throw new Exception();
			flag = stepFlag;
		
			// what kind of step are we adding?
			switch (flag)
			{
				// a normal, non-candle, non-drill, simple flowing step
				case Flag.Normal:
					// which foot moved last?
					switch (prev.GetLastFootMoved())
					{
						// last foot to move was the left foot
						case Foot.Left:
							// alternate to the right foot
							footMoved = Foot.Right;
							// is the right foot to be moving from the right arrow?								xxxR
							if (rightFoot == Pos.Right)
							{
								// is the left foot on the left arrow?											L00R
								if (leftFoot == Pos.Left)
									// move the right foot to either the up or down arrow
									rightFoot = rand.Next() % 2 == 0 ? Pos.Down : Pos.Up;
								// if the left foot's on the up arrow, move right to down arrow					00LR
								else if (leftFoot == Pos.Up)
									rightFoot = Pos.Down;
								// if the left foot's on the down arrow, move right to up arrow					0L0R
								else if (leftFoot == Pos.Down)
									rightFoot = Pos.Up;
								else
									// something is terribly wrong
									throw new Exception();
							}
							// is the right foot to be moving from the up or down arrow?						xRRx
							else if (rightFoot == Pos.Down || rightFoot == Pos.Up)
								// the only choice is to move to the right arrow, since this isn't a candle
								rightFoot = Pos.Right;
							// is the right foot on the left arrow (i.e. we just finished a crossover)?			Rxxx
							else
							{
								// to complete the crossover without adding a candle, move across from the left foot
								if (leftFoot == Pos.Down)													 // R0L0
									rightFoot = Pos.Up;
								else if (leftFoot == Pos.Up)												 // RL00
									rightFoot = Pos.Down;
								else
								{
									// we must have just finished a full 180 crossover							R00L
									// TODO - 180s not implemented yet
									throw new Exception();
								}
							}
							break;
						// last foot to move was the right foot
						case Foot.Right:
							// alternate to the left foot
							footMoved = Foot.Left;
							// is the left foot to be moving from the left arrow?								Lxxx
							if (leftFoot == Pos.Left)
							{
								// is the right foot on the right arrow?										L00R
								if (rightFoot == Pos.Right)
									// move the left foot to either the up or down arrow
									leftFoot = rand.Next() % 2 == 0 ? Pos.Down : Pos.Up;
								// if the right foot's on the up arrow, move left to down arrow					L0R0
								else if (rightFoot == Pos.Up)
									leftFoot = Pos.Down;
								// if the right foot's on the down arrow, move left to up arrow					LR00
								else if (rightFoot == Pos.Down)
									leftFoot = Pos.Up;
								else
									// something is terribly wrong
									throw new Exception();
							}
							// is the left foot to be moving from the up or down arrow?							xLLx
							else if (leftFoot == Pos.Down || leftFoot == Pos.Up)
								// the only choice is to move to the left arrow, since this isn't a candle
								leftFoot = Pos.Left;
							// is the left foot on the right arrow (i.e. we just finished a crossover)?			xxxL
							else
							{
								// to complete the crossover without adding a candle, move across from the right foot
								if (rightFoot == Pos.Down)													 // 00RL
									leftFoot = Pos.Up;
								else if (rightFoot == Pos.Up)												 // 0R0L
									leftFoot = Pos.Down;
								else
								{
									// we must have just finished a full 180 crossover							R00L
									// TODO - 180s not implemented yet
									throw new Exception();
								}
							}
							break;
							// last foot to move was ambiguous or nonexistent
						default:
							// always start with the left foot for consistency
							footMoved = Foot.Left;
							leftFoot = Pos.Left;
							break;
					}
					break;
				// a jump step, or two arrows at once
				case Flag.Jump:
					Foot lastFoot = prev.GetLastFootMoved();
					// first determine how many feet should move with this jump (0-2)
					if (rand.Next() % 100 < 10
						// if we're recovering from a crossover position, definitely stick with the zero jump
						|| (lastFoot == Foot.Left && prev.leftFoot == Pos.Right)
						|| (lastFoot == Foot.Right && prev.rightFoot == Pos.Left))
					{
						// neither foot will move... still switch feet for the next step/jump though
						if (lastFoot == Foot.Left)
							footMoved = Foot.Right;
						else
							footMoved = Foot.Left;
					}
					// only move both feet at once for the jump if the last step was a jump too
					else if (prev.flag == Flag.Jump && rand.Next() % 100 < 10
							// don't allow left-right to up-down jumps (ambiguous)
							&& !(leftFoot == Pos.Left && rightFoot == Pos.Right))
					{
						// both feet will move simultaneously across the pad (only one option per setup)
						if (leftFoot == Pos.Left && rightFoot == Pos.Down)								 // LR00
						{
							leftFoot = Pos.Up;
							rightFoot = Pos.Right;
						}
						else if (leftFoot == Pos.Left && rightFoot == Pos.Up)							 // L0R0
						{
							leftFoot = Pos.Down;
							rightFoot = Pos.Right;
						}
						else if (leftFoot == Pos.Up && rightFoot == Pos.Right)							 // 00LR
						{
							leftFoot = Pos.Left;
							rightFoot = Pos.Down;
						}
						else if (leftFoot == Pos.Down && rightFoot == Pos.Right)						 // 0L0R
						{
							leftFoot = Pos.Left;
							rightFoot = Pos.Up;
						}
						else 																	 // 0LR0 or 0RL0
						{
							leftFoot = Pos.Left;
							rightFoot = Pos.Right;
						}
						
						// still switch feet for the next step/jump
						if (prev.GetLastFootMoved() == Foot.Left)
							footMoved = Foot.Right;
						else
							footMoved = Foot.Left;
					}
					else
					{
						// occasionally throw in a candle-jump if we're in the right position
						if (((lastFoot == Foot.Left && prev.leftFoot == Pos.Left
								&& (prev.rightFoot == Pos.Down || prev.rightFoot == Pos.Up))
							|| (lastFoot == Foot.Right && prev.rightFoot == Pos.Right
								&& (prev.leftFoot == Pos.Down || prev.leftFoot == Pos.Up)))
							&& rand.Next() % 100 < 50)
							Move(Flag.Candle, rand, notes);
						else
							Move(Flag.Normal, rand, notes);
						// reset the flag to a jump because we actually did a jump, not a step
						flag = Flag.Jump;
					}
					break;
				// a drill step
				case Flag.Drill:
					// repeat the step before last
					switch (prev.GetLastFootMoved())
					{
							// if the last step was with the left, this one will be with the right
						case Foot.Left:
							footMoved = Foot.Right;
							break;
							// if the last step was with the right, this one will be with the left
						case Foot.Right:
							footMoved = Foot.Left;
							break;
						default:
							// pick a random foot and go with it
							footMoved = Foot.Left;// rand.Next() % 2 == 0 ? Foot.Left : Foot.Right; //TEST
							break;
					}
					break;
				// a candle
				case Flag.Candle:
					// figure out which foot moved last
					switch (prev.GetLastFootMoved())
					{
							// if the last step was with the left, this one will be with the right
						case Foot.Left:
							footMoved = Foot.Right;
							// move the right foot across the pad
							if (rightFoot == Pos.Down)
								rightFoot = Pos.Up;
							else if (rightFoot == Pos.Up)
								rightFoot = Pos.Down;
							else if (rightFoot == Pos.Left) // we must have just finished a crossover
								rightFoot = Pos.Right;
							else
								// this should never happen
								throw new Exception("ERROR: Invalid Candle Starting Position!");
							break;
							// if the last step was with the right, this one will be with the left
						case Foot.Right:
							footMoved = Foot.Left;
							// move the left foot across the pad
							if (leftFoot == Pos.Down)
								leftFoot = Pos.Up;
							else if (leftFoot == Pos.Up)
								leftFoot = Pos.Down;
							else if (leftFoot == Pos.Right) // we must have just finished a crossover
								leftFoot = Pos.Left;
							else
								// this should never happen
								throw new Exception("ERROR: Invalid Candle Starting Position!");
							break;
						default:
							// we cannot do a candle for an ambiguous step (for now at least?)
							throw new Exception("ERROR: Candle cannot follow an ambiguous step!");
					}
					break;
				// a double-tap step
				case Flag.DoubleTap:
					// repeat the last step without moving the foot
					footMoved = prev.GetLastFootMoved();
					// if the left foot's holding a trail, only move the right foot
					if (footHeld == Foot.Left)
						footMoved = Foot.Right;
					// if the right foot's holding a trail, only move the left foot
					else if (footHeld == Foot.Right)
						footMoved = Foot.Left;
					// both feet should not be held at this point
					else if (footHeld == Foot.Both)
						throw new Exception("ERROR: Hand step not handled properly");
					// if the last step was ambiguous, always start with the left foot
					else if (footMoved != Foot.Left && footMoved != Foot.Right)
						footMoved = Foot.Left;
					break;
				// a crossover
				case Flag.Crossover:
					switch (prev.GetLastFootMoved())
					{
						case Foot.Left:
							// move the right foot to the left arrow
							footMoved = Foot.Right;
							rightFoot = Pos.Left;
							break;
						case Foot.Right:
							// move the left foot to the right arrow
							footMoved = Foot.Left;
							leftFoot = Pos.Right;
							break;
						default:
							// this should never happen!
							throw new Exception();
					}
					break;
				// TODO
				case Flag.Cross180:
					// TODO - 180 crossovers not implemented yet
					throw new Exception();
				default:
					// this should never happen
					throw new Exception("ERROR: Unhandled Step Flag");
			}
			
			// add the beats
			if (flag == Flag.Jump)
			{
				Note leftFootType = Note.Step;
				Note rightFootType = Note.Step;
				// what combination of arrow types do we have?
				if (notes[(int)Note.Hold] == 1 && notes[(int)Note.Roll] == 0)
				{
					// there's a hold on one foot... make it the foot moved
					if (footMoved == Foot.Left)
					{
						leftFootType = Note.Hold;
						footHeld = Foot.Left;
					}
					else if (footMoved == Foot.Right)
					{
						rightFootType = Note.Hold;
						footHeld = Foot.Right;
					}
					else
						// this should never happen
						throw new Exception();
				}
				else if (notes[(int)Note.Hold] == 0 && notes[(int)Note.Roll] == 1)
				{
					// there's a roll on one foot... make it the foot moved
					if (footMoved == Foot.Left)
					{
						leftFootType = Note.Roll;
						footHeld = Foot.Left;
					}
					else if (footMoved == Foot.Right)
					{
						rightFootType = Note.Roll;
						footHeld = Foot.Right;
					}
					else
						// this should never happen
						throw new Exception();
				}
				else if (notes[(int)Note.Hold] > 0 && notes[(int)Note.Roll] > 0)
				{
					// there's a hold AND a roll... make the hold the foot moved
					footHeld = Foot.Both;
					if (footMoved == Foot.Left)
					{
						leftFootType = Note.Hold;
						rightFootType = Note.Roll;
					}
					else if (footMoved == Foot.Right)
					{
						leftFootType = Note.Roll;
						rightFootType = Note.Hold;
					}
					else
						// this should never happen
						throw new Exception();
				}
				else if (notes[(int)Note.Hold] > 1)
				{
					// two holds
					footHeld = Foot.Both;
					leftFootType = Note.Hold;
					rightFootType = Note.Hold;
				}
				else if (notes[(int)Note.Roll] > 1)
				{
					// two rolls
					footHeld = Foot.Both;
					leftFootType = Note.Roll;
					rightFootType = Note.Roll;
				}

				// add the beats to the chart
				beats[(int)leftFoot] = leftFootType;
				beats[(int)rightFoot] = rightFootType;
			}
			else if (footMoved == Foot.Left)
			{
				// make sure to add the right type of arrow (and start a hold if needed)
				if (notes[(int)Note.Hold] > 0)
				{
					beats[(int)leftFoot] = Note.Hold;
					if (footHeld == Foot.None)
						footHeld = Foot.Left;
					else
						footHeld = Foot.Both;
				}
				else if (notes[(int)Note.Roll] > 0)
				{
					beats[(int)leftFoot] = Note.Roll;
					if (footHeld == Foot.None)
						footHeld = Foot.Left;
					else
						footHeld = Foot.Both;
				}
				else
					beats[(int)leftFoot] = Note.Step;
			}
			else if (footMoved == Foot.Right)
			{
				// make sure to add the right type of arrow (and start a hold if needed)
				if (notes[(int)Note.Hold] > 0)
				{
					beats[(int)rightFoot] = Note.Hold;
					if (footHeld == Foot.None)
						footHeld = Foot.Right;
					else
						footHeld = Foot.Both;
				}
				else if (notes[(int)Note.Roll] > 0)
				{
					beats[(int)rightFoot] = Note.Roll;
					if (footHeld == Foot.None)
						footHeld = Foot.Right;
					else
						footHeld = Foot.Both;
				}
				else
					beats[(int)rightFoot] = Note.Step;
			}
			else
				// for now this should never happen
				throw new Exception();
			// and this should never happen either (unless I add Footswitches later)
			if (leftFoot == rightFoot)
				throw new Exception();
		}
	}
}
