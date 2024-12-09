
using System.Net.Http.Headers;
using System.Net.Http;
using System.Diagnostics;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace iTrentRestApiTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new HttpClient();           
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "*********************************");
            client.Timeout = Timeout.InfiniteTimeSpan;

            // configurable variables
            string organisation = "0650 PolyMAT Trust";
            string requestData = "employee"; 
            int recordsPerPage = 20;
            int maximumPageNumber = 0; // zero to get all pages
            int maximumRecordNumber = 0; 

            if (maximumRecordNumber > 0 && maximumRecordNumber < recordsPerPage)
                maximumRecordNumber = recordsPerPage;

            start:
            // pagination variables (empty for first request)
            string totalRecords = "";
            string pageNumber = "";
            string recordCount = "";
            string lastId = "";

            var rootNames = new Dictionary<string, string>
            {
                { "employee", "employeerecord" },
                { "position", "itrent" }
            };


            double totalElapsedSeconds = 0;           
            do
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://ce0218de.webitrent.com/ce0218de_web/wrd/run/etws001api.json");
                request.Headers.Clear();
                request.Headers.Add("iTrent-Operation", requestData);
                request.Headers.Add("PartyName", organisation);
                request.Headers.Add("RefNo", "");
                request.Headers.Add("Size", recordsPerPage.ToString());
                request.Headers.Add("Lastid", lastId);
                request.Headers.Add("Proccount", recordCount);
                request.Headers.Add("Number", pageNumber);

                Stopwatch stopwatch = Stopwatch.StartNew();
                var response = await client.SendAsync(request);
                stopwatch.Stop();
                double elapsedSeconds = ((double)stopwatch.ElapsedMilliseconds / 1000);
                totalElapsedSeconds += elapsedSeconds;
                try
                {
                    response.EnsureSuccessStatusCode();
                    string responseJson = await response.Content.ReadAsStringAsync();

                    JToken jToken;
                    try
                    {
                        jToken = JObject.Parse(responseJson);
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                    //Console.WriteLine(responseJson);
                    
                    JObject? jObjPage = (JObject?)jToken[rootNames[requestData]]!["page"];
                    if (jObjPage == null)
                        break;

                    string totalPages = jObjPage["totalpages"]!.ToString();
                    totalRecords = jObjPage["totalelements"]!.ToString();
                    pageNumber = jObjPage["number"]!.ToString();
                    recordCount = jObjPage["proccount"]!.ToString();
                    lastId = jObjPage["lastid"]!.ToString();
                    Console.WriteLine($"Page Number: {pageNumber} of {totalPages}, Records: {(int.Parse(pageNumber) - 1) * recordsPerPage + 1} - {recordCount} of {totalRecords} Response Time: {elapsedSeconds:N2} seconds");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            while (recordCount != totalRecords && int.Parse(pageNumber) != maximumPageNumber && (maximumRecordNumber > 0 ? int.Parse(recordCount) < maximumRecordNumber : true));
            Console.WriteLine($"\nRead {recordCount} records in {totalElapsedSeconds:N2} seconds ({((totalElapsedSeconds / int.Parse(recordCount)) * 1000):N0} ms per record)");
            Console.WriteLine("\n[r]un again");
            Console.WriteLine("[1]Run again (100 Records per page)");
            Console.WriteLine("[2]Run again (200 Records per page)");
            Console.WriteLine("[3]Run again (300 Records per page)");
            Console.WriteLine("[4]Run again (400 Records per page)");
            Console.WriteLine("[5]Run again (500 Records per page)");
            Console.WriteLine("[6]Run again (600 Records per page)");
            Console.WriteLine("[7]Run again (700 Records per page)");
            Console.WriteLine("[8]Run again (800 Records per page)");
            Console.WriteLine("[9]Run again (900 Records per page)");
            Console.WriteLine($"[0]Run again ({recordCount} Records per page)");
            var keyPress = Console.ReadKey(true);
            var tt = keyPress.ToString();
            if (keyPress.KeyChar == 'r')
                goto start;
            else if (keyPress.KeyChar > 48 && keyPress.KeyChar < 58)
            {
                recordsPerPage = (keyPress.KeyChar - 48) * 100;
                goto start;
            }
            else if (keyPress.KeyChar == '0')
            {
                recordsPerPage = int.Parse(recordCount);
                goto start;
            }
        }
    }
}
