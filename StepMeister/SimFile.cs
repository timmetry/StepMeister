using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StepFu
{
	public class SimFile
	{
		private StreamReader readFile;
		private StreamWriter backupFile;
		private StreamWriter writeFile;
		private string readDir;
		private string backupDir;
		private string writeDir;
		private string readName;
		private string backupName;
		private string writeName;
		private string readExt = ".sm";
		private string backupExt = ".smb";
		private string writeExt = ".sm";

		public SimFile(string dir, string name)
		{
			SetDir(dir);
			SetName(name);
		}

		public void SetDir(string dir)
		{
			readDir = dir + "\\backup\\";
            backupDir = dir + "\\backup\\";
			writeDir = dir;
		}

		public void SetName(string name)
		{
			readName = name;
			backupName = name;
			writeName = name;
		}

		public void RunStepFu(Options options, Random rand)
		{
			readFile = new StreamReader(readDir + readName + readExt);
			backupFile = new StreamWriter(backupDir + backupName + backupExt);
			writeFile = new StreamWriter(writeDir + writeName + writeExt);

			string readLine;
			Step lastStep = new Step();

			int count = 0;
			while ((readLine = readFile.ReadLine()) != null)
			{
				++count;
				// make sure the backup file stays identical to the original
				backupFile.WriteLine(readLine);
				// by default, will write the same line written
				string writeLine = readLine;

				// are we starting a new chart?
				if (readLine.ToUpper().Contains("#NOTES"))
					// start a new chart
					lastStep = new Step();

				// is this line the right format for a note?
				if (readLine.Length == 4)
				{
					// start reading the note
					int[] notes = new int[6];
					for (int i = 0; i < readLine.Length; ++i)
					{
						switch (readLine[i])
						{
							case '1':
								// a normal arrow... add a step
								++notes[(int)Note.Step];
								break;
							case '2':
								// the start of a freeze arrow... add a hold
								++notes[(int)Note.Hold];
								break;
							case '3':
								// the end of a hold or roll... add a trail
								++notes[(int)Note.Trail];
								break;
							case '4':
								// the start of a roll arrow... add a roll
								++notes[(int)Note.Roll];
								break;
							case 'M':
								// a mine
								++notes[(int)Note.Mine];
								break;
							case '0':
								// empty space... add nothing
								++notes[(int)Note.None];
								break;
							case 'L':
								// Lifts not supported... add nothing
								++notes[(int)Note.None];
								break;
							case 'F':
								// Fakes not supported... add nothing
								++notes[(int)Note.None];
								break;
							default:
								// this is an invalid character... ABORT!
								notes[(int)Note.None] = 10;
								i = readLine.Length;
								break;
						}
					}
					// is there a step to add?
					if (notes[(int)Note.Step] + notes[(int)Note.Hold] + notes[(int)Note.Trail] + notes[(int)Note.Roll] > 0)
					{
						// prepare to add the step
						lastStep = lastStep.NextStep(options, rand, notes);
						writeLine = lastStep.ToString();
					}
				}

				// write the line
				writeFile.WriteLine(writeLine);
			}
			readFile.Close();
			writeFile.Close();
			backupFile.Close();
		}
	}
}
