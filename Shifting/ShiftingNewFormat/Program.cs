using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ShiftingNewFormat
{
    class Program
    {
        class Decoration
        {
            public string Name;
            public string Hash;
            public int X;
            public int Y;
            public int Rot;
            public int FV;

            public bool Valid = false;

            public Decoration(string name)
            {
                this.Name = name;
            }

            public void Parse(params string[] lines)
            {
                Valid |= AttractValue(lines[0], "(?<=\"hash\":)[ ]?\\d+", out Hash);
                Valid |= AttractValue(lines[1], "(?<=\"x\":)[ ]?\\d+", out X);
                Valid |= AttractValue(lines[2], "(?<=\"y\":)[ ]?\\d+", out Y);
                Valid |= AttractValue(lines[3], "(?<=\"r\":)[ ]?\\d+", out Rot);
                Valid |= AttractValue(lines[4], "(?<=\"fv\":)[ ]?\\d+", out FV);
            }

            public void Shift(int x, int y)
            {
                this.X += x;
                this.Y += y;
            }

            private bool AttractValue(string source, string pattern, out int value)
            {
                var rg = new Regex(pattern);
                var m = rg.Match(source);

                value = 0;

                if (m.Success)
                    int.TryParse(m.Value, out value);

                return m.Success;
            }

            private bool AttractValue(string source, string pattern, out string value)
            {
                var rg = new Regex(pattern);
                var m = rg.Match(source);

                value = string.Empty;

                if (m.Success)
                    value = m.Value;

                return m.Success;
            }

            public override string ToString()
            {
                return string.Format("\"{0}\":{{{1},{2},{3},{4},{5}}}",
                                        Name,
                                        OutputValue("hash", Hash),
                                        OutputValue("x", X),
                                        OutputValue("y", Y),
                                        OutputValue("r", Rot),
                                        OutputValue("fv", FV));
            }
            private string OutputValue(string title, int value)
            {
                return string.Format("\"{0}\":{1}", title, value);
            }

            private string OutputValue(string title, string value)
            {
                return string.Format("\"{0}\":{1}", title, value);
            }
        }

        static string Help = "path (-+)xShift (-+)yShift";

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                ShowError("invalid args");
                return;
            }

            string path = args[0];

            int[] shifts = new int[2];

            int v;
            for (int i = 1; i < 3; i++)
            {
                if (ParseShit(args[i], out v) == false)
                {
                    ShowError("invalid args");
                    return;
                }

                shifts[i - 1] = v;
            }

            if (System.IO.File.Exists(path) == false)
            {
                ShowError("invalid file");
                return;
            }

            var lines = File.ReadAllLines(path);
            Queue<string> q = new Queue<string>(lines);

            string[] tmp = new string[5];

            List<Decoration> decorationList = new List<Decoration>();
            while (q.Count > 0)
            {
                string t = q.Dequeue();

                if (t.StartsWith("\""))
                {
                    for (int i = 0; i < tmp.Length; i++)
                        tmp[i] = string.Empty;

                    string name = t.Substring(1, t.LastIndexOf("\"") - 1);
                    Decoration decoration = new Decoration(name);

                    for (int i = 0; i < 5; i++)
                        tmp[i] = q.Dequeue();

                    decoration.Parse(tmp);

                    if (decoration.Valid)
                    {
                        decorationList.Add(decoration);
                        decoration.Shift(shifts[0], shifts[1]);
                    }
                    else
                    {
                        Console.WriteLine("failed {0}", name);
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < decorationList.Count; i++)
            {
                sb.Append(decorationList[i].ToString());
                if (i + 1 != decorationList.Count)
                    sb.Append(',');
                sb.AppendLine();
            }
            File.WriteAllText(path + "_new", sb.ToString());
        }
        static bool ParseShit(string target, out int shift)
        {
            shift = 0;
            if (target.Length < 2)
                return false;

            int sign = 1;
            switch (target[0])
            {
                case '+':
                    sign = 1;
                    break;
                case '-':
                    sign = -1;
                    break;
            }

            target = target.Substring(1);
            if (int.TryParse(target, out shift) == false)
                return false;

            shift *= sign;
            return true;
        }

        static void ShowError(string s)
        {
            Console.WriteLine(string.Format("{0}\nargs: {1}", s, Help));
        }
    }
}
