using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TLSharp;
using TLSharp.Core;

namespace TLFunctionalityLib
{
    public class SessionStore : ISessionStore
    {
        public string SeesionPath { get; set; }
        public string Name { get; set; }


        public Session Load(string sessionUserId)
        {

            string currentDir = Directory.GetCurrentDirectory();
            Console.WriteLine("current dir" + Directory.GetCurrentDirectory());
            Console.WriteLine("name : " + Name);

            string fileName = Directory.EnumerateFiles(currentDir).Where(x => x.Contains(Name) && x.Contains("session")).First();

            if (fileName == null)
                // error
                return null;


            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "temp_session_" + Name + ".dat");
            string sessId = filePath.Substring(filePath.LastIndexOf("_") + 1, filePath.IndexOf(".dat"));

            Session sess = Session.FromBytes(Encoding.ASCII.GetBytes(File.ReadAllText(filePath)), this, sessId);
            
            return sess;
        }

        public void Save(Session session)
        {
            string filePath = Path.Combine(SeesionPath, $"temp_session_" + Name + ".dat");
            string sessID = session.SessionUserId; 


            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(Encoding.ASCII.GetChars(session.ToBytes()));
            }
        }
    }
}
