using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using ArchiSteamFarm;
using SteamKit2;
using SteamKit2.Internal;

namespace ASFItemDropManager
{

    public sealed class ItemDropHandler : ClientMsgHandler
    {
        private SteamUnifiedMessages.UnifiedService<IInventory>? _inventoryService;
        private SteamUnifiedMessages.UnifiedService<IPlayer>? _PlayerService;

        ConcurrentDictionary<ulong, StoredResponse> Responses = new ConcurrentDictionary<ulong, StoredResponse>();



        public override void HandleMsg(IPacketMsg packetMsg)
        {
            var handler = Client.GetHandler<SteamUnifiedMessages>();

            if (packetMsg == null)
            {
                ASF.ArchiLogger.LogNullError(nameof(packetMsg));

                return;
            }

            switch (packetMsg.MsgType)
            {
                case EMsg.ClientGetUserStatsResponse:
                    break;
                case EMsg.ClientStoreUserStatsResponse:
                    break;
            }

        }






        internal string itemIdleingStart(Bot bot, uint appid)
        {
            ClientMsgProtobuf<CMsgClientGamesPlayed> response = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
            response.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = new GameID(appid),
                steam_id_gs = bot.SteamID
                //  steam_id_for_user = bot.SteamID

            });

            Client.Send(response);
            return "Start idling for " + appid;
        }

        internal string itemIdleingStop(Bot bot)
        {
            ClientMsgProtobuf<CMsgClientGamesPlayed> response = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
            {
                response.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed { game_id = 0 });
            }

            Client.Send(response);
            return "Stop idling ";
        }

        internal async Task<string> checkTime(uint appid, uint itemdefid, Bot bot, bool longoutput)
        {

            var steamUnifiedMessages = Client.GetHandler<SteamUnifiedMessages>();
            if (steamUnifiedMessages == null)
            {
                bot.ArchiLogger.LogNullError(nameof(steamUnifiedMessages));
                return "SteamUnifiedMessages Error";
            }

            CInventory_ConsumePlaytime_Request playtimeRequest = new CInventory_ConsumePlaytime_Request { appid = appid, itemdefid = itemdefid };
            _inventoryService = steamUnifiedMessages.CreateService<IInventory>();
            var playtimeResponse = await _inventoryService.SendMessage(x => x.ConsumePlaytime(playtimeRequest));
            var resultGamesPlayed = playtimeResponse.GetDeserializedResponse<CInventory_Response>();


            if (resultGamesPlayed.item_json == null) bot.ArchiLogger.LogGenericWarning(message: $"{resultGamesPlayed.item_json}");
            if (resultGamesPlayed == null) bot.ArchiLogger.LogNullError("resultGamesPlayed");


            CPlayer_GetOwnedGames_Request gamesOwnedRequest = new CPlayer_GetOwnedGames_Request { steamid = bot.SteamID,  include_appinfo = true, include_free_sub= true, include_played_free_games=true };
            _PlayerService = steamUnifiedMessages.CreateService<IPlayer>();
            var ownedReponse = await _PlayerService.SendMessage(x => x.GetOwnedGames(gamesOwnedRequest));
            var consumePlaytime = ownedReponse.GetDeserializedResponse<CPlayer_GetOwnedGames_Response>();
            consumePlaytime.games.ForEach(action => bot.ArchiLogger.LogGenericInfo(message: $"{action.appid} - {action.has_community_visible_stats} - {action.name} - {action.playtime_forever}"));
            var resultFilteredGameById = consumePlaytime.games.Find(game => game.appid ==  ((int)appid) );

            if (consumePlaytime.games == null) bot.ArchiLogger.LogNullError(nameof(consumePlaytime.games));
            if (resultFilteredGameById == null) bot.ArchiLogger.LogNullError("resultFilteredGameById ");

            var appidPlaytimeForever = 0;
            if (resultGamesPlayed != null && resultFilteredGameById != null)
            {
                bot.ArchiLogger.LogGenericDebug(message: $"Playtime for {resultFilteredGameById.name} is: {resultFilteredGameById.playtime_forever}");
                appidPlaytimeForever = resultFilteredGameById.playtime_forever;
            }

            // proceed only when the player has played the request game id
            if (resultGamesPlayed != null && resultGamesPlayed.item_json != "[]")
            {
                try
                {
                    var summstring = "";

                    foreach (var item in QuickType.ItemList.FromJson(resultGamesPlayed.item_json))
                    {
                        if (longoutput)
                        {
                            summstring += $"Item drop @{item.StateChangedTimestamp} => i.ID: {appid}_{item.Itemid}, i.Def: {item.Itemdefid} (a.PT: {appidPlaytimeForever}m)";
                        }
                        else
                        {
                            summstring += $"Item drop @{item.StateChangedTimestamp}";
                        }
                    }
                    return summstring;
                }
                catch (Exception e)
                {
                    bot.ArchiLogger.LogGenericError(message: e.Message);
                    return "Error while parse consumePlaytime";
                }

            }
            else
            {

                if (longoutput)
                {
                    return $"No item drop for game '{resultFilteredGameById.name}' with playtime {appidPlaytimeForever}m.";
                }
                else
                {
                    return $"No item drop.";
                }
            }
        }

        internal async Task<string> checkPlaytime(uint appid, Bot bot)
        {

            var steamUnifiedMessages = Client.GetHandler<SteamUnifiedMessages>();
            if (steamUnifiedMessages == null)
            {
                bot.ArchiLogger.LogNullError(nameof(steamUnifiedMessages));
                return "SteamUnifiedMessages Error";
            }

            CPlayer_GetOwnedGames_Request gamesOwnedRequest = new CPlayer_GetOwnedGames_Request { steamid = bot.SteamID, include_appinfo = true, include_free_sub= true, include_played_free_games=true };
            _PlayerService = steamUnifiedMessages.CreateService<IPlayer>();
            var ownedReponse = await _PlayerService.SendMessage(x => x.GetOwnedGames(gamesOwnedRequest));
            var consumePlaytime = ownedReponse.GetDeserializedResponse<CPlayer_GetOwnedGames_Response>();
            consumePlaytime.games.ForEach(action => bot.ArchiLogger.LogGenericInfo(message: $"{action.appid} - {action.has_community_visible_stats} - {action.name} - {action.playtime_forever}"));
            var resultFilteredGameById = consumePlaytime.games.Find(game => game.appid == ((int)appid) );

            if (consumePlaytime.games == null) bot.ArchiLogger.LogNullError(nameof(consumePlaytime.games));
            if (resultFilteredGameById == null) bot.ArchiLogger.LogNullError("resultFilteredGameById");

            uint appidPlaytimeForever = 0;
            bot.ArchiLogger.LogGenericDebug(message: $"Playtime for {resultFilteredGameById.name} is: {resultFilteredGameById.playtime_forever}");
            appidPlaytimeForever = Convert.ToUInt32(resultFilteredGameById.playtime_forever);
            uint appidPlaytimeHours = appidPlaytimeForever / 60;
            byte appidPlaytimeMinutes = Convert.ToByte(appidPlaytimeForever % 60);

            var summstring = "";
            summstring += $"Playtime for game '{resultFilteredGameById.name}' is {appidPlaytimeForever}m = {appidPlaytimeHours}h {appidPlaytimeMinutes}m";

            return summstring;
        }

        internal string itemDropDefList(Bot bot)
        {
            ClientMsgProtobuf<CMsgClientGamesPlayed> response = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            string IDDL_File = @"plugins\\ASFItemDropper\\idropdeflist.txt";
            string idropdeflist_txt = "";

            bool fileExists = File.Exists(IDDL_File);

            if (fileExists)
            {
                idropdeflist_txt = "\n";
                idropdeflist_txt += System.IO.File.ReadAllText(IDDL_File);
            }
            else
            {
                idropdeflist_txt = "## INFO: File 'idropdeflist.txt' does not exist.";
            }

            return idropdeflist_txt;
        }

    }

}
