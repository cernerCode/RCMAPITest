using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web.Helpers; 
namespace RCMAPITest.Controllers
{

      
    [ApiController]
    [Route("[controller]")]
    public class RCMAPIController : ControllerBase
    {
        //prod configs
        string MpageURL = "http://10.192.19.13/discern/prod.moh.ae/";
        string UserAPI = "EHS";
        string PassAPI = "fe3ba784-0b66-4798-8610-3a39305ff330";
        string URLAPI = "https://tmbapi.riayati.ae:8083/";

        //cert

        //string MpageURL = "http://10.192.19.15/discern/cert.moh.ae/";
        //string UserAPI = "GRP331";
        //string PassAPI = "faeba180-9e96-432e-9663-6688bac494ea";
        //string URLAPI = "https://o-tmbapi.riayati.ae:8083/";

        private readonly ILogger<RCMAPIController> _logger;
        public RCMAPIController(ILogger<RCMAPIController> logger)
        {
            _logger = logger;
        }
        public class ClaimResponse
        {
            public DateTime Date { get; set; }

            public int id { get; set; }

            
            public string? Description { get; set; }
        }

        [HttpGet(Name = "GetRCMAPI")]
        public IEnumerable<ClaimResponse> Get()
        {
            ClaimResponse obj = new ClaimResponse();
            obj.Date= DateTime.Now;
            obj.Description = "Success";
            obj.id = 1;

            string output =   "Welcome to the User";

            return Enumerable.Range(1, 1).Select(index => new ClaimResponse
            {
                Date = DateTime.Now.AddDays(index),
                Description ="Success" ,
                id = 1 
            })
            .ToArray();

        }


        [HttpPost(Name = "SubmitEligibility")]
        [Route("SubmitEligAuth")]
        public async Task<IEnumerable<ClaimResponse>> SubmitEligAuthAsync(Object Cobj)
        {
            logaction("API hit start Elig");
            string responseContent = "";
            try
            { 
            //INSERTION
              DAL obj = new DAL();
            obj.InsertQueryForEligAuth(Cobj);
            string jsonr = Cobj.ToString();// JsonConvert.SerializeObject(Cobj);

            logaction("Json Elig posted " + DateTime.Now.ToString() + " JSON :" + jsonr);
            var httpContent = new StringContent(jsonr, Encoding.UTF8, "application/json");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Username", UserAPI);
            httpClient.DefaultRequestHeaders.Add("Password", PassAPI);
            // Do the actual request and await the response
            var httpResponse = await httpClient.PostAsync(URLAPI+"api/Authorization/PostRequest", httpContent);
                //try api
                //var httpResponse1 = await httpClient.PostAsync("http://10.2.46.233:6001/RCMAPI/SubmitEligAuth", httpContent);
                //var responseContent1 = await httpResponse1.Content.ReadAsStringAsync();

                // If the response contains content we want to read it!

                if (httpResponse.Content != null)
            {
                  responseContent = await httpResponse.Content.ReadAsStringAsync();
                logaction("Json elig response " + DateTime.Now.ToString() + "  :" + responseContent);

                // From here on you could deserialize the ResponseContent back again to a concrete C# type using Json.Net
            }
            else
            {

            }


            dynamic Authreq = JsonConvert.DeserializeObject(jsonr);
            var eligType = Authreq.PriorRequest.Authorization.Type;
            return Enumerable.Range(1, 1).Select(index => new ClaimResponse
            {
                Date = DateTime.Now.AddDays(index),
                Description = eligType + " Posted Successfully to RCM Server & Riayti Response:"+ responseContent,
                id = 1
            })
            .ToArray();
            }
            catch(Exception ex)
            {
                logaction("Elig Error: " + ex.Message + ex.InnerException);
            }
            return null;

        }


        [HttpPost(Name = "SubmitAuth")]
        [Route("SubmitAuth")]
        public async Task<dynamic> SubmitAuthAsync(Object Cobj)
        {
            logaction("API hit start Eauth");
            string responseContent = "";
            try
            {  
            //INSERTION
            DAL obj = new DAL();
            //obj.InsertQueryForEligAuth(Cobj);
            string jsonr = Cobj.ToString();// JsonConvert.SerializeObject(Cobj);

            logaction("Json Auth posted " + DateTime.Now.ToString() + " JSON :" + jsonr);
            dynamic Authreq = JsonConvert.DeserializeObject(jsonr);
            var he = Authreq.PriorRequest.Header.SenderID;
            //dynamic data = JsonConvert.Deserialize(json, typeof(object));
            var httpContent = new StringContent(jsonr, Encoding.UTF8, "application/json");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Username", UserAPI);
            httpClient.DefaultRequestHeaders.Add("Password", PassAPI);
            // Do the actual request and await the response
            var httpResponse = await httpClient.PostAsync(URLAPI + "api/Authorization/PostRequest", httpContent);

            // If the response contains content we want to read it!
            
            if (httpResponse.Content != null)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync();
                logaction("Json auth response " + DateTime.Now.ToString() + "  :" + responseContent);

                // From here on you could deserialize the ResponseContent back again to a concrete C# type using Json.Net
            }
            else
            {

            }


                //dynamic AuthreqResp = JsonConvert.DeserializeObject(responseContent.ToString());
            }
            catch (Exception ex)
            {
                logaction("Eauth Error: " + ex.Message + ex.InnerException);
            }
            return responseContent;

        }

