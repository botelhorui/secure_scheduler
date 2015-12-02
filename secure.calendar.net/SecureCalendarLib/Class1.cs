using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureCalendarLib
{
    [Serializable]
    public class SecureCalendar
    {
        public string name = "someCalendar";
        public List<string> permissions = new List<string>();
        public List<string> events = new List<string>();
    }

    public class Util
    {
        public static string ObjectToXml(object output)
        {
            string objectAsXmlString;

            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(output.GetType());
            using (System.IO.StringWriter sw = new System.IO.StringWriter())
            {
                try
                {
                    xs.Serialize(sw, output);
                    objectAsXmlString = sw.ToString();
                }
                catch (Exception ex)
                {
                    objectAsXmlString = ex.ToString();
                }
            }

            return objectAsXmlString;
        }
    }
}
