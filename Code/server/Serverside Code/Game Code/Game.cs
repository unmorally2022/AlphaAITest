using System;
using System.Collections.Generic;
using PlayerIO.GameLibrary;
using System.Linq;

namespace AlphaAIServer
{
    public class Player : BasePlayer
    {
        public int Score = 0;
        public int itype = 0;
        public bool isReady = false;
        public string name = string.Empty;
        public string UserId = string.Empty;
        public int CurrentIndex = -1;//the index taken in the array of joined player
    }

    //public class Toad {
    //	public int id = 0;
    //	public float posx = 0;
    //	public float posz = 0;
    //}

    [RoomType("Death Match")]
    public class GameCode : Game<Player>
    {
        //private int last_toad_id = 0;
        //private List<Toad> Toads = new List<Toad>();

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
            //for(int x = 0; x < 10; x++) {

            //	int px = random.Next(-9, 9);
            //	int pz = random.Next(-9, 9);
            //	Toad temp = new Toad();
            //	temp.id = last_toad_id;
            //	temp.posx = px;
            //	temp.posz = pz;
            //	Toads.Add(temp);
            //	last_toad_id++;

            //}

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

                Broadcast("StartGame", 1);
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
            // scoring system
            Player winner = new Player();
            int maxscore = -1;
            foreach (Player pl in Players)
            {
                if (pl.Score > maxscore)
                {
                    winner = pl;
                    maxscore = pl.Score;
                }
            }

            // broadcast who won the round
            if (winner.Score > 0)
            {
                Broadcast("GameWinner", winner.UserId);
            }
            else
            {
                Broadcast("GameDraw");
            }

        }

        //This method is called before a user joins a room.
        //If you return false, the user is not allowed to join.
        public override bool AllowUserJoin(Player player)
        {
            //if (Players.Count() < 10 && !RoomClosed)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            return true;
        }

        //private void resetgame() {
        //	// scoring system
        //	Player winner = new Player();
        //	int maxscore = -1;
        //	foreach(Player pl in Players) {
        //		if(pl.toadspicked > maxscore) {
        //			winner = pl;
        //			maxscore = pl.toadspicked;
        //		}
        //	}

        //	// broadcast who won the round
        //	if(winner.toadspicked > 0) {
        //		Broadcast("Chat", "Server", winner.ConnectUserId + " picked " + winner.toadspicked + " Toadstools and won this round.");
        //	} else {
        //		Broadcast("Chat", "Server", "No one won this round.");
        //	}

        //	// reset everyone's score
        //	foreach(Player pl in Players) {
        //		pl.toadspicked = 0;
        //	}
        //	Broadcast("ToadCount", 0);
        //}

        //private void respawntoads() {
        //	if(Toads.Count == 10)
        //		return;

        //	System.Random random = new System.Random();
        //	// create new toads if there are less than 10
        //	for(int x = 0; x < 10 - Toads.Count; x++) {
        //		int px = random.Next(-9, 9);
        //		int pz = random.Next(-9, 9);
        //		Toad temp = new Toad();
        //		temp.id = last_toad_id;
        //		temp.posx = px;
        //		temp.posz = pz;
        //		Toads.Add(temp);
        //		last_toad_id++;

        //		// broadcast new toad information to all players
        //		Broadcast("Toad", temp.id, temp.posx, temp.posz);
        //	}
        //}

        // This method is called when the last player leaves the room, and it's closed down.
        public override void GameClosed()
        {
            Console.WriteLine("RoomId: " + RoomId);
        }

        // This method is called whenever a player joins the game
        public override void UserJoined(Player player)
        {
            //PlayersJoinedCount++;

            //for (int i = 0; i < PlayersJoinedId.Length; i++)
            //{
            //    if (PlayersJoinedId[i] == string.Empty)
            //    {
            //        player.UserId = player.ConnectUserId;
            //        player.CurrentIndex = i;
            //        player.name = player.JoinData["userName"];
            //        PlayersJoinedId[i] = player.UserId;
            //        break;
            //    }
            //}

            player.UserId = player.ConnectUserId;            
            player.name = player.JoinData["userName"];
            player.isReady = false;            

            //player is the new player join into room
            foreach (Player pl in Players)
            {
                foreach (Player pl1 in Players)
                {
                    //send others userid to every one except it self
                    if (pl.UserId != pl1.UserId && pl1.UserId != string.Empty)
                    {
                        pl.Send("OthersJoined", pl1.UserId, pl1.CurrentIndex, pl1.name);
                    }
                }

            }

            //// send current toadstool info to the player
            //foreach(Toad t in Toads) {
            //	player.Send("Toad", t.id, t.posx, t.posz);
            //}
        }

        // This method is called when a player leaves the game
        public override void UserLeft(Player player)
        {
            //PlayersJoinedCount--;
            Broadcast("PlayerLeft", player.ConnectUserId, player.CurrentIndex);
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
                case "SetReady":
                    player.isReady = message.GetBoolean(1);
                    Random rnd = new Random();
                    Broadcast("SetReady", player.ConnectUserId, message.GetFloat(0), message.GetBoolean(1), rnd.Next(0, 4));
                    break;
                case "Move":
                    Broadcast("Move",
                        player.ConnectUserId,
                        message.GetFloat(0), message.GetFloat(1), message.GetFloat(2),
                        message.GetFloat(3), message.GetFloat(4), message.GetFloat(5),
                        message.GetFloat(6)//GetRunSpeed                        
                        );
                    break;                
                case "CharacterState":
                    Broadcast("CharacterState", player.ConnectUserId, message.GetFloat(0));
                    break;
                case "Jumping":
                    Broadcast("Jumping", player.ConnectUserId, message.GetBoolean(0));
                    break;
                case "SetEnvironmet":
                    Broadcast("SetEnvironmet", player.ConnectUserId, message.GetBoolean(0), message.GetBoolean(1), message.GetBoolean(2));
                    break;
                //case "SetRunSpeed":
                //    Broadcast("SetRunSpeed", player.ConnectUserId, message.GetFloat(0));
                //    break;

                //case "MoveHarvest":
                //	// called when a player clicks on a harvesting node
                //	// sends back a harvesting command to the player, a move command to everyone else
                //	player.posx = message.GetFloat(0);
                //	player.posz = message.GetFloat(1);
                //	foreach (Player pl in Players)
                //	{
                //		if (pl.ConnectUserId != player.ConnectUserId)
                //		{
                //			pl.Send("Move", player.ConnectUserId, player.posx, player.posz);
                //		}
                //	}
                //	player.Send("Harvest", player.ConnectUserId, player.posx, player.posz);
                //	break;
                case "Chat":
                    foreach (Player pl in Players)
                    {
                        if (pl.ConnectUserId != player.ConnectUserId)
                        {
                            pl.Send("Chat", player.ConnectUserId, message.GetString(0));
                        }
                    }
                    break;
            }
        }
    }
}