using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Shifting
{
    class Program
    {
        class ObjectData
        {
            public string Desc;
            public string Hash;
            public int X;
            public int Y;
            public int Rot;
            public int Flip;
            public int Var;

            public bool Valid = false;

            public ObjectData(string raw)
            {
                Desc = raw.Substring(0, raw.IndexOf('=')).Trim(' ');

                raw = raw.Substring(raw.IndexOf('=') + 1);

                Valid = false;

                Valid |= AttractValue(raw, @"(?<=Hash=)\d+", out Hash);
                Valid |= AttractValue(raw, @"(?<=X=)\d+", out X);
                Valid |= AttractValue(raw, @"(?<=Y=)\d+", out Y);
                Valid |= AttractValue(raw, @"(?<=Rot=)\d+", out Rot);
                Valid |= AttractValue(raw, @"(?<=Flip=)\d+", out Flip);
                Valid |= AttractValue(raw, @"(?<=Var=)\d+", out Var);
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
                return string.Format("{0} = {{ Hash={1}, X={2}, Y={3}, Rot={4}, Flip={5}, Var={6} }}"
                                      , Desc, Hash, X, Y, Rot, Flip, Var);
            }
        }

        static string Help = "path (-+)xShift (-+)yShift";

        static void Main(string[] args)
        {
            //args = new string[] { "raw.txt", "+10", "-10" };

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

            var lines = System.IO.File.ReadAllLines(path);
            List<ObjectData> objectList = new List<ObjectData>();
            for (int i = 0; i < lines.Length; i++)
            {
                var r = lines[i];
                var o = new ObjectData(r);
                if (o.Valid == false)
                    Console.WriteLine(string.Format("Line: {0}, {1}", i, o.Desc));

                o.Shift(shifts[0], shifts[1]);
                objectList.Add(o);
            }

            var sb = new StringBuilder();
            foreach (var o in objectList)
                sb.Append(o.ToString()).AppendLine();

            string newPath = path + "_shifted";
            System.IO.File.WriteAllText(newPath, sb.ToString());
            Console.WriteLine("gen " + newPath);
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
