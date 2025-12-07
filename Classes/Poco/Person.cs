namespace TrackerDotNet.Classes.Poco
{
    public class Person
    {
        public int PersonID { get; set; }
        public string PersonName { get; set; }
        public string Abbreviation { get; set; }
        public bool? Enabled { get; set; }
        public int? NormalDeliveryDoW { get; set; }
        public string SecurityUsername { get; set; }
    }
}
