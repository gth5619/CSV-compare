using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;

namespace CSVCompare
{
    class Program
    {
        #region main
        static void Main(string[] args)
        {
            bool isError = false;
            string fileName = string.Empty;
            string SourceCsv = string.Empty;
            string DownloadedCSV = string.Empty;
            StringBuilder sbFileInfoReport = new StringBuilder();
            StringBuilder sbErrorTableLog = new StringBuilder();
            StringBuilder sbMismatchReport = new StringBuilder();

            try
            {
                if (args != null && args.Length == 2)
                {
                    SourceCsv = args[0];
                    DownloadedCSV = args[1];

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  ****************************************************** ");
                    Console.WriteLine(" ************ CSV Comparision EXE Started ************   ");
                    Console.WriteLine("  ******************************************************   ");
                    Console.WriteLine(" ");


                    sbFileInfoReport.Append("<html><title> Error Log</title><body><h2>Error Log Table</h2>");
                    sbFileInfoReport.Append("<table border = '1' bordercolor='green' style='width:50%'> <tr  bgcolor='white'> <th>  Source File Path   </th>    <th>  Downloaded File Path  </th>   </tr>");
                    sbFileInfoReport.Append("<tr>");
                    sbFileInfoReport.Append("<td>" + SourceCsv + "</td>");
                    sbFileInfoReport.Append("<td>" + DownloadedCSV + "</td>");
                    sbFileInfoReport.Append("</tr>");
                    sbFileInfoReport.Append("</table>");
                    sbFileInfoReport.Append("</br>");

                    var headersList = GetHeadersInfo(SourceCsv, DownloadedCSV);
                    List<string> sourceHeaderList = GetHeaderInfo(SourceCsv);
                    List<string> DownloadedHeaderList = GetHeaderInfo(DownloadedCSV);

                    if (sourceHeaderList == null || DownloadedHeaderList == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  -----> Source file and Destination file has Zero '0' Header. <-----");
                    }
                    else if (sourceHeaderList.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  -----> Source file has Zero '0' Headers. <-----");
                    }
                    else if (DownloadedHeaderList.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  -----> Destination file has Zero '0' Header. <-----");
                    }
                    else
                    {
                        IDictionary<int, string> SourceDictionary = FrameHeaderPosition(sourceHeaderList);
                        IDictionary<int, string> DownloadedDictionary = FrameHeaderPosition(DownloadedHeaderList);

                        StringBuilder sbRemoved = new StringBuilder();
                        StringBuilder sbOrderMismatch = new StringBuilder();

                        CSVLines objSource = ReadCSV(SourceCsv);
                        CSVLines objDownloaded = ReadCSV(DownloadedCSV);


                        sbMismatchReport.Append(" <table border = '1' bordercolor='green' style='width:50%'> <tr  bgcolor='pink'> <th> Row Number </th>    <th style='width:23%'> Column Name </th>    <th> Base CSV Data </th>	<th> Downloaded CSV Data</th>  </tr>");
                        bool isRowAvilable = isRowsavilable(objSource, objDownloaded);
                        if (isRowAvilable)
                        {
                            // just for printing purpose
                            if (!objSource.LineList.Count.Equals(objDownloaded.LineList.Count))
                            {
                                if (objSource.LineList.Count > objDownloaded.LineList.Count)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("  -----> Source file having more rows    <-----");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("  -----> Downloaded file having more rows    <-----");
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine("  ----->Source file and  Downloaded file  having equal rows    <-----");
                            }
                            //-------------------------------------------------



                            int SourceHeaderCounter = 0;
                            foreach (KeyValuePair<int, string> kvpSource in SourceDictionary)
                            {
                                int headerCounter = 0;
                                foreach (KeyValuePair<int, string> kvpDownloaded in DownloadedDictionary)
                                {
                                    if (kvpSource.Value.Equals(kvpDownloaded.Value))
                                    {
                                        if (kvpSource.Key != kvpDownloaded.Key)
                                        {
                                            // log position data
                                            sbOrderMismatch.Append("Column Name in Source CSV: ");
                                            sbOrderMismatch.Append(kvpSource.Value);
                                            sbOrderMismatch.Append("<br>");
                                            sbOrderMismatch.Append("Position in Source CSV: ");
                                            sbOrderMismatch.Append(kvpSource.Key);
                                            sbOrderMismatch.Append("<br>");
                                            sbOrderMismatch.Append("Position in downloaded CSV : ");
                                            sbOrderMismatch.Append(kvpDownloaded.Key);
                                            sbOrderMismatch.Append("<br>");
                                            sbOrderMismatch.Append("--------------------------------------------------------");
                                            sbOrderMismatch.Append("<br>");
                                        }

                                        int DownloadedPosition = kvpDownloaded.Key;
                                        DownloadedPosition--;

                                        //process rows
                                        if (!objSource.LineList.Count.Equals(objDownloaded.LineList.Count))
                                        {
                                            int rowCouter = 0;
                                            int dataCounter = 0;
                                            int EachLineNumber = 0;

                                            // Source file having more rows then downloaded
                                            if (objSource.LineList.Count > objDownloaded.LineList.Count)
                                            {

                                                #region Sourcefilehavingmorerows
                                                rowCouter = objSource.LineList.Count;
                                                dataCounter = objSource.LineList[0].RowList.Count;

                                                EachLineNumber = objDownloaded.LineList.Count;

                                                for (int lineNumber = 0; lineNumber < rowCouter; lineNumber++)
                                                {
                                                    if (!objSource.LineList[lineNumber].RowList[SourceHeaderCounter].Equals(EachLineNumber > lineNumber ? objDownloaded.LineList[lineNumber].RowList[DownloadedPosition] : string.Empty))
                                                    {
                                                        isError = true;
                                                        int Number = lineNumber + 2;
                                                        sbMismatchReport.Append("<tr>");
                                                        sbMismatchReport.Append("<td>" + Number + "</td>");
                                                        sbMismatchReport.Append("<td>" + kvpSource.Value + "</td>");
                                                        sbMismatchReport.Append("<td>" + HttpUtility.HtmlEncode(objSource.LineList[lineNumber].RowList[SourceHeaderCounter]) + "</td>");
                                                        sbMismatchReport.Append("<td>" + (EachLineNumber > lineNumber ? HttpUtility.HtmlEncode(objDownloaded.LineList[lineNumber].RowList[DownloadedPosition]) : string.Empty) + "</td>");
                                                        sbMismatchReport.Append("</tr>");
                                                    }
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                #region  SourceFileHavingLess
                                                // Source file having Less rows then downloaded


                                                rowCouter = objDownloaded.LineList.Count;
                                                dataCounter = objDownloaded.LineList[0].RowList.Count;

                                                EachLineNumber = objSource.LineList.Count;

                                                for (int lineNumber = 0; lineNumber < rowCouter; lineNumber++)
                                                {
                                                    if (!objDownloaded.LineList[lineNumber].RowList[DownloadedPosition].Equals(EachLineNumber > lineNumber ? objSource.LineList[lineNumber].RowList[SourceHeaderCounter] : string.Empty))
                                                    {
                                                        isError = true;
                                                        int Number = lineNumber + 2;
                                                        sbMismatchReport.Append("<tr>");
                                                        sbMismatchReport.Append("<td>" + Number + "</td>");
                                                        sbMismatchReport.Append("<td>" + kvpSource.Value + "</td>");//headersList[j]
                                                        sbMismatchReport.Append("<td>" + (EachLineNumber > lineNumber ? HttpUtility.HtmlEncode(objSource.LineList[lineNumber].RowList[SourceHeaderCounter]) : string.Empty) + "</td>");
                                                        sbMismatchReport.Append("<td>" + HttpUtility.HtmlEncode(objDownloaded.LineList[lineNumber].RowList[DownloadedPosition]) + "</td>");
                                                        sbMismatchReport.Append("</tr>");
                                                    }
                                                }
                                                #endregion
                                            }
                                        }
                                        else
                                        {
                                            #region SourceDownloadCountSame

                                            int rowCouter = objSource.LineList.Count;
                                            int dataCounter = objSource.LineList[0].RowList.Count;

                                            for (int lineNumber = 0; lineNumber < rowCouter; lineNumber++)
                                            {
                                                if (!objSource.LineList[lineNumber].RowList[SourceHeaderCounter].Equals(objDownloaded.LineList[lineNumber].RowList[DownloadedPosition]))
                                                {
                                                    isError = true;
                                                    int Number = lineNumber + 2;
                                                    sbMismatchReport.Append("<tr>");
                                                    sbMismatchReport.Append("<td>" + Number + "</td>");
                                                    sbMismatchReport.Append("<td>" + kvpSource.Value + "</td>");// headersList[j]
                                                    sbMismatchReport.Append("<td>" + HttpUtility.HtmlEncode(objSource.LineList[lineNumber].RowList[SourceHeaderCounter]) + "</td>");
                                                    sbMismatchReport.Append("<td>" + HttpUtility.HtmlEncode(objDownloaded.LineList[lineNumber].RowList[DownloadedPosition]) + "</td>");
                                                    sbMismatchReport.Append("</tr>");
                                                }
                                            }
                                            #endregion
                                        }
                                        #endregion

                                        //removeing item from Dictonary
                                        DownloadedDictionary.Remove(kvpDownloaded.Key);
                                        SourceDictionary.Remove(kvpSource.Key);
                                        break;
                                    }
                                    headerCounter++;
                                }
                                SourceHeaderCounter++;
                            }
                            // process log info table 
                            //Detailed Scenarios
                            #region Detailed Scenarios Log
                            sbErrorTableLog.Append("</br>");
                            sbErrorTableLog.Append("<h2>Error Details</h2>");
                            // sbErrorTableLog.Append("<table border = '1' bordercolor='green' style='width:50%'> <tr  bgcolor='white'> <th>  Source File Path   </th>    <th>  Downloaded File Path  </th>  <th>  Log error  </th> </tr>");
                            sbErrorTableLog.Append("<table border = '1' bordercolor='green' style='width:50%'>");

                            sbErrorTableLog.Append("<tr>");
                            if (SourceDictionary.Count > 0)
                            {
                                sbErrorTableLog.Append("<td>List of columns removed in downloaded CSV </td>");
                                sbErrorTableLog.Append("<td>" + ReportHeaders(SourceDictionary) + "</td>");
                            }
                            else
                            {
                                sbErrorTableLog.Append("<td>List of columns removed in downloaded CSV </td>");
                                sbErrorTableLog.Append("<td> All Headers Found </td>");
                            }
                            sbErrorTableLog.Append("</tr>");

                            sbErrorTableLog.Append("<tr>");
                            if (DownloadedDictionary.Count > 0)
                            {
                                sbErrorTableLog.Append("<tr>");
                                sbErrorTableLog.Append("<td> List of columns added in Downloaded CSV </td>");
                                sbErrorTableLog.Append("<td>" + ReportHeaders(DownloadedDictionary) + "</td>");
                                sbErrorTableLog.Append("</tr>");
                            }
                            else
                            {
                                sbErrorTableLog.Append("<td>List of columns added in Downloaded CSV </td>");
                                sbErrorTableLog.Append("<td> No columns addition</td>");
                            }
                            sbErrorTableLog.Append("</tr>");

                            sbErrorTableLog.Append("<tr>");
                            if (!string.IsNullOrEmpty(sbOrderMismatch.ToString()))
                            {
                                sbErrorTableLog.Append("<tr>");
                                sbErrorTableLog.Append("<td>Order Mismatch Headers </td>");
                                sbErrorTableLog.Append("<td>" + sbOrderMismatch.ToString() + "</td>");
                                sbErrorTableLog.Append("</tr>");
                            }
                            else
                            {
                                sbErrorTableLog.Append("<td>Order Mismatch headers: </td>");
                                sbErrorTableLog.Append("<td> '0' </td>");
                            }
                            sbErrorTableLog.Append("</tr>");
                            sbErrorTableLog.Append("</table>");
                            #endregion
                        }
                        else
                        {
                            // no data found
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("  ----->  No records Found  <----- ");

                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" Please Pass a valid arguments path  ");
                }

                #region log
                if (!isError)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    sbMismatchReport.Append(" <table border = '1' bordercolor='green' style='width:50%'> <tr  bgcolor='pink'> <th> Row Number </th>    <th> Coloum Name </th>    <th> Base CSV Data </th>	<th> Downloaded CSV Data</th>  </tr>");
                    sbMismatchReport.Append("<tr>  <td> NO issues </td>  <td> NO issues </td>  <td> NO issues </td>  <td> NO issues </td> </tr>");
                }
                sbMismatchReport.Append("</table></body></html>");
                // Save log
                if (!Directory.Exists(@"C:\temp\Tosca\Report Analysis\"))
                {
                    Directory.CreateDirectory(@"C:\temp\Tosca\Report Analysis\");
                }
                fileName = GetFileName(DownloadedCSV);

                //Final report to Print
                StringBuilder sbFinalReport = new StringBuilder();
                sbFinalReport.Append(sbFileInfoReport.ToString());
                sbFinalReport.Append(sbErrorTableLog.ToString());
                sbFinalReport.Append("<br>");
                sbFinalReport.Append(sbMismatchReport.ToString());


                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" Error Log HTML saved on : C:\\temp\\Tosca\\Report Analysis\\ ");
                File.WriteAllText(@"C:\temp\Tosca\Report Analysis\" + fileName + " " + DateTime.Now.ToString("dd-MM-yyyy  hh-mm-ss") + ".html", sbFinalReport.ToString());
                #endregion


                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" ******************************************************  ");
                Console.WriteLine(" ************ CSV Comparision EXE End  ************  ");
                Console.WriteLine(" ******************************************************  ");

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }


        #region  isRowsavilable
        public static bool isRowsavilable(CSVLines objSource, CSVLines objDownloaded)
        {
            bool isContainingRows = false;
            try
            {
                if (objSource.LineList == null || objSource.LineList.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" Source file rows Empty (Or) no data in CSV ");
                }
                else if (objDownloaded.LineList == null || objDownloaded.LineList.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" Downloaded file rows empty (Or) no data in CSV ");
                }
                else
                {
                    isContainingRows = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return isContainingRows;
        }
        #endregion


        #region Get Headers
        public static List<string> GetHeaderInfo(string filePath)
        {
            List<string> headingList = new List<string>();
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                //Console.WriteLine(" Reading headers from Source File ");
                //Console.WriteLine(" ");
                string[] fileContentsOne = File.ReadAllLines(filePath);

                //Console.WriteLine(" Reading headers from Downloaded File ");
                //Console.WriteLine(" ");

                if (fileContentsOne != null)
                {
                    string[] columnshead1 = fileContentsOne[0].Split(new char[] { ';' });
                    string[] headingsplit1 = columnshead1[0].Split(',');

                    //Console.WriteLine(" Veerifying the Source coloum and destnation file coloum data ");
                    //Console.WriteLine(" ");

                    if (headingsplit1 != null)
                    {
                        for (int i = 0; i < headingsplit1.Length; i++)
                        {
                            //Console.ForegroundColor = ConsoleColor.White;
                            //Console.WriteLine(" Added Headers: {0} ", headingsplit1[i]);
                            headingList.Add(headingsplit1[i].Trim());
                        }
                    }
                    else { return null; }
                }
                else { return null; }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return headingList;
        }
        #endregion


        #region Frame ReportHeaders
        public static string ReportHeaders(IDictionary<int, string> dic)
        {
            StringBuilder sbData = new StringBuilder();
            try
            {
                int counter = 1;
                foreach (KeyValuePair<int, string> item in dic)
                {
                    sbData.Append(counter);
                    sbData.Append(") ");
                    sbData.Append(item.Value);
                    sbData.Append("<br>");
                    counter++;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return sbData.ToString();
        }
        #endregion


        #region Frame Headers position
        public static IDictionary<int, string> FrameHeaderPosition(List<string> data)
        {
            IDictionary<int, string> Dictionary = new Dictionary<int, string>();
            try
            {
                for (int i = 1; i <= data.Count; i++)
                {
                    Dictionary.Add(i, data[i - 1].Replace("\"", "").Trim());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Dictionary;
        }
        #endregion

        #region Get Headers Data
        public static List<string> GetHeadersInfo(string filePathOne, string filePathTwo)
        {
            List<string> heading1 = new List<string>();
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine(" Reading headers from Source File ");
                Console.WriteLine(" ");
                string[] fileContentsOne = File.ReadAllLines(filePathOne);

                Console.WriteLine(" Reading headers from Downloaded File ");
                Console.WriteLine(" ");
                string[] fileContentsTwo = File.ReadAllLines(filePathTwo);


                if (fileContentsOne != null && fileContentsTwo != null)
                {
                    string[] columnshead1 = fileContentsOne[0].Split(new char[] { ';' });
                    string[] headingsplit1 = columnshead1[0].Split(',');

                    string[] columnshead2 = fileContentsTwo[0].Split(new char[] { ';' });
                    string[] headingsplit2 = columnshead2[0].Split(',');

                    Console.WriteLine(" Veerifying the Source coloum and destnation file coloum data ");
                    Console.WriteLine(" ");
                    if (!headingsplit1.Length.Equals(headingsplit2.Length))
                        return null;

                    for (int i = 0; i < headingsplit1.Length; i++)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(" Added Headers: {0} ", headingsplit1[i]);
                        heading1.Add(headingsplit1[i].Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return heading1;
        }
        #endregion

        #region ReadCSV
        public static CSVLines ReadCSV(string absolutePath)
        {

            CSVLines objSource = new CSVLines();
            objSource.LineList = new List<CSVRows>();
            CSVRows objSourceRow;
            try
            {
                using (TextReader fileReader = File.OpenText(absolutePath))
                {
                    var csv = new CsvReader(fileReader, CultureInfo.InvariantCulture);
                    csv.Configuration.HasHeaderRecord = true;
                    csv.Configuration.MissingFieldFound = null;
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        objSourceRow = new CSVRows();
                        foreach (var header in csv.Parser.Context.HeaderRecord)
                        {
                            objSourceRow.RowList.Add(csv.GetField<string>(header) ?? string.Empty);
                        }
                        objSource.LineList.Add(objSourceRow);
                    }
                }
            }
            catch (Exception ex) { throw ex; }
            return objSource;
        }
        #endregion

        #region GetFileName
        public static string GetFileName(string filePath)
        {
            string name = string.Empty;
            try
            {
                string[] arry = filePath.Split('\\');
                int length = arry.Length;
                name = arry[length - 1].Replace(".csv", string.Empty).Replace(".CSV", string.Empty);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return name;
        }
        #endregion
    }

    #region Class
    public class CSVLines
    {
        public List<CSVRows> LineList;
    }

    public class CSVRows
    {
        public List<string> RowList = new List<string>();
    }
    #endregion
}