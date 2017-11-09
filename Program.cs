using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ConsoleApp1
{
    class Student
    {
        public string Name { get; set; }
    }
    class TA
    {
        public string UserName { get; set; }
        public string PWD { get; set; }
        public SshClient Client { get; set; }
        public int Psa { get; set; }
        public List<Student> StudentList { get; set; }
        public TA()
        {
            Login();
            GetPSANum();
        }

        private void Login()
        {
            Console.WriteLine("Your 8a login:");
            UserName = Console.ReadLine();
            Console.WriteLine("Your 8a pwd:");
            PWD = Console.ReadLine();
            Client = new SshClient("ieng6.ucsd.edu", UserName, PWD);
            try
            {
                Client.Connect();
            }
            catch (SshAuthenticationException e)
            {
                Console.WriteLine("Login failed, retry");
                Login();
            }
            StudentList = new List<Student>();
        }
        private void GetPSANum()
        {            
            Console.WriteLine("Which psa you want? enter a number");
            Psa = Console.Read() - 48;
            if (Psa < 0 || Psa > 9)
            {
                GetPSANum();
            }
        }
        public void GetStudents()
        {
            var cmd = Client.RunCommand("ls ~/../");
            var students = cmd.Result.Split('\n');
            foreach (var student in students)
            {
                var taRegex = new Regex("^cs8af[0-9]+$");
                var studentRegex = new Regex("^cs8af[a-z]+$");
                if (studentRegex.Match(student).Success && student != "cs8afzz")
                {
                    StudentList.Add(new Student { Name = student });
                }
            }
        }
        public void CollectHW()
        {
            var client = new ScpClient("ieng6.ucsd.edu", UserName, PWD);
            client.Connect();
            foreach (var student in StudentList)
            {
                string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                    Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
                Directory.CreateDirectory(homePath + "/FA178Apsa" + Psa);
                Console.WriteLine("Downloading from " + student.Name + "...");
                var cmd = Client.RunCommand("cp ../" + student.Name + "/..psa" + Psa + ".tar.gz .");
                if (cmd.Error.Length == 0)
                {
                    Client.RunCommand("mkdir " + student.Name);
                    Client.RunCommand("tar xf ..psa" + Psa+ ".tar.gz -C " + student.Name);
                    //Client.RunCommand("rm " + student.Name + "/*.png");
                    //Client.RunCommand("rm " + student.Name + "/*.jpg");
                    //Client.RunCommand("rm " + student.Name + "/*.jpeg");
                    Directory.CreateDirectory(homePath + "/FA178Apsa" + Psa + "/" + student.Name);
                    var dir = new DirectoryInfo(homePath + "/FA178Apsa" + Psa + "/" + student.Name);
                    client.Download(student.Name, dir);
                    Client.RunCommand("rm -r " + student.Name);
                    Client.RunCommand("rm *.tar.gz");
                } else
                {
                    Console.WriteLine("Failed");
                }
            }
        }
    }
    class Program
    {

        static void Main(string[] args)
        {
            var ta = new TA();
            ta.GetStudents();
            ta.CollectHW();
        }

    }
}
