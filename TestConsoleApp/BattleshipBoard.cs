using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsoleApp
{
    class BattleshipBoard
    {
        private string[] board = new string[25];
        private List<int> enemyLocations = new List<int>();
        private List<int> enemyLocationsBackup = new List<int>();
        private int numGuesses = 20;

        private int twoShipHealth = 2;
        private int threeShipHealth = 3;
        private int fourShipHealth = 4;


        string dest = "destroyer";
        string sub = "submarine";
        string batt = "battleship";


        public BattleshipBoard()
        {
            
            for (int i = 0; i < 25; i++)
            {
                board[i] = "0";
            }

            PlaceEnemy();
            int[] temp = new int[9];
            enemyLocations.CopyTo(temp);
            enemyLocationsBackup.AddRange(temp);
        }
        public void PlayGame()
        {

            Display(-1);

            while (true)
            {
                //TellEnemyLocoations();
                Console.Write("Enter a location to guess or \"help\" for how to guess: ");
                string input = Console.ReadLine();
                int guess;
                if (input == "help")
                {
                    Display(-2);
                }
                else if (Int32.TryParse(input, out guess)) // add an else if to make sure that input is correct 
                {
                    if(guess < 25)
                    {
                        Display(guess);

                        if (IsGameOver())
                        {
                            Win();
                            Console.ReadLine();
                            break;
                        }

                        if (numGuesses < 0)
                        {
                            Lose();
                            Console.ReadLine();
                            break;
                        }
                    }
                    else
                    {
                        // input is too big
                        Display(-4); // -4 is option for input being too big
                        continue;
                    }
                }
                else
                {
                    // input is invalid
                    Display(-5); // -5 is option for input being invalid
                    continue;
                }
            }
        }

        private void Lose()
        {
            for (int i = 0; i < board.Length; i++)
            {
                if (enemyLocationsBackup.Contains(i))
                {
                    // does B | S | D look better than 4 | 3 | 2
                    board[i] = (enemyLocationsBackup.IndexOf(i) < 4) ? "4" : (enemyLocationsBackup.IndexOf(i) < 7) ? "3" : "2";
                }
                else
                {
                    board[i] = "0";
                }
                
            }

            Console.Clear();
            int count = 0;
            Console.Write("\t");
            foreach (string item in board)
            {
                Console.Write((count % 5 != 0) ? item + " " : "\n\t" + item + " ");
                count++;
            }
            Console.WriteLine("\n------------------------------------------\n\tYOU LOSE\n------------------------------------------");
        }

        private void Win()
        {
            Console.Clear();
            Console.WriteLine("------------------------------------------\n\tYOU WIN\n------------------------------------------");
        }

        public void Display(int guess)
        {
            int hitSuccess = 0; // default to miss
            int shipDead = 0;
            int count = 0;
            Console.Clear();
            if(guess > -1)
            {
                hitSuccess = CheckHit(guess);  // checkHit returns 2 - Hit | 1 - already guessed here | 0 - Miss

                shipDead = IsShipDead(guess); // returns 2 | 4 | 3 | 0 to specify which ship is dead or
                                              // returns 0 if no ship is dead

                // decrement numGuesses here so that it will display in the output
                // do not decremnt if the user has guessed the same spot again
                if (hitSuccess != 1)
                {
                    numGuesses--;
                }

            }
            // replace with ascii banner later
            Console.WriteLine("------------------------------------------\n\tBATTLESHIP\n------------------------------------------");
            Console.Write("\t");
            foreach (string item in board)
            {
                Console.Write((count % 5 != 0) ? item + " " : "\n\t" + item + " ");
                count++;
            }

            // --------------------- HELP --------------------
            if (guess == -2) // -2 is option for help display
            {
                Console.Write("\n\n\t");
                for (int i = 0; i < board.Length; i++)
                {
                    if(i < 10)
                    {
                        Console.Write((i % 5 != 0) ? i + "  " : "\n\t" + i + "  ");
                    }
                    else
                    {
                        Console.Write((i % 5 != 0) ? i + " " : "\n\t" + i + " ");
                    }
                }          
            }

            
            Console.WriteLine("\n------------------------------------------");
            Console.WriteLine($"\n{numGuesses} guesses left | {dest} | {sub} | {batt}");
            Console.WriteLine("------------------------------------------");

            // hit |miss | same guess output
            if (guess > -1)
            {
                Console.WriteLine((hitSuccess == 2) ? "Hit" : (hitSuccess == 1) ? "Already guessed here" : "Miss");

                // which ship sank output
                if(shipDead != 0)
                {
                    Console.WriteLine((shipDead == 2) ? "You sank my Destroyer" : (shipDead == 3) ? "You sank my Submarine" : "You sank my Battleship");
                }
            }
            else if (guess == -4) // -4 is option for too big a number
            {
                Console.WriteLine("Input too large.\nPlease enter a number in the range (0-24)");
            }
            else if (guess == -5) // -5 is option for invalid input
            {
                Console.WriteLine("Invalid input.\nPlease enter a number in the range (0-24)");
            }
        }


        // logic to make sure that ships placed horizontally dont overflow to the next row 
        // and that ships placed vertically don't overflow to the next column
        // also makes sure that ships don't overlap
        private void PlaceEnemy()
        {
            Random rand = new Random();
            bool horiOrVert;
            bool validSpot;
            int startingPoint;
            
            for (int shipLength = 4; shipLength > 1; shipLength--)
            {
                horiOrVert = (rand.Next(2) == 1); // if the expression = 1 the horizontal placement | if expression = 0 vertical placement; 
                                                                // true = Horizontal placement | false = vertical placement

                // ----------------------HORIZONTAL PLACEMENT CODE-----------------------------------------
                if (horiOrVert)
                {
                    do
                    {
                        startingPoint = rand.Next(25);
                        Console.WriteLine($"attempting to place enemy ship of length {shipLength} at {startingPoint}");

                        // makes sure that the ships cannot be placed on top of one another horizontally
                        if (SpotAvailable(startingPoint, shipLength, horiOrVert))
                        {
                            // makes sure that a ship doesn't go over to the next line
                            // for example the 3 length ship cannot be placed in columns 3 or 4
                            // using the expression we get (col(3) + (3-1)) % 5 > 3 - 2
                            //                             (3 + 2) % 5 > 1
                            //                             (5) % 5 > 1
                            //                             which returns false b/c 1 is not greater than 1
                            //                             so it goes into the else block
                            if ((startingPoint + shipLength - 1) % 5 > shipLength - 2)
                            {
                                validSpot = true;
                                Console.WriteLine("success");
                            }
                            else
                            {
                                validSpot = false;
                                Console.WriteLine("fail");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Spot already taken");
                            validSpot = false;
                        }

                    }
                    while (!validSpot);
                    for (int i = 0; i < shipLength; i++)
                    {
                        //board[startingPoint + i] = shipLength;
                        enemyLocations.Add((startingPoint + i) % 25); // add the address of this part of the ship (0-24) to the enemyLocations list
                    }
                }

                // ----------------------VERTICAL PLACEMENT CODE-----------------------------------------
                else
                {
                    do
                    {
                        // generate a random spot
                        // you can never start a ship in the bottom row
                        // so rand can only be in range (0 - 19 ) for 2 ship | (0 - 14) for 3 ship | (0-9) for 4 ship | (0-4) for 5 ship
                        startingPoint = rand.Next((6 - shipLength) * 5); // this guarantees~ that the ship will be in a valid spot

                        // check if another ship occupies any spot this ship will take up
                        validSpot = SpotAvailable(startingPoint, shipLength, horiOrVert);
                        Console.WriteLine(validSpot ? "success" : "spot already occupied");


                    } while (!validSpot);

                    // place ship
                    for (int i = 0; i < shipLength; i++)
                    {
                        //board[startingPoint + (i * 5)] = shipLength;
                        enemyLocations.Add((startingPoint + (i * 5)) % 25); // add the address of this part of the ship (0-24) to the enemyLocations list
                    }
                }
            }
        }


        // used for ship placement logic
        // return false if another ship occupies any of the spots for the current ship will be placed in and true otherwise
        private bool SpotAvailable(int spotToCheck, int shipLength, bool horiOrVert)
        {
            for (int i = 0; i < shipLength; i++)
            {
                // if attempting to place horizontally use spotToCheck + i
                // if attempting to place vertically use spotToCheck + (i * 5)
                if (enemyLocations.Contains((horiOrVert) ? (spotToCheck + i) : (spotToCheck + (i * 5))))
                {
                    return false;
                }
            }

            return true;
        }

        // debug method to test if the enemy is being placed correctly and test out the shooting method
        public void TellEnemyLocoations()
        {
            Console.Write("Ship is located at points: [");
            foreach(int shipPart in enemyLocations)
            {
                Console.Write($" {shipPart}");
            }
            Console.WriteLine("]");
        }



        // determines if the specified location is a hit a miss or a location that you have already guessed
        // returns ship that you 2 - Hit | 1 - already guessed here | 0 - Miss 
        public int CheckHit(int hitAttempt)
        {
            // --------------------- HIT --------------------
            if (enemyLocations.Contains(hitAttempt))
            {
                board[hitAttempt] = "1";
                // Method to check ship that you hit's health/ if you have won
                //Console.WriteLine("Hit");
                return 2;
            }

            // --------------------- ALREADY GUESSED HERE --------------------
            else if (board[hitAttempt] != "0") // if location != 0 you've already guessed here
            {
                //Console.WriteLine("Already guessed here.");
                return 1;

            }

            // --------------------- MISS --------------------
            else
            {
                board[hitAttempt] = "X";
                //Console.WriteLine("Miss");
                return 0;
            }
            

        }

        // returns the ship that was killed  (2, 3, 4)
        // or if no ship was killed it returns 0
        public int IsShipDead(int hitAttempt) 
        {
            int whichShipPart = enemyLocations.IndexOf(hitAttempt); // the part that was attacked
            int whichShip;                                          // the ship that part belongs to

            if (whichShipPart != -1) // if the hit attempt actaully hit a ship part
            {
                whichShip = (whichShipPart < 4) ? 4 : (whichShipPart < 7) ? 3 : 2; // find out which ship the part belongs to
            }
            else
            {
                whichShip = -1;
            }



            // then decrement that ships health by 1
            switch (whichShip)
            {
                // --------------------- Destroyer --------------------
                case 2:
                    twoShipHealth--;
                    if (twoShipHealth == 0)
                    {
                        dest = (twoShipHealth > 0) ? "Destroyer" : "";
                        // set that part of the ship in the enemiesLocations to -1
                        enemyLocations[whichShipPart] = -1;
                        return whichShip;
                    }
                    break;

                // --------------------- Submarine --------------------
                case 3:
                    threeShipHealth--;
                    if (threeShipHealth == 0)
                    {
                        sub = (threeShipHealth > 0) ? "Submarine" : "";
                        // set that part of the ship in the enemiesLocations to -1
                        enemyLocations[whichShipPart] = -1;
                        return whichShip;
                    }
                    break;

                // --------------------- Battleship --------------------
                case 4:
                    fourShipHealth--;
                    if (fourShipHealth == 0)
                    {
                        batt = (fourShipHealth > 0) ? "Battleship" : "";
                        // set that part of the ship in the enemiesLocations to -1
                        enemyLocations[whichShipPart] = -1;
                        return whichShip;
                    }
                    break;

                default: // default not used
                    break;
            }
            // --------------------- Miss --------------------
            // no ship part was hit return 0
            return 0;

        }

        // returns true if all ships are dead
        // false otherwise
        public bool IsGameOver()
        {
            return (twoShipHealth == 0 && threeShipHealth == 0 && fourShipHealth == 0) ? true : false;
        }
    }
}
