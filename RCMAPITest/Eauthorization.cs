namespace RCMAPITest
{
    public class Eauthorization
    {
    }
    public class Activity
    {
        public string ID { get; set; }
        public string ActivityReference { get; set; }
        public string Start { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public string Code { get; set; }
        public string Quantity { get; set; }
        public string Unit { get; set; }
        public string Net { get; set; }
        public string Clinician { get; set; }
        public string Duration { get; set; }
        public List<Observation> Observation { get; set; }
    }

    public class Diagnosis
    {
        public string Type { get; set; }
        public string Code { get; set; }
        public List<DxInfo> DxInfo { get; set; }
    }

    public class DxInfo
    {
        public string Type { get; set; }
        public string Code { get; set; }
    }


    public class Observation
    {
        public string Type { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
    }
    //public class Authorization
    //{
    //    public string Type { get; set; }
    //    public string ID { get; set; }
    //    public string RequestType { get; set; }
    //    public string Gender { get; set; }
    //    public string RequestReferenceNumber { get; set; }
    //    public string MemberID { get; set; }
    //    public string Weight { get; set; }
    //    public string EmiratesIDNumber { get; set; }
    //    public string DateOrdered { get; set; }
    //    public string DateOfBirth { get; set; }
    //    public Encounter Encounter { get; set; }
    //    public List<Diagnosis> Diagnosis { get; set; }
    //    public List<Activity> Activity { get; set; }
    //}

}
