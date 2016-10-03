//----------------------------------------------------------------------------//
//               █      █                                                     //
//               ████████                                                     //
//             ██        ██                                                   //
//            ███  █  █  ███        GameCore.cs                               //
//            █ █        █ █        CoreSokoban                               //
//             ████████████                                                   //
//           █              █       Copyright (c) 2016                        //
//          █     █    █     █      AmazingCow - www.AmazingCow.com           //
//          █     █    █     █                                                //
//           █              █       N2OMatt - n2omatt@amazingcow.com          //
//             ████████████         www.amazingcow.com/n2omatt                //
//                                                                            //
//                  This software is licensed as GPLv3                        //
//                 CHECK THE COPYING FILE TO MORE DETAILS                     //
//                                                                            //
//    Permission is granted to anyone to use this software for any purpose,   //
//   including commercial applications, and to alter it and redistribute it   //
//               freely, subject to the following restrictions:               //
//                                                                            //
//     0. You **CANNOT** change the type of the license.                      //
//     1. The origin of this software must not be misrepresented;             //
//        you must not claim that you wrote the original software.            //
//     2. If you use this software in a product, an acknowledgment in the     //
//        product IS HIGHLY APPRECIATED, both in source and binary forms.     //
//        (See opensource.AmazingCow.com/acknowledgment.html for details).    //
//        If you will not acknowledge, just send us a email. We'll be         //
//        *VERY* happy to see our work being used by other people. :)         //
//        The email is: acknowledgment_opensource@AmazingCow.com              //
//     3. Altered source versions must be plainly marked as such,             //
//        and must not be misrepresented as being the original software.      //
//     4. This notice may not be removed or altered from any source           //
//        distribution.                                                       //
//     5. Most important, you must have fun. ;)                               //
//                                                                            //
//      Visit opensource.amazingcow.com for more open-source projects.        //
//                                                                            //
//                                  Enjoy :)                                  //
//----------------------------------------------------------------------------//
// Usings //
//System
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;


namespace com.amazingcow.Sokoban
{
    public class GameCore
    {
        ////////////////////////////////////////////////////////////////////////
        // Enums                                                              //
        ////////////////////////////////////////////////////////////////////////
        public enum MoveDirection {
            Left,
            Right,
            Up,
            Down
        };

        public enum MoveType {
            Valid,
            Invalid,
            Box,
            BoxOnGoal
        }


        ////////////////////////////////////////////////////////////////////////
        // Properties                                                         //
        ////////////////////////////////////////////////////////////////////////
        public Coord       PlayerCoord     { get; private set; }
        public List<Coord> WallCoords      { get; private set; }
        public List<Coord> GoalCoords      { get; private set; }
        public List<Coord> BoxCoords       { get; private set; }
        public List<Coord> BoxOnGoalCoords { get; private set; }
        public List<Coord> FloorCoords     { get; private set; }

        public int FieldHeight { get; private set; }
        public int FieldWidth  { get; private set; }

        public int Moves  { get; private set; }
        public int Pushes { get; private set; }

        public bool IsGameOver
        {
            get {
                return BoxOnGoalCoords.Count == GoalCoords.Count;
            }
        }

        public string SnapshotString { get; private set; }


        ////////////////////////////////////////////////////////////////////////
        // iVars                                                              //
        ////////////////////////////////////////////////////////////////////////
        readonly Dictionary<MoveDirection, Coord> _movesCoordsDict;
        SokReader _level;


        ////////////////////////////////////////////////////////////////////////
        // CTOR                                                                //
        ////////////////////////////////////////////////////////////////////////
        public GameCore(SokReader level)
        {
            //iVars
            _level = level;

            _movesCoordsDict = new Dictionary<MoveDirection, Coord>(4);
            _movesCoordsDict.Add(MoveDirection.Left,  Coord.Left );
            _movesCoordsDict.Add(MoveDirection.Right, Coord.Right);
            _movesCoordsDict.Add(MoveDirection.Up,    Coord.Up   );
            _movesCoordsDict.Add(MoveDirection.Down,  Coord.Down );

            //Post init
            Reset();
        }


