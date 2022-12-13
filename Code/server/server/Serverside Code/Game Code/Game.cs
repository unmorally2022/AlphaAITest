using System;
using System.Collections.Generic;
using PlayerIO.GameLibrary;
using System.Linq;

namespace AlphaAIServer
{
    public class Player : BasePlayer
    {
        public int Score = 0;
        public int iType = 0;
        public bool isReady = false;
        public string name = string.Empty;
        public string UserId = string.Empty;
        public int SpawnPointIndex = 0;
        //public int CurrentIndex = -1;//the index taken in the array of joined player
    }

    public class Coin {
        public int id;
        public int SpawnIndex;
        public int isSpawned;
    }

    [RoomType("Death Match")]
    public class GameCode : Game<Player>
    {
        private int last_coin_id = 0;
        private Coin[] Coins;

        //private List<string> PlayersJoinedId;
        //private string[] PlayersJoinedId;//, PlayersJoinedName;
        //private int PlayersJoinedCount;

        private int SeccondWaitingForOtherPlayer;
        private int SeccondToStart;
        private int SeccondInGameLeft;

        private Timer timerWaitingOtherPlayer;
        private Timer timerStartingGame;
        private Timer timerInGame;

        private bool RoomClosed = false;

        // This method is called when an instance of your the game is created
        public override void GameStarted()
        {
            // anything you write to the Console will show up in the 
            // output window of the development server
            Console.WriteLine("Game is started: " + RoomId);

            //PlayersJoinedId = new List<string>();
            //PlayersJoinedName = new string[4] { "", "", "", "" };
            //PlayersJoinedCount = 0;
            SeccondToStart = 3;

            //// spawn 10 toads at server start
            //System.Random random = new System.Random();
            Coins = new Coin[20];
            for (int x = 0; x < 20; x++)
            {                
                Coin coin = new Coin();
                coin.id = last_coin_id;
                coin.SpawnIndex = x;
                coin.isSpawned = 1;
                Coins[x] = coin;
                last_coin_id++;
            }

            //// respawn new toads each 5 seconds
            //AddTimer(respawntoads, 5000);
            //// reset game every 2 minutes
            //AddTimer(resetgame, 120000);
            SeccondWaitingForOtherPlayer = 10;
            timerWaitingOtherPlayer = AddTimer(WaitingOtherPlayer, 1000);
            //timerStartingGame = AddTimer(StartGame, 1000);
            //timerInGame = AddTimer(WaitingOtherPlayer, 10000);


        }

        private void WaitingOtherPlayer()
        {
            if (SeccondWaitingForOtherPlayer <= 1) {
                int isReadyCount = 0;

                foreach (Player pl in Players)
                {                   
                    //send others userid to every one except it self
                    if (pl.isReady == true && pl.UserId != string.Empty)
                    {
                        isReadyCount++;
                    }
                }

                if (isReadyCount > 1)
                {
                    if (timerWaitingOtherPlayer != null)
                        timerWaitingOtherPlayer.Stop();

                    timerStartingGame = AddTimer(StartGame, 1000);
                }
                else
                {
                    SeccondWaitingForOtherPlayer = 10;
                    Broadcast("WaitingOtherPlayer", SeccondWaitingForOtherPlayer);
                }
            } else {
                SeccondWaitingForOtherPlayer--;
                Broadcast("WaitingOtherPlayer", SeccondWaitingForOtherPlayer);                
            }

        }

        private void StartGame() {
            RoomClosed = true;
            if (SeccondToStart <= 0)
            {
                if (timerStartingGame != null)
                {
                    timerStartingGame.Stop();
                }

                //set room visibility to false
                Visible = false;

                //randomize coin position
                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        int tempPos = Coins[i].SpawnIndex;
                        Coins[i].SpawnIndex = Coins[j].SpawnIndex;
                        Coins[j].SpawnIndex = tempPos;
                    }
                }

                for (int i = 0; i < 10; i++)
                {                    
                    Broadcast("Coin", Coins[i].id, Coins[i].SpawnIndex, Coins[i].isSpawned);
                }

                Broadcast("StartGame");

