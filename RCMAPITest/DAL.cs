using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace RCMAPITest
{
    public class DAL
    {
        string conString = @"User Id=cert_admin;Password=changeme22;  
             (DESCRIPTION =
    (ADDRESS_LIST =
      (ADDRESS = (PROTOCOL = TCP)(HOST = 10.2.46.237)(PORT = 1560))
    )
    (CONNECT_DATA =
      (SERVICE_NAME = certdb)
    )
  )";       //cert db
       // string connectionstring = "Data Source = 10.2.46.237:1560/certdb; User ID = cert_admin; Password = changeme22";
       //prod db
        string connectionstring = "Data Source = cerrcmpdb-scan.moh.ae:2066/certdb; User ID = cert_admin; Password = changeme2022";
        string Elig_auth_table = "ELIG_AUTH_REQUEST";
        string Claims_table = "Claims_Request";
        public int InsertData(string insertQuery)
        {
            int res = 0;
            using (System.Data.Common.DbConnection connection = new System.Data.OracleClient.OracleConnection(connectionstring))
            {
                 connection.Open();

                 using (var command = connection.CreateCommand())
                {
                    command.CommandText = insertQuery;// "insert into riayati_response (ID,ENTITYID)  values ((select NVL(max(id),0) + 1 from riayati_response),'" + 123 + "')";
                   res = command.ExecuteNonQuery();
                }
            }
            return res;
        }
        public string InsertQueryForClaim(dynamic obj)
        {
            dynamic eligreq = Newtonsoft.Json.JsonConvert.DeserializeObject(obj.ToString());
            // var eligType = Authreq.PriorRequest.Authorization.Type;
            string Query = "insert into " + Claims_table + " (ID,HEADERSENDERID,HEADERRECEIVERID,HEADERTRANSACTIONDATE,HEADERRECORDCOUNT,HEADERDISPOSITIONFLAG,HEADERPAYERID,CLAIM_ID,CLAIM_IDPAYER,CLAIM_TYPE,CLAIM_MEMBERID,CLAIM_PROVIDERID,CLAIM_NATIONALIDNUMBER,CLAIM_GENDER,CLAIM_DATEOFBIRTH,CLAIM_ENCOUNTERFACILITYID,CLAIM_ENCOUNTERTYPE,CLAIM_ENCOUNTERPATIENTID,CLAIM_ENCOUNTERSTART,CLAIM_ENCOUNTEREND,CLAIM_ENCOUNTERSTARTTYPE,CREATEDBY,CREATIONDATE,UPDATEDBY,UPDATEDATE)";
            Query += " VALUES ((select NVL(max(id),0) + 1 from "+Claims_table+") ";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.ID + "'";
            Query += "  ,'Eligibility'";
            Query += "  ,'" + eligreq.PriorRequest.Header.SenderID + "'";
            Query += "  ,'" + eligreq.PriorRequest.Header.ReceiverID + "'";
            Query += "  ,to_date('" + eligreq.PriorRequest.Authorization.DateOrdered + "', 'DD/MM/YYYY hh24:mi')";
            Query += "  ,'" + eligreq.PriorRequest.Header.RecordCount + "'";
            Query += "  ,'" + eligreq.PriorRequest.Header.DispositionFlag + "'";
            Query += "  ,'" + eligreq.PriorRequest.Header.PayerID + "'";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.Type + "'";

            Query += "  ,'" + eligreq.PriorRequest.Authorization.ID + "'";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.RequestType + "'";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.Gender + "'";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.MemberID + "'";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.EmiratesIDNumber + "'";
            Query += "  ,to_date('" + eligreq.PriorRequest.Authorization.DateOrdered + "', 'DD/MM/YYYY hh24:mi')";
            Query += "  ,to_date('" + eligreq.PriorRequest.Authorization.DateOfBirth + "', 'DD/MM/YYYY hh24:mi')";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.Encounter.FacilityID + "'";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.Encounter.Type + "'";
            Query += "  ,'OL Server'";
            Query += "  , to_date(to_char(sysdate,'DD/MM/YYYY HH:MI'), 'DD/MM/YYYY hh24:mi:ss')";
            Query += "  ,'OL SERVER'";
            Query += "  ,to_date(to_char(sysdate,'DD/MM/YYYY HH:MI'), 'DD/MM/YYYY hh24:mi:ss'))";
            var res = InsertData(Query);
            return Query;

        }
        public string InsertQueryForEligAuth(dynamic obj)
        {
            dynamic eligreq = Newtonsoft.Json.JsonConvert.DeserializeObject(obj.ToString());
            // var eligType = Authreq.PriorRequest.Authorization.Type;
            string Query = "insert into " + Elig_auth_table + " (ID,ENTITYID,ENTITYNAME,PRIORREQUESTHEADERSENDERID,PRIORREQUESTHEADERRECEIVERID,PRIORREQUESTHEADERTRANSACTIONDATE,PRIORREQUESTHEADERRECORDCOUNT,PRIORREQUESTHEADERDISPOSITIONFLAG,PRIORREQUESTHEADERPAYERID,PRIORREQUESTAUTHORIZATIONTYPE,PRIORREQUESTAUTHORIZATIONID,PRIORREQUESTAUTHORIZATIONREQUESTTYPE,PRIORREQUESTAUTHORIZATIONGENDER,PRIORREQUESTAUTHORIZATIONMEMBERID,PRIORREQUESTAUTHORIZATIONEMIRATESIDNUMBER,PRIORREQUESTAUTHORIZATIONDATEORDERED,PRIORREQUESTAUTHORIZATIONDATEOFBIRTH,PRIORREQUESTAUTHORIZATIONENCOUNTERFACILITYID,PRIORREQUESTAUTHORIZATIONENCOUNTERTYPE,CREATEDBY,CREATEDATE,UPDATEDBY,UPDATEDATE)";
            Query += " VALUES ((select NVL(max(id),0) + 1 from elig_auth_request) ";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.ID + "'";
            Query += "  ,'Eligibility'";
            Query += "  ,'" + eligreq.PriorRequest.Header.SenderID + "'";
            Query += "  ,'" + eligreq.PriorRequest.Header.ReceiverID + "'";
            Query += "  ,to_date('" + eligreq.PriorRequest.Authorization.DateOrdered + "', 'DD/MM/YYYY hh24:mi')";
            Query += "  ,'" + eligreq.PriorRequest.Header.RecordCount + "'";
            Query += "  ,'" + eligreq.PriorRequest.Header.DispositionFlag + "'";
            Query += "  ,'" + eligreq.PriorRequest.Header.PayerID + "'";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.Type + "'";

            Query += "  ,'" + eligreq.PriorRequest.Authorization.ID + "'";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.RequestType + "'";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.Gender + "'";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.MemberID + "'";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.EmiratesIDNumber + "'";
            Query += "  ,to_date('" + eligreq.PriorRequest.Authorization.DateOrdered + "', 'DD/MM/YYYY hh24:mi')";
            Query += "  ,to_date('" + eligreq.PriorRequest.Authorization.DateOfBirth + "', 'DD/MM/YYYY hh24:mi')";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.Encounter.FacilityID + "'";
            Query += "  ,'" + eligreq.PriorRequest.Authorization.Encounter.Type + "'";
            Query += "  ,'OL Server'";
            Query += "  , to_date(to_char(sysdate,'DD/MM/YYYY HH:MI'), 'DD/MM/YYYY hh24:mi:ss')";
            Query += "  ,'OL SERVER'";
            Query += "  ,to_date(to_char(sysdate,'DD/MM/YYYY HH:MI'), 'DD/MM/YYYY hh24:mi:ss'))";
            var res = InsertData(Query);
            return Query;

        }
    }
}
