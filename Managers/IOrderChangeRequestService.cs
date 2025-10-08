using TrackerDotNet.Classes;

namespace TrackerDotNet.Managers
{
    public interface IOrderChangeRequestService
    {
        ChangeRequestResult Submit(ChangeRequestSubmission submission);
    }

    public class ChangeRequestResult
    {
        public bool Success { get; set; }
        public string Reference { get; set; }
        public string Message { get; set; }
    }
}