#region Usings
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AmazingCow.GameCores.CoreCoord;
#endregion //Usings

namespace AmazingCow.GameCores.CoreSokoban
{
    public class SokReader
    {
        #region Constants
        //::::::::::::::::::::::::::: Board ::::::::::::::::::::::::::
        //:: Legend.................:      :.................Legend ::
        //::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //:: Wall...................: #  # :...................Wall ::
        //:: Pusher.................: p  @ :.................Pusher ::
        //:: Pusher on goal square..: P  + :..Pusher on goal square ::
        //:: Box....................: b  $ :....................Box ::
        //:: Box on goal square.....: B  * :.....Box on goal square ::
        //:: Goal square............: .  . :............Goal square ::
        //:: Floor..................:      :..................Floor ::
        //:: Floor..................: -  _ :..................Floor ::
        //Taken from: 
        //  http://www.sokobano.de/wiki/index.php?title=Sok_format
        private static readonly char[] kChars_WallChars    = {'#'};
        private static readonly char[] kChars_Pusher       = {'p', '@'};
        private static readonly char[] kChars_PusherOnGoal = {'P', '+'};
        private static readonly char[] kChars_Box          = {'b', '$'};
        private static readonly char[] kChars_BoxOnGoal    = {'B', '*'};
        private static readonly char[] kChars_Goal         = {'.'};
        private static readonly char[] kChars_Floor        = {' ', '-', '_'};
        #endregion //Constants


        #region Properties
        public Coord       PlayerCoord;

        public List<Coord> WallCoords;
        public List<Coord> GoalCoords;
        public List<Coord> BoxCoords;
        public List<Coord> BoxOnGoalCoords;
        public List<Coord> FloorCoords;

        public List<List<char>> Field;
        #endregion //Properties

        #region CTOR 
        public SokReader(String filename)
        {
            //Init the lists.
            WallCoords      = new List<Coord>();
            GoalCoords      = new List<Coord>();
            BoxCoords       = new List<Coord>();
            BoxOnGoalCoords = new List<Coord>();
            FloorCoords     = new List<Coord>();

            //Read the file.
            ParseFile(filename);
        }
        #endregion //CTOR


        #region Helper Methods
        public void ParseFile(String filename)
        {
            var text_lines = File.ReadAllLines(filename);
            var curr_coord = new Coord();

            Field = new List<List<char>>(text_lines.Length);
            foreach(var text_line in text_lines)
            {                
                var char_list = new List<Char>(text_line.Length);
                foreach(var curr_char in text_line)
                {
                    //Wall
                    if(kChars_WallChars.Contains(curr_char))
                    {
                        WallCoords.Add(curr_coord);
                    }
                    //Pusher
                    else if(kChars_Pusher.Contains(curr_char) 
                         || kChars_PusherOnGoal.Contains(curr_char))
                    {
                        PlayerCoord = curr_coord;
                    }
                    //Box 
                    else if(kChars_Box.Contains(curr_char))
                    {
                        BoxCoords.Add(curr_coord);
                    }
                    //Box On Goal
                    else if(kChars_BoxOnGoal.Contains(curr_char))
                    {
                        BoxOnGoalCoords.Add(curr_coord);
                        GoalCoords.Add     (curr_coord);
                    }
                    //Box 
                    else if(kChars_Goal.Contains(curr_char))
                    {
                        GoalCoords.Add(curr_coord);
                    }

                    //Floor is everywhere!
                    FloorCoords.Add(curr_coord);

                    ++curr_coord.X;
                }

                curr_coord.X  = 0;
                curr_coord.Y += 1;
            }//foreach(var text_line in text_lines)
        }
        #endregion //Helper Methods
    
    }//class SokReader
}//namespace AmazingCow.GameCores.CoreSokoban