                SeccondInGameLeft = 120;
                timerInGame = AddTimer(InGameCountDown, 1000);
            }
            else {
                Broadcast("StartingGame", SeccondToStart);
                SeccondToStart--;                
            }
        }

        private void InGameCountDown() {
            
            SeccondInGameLeft--;
            if (SeccondInGameLeft <= 0)
            {
                if (timerInGame != null)
                    timerInGame.Stop();
                EndGame();
            }
            else {
                Broadcast("InGameCountDown", SeccondInGameLeft);
            }
        }

        private void EndGame() {
            Broadcast("GameFinish");
            var ordered = Players.OrderByDescending(o => o.Score);
            int index = 0;
            int star = 3;
            foreach (Player pl in ordered)
            {
                Broadcast("GameResult", index, star, pl.UserId, pl.name, pl.Score);
                index++;
                if (star > 0)
                    star--;
            }

        }

        //This method is called before a user joins a room.
        //If you return false, the user is not allowed to join.
        public override bool AllowUserJoin(Player player)
        {            
            if (RoomClosed)
                return false;
            else
                return true;
        }

        

        // This method is called when the last player leaves the room, and it's closed down.
        public override void GameClosed()
        {
            Console.WriteLine("Game Closed RoomId: " + RoomId);
        }

        // This method is called whenever a player joins the game
        public override void UserJoined(Player player)
        {
            player.UserId = player.ConnectUserId;            
            player.name = player.JoinData["userName"];
            player.isReady = false;
            Random rand = new Random();
            player.SpawnPointIndex = rand.Next(0, 4);
        }

        // This method is called when a player leaves the game
        public override void UserLeft(Player player)
        {
            //PlayersJoinedCount--;
            Broadcast("PlayerLeft", player.ConnectUserId);
            //for (int i = 0; i < Players.Count(); i++)
            //{
            //    if (PlayersJoinedId[i] == player.ConnectUserId)
            //    {
            //        //player.CurrentIndex = i;
            //        PlayersJoinedId[i] = "";
            //        break;
            //    }
            //}
            if (Players.Count() <= 0) {
                foreach (Player p in Players)
                {
                    p.Disconnect();
                }
            }
            //if (PlayersJoinedId[0] == string.Empty && PlayersJoinedId[1] == string.Empty && PlayersJoinedId[2] == string.Empty && PlayersJoinedId[3] == string.Empty)
            //{
            //    foreach (Player p in Players)
            //    {
            //        p.Disconnect();
            //    }
            //}
            //	GameClosed();
        }

        // This method is called when a player sends a message into the server code
        public override void GotMessage(Player player, Message message)
        {
            switch (message.Type)
            {
                //debug purpose
                case "test1":
                    Random rand = new Random();                    
                    player.Score = rand.Next(0, 10);
                    player.Send("UpdateScore", player.Score);
                    break;
                case "test2":
                    SeccondInGameLeft = 0;
                    break;
                case "test3":

                    break;
                //debug purpose end
                case "RequestPlayers":
                    //tell others joined player that player is joined
                    foreach (Player pl in Players)
                    {
                        foreach (Player pl1 in Players)
                        {
                            //send others userid to every one except it self
                            if (pl.UserId != pl1.UserId && pl1.UserId != string.Empty)
                            {
                                pl.Send("RequestPlayers", pl1.UserId, pl1.name, pl1.isReady, pl1.iType, pl1.SpawnPointIndex);
                            }
                        }
                    }
                    break;
                case "SetReady":
                    player.isReady = message.GetBoolean(1);
                    player.iType = message.GetInt(0);
                    Broadcast("SetReady", player.ConnectUserId, player.iType, player.isReady, player.SpawnPointIndex);
                    //pl.Send  ("OthersJoined", pl1.UserId, pl1.name, pl1.isReady, pl1.itype, pl.SpawnPointIndex);
                    break;
                case "Move":
                    Broadcast("Move",
                        player.ConnectUserId,
                        message.GetFloat(0), message.GetFloat(1), message.GetFloat(2),
                        message.GetFloat(3), message.GetFloat(4), message.GetFloat(5),
                        message.GetFloat(6),//forward
                        message.GetFloat(7),//turn
                        message.GetBoolean(8),//ongground
                        message.GetFloat(9)
                        );
                    break;                
                case "CharacterState":
                    Broadcast("CharacterState", player.ConnectUserId, message.GetFloat(0));
                    break;
                case "CoinHit":
                    Player _player = Players.FirstOrDefault(c => c.UserId.Equals(player.ConnectUserId));
                    if (_player != null) {
                        _player.Score++;
                        player.Send("UpdateScore", player.Score);
                    }
                    Broadcast("CoinHit", message.GetInt(0), message.GetInt(1));
                    break;
                case "SendChat":
                    Broadcast("SendChat", player.ConnectUserId, message.GetString(0));
                    break;
            }
        }
    }
}