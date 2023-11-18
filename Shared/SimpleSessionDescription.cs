namespace CBC.Server;
   public class SessionDescription
{
    public string type { get; set; }
    public string sdp { get; set; }

    public override string ToString()
    {
        return $"Type: {type}, SDP: {sdp}";
    }
}