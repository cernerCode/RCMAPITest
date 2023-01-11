namespace RCMAPITest
{
    public class Eligibility
    {

    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Authorization:Elig_Req
    {
        public string Type { get; set; }
        public string ID { get; set; }
        public string RequestType { get; set; }
        public string Gender { get; set; }
        public string MemberID { get; set; }
        public string EmiratesIDNumber { get; set; }
        public string DateOrdered { get; set; }
        public string DateOfBirth { get; set; }
        public Encounter Encounter { get; set; }
    }

    public class Encounter
    {
        public string FacilityID { get; set; }
        public int Type { get; set; }
    }

    public class Header
    {
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public string TransactionDate { get; set; }
        public int RecordCount { get; set; }
        public string DispositionFlag { get; set; }
        public string PayerID { get; set; }
    }

    public class PriorRequest
    {
        public Header Header { get; set; }
        public Authorization Authorization { get; set; }
    }

    public class Elig_Req
    { 
        public PriorRequest PriorRequest { get; set; }
    }


}