        [HttpPost(Name = "SubmitClaim")]
        [Route("SubmitClaim")]
        public async Task<dynamic> SubmitClaimAsync(Object Cobj)
        {
            logaction("API hit start Claim");
            string responseContent = "";
            try
            {  
            //INSERTION
            DAL obj = new DAL();
            //obj.InsertQueryForEligAuth(Cobj);
            string jsonr = Cobj.ToString();// JsonConvert.SerializeObject(Cobj);
            logaction("Json claim posted "+DateTime.Now.ToString()+" JSON :"+jsonr);
            dynamic claimreq = JsonConvert.DeserializeObject(jsonr);
            var he = claimreq.Submission.Claim;
                dynamic claimid = JsonConvert.DeserializeObject(he.ToString());
                  var yourObject = System.Text.Json.JsonDocument.Parse(jsonr);
                var Claimidval = yourObject.RootElement.GetProperty("Submission").GetProperty("Claim")[0]
                                .GetProperty("CLAIMID");
                if (claimreq.Submission.Header.ReceiverID== "MOHAPTPA003")
                {
                    claimreq.Submission.Header.ReceiverID = "MohapTPA003";
                }
            //dynamic data = JsonConvert.Deserialize(json, typeof(object));
            var httpContent = new StringContent(claimreq.ToString(), Encoding.UTF8, "application/json");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Username", UserAPI);
            httpClient.DefaultRequestHeaders.Add("Password", PassAPI);
            // Do the actual request and await the response
            var httpResponse = await httpClient.PostAsync(URLAPI + "api/Claim/PostSubmission", httpContent);

            // If the response contains content we want to read it!
         
            if (httpResponse.Content != null)
            {
                responseContent = await httpResponse.Content.ReadAsStringAsync();
                 logaction("Json claim response " + DateTime.Now.ToString() + "  :" + responseContent);
                    dynamic ClaimResp= JsonConvert.DeserializeObject(responseContent);
                    var Statuscode = ClaimResp.StatusCode;
                    var Errorlist = "Status:" + Statuscode.ToString() ;// ClaimResp.Error;

                    if(Statuscode.ToString()=="400")
                    {
                        var yourObjectError = System.Text.Json.JsonDocument.Parse(ClaimResp.ToString());
                        var ClaimidvalError = yourObjectError.RootElement.GetProperty("Error")[0]
                                        .GetProperty("ErrorText");
                        Errorlist += " "+ ClaimidvalError.ToString(); 
                    }
                    else if(Statuscode.ToString() == "201")
                    {
                        Errorlist += " Successfully added to PO " + DateTime.Now.ToString("dd MMM yy HH mm");
                    }

                    HttpMessageHandler handlerMpage = new HttpClientHandler()
                    {
                    };
                    //  string mpageurl = "http://10.192.19.15/discern/cert.moh.ae/mpages/reports/minh_ae_post_eligibilty_status?parameters=" + "'" + eligauthResult + "','" + eligauthID + "'";
                    string mpageurl = MpageURL + "mpages/reports/minh_ae_post_rpo_response?parameters="+"'"+Claimidval+"',"+"'"+Errorlist.ToString()+"'";// AuthMpageURL + "mpages/reports/minh_ae_eauth_response_inbound";

                    var httpClientMpage = new HttpClient(handlerMpage)
                    {
                        BaseAddress = new Uri(mpageurl),
                        Timeout = new TimeSpan(0, 2, 0)
                    };
                    var plainTextBytes1 = System.Text.Encoding.UTF8.GetBytes("cernertest@cert.moh.ae:wareed");
                    string val1 = System.Convert.ToBase64String(plainTextBytes1);
                    httpClientMpage.DefaultRequestHeaders.Add("Authorization", "Basic " + val1);

                    //httpClientMpage.DefaultRequestHeaders.Add("ContentType", "application/json");

                    //This is the key section you were missing    
                    //plainTextBytes1 = System.Text.Encoding.UTF8.GetBytes("cernertest@cert.moh.ae:wareed");
                    //val1 = System.Convert.ToBase64String(plainTextBytes1);
                    //httpClientMpage.DefaultRequestHeaders.Add("Authorization", "Basic " + val1);

                    HttpResponseMessage responseMpage = httpClientMpage.GetAsync(mpageurl).Result;
                    string contentMpage = string.Empty;
                    string dataMpage = responseMpage.Content.ReadAsStringAsync().Result;
                    logaction("Claim mpage url:"+ mpageurl + "Claim immediate resp: "+dataMpage);
                    if (dataMpage.ToLower().Contains("success"))
                    {


                    }



                    // From here on you could deserialize the ResponseContent back again to a concrete C# type using Json.Net
                }
                else
            {

            }
            }
            catch (Exception ex)
            {
                logaction("Claim Error: " + ex.Message + ex.InnerException);
            }

            //dynamic AuthreqResp = JsonConvert.DeserializeObject(responseContent.ToString());

            return responseContent;

        }
        void logaction(string data)
        {
            //check size take backup
            long length = new System.IO.FileInfo(@"logfiles/logs.txt").Length;
            if(length > 2000000)
            {
                System.IO.File.Copy(@"logfiles/logs.txt", @"logfiles/logs" + DateTime.Now.ToString("ddMMyyhhmmss")+".txt", true);
                System.IO.File.WriteAllText(@"logfiles/logs.txt", "" + Environment.NewLine);

            }
            System.IO.File.AppendAllText(@"logfiles/logs.txt", data + Environment.NewLine);
             
        }

        // GET: RCMAPIController
        //public ActionResult Index()
        //{
        //    return View();
        //}

        //// GET: RCMAPIController/Details/5
        //public ActionResult Details(int id)
        //{
        //    return Json("[{resp: 1}]");
        //}

        //// GET: RCMAPIController/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: RCMAPIController/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: RCMAPIController/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: RCMAPIController/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: RCMAPIController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: RCMAPIController/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
    
}
