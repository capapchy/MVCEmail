using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMailService
{
    public class TxtFileControl
    {
        public TxtFileControl()
        {

        }
        public void deleteFile(String filName)
        {
            try
            {
                if (File.Exists(filName))
                {
                    FileInfo fi = new FileInfo(filName);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(filName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void newFile(String filName)
        {
            try
            {
                if (!File.Exists(filName))
                {
                    FileStream fs1 = new FileStream(filName, FileMode.Create, FileAccess.Write);//创建写入文件 
                    //StreamWriter sw = new StreamWriter(fs1);
                    //sw.WriteLine("B903646,OK");//开始写入值
                    //sw.Close();
                    fs1.Close();
                }
                else
                {
                    FileStream fs = new FileStream(filName, FileMode.Open, FileAccess.Write);
                    //StreamWriter sr = new StreamWriter(fs);
                    //sr.WriteLine("B506027,NG,E101");//开始写入值
                    //sr.Close();
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public String readLine(String filName, String sLine)
        {
            StreamReader reader1;//String str = ""; int lg = 1;
            String str = "N/A";//, data = "", log = "0";
            // String fileName = "I";
            String status = "NG";
            //fileName = DateTime.Now.GetDateTimeFormats('D')[0] + ".TXT"; // += string.Format("{0:yMdd}", DateTime.Now).Substring(1) + ".TXT";
            try
            {
                reader1 = new StreamReader(filName);
                str = reader1.ReadLine();
                while ((str != null) && (str.Length > 1))
                {
                    String[] dataLine = str.Split(',');
                    if (dataLine[0].Equals(sLine))
                    {
                        status = "OK," + str;
                        break;
                    }
                    str = reader1.ReadLine();
                }
                reader1.Close();
                return status;
            }
            catch (Exception e)
            {
                return "NG," + e.Message;//Console.WriteLine(e.Message); ;
            }
        }

        public String addLine(String filName, String sLine)
        {
            //fileName = DateTime.Now.GetDateTimeFormats('D')[0] + ".TXT";
            ArrayList al = new ArrayList();
            String str = "";
            try
            {
                StreamReader reader1 = new StreamReader(filName);
                //int cx = 0;
                while (str != null)
                {
                    //Boolean bl = false;
                    str = reader1.ReadLine();
                    if (str != null)
                    {
                        //if (!str.Split('\t')[0].Equals("Old"))
                        // str = "Old\t" + str;
                        al.Add(str);
                    }
                }
                al.Add(sLine);
                reader1.Close();

                StreamWriter sw = new StreamWriter(filName);
                foreach (string s in al)
                {
                    sw.WriteLine(s);
                }
                sw.Close();
                al.Clear();
                return "OK";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return "NG" + e.Message;
            }
        }
        public String deleteLine(String filName, String sLine)
        {
            ArrayList al = new ArrayList();
            String str = "";
            try
            {
                StreamReader reader1 = new StreamReader(filName);
                //int cx = 0;
                while (str != null)
                {
                    //Boolean bl = false;
                    str = reader1.ReadLine();
                    if (str != null)
                    {
                        if (!str.Split(',')[0].Equals(sLine))
                            al.Add(str);
                    }
                }
                reader1.Close();

                StreamWriter sw = new StreamWriter(filName);
                foreach (string s in al)
                {
                    sw.WriteLine(s);
                }
                sw.Close();
                al.Clear();
                return "OK";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return "NG," + e.Message;
            }
        }

        public String modifyLine(String filName, String sLine, String sLine2)
        {

            ArrayList al = new ArrayList();
            String str = "";
            try
            {
                StreamReader reader1 = new StreamReader(filName);
                //int cx = 0;
                while (str != null)
                {
                    //Boolean bl = false;
                    str = reader1.ReadLine();
                    if (str != null)
                    {
                        if (!str.Split(',')[0].Equals(sLine))
                            al.Add(str);
                        else
                            al.Add(sLine2);
                    }
                }
                reader1.Close();

                StreamWriter sw = new StreamWriter(filName);
                foreach (string s in al)
                {
                    sw.WriteLine(s);
                }
                sw.Close();
                al.Clear();
                return "OK";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return "NG," + e.Message;
            }
        }
    }
}
