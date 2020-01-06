using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace ImageTool
{

	class Program
	{
        private const string CMD_DIFF = "-diff";
        private const string CMD_FAST_DIFF = "-fdiff";
        private const string CMD_FAST2_DIFF = "-f2diff";
        private const string CMD_COMBINE = "-comb";
        private const string CMD_COMBINE_IFDIFF = "-combdiff";
        private const string CMD_VIEW = "-view";

		private enum EOperation { DIFF, COMBINE, FAST_DIFF, FAST2_DIFF, COMBINE_IFDIFF, VIEW }

		private static EOperation _oper;
		private static string[] _filesIn1;
		private static string[] _filesIn2;
		private static string[] _filesOut;

		static void Main(string[] args)
		{
            int total = 0;
			if (!ParseArgs(args))
			{
				PrintUsage();
				return;
			}

			if (_oper == EOperation.DIFF)
			{
				for (int i = 0; i < _filesIn1.Length; i++)
				{
					double diffRes = ImgUtil.Compare(_filesIn1[i], _filesIn2[i]);
                    if (diffRes == 0) continue;
					Console.WriteLine(Path.GetFileName(_filesIn1[i]) + " vs " + Path.GetFileName(_filesIn2[i]) + ": " + diffRes.ToString("0.000"));
                    total++;
				}
                Console.WriteLine("Total: " + total + " / " + _filesIn1.Length);
			}

			if (_oper == EOperation.FAST_DIFF)
			{
				for (int i = 0; i < _filesIn1.Length; i++)
                    if (!ImgUtil.FastCompare(_filesIn1[i], _filesIn2[i]))
                    {
                        Console.WriteLine(Path.GetFileName(_filesIn1[i]) + " vs " + Path.GetFileName(_filesIn2[i]));
                        total++;
                    }
                Console.WriteLine("Total: " + total + " / " + _filesIn1.Length);
			}

			if (_oper == EOperation.FAST2_DIFF)
			{
				for (int i = 0; i < _filesIn1.Length; i++)
					if (!ImgUtil.FastCompare2(_filesIn1[i], _filesIn2[i]))
					{
						Console.WriteLine(Path.GetFileName(_filesIn1[i]) + " vs " + Path.GetFileName(_filesIn2[i]));
						total++;
					}
				Console.WriteLine("Total: " + total + " / " + _filesIn1.Length);
			}

            if (_oper == EOperation.COMBINE)
            {
                for (int i = 0; i < _filesIn1.Length; i++)
                    ImgUtil.Combine(_filesIn1[i], _filesIn2[i], _filesOut[i]);
            }

            if (_oper == EOperation.COMBINE_IFDIFF)
            {
                for (int i = 0; i < _filesIn1.Length; i++)
                    if (!ImgUtil.FastCompare2(_filesIn1[i], _filesIn2[i]))
                    {
                        Console.WriteLine(Path.GetFileName(_filesIn1[i]) + " vs " + Path.GetFileName(_filesIn2[i]));
                        ImgUtil.Combine(_filesIn1[i], _filesIn2[i], _filesOut[i]);
                        total++;
                    }
                Console.WriteLine("Total: " + total + " / " + _filesIn1.Length);
            }

			if (_oper == EOperation.VIEW)
			{
				ImgForm img = new ImgForm();
				img.SetFile(_filesIn1[0]);
				img.ShowDialog();
				//Application.EnableVisualStyles();
				//Application.SetCompatibleTextRenderingDefault(false);
				//Application.Run(new ImgForm());
			}
		}

		private static void PrintUsage()
		{
			Console.WriteLine("usage: imagetool <command> <src1> <src2> [<dst>] ");
			Console.WriteLine(CMD_DIFF + " <src1> <src2> \t\t\t difference between 2 files / directories");
            Console.WriteLine(CMD_FAST_DIFF + " <src1> <src2> \t\t\t fast difference between 2 files / directories");
            Console.WriteLine(CMD_FAST2_DIFF + " <src1> <src2> \t\t\t fast difference between 2 files / directories");
            Console.WriteLine(CMD_COMBINE + " <src1> <src2> <dst> \t\t combine 2 files / directories");
            Console.WriteLine(CMD_COMBINE_IFDIFF + " <src1> <src2> <dst> \t\t combine files if they differ");
            Console.WriteLine(CMD_VIEW + " <filepath> \t\t\t view image");
		}

		private static bool ParseArgs(string[] args)
		{
			if (args.Length < 2) return false;

			if (args[0] == CMD_DIFF || args[0] == CMD_FAST_DIFF || args[0] == CMD_FAST2_DIFF)
			{
				if (args.Length != 3) return false;
				if (args[0] == CMD_DIFF) _oper = EOperation.DIFF;
				else if (args[0] == CMD_FAST_DIFF) _oper = EOperation.FAST_DIFF;
				else _oper = EOperation.FAST2_DIFF;
				
				_filesIn1 = GetFilesWithPattern(args[1]);
				_filesIn2 = GetFilesWithPattern(args[2]);
				if (_filesIn1.Length != _filesIn2.Length)
				{
					Console.Error.WriteLine("files number differs");
					return false;
				}
				return true;
			}

            if (args[0] == CMD_COMBINE || args[0] == CMD_COMBINE_IFDIFF) 
			{
				if (args.Length != 4) return false;
                if (args[0] == CMD_COMBINE) _oper = EOperation.COMBINE;
                else _oper = EOperation.COMBINE_IFDIFF;

				_filesIn1 = GetFilesWithPattern(args[1]);
				_filesIn2 = GetFilesWithPattern(args[2]);
				
				if (_filesIn1.Length != _filesIn2.Length)
				{
					Console.Error.WriteLine("files number differs");
					return false;
				}

                if (args[1].Contains('*'))
                {
                    _filesOut = _filesIn1.Select(item => Path.Combine(args[3], Path.GetFileName(item))).ToArray();
                }
                else
                {
                    _filesOut = GetFilesWithPattern(args[3]);
                }

				return true;
			}

            if (args[0] == CMD_VIEW)
			{
				if (args.Length != 2) return false;
				_oper = EOperation.VIEW;
				_filesIn1 = GetFilesWithPattern(args[1]);
				if (_filesIn1.Length != 1)
				{
					Console.Error.WriteLine("Wrong file number");
					return false;
				}
				return true;
			}
			
			return false;
		}

		private static string[] GetFilesWithPattern(string dirOrFile)
		{
			if (dirOrFile.Contains("*."))
			{
				string[] fields = dirOrFile.Split(new string[] {"*."}, StringSplitOptions.None);
				string[] res = Directory.GetFiles(fields[0], "*." + fields[1]);
				return res;
			}

            try
            {
                FileAttributes attr = File.GetAttributes(dirOrFile);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    string[] res = Directory.GetFiles(dirOrFile);
                    return res;
                }
            }
            catch
            {
                // in this case it's assumed to be a file...
            }

			return new string[] { dirOrFile };
		}
	}
}
