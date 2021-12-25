using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IpMaskingLogic;


namespace IPMASK
{
    public class Program
    {
        // method to read log files
        public static IEnumerable<string> GetLogFile(string path)
        {
            // initiataing a new instance of the FileInfo class to find file size
            var fileInfo = new FileInfo(path);
            var fileSize = fileInfo.Length;
            var fiveMB = 5 * 1024 * 1024;

            // throws exception if file size is bigger than 5MB
            if ((fileSize / (1024 * 1024) > fiveMB))
            {
                throw new Exception("You can upload a file up to 5 MB");
            }

            // getting all the lines of the file and returning them
            var enumLines = File.ReadLines(path, Encoding.UTF8);
            return enumLines;

        }

        public static IEnumerable<string> WriteLogFile(IEnumerable<string> fileLines, Regex ipRegex, List<string> outputLines, string path)
        {
            //creating new list
            List<string> fileLinesList = new List<string>();

            //for each row in the input file 
            foreach (string line in fileLines)
            {
                MatchCollection result = ipRegex.Matches(line);
                if ((result.Count > 0) && result[0].Success)
                {
                    //if ip address was found adding the row with masked ip address to the new list
                    //removing the added element from "outputLines" list to get the next element
                    fileLinesList.Add(outputLines[0]);
                    outputLines.RemoveAt(0);
                }
                else
                {
                    //if ip address wasn't found in the row, adding it as is to the new list.
                    fileLinesList.Add(line);
                }
            }

            //writing the new list with the masked ip addresses to a new file
            File.WriteAllLines(path, fileLinesList);

            // getting all the lines of the file and returning them
            var newFile = File.ReadLines(path, Encoding.UTF8);
            return newFile;

        }


        public static void Main(string[] args)
        {
            var path = "C:/Users/revit/Projects/IpMasking/IPMASK/TextFile.log";

            // sending the path file to a method to read and return the file rows.
            var fileLines = GetLogFile(path);

            //regular expression for C class network ip addresses
            Regex ipRegex = new Regex(@"(?:19[2-9]|2[0-1]\d|22[0-3])(?:\.(?:\d{1,2}|1\d\d|2[0-4]\d|25[0-5])){3}");

            //new list for the ip addresses
            List<string> outputIp = new List<string>();

            //new list for the file rows.
            List<string> outputLines = new List<string>();

            //iterating over the file lines 
            foreach (string line in fileLines)
            {
                //checking if the row contains an ip address matching the regex expression
                MatchCollection result = ipRegex.Matches(line);

                // if such ip address was found  add the ip address to "outputIp" list and the row to "outputLines" list
                if ((result.Count > 0) && result[0].Success)
                {
                    outputIp.Add(result[0].Value);
                    outputLines.Add(line);
                }
            }

            // creating masked ip addresses
            // the method written in separate dll 
            if (outputIp.Count() > 0)
            {
                IpMasking ipMasking = new IpMasking();
                ipMasking.createIpMasking(ref outputIp);
            }

            //iterating over file rows and replacing original ip addresses with the masked ones.
            if (outputLines.Count > 0)
            {
                for (int i = 0; i < outputLines.Count(); i++)
                {
                    MatchCollection res = ipRegex.Matches(outputLines[i]);
                    if ((res.Count > 0) && res[0].Success)
                    {
                        outputLines[i] = outputLines[i].Replace(res[0].Value, outputIp[i]);
                    }
                }
            }

            // new file path
            var path2 = "C:/Users/revit/Projects/IpMasking/IPMASK/NewFile.log";

            // the original file with masked IP addresses replacing the original IP addresses.
            var newFile = WriteLogFile(fileLines, ipRegex, outputLines, path2);
        }
    }
}
