using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DileepJavaScriptTool
{
    internal class Program
    {
        public static int currentSection = 1;
        public static bool SectionStartIndexSet=false;
        static void Main(string[] args)
        {           
            
            string filePath = "C:\\DileepJavaScriptTool\\InputFiles\\SampleInput.txt";

            string fileContents = File.ReadAllText(filePath);
           

            string[] chaptersInfo = fileContents.Split(new string[] { "CHAPTER" }, StringSplitOptions.None);//Split the file into chapters

            Document doc = new Document();

            SetDocumentDetails(chaptersInfo[0], doc);   

            if(chaptersInfo.Length>1)
            {
                for(int i = 1; i < chaptersInfo.Length; i++)
                {
                    SectionStartIndexSet = false;
                    getChapterDetails(doc, chaptersInfo[i].Split(new string[] { "\n" }, StringSplitOptions.None));
                }
            }
            else            
                getChapterDetails(doc, chaptersInfo[0].Split(new string[] { "\n" }, StringSplitOptions.None));     

            printDocument(doc);
        }

        private static void saveDocument(string filePath, string content)
        {         

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Write the string to the file
                    writer.Write(content);
                }

                //Console.WriteLine("String successfully written to the file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void printDocument(Document doc)
        {
            StringBuilder sb = new StringBuilder();

            writeDocumentTag(doc.DocumentName,doc.Summary,sb);

            sb.AppendLine("Chapters: [");

            sb.AppendLine(" {");
            
            foreach (var chapter in doc.Chapters)
            {
                sb.AppendLine("{");
                WriteNameTag(chapter.ChapterName, sb);
                WriteSectionNameTag(chapter.Sections, sb);
                sb.AppendLine("},");
            }
            sb.AppendLine("],");
            sb.AppendLine(" },");
            var x = sb.ToString();
            //Console.WriteLine(x);
            saveDocument("C:\\DileepJavaScriptTool\\OutPut\\OutputSample.js", x);
            Console.ReadLine();
        }

        private static void writeDocumentTag(string docName, string summary, StringBuilder sb)
        {
           sb.AppendLine("{");
            WriteNameTag(docName,sb);           
            writeSummaryTag(summary,sb);
            //sb.AppendLine("},");
            //Console.WriteLine(sb.ToString());
        }

        private static void getChapterDetails(Document doc, string[] lines)
        {
            Chapter chapter = new Chapter();
            getSectionInfo(lines, chapter);
            doc.Chapters.Add(chapter);
        }

        private static void getSectionInfo(string[] lines,Chapter chapter)
        {
            List<Section> sections = new List<Section>();
            //currentSection = 1;
            //bool SectionStartIndexSet = false;
            StringBuilder sessionBuilder = new StringBuilder();
            StringBuilder chapterCaptionBuilder=new StringBuilder(); 
            string currSectionName = string.Empty;
            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    if (!String.IsNullOrEmpty(lines[i].Replace("\r", "")) )
                    {
                        if (!SectionStartIndexSet && !lines[i].StartsWith(currentSection.ToString()) && !lines[i].StartsWith(currentSection.ToString() + "."))
                        {
                            chapterCaptionBuilder.AppendLine(lines[i]);
                            continue;
                        }
                        if (lines[i].StartsWith(currentSection.ToString()) && lines[i].StartsWith(currentSection.ToString() + "."))
                        {
                            SectionStartIndexSet = true;
                            var secName = lines[i].Split('.');
                            if ((secName.Length > 2))
                            {
                                AddDoubleQuotesAndComma(sessionBuilder, secName[2]);
                            }
                            currSectionName = currentSection.ToString() + ". " + secName[1];
                            SectionStartIndexSet = true;
                            validateNextLine(lines, sections, sessionBuilder, currSectionName, i);
                            continue;
                        }

                        AddDoubleQuotesAndComma(sessionBuilder, lines[i]);
                    }
                    validateNextLine(lines, sections, sessionBuilder, currSectionName, i);

                }
                catch (Exception)
                {
                    Console.WriteLine("Error Line: " + lines[i]);
                    return;
                }
               
            }
            sections.Add(new Section(currSectionName, sessionBuilder.ToString()));
            currentSection++;
            sessionBuilder.Clear();

            chapter.ChapterName = chapterCaptionBuilder.ToString();
            chapter.Sections = sections;
        }

        private static void validateNextLine(string[] lines, List<Section> sections, StringBuilder sessionBuilder, string currSectionName, int i)
        {
            if (i + 1 < lines.Length && SectionStartIndexSet && lines[i + 1].StartsWith((currentSection + 1).ToString()) && lines[i + 1].StartsWith((currentSection + 1).ToString()+"."))
            {
                currentSection++;
                AddDoubleQuotesAndComma(sessionBuilder, lines[i]);
                sections.Add(new Section(currSectionName, sessionBuilder.ToString()));
                sessionBuilder.Clear();
            }
        }

        public static void SetDocumentDetails(string docTitleSection, Document doc)
        {
            StringBuilder builder = new StringBuilder();

            var lines=docTitleSection.Split(new string[] { "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line.Replace("\r", "")))
                    continue;

                if (line.StartsWith("1.") || line.TrimStart().StartsWith("CHAPTER"))
                    break;                

                    if (string.IsNullOrEmpty(doc.DocumentName))
                    {
                        doc.DocumentName = line.Replace("\r", "");
                        continue;
                    }
                    if (!string.IsNullOrEmpty(doc.DocumentName))                   
                        builder.AppendLine(line.Replace("\r", ""));
                
            }
            doc.Summary = builder.ToString();
        }

        public static void WriteNameTag(string Summary,StringBuilder sb )
        {           

            sb.Append("name:");
            AddDoubleQuotesAndComma(sb, Summary);
           
        }

        public static string writeSummaryTag(string Summary,StringBuilder sb)
        {           

            sb.AppendLine("summary:");
            AddDoubleQuotesAndComma(sb, Summary);


            return sb.ToString();
        }
       

        public static void WriteSectionNameTag(List<Section> sections,StringBuilder DocBuilder)
        {            
            DocBuilder.AppendLine("sections: [");
            try
            {
                foreach (var item in sections)
                    WriteSectionsTag(item.SectionName, item.Details, DocBuilder);
                DocBuilder.AppendLine("],");
            }
            catch (Exception)
            {

                
            }
            
        }

        public static void WriteSectionsTag(string sectionName, string Details, StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("{");
            WriteNameTag(sectionName, stringBuilder);
            WriteDetailsTag(Details, stringBuilder);
            stringBuilder.AppendLine("},");
        }


        private static void WriteDetailsTag(string Details,StringBuilder sb)
        {
            sb.AppendLine("details: [");
            sb.AppendLine(Details);
            sb.AppendLine("]");

           

        }

        private static void AddDoubleQuotesAndComma(StringBuilder builder, string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value.Replace("\r", "")))
                return;

            builder.AppendLine(AddDoubleQuotesAndCommaString(value.Trim()));
        }

        private static string AddDoubleQuotesAndCommaString(string value)
        {
            if (string.IsNullOrEmpty(value.Replace("\r", "")))
                return string.Empty;

            return "\"" + value.Replace("\r", "") + "\",";
        }


        public class Document
        {
            public Document()
            {
                Chapters = new List<Chapter>();
            }
            public string DocumentName { get; set; }
            public string Summary { get; set; }

            public List<Chapter> Chapters { get; set; }
        }

        public class Chapter
        {
            public string ChapterName { get; set; }
            public List<Section> Sections { get; set; }
        }

        public class Section
        {
            public Section(string Name, string Details)
            {
                this.SectionName = Name;
                this.Details = Details;
            }           
            public string SectionName { get; set; }
            public string Details { get; set; }
        }


    }
}