        ////////////////////////////////////////////////////////////////////////
        // Action Methods                                                     //
        ////////////////////////////////////////////////////////////////////////
        public MoveType MovePlayer(MoveDirection direction)
        {
            var originalCoord = PlayerCoord;
            var targetCoord   = PlayerCoord + _movesCoordsDict[direction];

            //Against the wall.
            if(IsWall(targetCoord))
                return MoveType.Invalid;

            //Against the box.
            if(IsBox(targetCoord) || IsBoxOnGoal(targetCoord))
                return TryMoveBox(targetCoord, direction);

            //Did move but did not push any box.
            ++Moves;
            PlayerCoord     = targetCoord;
            SnapshotString += Char.ToLower(direction.ToString()[0]);

            return MoveType.Valid;
        }

        public void Undo()
        {
            if(SnapshotString.Length == 0)
                return;

            var newSnapshotString = SnapshotString.Remove(SnapshotString.Length-1);
            Reset();

            UndoHelper(newSnapshotString);
        }


        ////////////////////////////////////////////////////////////////////////
        // Query Methods                                                      //
        ////////////////////////////////////////////////////////////////////////
        public bool IsWall(Coord coord)
        {
            return WallCoords.Contains(coord);
        }

        public bool IsBox(Coord coord)
        {
            return BoxCoords.Contains(coord);
        }

        public bool IsBoxOnGoal(Coord coord)
        {
            return BoxOnGoalCoords.Contains(coord);
        }

        public bool IsGoal(Coord coord)
        {
            return GoalCoords.Contains(coord);
        }


        ////////////////////////////////////////////////////////////////////////
        // Private Methods                                                    //
        ////////////////////////////////////////////////////////////////////////
        void Reset()
        {
            //Properties
            PlayerCoord = _level.PlayerCoord;

            //new because we need a copy.
            WallCoords      = new List<Coord>(_level.WallCoords);
            GoalCoords      = new List<Coord>(_level.GoalCoords);
            BoxCoords       = new List<Coord>(_level.BoxCoords);
            BoxOnGoalCoords = new List<Coord>(_level.BoxOnGoalCoords);
            FloorCoords     = new List<Coord>(_level.FloorCoords);

            FieldHeight = _level.Field.GetLength(0);
            FieldWidth  = _level.Field.GetLength(1);

            Moves  = 0;
            Pushes = 0;

            SnapshotString = "";
        }


        MoveType TryMoveBox(Coord boxCoord, MoveDirection direction)
        {
            var originalCoord = boxCoord;
            var targetCoord   = boxCoord + _movesCoordsDict[direction];

            //Check Wall.
            if(IsWall(targetCoord))
                return MoveType.Invalid;

            //Check Other Boxes.
            if(IsBox(targetCoord) || IsBoxOnGoal(targetCoord))
                return MoveType.Invalid;

            //Remove the original coord and add the new coord
            //This makes the box "moves"
            if(IsGoal(originalCoord))
                BoxOnGoalCoords.Remove(originalCoord);
            else
                BoxCoords.Remove(originalCoord);

            var moveType = MoveType.Box;
            if(IsGoal(targetCoord))
            {
                BoxOnGoalCoords.Add(targetCoord);
                moveType = MoveType.BoxOnGoal;
            }
            else
            {
                BoxCoords.Add(targetCoord);
                moveType = MoveType.Box;
            }

            //Player did push the box and now is stand
            //on the originalCoord.
            Pushes++;
            PlayerCoord     = originalCoord;
            SnapshotString += Char.ToUpper(direction.ToString()[0]);

            return moveType;
        }


        private void UndoHelper(string newSnapshotString)
        {
            foreach(var c in newSnapshotString)
            {
                switch(Char.ToLower(c))
                {
                    case 'u' : MovePlayer(MoveDirection.Up   ); break;
                    case 'd' : MovePlayer(MoveDirection.Down ); break;
                    case 'r' : MovePlayer(MoveDirection.Right); break;
                    case 'l' : MovePlayer(MoveDirection.Left ); break;
                }
            }
        }


     }//class GameCore
}//namespace com.amazingcow.Sokoban

