using SteamKit2.Internal;

namespace ASFItemDropManager
{
    class StoredResponse
    {
        public bool Success { get; set; }
        public CMsgClientGetUserStatsResponse? Response { get; set; }
    }
}
