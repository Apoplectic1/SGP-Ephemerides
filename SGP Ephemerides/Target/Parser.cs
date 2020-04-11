using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SGP_Ephemerides.Target
{
    public class Parser
    {
        private List<SGP_Ephemerides.Target.Target> ObjectList { get; set; }
        private JObject mSgfFile;
        private SGP_Ephemerides.Target.Target mObject;
        
        public Parser()
        {
            mSgfFile = new JObject();
            ObjectList = new List<Target>();
        }
        ~Parser()
        {
            mSgfFile = null;
            mObject = null;
            ObjectList = null;
        }

        private bool OpenSgfFile(string fileName)
        {
            try
            {
                mSgfFile = JObject.Parse(File.ReadAllText(fileName));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string ObjectName
        {
            get
            {
                return (string)mSgfFile["arEventGroups"][0]["sName"];
            }
        }
        private double RightAscension
        {
            get
            {
                string raString;
                double rightAscension;
                bool status;

                raString = (string)mSgfFile["arEventGroups"][0]["siReference"]["nRightAscension"];

                status = Double.TryParse(raString, out rightAscension);

                if (status)
                {
                    return Math.Round(rightAscension, 6);
                }
                else
                {
                    return Double.NaN;
                }
            }
        }
        private double Declination
        {
            get
            {
                string decString;
                double declination;
                bool status;

                decString = (string)mSgfFile["arEventGroups"][0]["siReference"]["nDeclination"];

                status = Double.TryParse(decString, out declination);

                if (status)
                {
                    return Math.Round(declination, 6);
                }
                else
                {
                    return Double.NaN;
                }
            }
        }

        public List<SGP_Ephemerides.Target.Target> BuildObjectList(string sequenceFolder, IProgress<Tuple<int, int>> progress)
        {
            List<string> recursedTargetList;

            ObjectList.Clear();

            if (String.IsNullOrWhiteSpace(sequenceFolder) == false)
            {
                recursedTargetList = System.IO.Directory.GetFiles(sequenceFolder, "*.sgf", SearchOption.AllDirectories).ToList<string>();

                progress.Report(Tuple.Create(0, recursedTargetList.Count));

                foreach (string target in recursedTargetList)
                {
                    // First eliminates objects that won't open or are inappropriate
                    if (OpenSgfFile(target) == false)
                    {
                        continue;
                    }

                    if (ObjectName.Contains("flat", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (ObjectName.Contains("calibrat", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    mObject = new SGP_Ephemerides.Target.Target();

                    mObject.Name = ObjectName;
                    mObject.Directory = target;
                    mObject.RightAscension = RightAscension;
                    mObject.Declination = Declination;

                    ObjectList.Add(mObject);
                    progress.Report(Tuple.Create(ObjectList.Count, recursedTargetList.Count));
                }
            }

            return ObjectList;
        }
    }

    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}
