using Newtonsoft.Json;
using System.Data;
using System.Data.OracleClient;
using System.Text;

namespace RCMProcessService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                try
                {
                    CallRCMProcess();
                }
                catch
                { 
                    continue;
                }
                await Task.Delay(120000, stoppingToken);
            }
        }
        string connectionstring = "";
        string SetupType = "";
        string riayatiUrl = "";// "";
        string riayatiuser = "";// ConfigFile.RCMConfig.riayatiuser; ;// ""; 
        string riayatipass = "";//  ConfigFile.RCMConfig.riayatipass; ;// "";
        int delay = 0;//  Convert.ToInt32(ConfigFile.RCMConfig.delay); ;// "";
        string eligMpageURL = "";// ConfigFile.RCMConfig.eligMpageURL; ;
        string AuthMpageURL = "";// ConfigFile.RCMConfig.AuthMpageURL; ;

        void CallRCMProcess()
        {
            try
            {
                Console.WriteLine("Service Start Call Get new For Eauth & Eligibility");
               ////try config
                //var builder = new ConfigurationBuilder().$AddJsonFile("config.json", optional: false);
                var configdata = System.IO.File.ReadAllText("config.json");
                dynamic ConfigFile = JsonConvert.DeserializeObject(configdata);
                connectionstring = ConfigFile.RCMConfig.ConnectionString;
                SetupType = ConfigFile.RCMConfig.SetupType;
                riayatiUrl = ConfigFile.RCMConfig.riayatiUrl; ;// "";
                riayatiuser = ConfigFile.RCMConfig.riayatiuser; ;// ""; 
                riayatipass = ConfigFile.RCMConfig.riayatipass; ;// "";
                delay = Convert.ToInt32(ConfigFile.RCMConfig.delay); ;// "";
                eligMpageURL = ConfigFile.RCMConfig.eligMpageURL; ;
                AuthMpageURL = ConfigFile.RCMConfig.AuthMpageURL; ;

                getNewEligEauth();//getnew for eath
                ViewEntitiesForEauthElig();//view entity details from database

                getNewClaim();                        // get entity id from getnew claim
                ViewEntitiesForClaim();//view entity details from database

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.InnerException);
                 
            }
        }
        async void ViewEntitiesForEauthElig()
        {

            DataTable GetnewData = GetdatafromQuery("select * from getnew_response where status='Pending' and EntityName='EligEauth' order by id desc");
            Console.WriteLine("EligEauth Count:" + GetnewData.Rows.Count);
            var Resv = SPExecData("SETPROCESSEDGETNEWEligAuth");
            foreach (DataRow row in GetnewData.Rows)
            {
                string EntityvalueID = row["ENTITYID"].ToString();
                string StatusValue = row["STATUS"].ToString();
                if (StatusValue == "Pending")
                {
                    ViewEligEauthByEntityID(EntityvalueID);
                }
            }

        }

        async void ViewEntitiesForClaim()
        {

            DataTable GetnewData = GetdatafromQuery("select * from getnew_response where status='Pending' and EntityName='Claim' order by id desc");
            var Resv = SPExecData("SETPROCESSEDGETNEWClaim");
            Console.WriteLine("Claim Count:" + GetnewData.Rows.Count);
            foreach (DataRow row in GetnewData.Rows)
            {
                string EntityvalueID = row["ENTITYID"].ToString();
                string StatusValue = row["STATUS"].ToString();
                if (StatusValue == "Pending")
                {
                    ViewClaimByEntityID(EntityvalueID);
                }
            }

        }


        async void getNewEligEauth()
        {

            try
            {
                HttpClient client = new HttpClient { BaseAddress = new Uri(riayatiUrl) };
                client.DefaultRequestHeaders.Add("Username", riayatiuser);
                client.DefaultRequestHeaders.Add("Password", riayatipass);
                HttpResponseMessage response = await client.GetAsync("Authorization/GetNew");
                Console.WriteLine(response);
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                var data = response.Content.ReadAsStringAsync().Result;
                Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(data);
                foreach (var item in myDeserializedClass.Entities)
                {
                    var Entityvalue = item.ID;
                    Console.WriteLine("Enitity ID view start :" + Entityvalue);
                    insertEauthElig_Getnew(item);


                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + ex.InnerException);

            }

        }

        async void getNewClaim()
        {
            try
            { 
            Console.WriteLine("Claim Get New Started");

            HttpClient client = new HttpClient { BaseAddress = new Uri(riayatiUrl) };
            client.DefaultRequestHeaders.Add("Username", riayatiuser);
            client.DefaultRequestHeaders.Add("Password", riayatipass);
            //HttpResponseMessage response = await client.GetAsync("Claim/GetNew");
            var response = Task.Run(async () => await client.GetAsync("Claim/GetNew")).Result;


            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            var data = response.Content.ReadAsStringAsync().Result;
            Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(data);
            foreach (var item in myDeserializedClass.Entities)
            {
                var Entityvalue = item.ID;
                Console.WriteLine("Enitity ID view start :" + Entityvalue);
                insertClaim_Getnew(item);


            }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + ex.InnerException);
            }

        }

        async void ViewEligEauthByEntityID(string Entityid)

        {

            HttpClient clientView = new HttpClient { BaseAddress = new Uri(riayatiUrl) };
            clientView.DefaultRequestHeaders.Add("Username", riayatiuser);
            clientView.DefaultRequestHeaders.Add("Password", riayatipass);

            //var responseView = await  clientView.GetAsync("Authorization/View?id=" + Entityid);
            //response = await client.GetAsync("Authorization/View?id=" + "6380a08b7c332d2a78018b78");
            var responseView = Task.Run(async () => await clientView.GetAsync("Authorization/View?id=" + Entityid)).Result;
            dynamic dataView = responseView.Content.ReadAsStringAsync().Result;
            //data1.Authorization.

            dynamic Authreq = JsonConvert.DeserializeObject(dataView.ToString());


            var activity = Authreq.Entity.Authorization.Activity;
            if (activity.ToString().Length < 5)
            {
                // Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            }
            if (Authreq.StatusCode.ToString() == "200" && activity.ToString().Length < 5)
            {
                var eligauthResult = Authreq.Entity.Authorization.Result;
                var eligauthID = Authreq.Entity.Authorization.ID;
                Authreq.UserMessage = Entityid;
                insertEauthElig_View(Authreq);
                Console.WriteLine("Writing to Millenium  Eligibility " + eligauthID + "--" + eligauthResult);
                //update mpage api
                HttpMessageHandler handlerMpage = new HttpClientHandler()
                {
                };
                string mpageurl = eligMpageURL + "mpages/reports/minh_ae_post_eligibilty_status?parameters=" + "'" + eligauthResult + "','" + eligauthID + "'";
                var httpClientMpage = new HttpClient(handlerMpage)
                {
                    BaseAddress = new Uri(mpageurl),
                    Timeout = new TimeSpan(0, 2, 0)
                };

                httpClientMpage.DefaultRequestHeaders.Add("ContentType", "application/json");

                //This is the key section you were missing    
                var plainTextBytes1 = System.Text.Encoding.UTF8.GetBytes("cernertest@cert.moh.ae:wareed");
                string val1 = System.Convert.ToBase64String(plainTextBytes1);
                httpClientMpage.DefaultRequestHeaders.Add("Authorization", "Basic " + val1);

                HttpResponseMessage responseMpage = httpClientMpage.GetAsync(mpageurl).Result;
                string contentMpage = string.Empty;
                string dataMpage = responseMpage.Content.ReadAsStringAsync().Result;
                if (dataMpage.ToLower().Contains("success"))
                {


                }

            }
            else if (activity.ToString().Length > 5)
            {
                var httpContent = new StringContent("parameters='" + Authreq + "'", Encoding.UTF8, "application/text");
                string mpageurl = eligMpageURL + "mpages/reports/minh_ae_eauth_response_inbound";

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("ContentType", "application/text");

                //This is the key section you were missing    
                var plainTextBytes1 = System.Text.Encoding.UTF8.GetBytes("cernertest@cert.moh.ae:wareed");
                string val1 = System.Convert.ToBase64String(plainTextBytes1);
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val1);


                // Do the actual request and await the response
                // var httpResponse = await httpClient.PostAsync(mpageurl, httpContent);
                var httpResponse = Task.Run(async () => await httpClient.PostAsync(mpageurl, httpContent)).Result;

                // If the response contains content we want to read it!
                string responseContent = "";
                if (httpResponse.Content != null)
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync();

                    // From here on you could deserialize the ResponseContent back again to a concrete C# type using Json.Net
                }



                var eligauthResult = Authreq.Entity.Authorization.Result;
                var eligauthID = Authreq.Entity.Authorization.ID;
                Authreq.UserMessage = Entityid;
                insertEauth_View(Authreq);
                Console.WriteLine("Writing to Millenium  Eauth " + eligauthID + "--" + eligauthResult);
                //update mpage api
                HttpMessageHandler handlerMpage = new HttpClientHandler()
                {
                };
                // string mpageurl = AuthMpageURL+"mpages/reports/minh_ae_post_eligibilty_status?parameters=" + "'" + eligauthResult + "','" + eligauthID + "'";

                var httpClientMpage = new HttpClient(handlerMpage)
                {
                    BaseAddress = new Uri(mpageurl),
                    Timeout = new TimeSpan(0, 2, 0)
                };

                httpClientMpage.DefaultRequestHeaders.Add("ContentType", "application/json");

                //This is the key section you were missing    
                plainTextBytes1 = System.Text.Encoding.UTF8.GetBytes("cernertest@cert.moh.ae:wareed");
                val1 = System.Convert.ToBase64String(plainTextBytes1);
                httpClientMpage.DefaultRequestHeaders.Add("Authorization", "Basic " + val1);

                HttpResponseMessage responseMpage = httpClientMpage.GetAsync(mpageurl).Result;
                string contentMpage = string.Empty;
                string dataMpage = responseMpage.Content.ReadAsStringAsync().Result;
                if (dataMpage.ToLower().Contains("success"))
                {


                }


            }

        }
        async void ViewClaimByEntityID(string Entityid)

        {


            HttpClient clientView = new HttpClient { BaseAddress = new Uri(riayatiUrl) };
            clientView.DefaultRequestHeaders.Add("Username", riayatiuser);
            clientView.DefaultRequestHeaders.Add("Password", riayatipass);

            //var responseView = await  clientView.GetAsync("Authorization/View?id=" + Entityid);
            //response = await client.GetAsync("Authorization/View?id=" + "6380a08b7c332d2a78018b78");
            var responseView = Task.Run(async () => await clientView.GetAsync("Claim/View?id=" + Entityid)).Result;
            dynamic dataView = responseView.Content.ReadAsStringAsync().Result;
            //data1.Authorization.

            dynamic Claimreq = JsonConvert.DeserializeObject(dataView.ToString());
            Console.WriteLine(Claimreq);

            //HttpMessageHandler handlerMpage = new HttpClientHandler()
            //{
            //};
            ////  string mpageurl = "http://10.192.19.15/discern/cert.moh.ae/mpages/reports/minh_ae_post_eligibilty_status?parameters=" + "'" + eligauthResult + "','" + eligauthID + "'";
            //string mpageurl = eligMpageURL+"mpages/reports/minh_ae_post_rpo_response?parameters=";// AuthMpageURL + "mpages/reports/minh_ae_eauth_response_inbound";

            //var httpClientMpage = new HttpClient(handlerMpage)
            //{
            //    BaseAddress = new Uri(mpageurl),
            //    Timeout = new TimeSpan(0, 2, 0)
            //};
            //var plainTextBytes1 = System.Text.Encoding.UTF8.GetBytes("cernertest@cert.moh.ae:wareed");
            //string val1 = System.Convert.ToBase64String(plainTextBytes1);
            //httpClientMpage.DefaultRequestHeaders.Add("Authorization", "Basic " + val1);

            //httpClientMpage.DefaultRequestHeaders.Add("ContentType", "application/json");

            ////This is the key section you were missing    
            //plainTextBytes1 = System.Text.Encoding.UTF8.GetBytes("cernertest@cert.moh.ae:wareed");
            //val1 = System.Convert.ToBase64String(plainTextBytes1);
            //httpClientMpage.DefaultRequestHeaders.Add("Authorization", "Basic " + val1);

            //HttpResponseMessage responseMpage = httpClientMpage.GetAsync(mpageurl).Result;
            //string contentMpage = string.Empty;
            //string dataMpage = responseMpage.Content.ReadAsStringAsync().Result;
            //if (dataMpage.ToLower().Contains("success"))
            //{


            //}


        }

        int SPExecData(string query)
        {
            int res = 0;
            try
            {
                using (System.Data.Common.DbConnection connection = new System.Data.OracleClient.OracleConnection(connectionstring))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;// "insert into riayati_response (ID,ENTITYID)  values ((select NVL(max(id),0) + 1 from riayati_response),'" + 123 + "')";
                        command.CommandType = CommandType.StoredProcedure;
                        res = command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {


            }
            return res;
        }
        int InsertData(string insertQuery)
        {
            int res = 0;
            try
            {
                using (System.Data.Common.DbConnection connection = new System.Data.OracleClient.OracleConnection(connectionstring))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = insertQuery;// "insert into riayati_response (ID,ENTITYID)  values ((select NVL(max(id),0) + 1 from riayati_response),'" + 123 + "')";
                        res = command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {


            }
            return res;
        }
        async void insertEauthElig_Getnew(dynamic Entity_ID)
        {
            var commandText = @"";
            //  commandText += " insert into getnew_response (ID,ENTITYID,ENTITYNAME,RECEIVERID,SENDERID,RECORDCOUNT,DOWNLOADED,DOWNLOADEDDATEGENERATEDSTRING,STATUS)  select (select NVL(max(id),0) + 1 from getnew_response),'" + Entity_ID.ID + "','EligEauth','" + Entity_ID.ReceiverID + "','" + Entity_ID.SenderID + "','" + Entity_ID.RecordCount + "','" + Entity_ID.Downloaded + "','','Pending'  from dual  where not exists(select * from getnew_response where entityid='" + Entity_ID.ID+"')";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";
            commandText += " insert into getnew_response (ID,ENTITYID,ENTITYNAME,RECEIVERID,SENDERID,RECORDCOUNT,DOWNLOADED,DOWNLOADEDDATEGENERATEDSTRING,STATUS,CREATIONDATE,TRANSACTIONDATE)  select (select NVL(max(id),0) + 1 from getnew_response),'" + Entity_ID.ID + "','EligEauth','" + Entity_ID.ReceiverID + "','" + Entity_ID.SenderID + "','" + Entity_ID.RecordCount + "','" + Entity_ID.Downloaded + "','','Pending',TO_DATE('" + Entity_ID.CreationDate + "','DD-MM-YYYY HH24:MI'),TO_DATE('" + Entity_ID.TransactionDate + "','DD-MM-YYYY HH24:MI') from dual ";// where not exists(select * from getnew_response where entityid='" + Entity_ID.ID + "');";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";

            //commandText = " insert into getnew_response (ID,ENTITYID,ENTITYNAME,RECEIVERID,SENDERID,STATUS)  select (select NVL(max(id),0) + 1 from getnew_response),'" + Entity_ID.ID + "','EligEauth','" + Entity_ID.ReceiverID + "','" + Entity_ID.SenderID +  "','Pending' from dual  where not exists(select * from getnew_response where entityid='" + Entity_ID.ID+"');";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";
            // commandText += " insert into getnew_response (ID,ENTITYID,ENTITYNAME,RECEIVERID,SENDERID,RECORDCOUNT,DOWNLOADED,DOWNLOADEDDATEGENERATEDSTRING,STATUS,CREATIONDATE,TRANSACTIONDATE)  select (select NVL(max(id),0) + 1 from getnew_response),'" + Entity_ID.ID + "','EligEauth','" + Entity_ID.ReceiverID + "','" + Entity_ID.SenderID + "','" + Entity_ID.RecordCount + "','" + Entity_ID.Downloaded + "','','Pending',TO_DATE('" + Entity_ID.CreationDate + "','DD-MM-YYYY HH24:MI'),TO_DATE('" + Entity_ID.TransactionDate + "','DD-MM-YYYY HH24:MI') from dual  where not exists(select * from getnew_response where entityid='" + Entity_ID.ID + "')";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";
            ; ;
            // commandText += " insert into riayati_response (ID,ENTITYID,ENTITYNAME)  values ((select NVL(max(id),0) + 1 from riayati_response),'" + Entity_ID + "','EligEauth');";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";
            //commandText += " END IF;  END;  ";


            int res = InsertData(commandText);

            if (res > 0)
            {
                Console.WriteLine("insertion for getnew entity id " + Entity_ID.ID);
                setdownloadedForEauthElig(Entity_ID.ID);

            }

        }

        async void insertClaim_Getnew(dynamic Entity_ID)
        {
            var commandText = @"";
            //  commandText += " insert into getnew_response (ID,ENTITYID,ENTITYNAME,RECEIVERID,SENDERID,RECORDCOUNT,DOWNLOADED,DOWNLOADEDDATEGENERATEDSTRING,STATUS)  select (select NVL(max(id),0) + 1 from getnew_response),'" + Entity_ID.ID + "','EligEauth','" + Entity_ID.ReceiverID + "','" + Entity_ID.SenderID + "','" + Entity_ID.RecordCount + "','" + Entity_ID.Downloaded + "','','Pending'  from dual  where not exists(select * from getnew_response where entityid='" + Entity_ID.ID+"')";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";
            commandText += " insert into getnew_response (ID,ENTITYID,ENTITYNAME,RECEIVERID,SENDERID,RECORDCOUNT,DOWNLOADED,DOWNLOADEDDATEGENERATEDSTRING,STATUS,CREATIONDATE,TRANSACTIONDATE)  select (select NVL(max(id),0) + 1 from getnew_response),'" + Entity_ID.ID + "','Claim','" + Entity_ID.ReceiverID + "','" + Entity_ID.SenderID + "','" + Entity_ID.RecordCount + "','" + Entity_ID.Downloaded + "','','Pending',TO_DATE('" + Entity_ID.CreationDate + "','DD-MM-YYYY HH24:MI'),TO_DATE('" + Entity_ID.TransactionDate + "','DD-MM-YYYY HH24:MI') from dual ";// where not exists(select * from getnew_response where entityid='" + Entity_ID.ID + "');";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";

            //commandText = " insert into getnew_response (ID,ENTITYID,ENTITYNAME,RECEIVERID,SENDERID,STATUS)  select (select NVL(max(id),0) + 1 from getnew_response),'" + Entity_ID.ID + "','EligEauth','" + Entity_ID.ReceiverID + "','" + Entity_ID.SenderID +  "','Pending' from dual  where not exists(select * from getnew_response where entityid='" + Entity_ID.ID+"');";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";
            // commandText += " insert into getnew_response (ID,ENTITYID,ENTITYNAME,RECEIVERID,SENDERID,RECORDCOUNT,DOWNLOADED,DOWNLOADEDDATEGENERATEDSTRING,STATUS,CREATIONDATE,TRANSACTIONDATE)  select (select NVL(max(id),0) + 1 from getnew_response),'" + Entity_ID.ID + "','EligEauth','" + Entity_ID.ReceiverID + "','" + Entity_ID.SenderID + "','" + Entity_ID.RecordCount + "','" + Entity_ID.Downloaded + "','','Pending',TO_DATE('" + Entity_ID.CreationDate + "','DD-MM-YYYY HH24:MI'),TO_DATE('" + Entity_ID.TransactionDate + "','DD-MM-YYYY HH24:MI') from dual  where not exists(select * from getnew_response where entityid='" + Entity_ID.ID + "')";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";
            ; ;
            // commandText += " insert into riayati_response (ID,ENTITYID,ENTITYNAME)  values ((select NVL(max(id),0) + 1 from riayati_response),'" + Entity_ID + "','EligEauth');";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";
            //commandText += " END IF;  END;  ";


            int res = InsertData(commandText);

            if (res > 0)
            {
                Console.WriteLine("insertion for getnew entity id " + Entity_ID.ID);
                setdownloadedForClaim(Entity_ID.ID);

            }

        }
        async void insertEauthElig_View(dynamic Obj)
        {
            int res = 0;
            try
            {
                var commandText = @"";
                commandText += " insert into view_response (ID,ENTITYTYPE,ENTITY_ID,ENTITYNAME,ENTITY_RECEIVERID,ENTITY_SENDERID,ENTITY_PAYERID,AUTHORIZATION_RESULT,AUTHORIZATION_IDPAYER,AUTHORIZATION_COMMENTS,STATUSCODE,MESSAGE,SUCCESS_STATUS,USERMESSAGE,ENTITY_START,ENTITY_END) ";
                commandText += " select (select NVL(max(id),0) + 1 from view_response),'Eligibility','" + Obj.Entity.Authorization.ID + "','Eligibility','" + Obj.Entity.Header.ReceiverID + "','" + Obj.Entity.Header.SenderID + "','" + Obj.Entity.Header.PayerID + "','" + Obj.Entity.Authorization.Result + "','" + Obj.Entity.Authorization.IDPayer + "','" + Obj.Entity.Authorization.Comments + "','" + Obj.StatusCode + "','" + Obj.Message + "','" + Obj.Success + "','" + Obj.UserMessage + "' , TO_DATE('" + Obj.Entity.Authorization.Start + "','DD-MM-YYYY HH24:MI'),TO_DATE('" + Obj.Entity.Authorization.Start + "','DD-MM-YYYY HH24:MI')  from dual   ";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        // commandText += " values( (select NVL(max(id),0) + 1 from view_response),'Eligibility','" + Obj.Entity.Authorization.ID + "','','','','','','','','','','','')      ";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        //commandText +=Environment.NewLine+ " select  * from view_response where ENTITY_ID='"+ Obj.Entity.Authorization.ID + "';";
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        //int res = InsertData(commandText);


                using (System.Data.Common.DbConnection connection = new System.Data.OracleClient.OracleConnection(connectionstring))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = commandText;// "insert into riayati_response (ID,ENTITYID)  values ((select NVL(max(id),0) + 1 from riayati_response),'" + 123 + "')";
                                                          //command.CommandText = @" insert into view_response (ID,ENTITYTYPE,ENTITY_ID,ENTITYNAME,ENTITY_RECEIVERID,ENTITY_SENDERID,ENTITY_PAYERID,AUTHORIZATION_RESULT,AUTHORIZATION_IDPAYER,AUTHORIZATION_COMMENTS,STATUSCODE,MESSAGE,SUCCESS_STATUS,USERMESSAGE)  select (select NVL(max(id),0) + 1 from view_response),'Eligibility','63330097232','Eligibility','Cerner','MohapTPA003','INS999','Yes','AV_1312222022133850','Aq_Direct_pr','200','Item Loaded Successfully','True','Item Loaded Successfully'   from dual  where not exists(select * from view_response where ENTITY_ID='63330097232') ";

                        //ommand.Parameters.Add("");


                        res = command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

            if (res > 0)
            {
                Console.WriteLine("insertion for getnew entity id " + Obj.ID);
                //setdownloadedForEauthElig(Entity_ID);

            }

        }
        async void insertEauth_View(dynamic Obj)
        {
            int res = 0;
            try
            {
                var commandText = @"";
                commandText += " insert into view_response (ID,ENTITYTYPE,ENTITY_ID,ENTITYNAME,ENTITY_RECEIVERID,ENTITY_SENDERID,ENTITY_PAYERID,AUTHORIZATION_RESULT,AUTHORIZATION_IDPAYER,AUTHORIZATION_COMMENTS,STATUSCODE,MESSAGE,SUCCESS_STATUS,USERMESSAGE,ENTITY_START,ENTITY_END) ";
                commandText += " select (select NVL(max(id),0) + 1 from view_response),'Eauth','" + Obj.Entity.Authorization.ID + "','Eauth','" + Obj.Entity.Header.ReceiverID + "','" + Obj.Entity.Header.SenderID + "','" + Obj.Entity.Header.PayerID + "','" + Obj.Entity.Authorization.Result + "','" + Obj.Entity.Authorization.IDPayer + "','" + Obj.Entity.Authorization.Comments + "','" + Obj.StatusCode + "','" + Obj.Message + "','" + Obj.Success + "','" + Obj.UserMessage + "' , TO_DATE('" + Obj.Entity.Authorization.Start + "','DD-MM-YYYY HH24:MI'),TO_DATE('" + Obj.Entity.Authorization.Start + "','DD-MM-YYYY HH24:MI')  from dual   ";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            // commandText += " values( (select NVL(max(id),0) + 1 from view_response),'Eligibility','" + Obj.Entity.Authorization.ID + "','','','','','','','','','','','')      ";// "INSERT INTO dept (deptno, dname, loc) VALUES (10,'Accounting','New York')";
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            //commandText +=Environment.NewLine+ " select  * from view_response where ENTITY_ID='"+ Obj.Entity.Authorization.ID + "';";
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            //int res = InsertData(commandText);


                using (System.Data.Common.DbConnection connection = new System.Data.OracleClient.OracleConnection(connectionstring))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = commandText;// "insert into riayati_response (ID,ENTITYID)  values ((select NVL(max(id),0) + 1 from riayati_response),'" + 123 + "')";
                                                          //command.CommandText = @" insert into view_response (ID,ENTITYTYPE,ENTITY_ID,ENTITYNAME,ENTITY_RECEIVERID,ENTITY_SENDERID,ENTITY_PAYERID,AUTHORIZATION_RESULT,AUTHORIZATION_IDPAYER,AUTHORIZATION_COMMENTS,STATUSCODE,MESSAGE,SUCCESS_STATUS,USERMESSAGE)  select (select NVL(max(id),0) + 1 from view_response),'Eligibility','63330097232','Eligibility','Cerner','MohapTPA003','INS999','Yes','AV_1312222022133850','Aq_Direct_pr','200','Item Loaded Successfully','True','Item Loaded Successfully'   from dual  where not exists(select * from view_response where ENTITY_ID='63330097232') ";

                        //ommand.Parameters.Add("");


                        res = command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

            if (res > 0)
            {
                Console.WriteLine("insertion for getnew entity id " + Obj.ID);
                //setdownloadedForEauthElig(Entity_ID);

            }

        }
        async void setdownloadedForClaim(string Entity_ID)
        {
            HttpClient clientSetdownload = new HttpClient { BaseAddress = new Uri(riayatiUrl) };
            clientSetdownload.DefaultRequestHeaders.Accept.Clear();
            clientSetdownload.DefaultRequestHeaders.Add("Username", riayatiuser);
            clientSetdownload.DefaultRequestHeaders.Add("Password", riayatipass);
            var parameters = new Dictionary<string, string> { { "Id", Entity_ID } }; // Dictionary type string,string
            var urlParams = new FormUrlEncodedContent(parameters);
            // HttpResponseMessage responseSetdownload = await clientSetdownload.PostAsync("Authorization/SetDownloaded", urlParams);
            var responseSetdownload = Task.Run(async () => await clientSetdownload.PostAsync("Claim/SetDownloaded", urlParams)).Result;


            // Cast the response content to your object using the method response.Content.ReadAsAsync
            var data = responseSetdownload.Content.ReadAsStringAsync().Result;
        }

        async void setdownloadedForEauthElig(string Entity_ID)
        {
            HttpClient clientSetdownload = new HttpClient { BaseAddress = new Uri(riayatiUrl) };
            clientSetdownload.DefaultRequestHeaders.Accept.Clear();
            clientSetdownload.DefaultRequestHeaders.Add("Username", riayatiuser);
            clientSetdownload.DefaultRequestHeaders.Add("Password", riayatipass);
            var parameters = new Dictionary<string, string> { { "Id", Entity_ID } }; // Dictionary type string,string
            var urlParams = new FormUrlEncodedContent(parameters);
            // HttpResponseMessage responseSetdownload = await clientSetdownload.PostAsync("Authorization/SetDownloaded", urlParams);
            var responseSetdownload = Task.Run(async () => await clientSetdownload.PostAsync("Authorization/SetDownloaded", urlParams)).Result;


            // Cast the response content to your object using the method response.Content.ReadAsAsync
            var data = responseSetdownload.Content.ReadAsStringAsync().Result;
        }

        DataTable GetdatafromQuery(string query)
        {

            DataTable dt = new DataTable();
            try
            { 
            using (System.Data.Common.DbConnection connection = new System.Data.OracleClient.OracleConnection(connectionstring))
            {
                connection.Open();
                OracleCommand cmd = null;
                OracleDataReader reader = null;


                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;// "insert into riayati_response (ID,ENTITYID)  values ((select NVL(max(id),0) + 1 from riayati_response),'" + 123 + "')";
                                                // var z = command.ExecuteNonQuery();
                    var x = command.ExecuteReader();

                    dt = new DataTable();
                    dt.Load(x);
                }
                connection.Close();
            }



            }
            catch(Exception ex)
            {
                Console.WriteLine("query error" + query);
                Console.WriteLine(ex.Message + ex.InnerException);

            }



            return dt;
        }
        public class RCMConfig
        {

            public string ConnectionString { get; set; }
            public string SetupType { get; set; }


        }
        public class Entity
        {
            public string ID { get; set; }
            public string SenderID { get; set; }
            public string ReceiverID { get; set; }
            public int RecordCount { get; set; }
            public string TransactionDate { get; set; }
            public string CreationDate { get; set; }
            public bool Downloaded { get; set; }
            public string DownloadedDateGeneratedString { get; set; }
        }

        public class Root
        {
            public List<Entity> Entities { get; set; }
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public bool Success { get; set; }
            public string UserMessage { get; set; }
            public List<object> Error { get; set; }
        }

    }
}