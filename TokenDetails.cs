namespace InfraLib
{
    public class TokenDetails
    {
        public string access_token { get; set; }


        public int expires_in { get; set; }

        public DateTime expires_in_UTC { get; set; }

        public string issued_at { get; set; }

        public DateTime issued_at_UTC { get; set; }

        public string status { get; set; }

        public string tokenType { get; set; }
    }
}
