using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ArchiSteamFarm;
using ArchiSteamFarm.Localization;
using SteamKit2;
using SteamKit2.Internal;
using Newtonsoft.Json;

namespace ASFItemDropManager
{

    public sealed class ItemDropHandler : ClientMsgHandler
    {
        private SteamUnifiedMessages.UnifiedService<IInventory> _inventoryService;
        private SteamUnifiedMessages.UnifiedService<IPlayer> _PlayerService;

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

        internal async Task<string> checkTime(uint appid, uint itemdefid, Bot bot)
        {
            CInventory_ConsumePlaytime_Request playtimeResponse = new CInventory_ConsumePlaytime_Request { appid = appid, itemdefid = itemdefid };
            CPlayer_GetOwnedGames_Request gamesOwnedRequest = new CPlayer_GetOwnedGames_Request { steamid = bot.SteamID };

            var steamUnifiedMessages = Client.GetHandler<SteamUnifiedMessages>();

            _inventoryService = steamUnifiedMessages.CreateService<IInventory>();
            _PlayerService = steamUnifiedMessages.CreateService<IPlayer>();

            var consumePlaytimeResponse = await _inventoryService.SendMessage(x => x.ConsumePlaytime(playtimeResponse));
            var consumePlaytime = consumePlaytimeResponse.GetDeserializedResponse<CInventory_Response>();

            var ownedReponse = await _PlayerService.SendMessage(x => x.GetOwnedGames(gamesOwnedRequest));
            var resultGamesPlayed = consumePlaytimeResponse.GetDeserializedResponse<CPlayer_GetOwnedGames_Response>();
            var resultFilteredGameById = resultGamesPlayed.games.Find(game => game.appid == appid);
            var appidPlaytimeForever = 0;
            if (resultGamesPlayed != null && resultFilteredGameById != null)
            {
                appidPlaytimeForever = resultFilteredGameById.playtime_forever;
            }


            if (consumePlaytime.item_json != "[]")
            {
                try
                {
                    Console.WriteLine(consumePlaytime.item_json);
                    var summstring = "";

                    foreach (var item in QuickType.ItemList.FromJson(consumePlaytime.item_json))
                    {
                        summstring += $"Item drop @ {item.StateChangedTimestamp} => i.ID: {appid}_{item.Itemid}, i.Def: {item.Itemdefid} (playtime: {appidPlaytimeForever})";
                    }
                    return summstring;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
            return $"No item drop for game {appid} with playtime {appidPlaytimeForever}.";
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