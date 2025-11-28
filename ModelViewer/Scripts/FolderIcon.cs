using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toys;

namespace ModelViewer
{
    internal class FolderIcon
    {
        string workPath;
        const string fileName = "desktop.ini";
        const string iniBase = "[.ShellClassInfo]\nIconResource=pr236.ico,0\n[ViewState]\nMode=\nVid=\nFolderType=Generic";
        string file;
        bool newFile = false;
        Dictionary<string, Dictionary<string, string>> values;
        public FolderIcon(string folder)
        {
            if (!Directory.Exists(folder))
                throw new Exception("Not a folder");
            workPath = folder;

            values = new Dictionary<string, Dictionary<string, string>>();

            file = Path.Combine(workPath, fileName);

            if (File.Exists(file))
            {
                //ShowAtribs(file);
                ProcessIni(File.ReadAllLines(file));
            }
            else
            {
                ProcessIni(iniBase.Split('\n'));
                newFile = true;
            }
        }

        public string Icon
        {
            get
            {
                var res = values["[.ShellClassInfo]"]["IconResource"];
                var res2 = res.Split(',');
                return res2[0];
            }

            set
            {
                var res = values["[.ShellClassInfo]"]["IconResource"];
                var res2 = res.Split(',');
                res2[0] = value;

                res = string.Join(",", res2);
                values["[.ShellClassInfo]"]["IconResource"] = res;
            }
        }

        void ProcessIni(string[] lines)
        {
            var pattert = @"([^\s=]+)";
            Dictionary<string, string> context = null;
            foreach (var line in lines)
            {
                if (line.StartsWith('['))
                {
                    context = new Dictionary<string, string>();
                    values.Add(line, context);
                }
                else if (context != null)
                {
                    var attribs = Regex.Matches(line, pattert);
                    if (attribs.Count >= 2)
                        context.Add(attribs[0].Value, attribs[1].Value);
                    if (attribs.Count == 1)
                        context.Add(attribs[0].Value, "");
                }
            }
        }

        public void SaveIni()
        {
            //remove attributes to write file
            var dir = Path.GetDirectoryName(file);
            Console.WriteLine("dir atribs");
            ShowAtribs(dir);
            if (newFile)
            {
                //mark dir to display icon
                //var dir = Path.GetDirectoryName(file);
                var dirAtrbst = File.GetAttributes(dir);
                File.SetAttributes(dir, dirAtrbst | FileAttributes.ReadOnly);
            }
            else
            {
                Console.WriteLine("file  atribs");
                ShowAtribs(file);

                var fAttribs = File.GetAttributes(file);
                fAttribs &= ~FileAttributes.System & ~FileAttributes.Hidden;
                File.SetAttributes(file, fAttribs);

            }
            
            
            //Shift-JIS encoding for jappanece characters 
            //Since explorer doesnt work with UTF characters
            var encoding = CodePagesEncodingProvider.Instance.GetEncoding(932);
            using (var sw = new StreamWriter(file, false, encoding))
            {
                foreach (var group in values)
                {
                    sw.WriteLine(group.Key);
                    foreach (var atrib in group.Value)
                    {
                        sw.WriteLine($"{atrib.Key}={atrib.Value}");
                    }
                }
            }
            
            //set system atribute & hidden attributes
            var fileAttribs = File.GetAttributes(file);
            fileAttribs |= FileAttributes.System | FileAttributes.Hidden;
            fileAttribs &= ~FileAttributes.NotContentIndexed;
            File.SetAttributes(file, fileAttribs);

            newFile = false;
        }

        void ShowAtribs(string file)
        {
            var fileAttribs = File.GetAttributes(file);
            foreach (var attrib in Enum.GetValues(typeof(FileAttributes)))
                if ((fileAttribs & (FileAttributes)attrib) > 0)
                    Console.WriteLine((FileAttributes)attrib);
        }

        void ShowAtribs(FileAttributes fileAttribs)
        {
            foreach (var attrib in Enum.GetValues(typeof(FileAttributes)))
                if ((fileAttribs & (FileAttributes)attrib) > 0)
                    Console.WriteLine((FileAttributes)attrib);
        }
    }
}
