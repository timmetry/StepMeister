using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StepFu
{
	class Program
	{
		static void Main(string[] args)
		{
			Random rand = new Random();
            SimFile file = new SimFile("C:\\Users\\Tim\\Games\\Stepmania 3.95\\Songs\\_Caveman\\ResurrectionResurrected\\", "ResurrectionResurrected");
			Options opt = new Options();
			opt.SetNormalMode();
			file.RunStepFu(opt, rand);
		}
	}
}
