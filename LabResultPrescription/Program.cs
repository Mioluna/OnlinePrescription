using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Net.Cache;
using System.Security;

namespace LabResultPrescription
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            TcpListener listener = new TcpListener(IPAddress.Any, 8888);
            listener.Start();


            TcpClient client = listener.AcceptTcpClient();
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StringBuilder messageBuilder = new StringBuilder();
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                messageBuilder.AppendLine(line + '\r');
            }
            string hl7Message = messageBuilder.ToString();
            Console.WriteLine("Received HL7 Message:");
            Console.WriteLine(hl7Message);
            

            List<string> Supplements = new List<string>();
            List<string> Critical = new List<string>();

            int Age = 0;
            string genderAtBirth = null;
            bool advisedToSeeDoc = false;

            ParseMessage(hl7Message, Supplements, Critical, ref Age, ref genderAtBirth);

            if(Critical.Count != 0)
            { advisedToSeeDoc = true; }

            if (advisedToSeeDoc)
            {
                Console.WriteLine("Supplements cannot be prescribed. It is advised for you to see a healthcare professional due to critically abnormal results in:");
                foreach (string name in Critical)
                {
                    Console.WriteLine(name + "\n");
                }
            }
            else
            {
                Console.WriteLine("Supplements are prescribed for the following:");
                foreach (string name in Supplements)
                {
                    Console.WriteLine(name + "\n");

                }
            }

            Console.ReadLine();

            reader.Close();
            stream.Close();
            client.Close();

        }

        static void ParseMessage(string message, List<string> Supp, List<string> Critical, ref int age, ref string genderAtBirth)
        {
            string[] segments = message.Split(new[] { '\r', '\n' });
           
            foreach (string segment in segments)
            {
                if (segment.StartsWith("PID"))
                {

                    string[] fields = segment.Split('|');

                    string birthDate = fields[7];
                    DateTime dateofB = DateTime.ParseExact(birthDate, "yyyyMMdd", null);

                    age = DateTime.Today.Year - dateofB.Year;
                    genderAtBirth = fields[8];
                }

                if (segment.StartsWith("OBX"))
                {
                    string[] fields = segment.Split('|');

                    string observationID = fields[3];
                    string flag = fields[8];

                    string[] subfields = fields[3].Split('^');

                    if (flag == "L")
                    {
                        Supp.Add(subfields[1]);
                    }
                    else if (flag == "H" || flag == "LL" || flag == "HH" || flag == "AA" || flag == "<" || flag == ">")
                    {
                        Critical.Add(subfields[1]);
                    }
                  

                }

            }

        }
    }
}
